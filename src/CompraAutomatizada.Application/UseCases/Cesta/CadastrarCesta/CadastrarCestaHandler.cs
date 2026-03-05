using CompraAutomatizada.Application.Services;
using CompraAutomatizada.Domain.Aggregates.CestaTopFiveAggregate;
using CompraAutomatizada.Domain.Interfaces.Repositories;
using MediatR;

namespace CompraAutomatizada.Application.UseCases.Cesta.CadastrarCesta;

public class CadastrarCestaHandler : IRequestHandler<CadastrarCestaCommand, CadastrarCestaResponse>
{
    private readonly ICestaTopFiveRepository _cestaRepository;
    private readonly IClienteRepository _clienteRepository;
    private readonly IRebalanceamentoService _rebalanceamento;

    public CadastrarCestaHandler(
        ICestaTopFiveRepository cestaRepository,
        IClienteRepository clienteRepository,
        IRebalanceamentoService rebalanceamento)
    {
        _cestaRepository = cestaRepository;
        _clienteRepository = clienteRepository;
        _rebalanceamento = rebalanceamento;
    }

    public async Task<CadastrarCestaResponse> Handle(CadastrarCestaCommand request, CancellationToken cancellationToken)
    {
        var cestaAtiva = await _cestaRepository.GetAtivaAsync(cancellationToken);

        List<string>? ativosRemovidos = null;
        List<string>? ativosAdicionados = null;
        CestaAnteriorInfo? cestaAnteriorInfo = null;
        bool rebalanceamentoDisparado = false;

        if (cestaAtiva is not null)
        {
            var tickersAntigos = cestaAtiva.Itens.Select(i => i.Ticker).ToHashSet();
            var tickersNovos = request.Itens.Select(i => i.Ticker.ToUpper()).ToHashSet();

            ativosRemovidos = tickersAntigos.Except(tickersNovos).ToList();
            ativosAdicionados = tickersNovos.Except(tickersAntigos).ToList();

            cestaAtiva.Desativar();
            await _cestaRepository.UpdateAsync(cestaAtiva, cancellationToken);

            cestaAnteriorInfo = new CestaAnteriorInfo(
                cestaAtiva.Id,
                cestaAtiva.Nome,
                cestaAtiva.DataDesativacao!.Value
            );
        }

        var itens = request.Itens.Select(i => (i.Ticker, i.Percentual));
        var novaCesta = CestaTopFive.Criar(request.Nome, itens);
        await _cestaRepository.AddAsync(novaCesta, cancellationToken);
        await _cestaRepository.SaveChangesAsync(cancellationToken);

        string mensagem;

        if (cestaAtiva is not null)
        {
            var totalClientes = await _clienteRepository.CountAtivosAsync(cancellationToken);
            await _rebalanceamento.RebalancearPorMudancaDeCestaAsync(cestaAtiva, novaCesta, cancellationToken);
            rebalanceamentoDisparado = true;
            mensagem = $"Cesta atualizada. Rebalanceamento disparado para {totalClientes} clientes ativos.";
        }
        else
        {
            mensagem = "Primeira cesta cadastrada com sucesso.";
        }

        return new CadastrarCestaResponse(
            novaCesta.Id,
            novaCesta.Nome,
            novaCesta.Ativa,
            novaCesta.DataCriacao,
            request.Itens,
            rebalanceamentoDisparado,
            cestaAnteriorInfo,
            ativosRemovidos,
            ativosAdicionados,
            mensagem
        );
    }
}
