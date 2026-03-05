using CompraAutomatizada.Domain.Aggregates.CotacaoAggregate;
using CompraAutomatizada.Domain.Common;
using System.Text;

namespace CompraAutomatizada.Infrastructure.Cotahist;

public static class CotahistParser
{
    private static readonly HashSet<string> BdiPermitidos = new() { "02", "96" };

    public static IEnumerable<Cotacao> Parse(string caminhoArquivo)
    {
        if (!File.Exists(caminhoArquivo))
            throw new DomainException($"Arquivo COTAHIST não encontrado: {caminhoArquivo}");

        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var encoding = Encoding.GetEncoding("ISO-8859-1");

        var cotacoes = new List<Cotacao>();

        foreach (var linha in File.ReadLines(caminhoArquivo, encoding))
        {
            if (linha.Length < 245 || linha[..2] != "01")
                continue;

            var codbdi = linha.Substring(10, 2);
            if (!BdiPermitidos.Contains(codbdi))
                continue;

            try
            {
                var ticker = linha.Substring(12, 12).Trim();
                var dataPregaoStr = linha.Substring(2, 8);
                var precoAbertura = ParsePreco(linha.Substring(56, 13));
                var precoMaximo = ParsePreco(linha.Substring(69, 13));
                var precoMinimo = ParsePreco(linha.Substring(82, 13));
                var precoFechamento = ParsePreco(linha.Substring(108, 13));
                var volume = ParseVolume(linha.Substring(170, 18));

                var dataPregao = DateOnly.ParseExact(dataPregaoStr, "yyyyMMdd");

                cotacoes.Add(Cotacao.Criar(
                    ticker,
                    dataPregao,
                    precoAbertura,
                    precoFechamento,
                    precoMaximo,
                    precoMinimo,
                    volume
                ));
            }
            catch
            {
                continue;
            }
        }

        return cotacoes;
    }

    public static IEnumerable<Cotacao> ParseFiltrandoTickers(string caminhoArquivo, IEnumerable<string> tickers)
    {
        var tickersUpper = tickers.Select(t => t.ToUpper()).ToHashSet();
        return Parse(caminhoArquivo).Where(c => tickersUpper.Contains(c.Ticker));
    }

    private static decimal ParsePreco(string valor)
    {
        var raw = long.Parse(valor.Trim());
        return raw / 100m;
    }

    private static long ParseVolume(string valor)
        => long.Parse(valor.Trim());
}
