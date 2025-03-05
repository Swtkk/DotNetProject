using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

public class KafkaConsumerService : BackgroundService
{
    private readonly string _kafkaTopic = "memes-topic";
    private readonly string _kafkaBootstrapServers = "localhost:9092"; // Wymuszenie IPv4
    private readonly ILogger<KafkaConsumerService> _logger;
    private static string _lastMemeUrl = "";

    public KafkaConsumerService(ILogger<KafkaConsumerService> logger)
    {
        _logger = logger;
    }

    public static string GetLastMemeUrl()
    {
        return _lastMemeUrl;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaBootstrapServers,
            GroupId = "meme-group",
            AutoOffsetReset = AutoOffsetReset.Latest
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(_kafkaTopic);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                _lastMemeUrl = consumeResult.Message.Value;
                _logger.LogInformation($"✅ Otrzymano mema z Kafka: {_lastMemeUrl}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Błąd Kafka: {ex.Message}");
            }

            await Task.Delay(1000, stoppingToken);
        }
    }
}