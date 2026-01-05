using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Serilog;
using System.Text;

namespace MKIL.DotnetTest.Shared.Lib.Service
{
    public interface IDeadLetterQueueService
    {
        public Task SendToDeadLetterQueueAsync(ConsumeResult<string, string> failedMessage, Exception exception, CancellationToken cancellationToken = default);
    }

    public class DeadLetterQueueService : IDeadLetterQueueService
    {
        private readonly IProducer<string, string> _producer;
        private readonly ILogger _logger;
        private readonly string _dlqTopic;
        private readonly string _bootstrapServer;

        public DeadLetterQueueService(IConfiguration configuration)
        {
            _logger = Log.ForContext<DeadLetterQueueService>();
            _dlqTopic = configuration["Kafka:Topic:DeadLetter"] ?? throw new InvalidOperationException("Kafka:Topic:DeadLetter configuration is missing");
            _bootstrapServer = configuration["Kafka:BootstrapServers"] ?? throw new InvalidOperationException("Kafka:BootstrapServers configuration is missing");

            var config = new ProducerConfig
            {
                BootstrapServers = _bootstrapServer,
                ClientId = "dlq-producer",
                Acks = Acks.All, // Ensure DLQ messages are persisted
                MessageTimeoutMs = 10000
            };

            _producer = new ProducerBuilder<string, string>(config)
                .SetErrorHandler((_, error) =>
                {
                    _logger.Error("DLQ Producer Error: {Reason}", error.Reason);
                })
                .Build();
        }

        public async Task SendToDeadLetterQueueAsync(
            ConsumeResult<string, string> failedMessage,
            Exception exception,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var headers = CreateDlqHeaders(failedMessage, exception);

                var message = new Message<string, string>
                {
                    Key = failedMessage.Message.Key,
                    Value = failedMessage.Message.Value,
                    Headers = headers
                };

                var result = await _producer.ProduceAsync(_dlqTopic, message, cancellationToken);

                _logger.Warning(
                    "Message sent to DLQ. Original Topic: {OriginalTopic}, Offset: {Offset}, DLQ Partition: {DlqPartition}",
                    failedMessage.Topic,
                    failedMessage.Offset.Value,
                    result.Partition.Value);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "Failed to send message to DLQ! Original message may be lost.");
            }
        }

        private Headers CreateDlqHeaders(ConsumeResult<string, string>? failedMessage, Exception exception)
        {
            var headers = new Headers();

            if(failedMessage != null)
            {
                // Copy existing headers
                foreach (var header in failedMessage.Message.Headers)
                {
                    headers.Add(header.Key, header.GetValueBytes());
                }

                // Add DLQ metadata
                AddHeader(headers, "dlq-original-topic", failedMessage.Topic);
                AddHeader(headers, "dlq-original-partition", failedMessage.Partition.Value.ToString());
                AddHeader(headers, "dlq-original-offset", failedMessage.Offset.Value.ToString());
            }
               
            AddHeader(headers, "dlq-error-type", exception.GetType().Name);
            AddHeader(headers, "dlq-error-message", exception.Message);
            AddHeader(headers, "dlq-timestamp", DateTimeOffset.UtcNow.ToString("O"));
            AddHeader(headers, "dlq-stack-trace", exception.StackTrace ?? "N/A");

            return headers;
        }

        private void AddHeader(Headers headers, string key, string value)
        {
            headers.Add(key, Encoding.UTF8.GetBytes(value));
        }

        public void Dispose()
        {
            _producer?.Flush(TimeSpan.FromSeconds(10));
            _producer?.Dispose();
        }
    }
}
