using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using CompraAutomatizada.Infrastructure.Cotahist;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CompraAutomatizada.Infrastructure.Services;

public class CotacaoService : ICotacaoService
{
    private readonly ICotacaoRepository _cotacaoRepository;
    private readonly IConfiguration _configuration;
    private readonly ILogger<CotacaoService> _logger;

    public CotacaoService(
        ICotacaoRepository cotacaoRepository,
        IConfiguration configuration,
        ILogger<CotacaoService> logger)
    {
        _cotacaoRepository = cotacaoRepository;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<Cotacao?> ObterUltimaByTickerAsync(string ticker, CancellationToken cancellationToken = default)
        => await _cotacaoRepository.GetUltimaByTickerAsync(ticker, cancellationToken);

    public async Task<IReadOnlyDictionary<string, decimal>> ObterCotacoesFechamentoAsync(IEnumerable<string> tickers, DateOnly? dataReferencia = null, CancellationToken cancellationToken = default)
    {
        var cotacoes = dataReferencia.HasValue
            ? await _cotacaoRepository.GetUltimasByTickersAteDataAsync(tickers, dataReferencia.Value, cancellationToken)
            : await _cotacaoRepository.GetUltimasByTickersAsync(tickers, cancellationToken);

        return cotacoes.ToDictionary(c => c.Ticker, c => c.PrecoFechamento);
    }


    public async Task ImportarCotahistAsync(string caminhoArquivo, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Importando COTAHIST: {Arquivo}", caminhoArquivo);

        var todasCotacoes = CotahistParser.Parse(caminhoArquivo).ToList();

        if (!todasCotacoes.Any())
        {
            _logger.LogWarning("Nenhuma cotação encontrada no arquivo {Arquivo}.", caminhoArquivo);
            return;
        }

        var dataPregao = todasCotacoes.First().DataPregao;
        var tickersExistentes = await _cotacaoRepository.GetTickersParaDataAsync(dataPregao, cancellationToken);

        // Deduplica por ticker — prioriza BDI 02 (lote padrão) sobre 96 (fracionário)
        var novas = todasCotacoes
            .GroupBy(c => c.Ticker)
            .Select(g => g.First())
            .Where(c => !tickersExistentes.Contains(c.Ticker))
            .ToList();

        if (!novas.Any())
        {
            _logger.LogInformation("Todas as cotações do arquivo já estão importadas.");
            return;
        }

        await _cotacaoRepository.AddRangeAsync(novas, cancellationToken);
        await _cotacaoRepository.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("{Total} cotações importadas com sucesso.", novas.Count);
    }


    public async Task ImportarPastaCotacoesAsync(CancellationToken cancellationToken = default)
    {
        var pastaConfig = _configuration["Cotahist:Pasta"] ?? "cotacoes";

        var pasta = Path.GetFullPath(pastaConfig);

        if (!Directory.Exists(pasta))
        {
            _logger.LogWarning("Pasta de cotações não encontrada: {Pasta}", pasta);
            return;
        }

        var arquivos = Directory.GetFiles(pasta, "COTAHIST_D*.TXT").ToList();

        if (!arquivos.Any())
        {
            _logger.LogWarning("Nenhum arquivo COTAHIST encontrado em: {Pasta}", pasta);
            return;
        }

        foreach (var arquivo in arquivos)
            await ImportarCotahistAsync(arquivo, cancellationToken);
    }
}
