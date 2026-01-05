using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text.Json;
using ILogger = Serilog.ILogger;

namespace MKIL.DotnetTest.Shared.Lib.Messaging
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(string topic, T message, string? correlationId = null, CancellationToken cancellationToken = default);
    }

    public class KafkaEventPublisher : IEventPublisher, IDisposable
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger _logger;

        public KafkaEventPublisher(IConfiguration configuration)
        {
            // Use Serilog's static logger with context
            _logger = Log.ForContext<KafkaEventPublisher>();

            var config = new ProducerConfig
            {
                BootstrapServers = configuration["Kafka:BootstrapServers"],
                Acks = Acks.All,
                EnableIdempotence = true,
                MessageTimeoutMs = 10000,
                RequestTimeoutMs = 5000
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

            try
            {
                var serializedMessage = JsonSerializer.Serialize(message);

                _logger.Debug("Publishing message {MessageId} to topic {Topic}", correlationId, topic);

                var kafkaMessage = new Message<string, string>
                {
                    Key = correlationId,
                    Value = serializedMessage,
                    Headers = new Headers
                {
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
