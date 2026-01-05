using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MKIL.DotnetTest.OrderService.Domain.Interface;
using MKIL.DotnetTest.Shared.Lib.DTO;
using MKIL.DotnetTest.Shared.Lib.Messaging.Utilities;
using MKIL.DotnetTest.Shared.Lib.Service;
using MKIL.DotnetTest.UserService.Domain.Interfaces;
using Serilog;
using Serilog.Context;
using System.Net.Sockets;
using System.Text.Json;
using ILogger = Serilog.ILogger;

namespace MKIL.DotnetTest.UserService.Infrastructure.BackgroundServices
{
    public class OrderCreatedEventConsumer : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger _logger;
        private readonly IConfiguration _configuration;
        private readonly string _topic_NewOrder;
        private readonly string _groupId;
        private readonly string _bootstrapServer;

        public OrderCreatedEventConsumer(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
            _logger = Log.ForContext<OrderCreatedEventConsumer>();

            _bootstrapServer = _configuration["Kafka:BootstrapServers"]
                ?? throw new InvalidOperationException("Kafka:BootstrapServers configuration is missing");
            _topic_NewOrder = _configuration["Kafka:Topic:NewOrder"]
                ?? throw new InvalidOperationException("Kafka:Topic:NewOrder configuration is missing");
            _groupId = _configuration["Kafka:GroupId"]
                ?? throw new InvalidOperationException("Kafka:GroupId configuration is missing");

        }


        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.Information("UserCreatedEventConsumer starting. GroupId: {GroupId}, Topic: {Topic}",
                _groupId, _topic_NewOrder);

            await Task.Run(() => Consume_NewOrder_Messages(stoppingToken), stoppingToken);
        }

        private async Task Consume_NewOrder_Messages(CancellationToken stoppingToken)
        {
            #region Configure Consumer
            var config = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServer,
                GroupId = _groupId,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                EnableAutoCommit = false,
                AllowAutoCreateTopics = true,
                SessionTimeoutMs = 10000,
                HeartbeatIntervalMs = 3000
            };

            using var consumer = new ConsumerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.Error("Kafka Consumer Error: {Reason} - {Code}", error.Reason, error.Code);
                })
                .SetPartitionsAssignedHandler((_, partitions) =>
                {
                    _logger.Information("Partitions assigned: {Partitions}",
                        string.Join(", ", partitions.Select(p => $"{p.Partition}")));
                })
                .Build();

            consumer.Subscribe(_topic_NewOrder);
            _logger.Information("Subscribed to topic: {Topic}", _topic_NewOrder);
            #endregion Configure Consumer

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    ConsumeResult<string, string>? result = null;

                    try
                    {
                        result = consumer.Consume(TimeSpan.FromSeconds(1));

                        if (result == null) continue;

                        var messageId = GetHeaderValue(result.Message.Headers, "CorrelationId");

                        LogContext.PushProperty("CorrelationId", messageId);
                        LogContext.PushProperty("Partition", result.Partition.Value);
                        LogContext.PushProperty("Offset", result.Offset.Value);

                        _logger.Information("Received message from {Topic}", result.Topic);

                        // Use retry handler
                        await RetryHandler.ExecuteAsync(
                            action: async () =>
                            {
                                #region FOR TESTING PURPOSE ONLY
                                if (result.Message.Value == "\"TEST PERMANENT ERROR\"")
                                    throw new JsonException("From TEST PERMANENT ERROR");

                                if (result.Message.Value == "\"TEST TRANSIENT ERROR\"")
                                    throw new SocketException((int)SocketError.TimedOut, "From TEST TRANSIENT ERROR");
                                #endregion FOR TESTING PURPOSE ONLY

                                await ProcessNewOrder(consumer, result, stoppingToken);
                            },
                            maxRetries: 3,
                            onRetry: (ex, attempt, delay) =>
                            {
                                _logger.Warning(ex,
                                    "Transient error on attempt {Attempt}/3. " +
                                    "Retrying after {Delay}ms. Error: {ErrorType}",
                                    attempt, delay.TotalMilliseconds, ex.GetType().Name);
                            },
                            cancellationToken: stoppingToken
                        );

                        consumer.Commit(result);
                    }
                    catch (Exception ex)
                    {
                        _logger.Error(ex, "Error processing message");
                        using (var scope = _scopeFactory.CreateScope())
                        {
                            var dlqService = scope.ServiceProvider.GetRequiredService<IDeadLetterQueueService>();

                            await dlqService.SendToDeadLetterQueueAsync(result!, ex, stoppingToken);
                        }
                        consumer.Commit(result);
                        await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                _logger.Information("UserCreatedEventConsumer is stopping gracefully");
            }
            catch (Exception ex)
            {
                _logger.Fatal(ex, "UserCreatedEventConsumer had an error not handled");
            }
            finally
            {
                try
                {
                    consumer.Close();
                    _logger.Information("Kafka consumer closed successfully");
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "Error closing Kafka consumer");
                }
            }
        }

        public async Task ProcessNewOrder(IConsumer<string, string> consumer, ConsumeResult<string, string> result, CancellationToken stoppingToken)
        {
            UserDto? userDto = JsonSerializer.Deserialize<UserDto>(result.Message.Value);

            if (userDto != null)
            {
                _logger.Debug("Deserialized event for UserId: {UserId}", userDto.Id);

                // Process with scoped services
                using (var scope = _scopeFactory.CreateScope())
                {
                    var userService = scope.ServiceProvider.GetRequiredService<IUserService>();

                    //await orderService.
                    await userService (userDto, stoppingToken);

                    // Commit after successful processing
                    consumer.Commit(result);

                }

                _logger.Information(
                    "Successfully processed and committed message for UserId: {UserId}",
                    userDto.Id
                );
            }
            else
            {
                _logger.Warning("Failed to deserialize message, skipping");
                consumer.Commit(result); // Commit to avoid reprocessing
            }
        }


        private string GetHeaderValue(Headers headers, string key)
        {
            var header = headers.FirstOrDefault(h => h.Key == key);
            return header != null
                ? System.Text.Encoding.UTF8.GetString(header.GetValueBytes())
                : "unknown";
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.Information("UserCreatedEventConsumer stop requested");
            await base.StopAsync(cancellationToken);
            _logger.Information("UserCreatedEventConsumer stopped");
        }
    }
}

