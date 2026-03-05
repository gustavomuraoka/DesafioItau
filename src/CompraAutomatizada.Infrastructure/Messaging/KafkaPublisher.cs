using Confluent.Kafka;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace CompraAutomatizada.Infrastructure.Messaging;

public interface IKafkaPublisher
{
    Task PublicarAsync<T>(string topico, string chave, T mensagem, CancellationToken cancellationToken = default);
}

public class KafkaPublisher : IKafkaPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaPublisher> _logger;

    public KafkaPublisher(IConfiguration configuration, ILogger<KafkaPublisher> logger)
    {
        _logger = logger;

        var bootstrapServers = configuration["Kafka:BootstrapServers"]
            ?? throw new InvalidOperationException("Kafka:BootstrapServers não configurado.");

        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader,
            MessageTimeoutMs = 5000
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublicarAsync<T>(string topico, string chave, T mensagem, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(mensagem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        var kafkaMessage = new Message<string, string>
        {
            Key = chave,
            Value = json
        };

        try
        {
            var result = await _producer.ProduceAsync(topico, kafkaMessage, cancellationToken);

            _logger.LogInformation(
                "Kafka: mensagem publicada. Tópico: {Topico}, Partição: {Particao}, Offset: {Offset}",
                result.Topic, result.Partition, result.Offset);
        }
        catch (ProduceException<string, string> ex)
        {
            _logger.LogError(ex, "Kafka: falha ao publicar no tópico {Topico}.", topico);
            throw;
        }
    }

    public void Dispose() => _producer?.Dispose();
}
