using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using MKIL.DotnetTest.Shared.Lib.Utilities;
using Polly;
using Polly.Retry;
using Serilog;
using Serilog.Context;
using System.Net.Sockets;
using System.Text.Json;
using ILogger = Serilog.ILogger;

namespace MKIL.DotnetTest.Shared.Lib.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string topic, T message, string? correlationId = null, CancellationToken cancellationToken = default);
        Task<bool> IsHealthyAsync();
    }

    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger _logger;
        private readonly AsyncRetryPolicy _retryPolicy;
        private const int MaxRetryAttempts = 3;
        private readonly string _bootstrapServers;

        public KafkaEventPublisher(IConfiguration configuration)
        {
            // Use Serilog's static logger with context
            _logger = Log.ForContext<KafkaEventPublisher>();

            // Configure retry policy
            _retryPolicy = Policy
                .Handle<Exception>(ex => TransientErrorDetector.IsTransient(ex))
                .WaitAndRetryAsync(
                    retryCount: MaxRetryAttempts,
                    sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (exception, timeSpan, retryCount, context) =>
                    {
                        _logger.Warning(
                            "Retry {RetryCount} / {MaxRetryAttempts} after {Delay}ms due to: {Exception}",
                            retryCount,
                            MaxRetryAttempts,
                            timeSpan.TotalMilliseconds,
                            exception.Message
                        );
                    }
                );

            _bootstrapServers = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers missing configuration");

            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServers,
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageTimeoutMs = 10000,
                RequestTimeoutMs = 5000,

                // Additional resilience settings
                MessageSendMaxRetries = 3,
                RetryBackoffMs = 100,
                SocketKeepaliveEnable = true,

                // Compression for better network efficiency
                CompressionType = CompressionType.Snappy
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.Error("Kafka Producer Error: {Reason} - {Code}", error.Reason, error.Code);
                })
                .Build();

            _logger.Information("KafkaEventPublisher initialized with broker: {BootstrapServers}",
                configuration["Kafka:BootstrapServers"]);
        }

        public async Task PublishAsync<T>(string topic, T message, string? correlationId, CancellationToken cancellationToken = default)
        {
            if(correlationId == null) 
                correlationId = Guid.NewGuid().ToString();

            LogContext.PushProperty("CorrelationId", correlationId);

            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);

                _logger.Debug("Publishing message {MessageId} to topic {Topic}", correlationId, topic);

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    #region FOR TESTING PURPOSE ONLY
                    if(message is string) 
                    {
                        string? msgStr = message as string;
                        if (msgStr == "TEST FOR PRODUCER PERMANENT ERROR")
                            throw new JsonException("From TEST PERMANENT ERROR");

                        if (msgStr  == "TEST FOR PRODUCER TRANSIENT ERROR")
                            throw new SocketException((int)SocketError.TimedOut, "From TEST TRANSIENT ERROR");
                    }
                    
                    #endregion FOR TESTING PURPOSE ONLY

                    var kafkaMessage = new Message<string, string>
                    {
                        Key = correlationId,
                        Value = serializedMessage,
                        Headers = new Headers {
                            { "CorrelationId", System.Text.Encoding.UTF8.GetBytes(correlationId) },
                            { "timestamp", System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString("o")) }
                        }
                    };

                    var result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

                    _logger.Information(
                        "Successfully published message {MessageId} to {Topic} at partition {Partition}, offset {Offset}",
                        correlationId,
                        topic,
                        result.Partition.Value,
                        result.Offset.Value
                    );

                });
                    
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.Error(ex,
                    "Failed to publish message {MessageId} to topic {Topic}. Error code: {ErrorCode}",
                    correlationId,
                    topic,
                    ex.Error.Code
                );
                throw;
            }
            catch (Exception ex)
            {
                _logger.Error(ex,
                    "Unexpected error publishing message {MessageId} to topic {Topic}",
                    correlationId,
                    topic
                );
                throw;
            }
        }

        public async Task<bool> IsHealthyAsync()
        {
            try
            {
                using var adminClient = new AdminClientBuilder(new AdminClientConfig
                {
                    BootstrapServers = _bootstrapServers
                }).Build();

                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                return metadata.Brokers.Any();
            }
            catch (Exception ex)
            {
                _logger.Warning(ex, "Kafka health check failed");
                return false;
            }
        }


        public void Dispose()
        {
            try
            {
                _logger.Information("Flushing and disposing Kafka producer");
                _producer?.Flush(TimeSpan.FromSeconds(10));
                _producer?.Dispose();
                _logger.Information("Kafka producer disposed successfully");
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Error disposing Kafka producer");
            }
        }
    }
}
