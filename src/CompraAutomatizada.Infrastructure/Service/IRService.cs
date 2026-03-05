using CompraAutomatizada.Application.DTOs;
using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Aggregates.EventoIRAggregate;
using CompraAutomatizada.Domain.Enums;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using CompraAutomatizada.Infrastructure.Messaging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CompraAutomatizada.Infrastructure.Services;

public class IRService : IIRService
{
    private readonly IEventoIRRepository _eventoIRRepository;
    private readonly IKafkaPublisher _kafkaPublisher;
    private readonly IConfiguration _configuration;
    private readonly ILogger<IRService> _logger;

    private const decimal AliquotaDedoDuro = 0.00005m;
    private const decimal AliquotaIRVenda = 0.20m;
    private const decimal LimiteIsencaoVenda = 20000m;

    public IRService(
        IEventoIRRepository eventoIRRepository,
        IKafkaPublisher kafkaPublisher,
        IConfiguration configuration,
        ILogger<IRService> logger)
    {
        _eventoIRRepository = eventoIRRepository;
        _kafkaPublisher = kafkaPublisher;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task ProcessarDedoDuroAsync(long clienteId, string ticker, int quantidade, decimal precoUnitario, CancellationToken cancellationToken = default)
    {
        var valorOperacao = Math.Round(quantidade * precoUnitario, 2);

        var evento = EventoIR.CriarDedoDuro(clienteId, ticker, valorOperacao);
        await _eventoIRRepository.AddAsync(evento, cancellationToken);

        await _eventoIRRepository.SaveChangesAsync(cancellationToken);

        await PublicarEventoAsync(evento, cancellationToken);
    }

    public async Task ProcessarIRVendaAsync(long clienteId, string ticker, int quantidade, decimal precoVenda, decimal precoMedioAquisicao, CancellationToken cancellationToken = default)
    {
        var valorOperacao = Math.Round(quantidade * precoVenda, 2);
        var custoAquisicao = Math.Round(quantidade * precoMedioAquisicao, 2);
        var lucroLiquido = valorOperacao - custoAquisicao;

        var totalVendasMes = await _eventoIRRepository.GetTotalVendasNoMesAsync(
            clienteId, DateTime.UtcNow.Year, DateTime.UtcNow.Month, cancellationToken);

        var totalComEstaVenda = totalVendasMes + valorOperacao;

        if (totalComEstaVenda <= LimiteIsencaoVenda || lucroLiquido <= 0)
            return;

        var evento = EventoIR.CriarIrVenda(clienteId, ticker, valorOperacao, lucroLiquido);
        await _eventoIRRepository.AddAsync(evento, cancellationToken);

        await _eventoIRRepository.SaveChangesAsync(cancellationToken);

        await PublicarEventoAsync(evento, cancellationToken);
    }

    private async Task PublicarEventoAsync(EventoIR evento, CancellationToken cancellationToken)
    {
        var topico = _configuration["Kafka:TopicoIR"]
            ?? "compra-programada.eventos-ir";

        var mensagem = new EventoIRMessage(
            evento.ClienteId,
            evento.Ticker,
            evento.Tipo.ToString(),
            evento.ValorOperacao,
            evento.ValorIr,
            evento.LucroLiquido,
            evento.DataEvento
        );

        try
        {
            await _kafkaPublisher.PublicarAsync(topico, evento.ClienteId.ToString(), mensagem, cancellationToken);
            evento.MarcarComoPublicado();
            await _eventoIRRepository.UpdateAsync(evento, cancellationToken);
            await _eventoIRRepository.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Falha ao publicar evento IR no Kafka. ClienteId: {ClienteId}", evento.ClienteId);
        }
    }
}
