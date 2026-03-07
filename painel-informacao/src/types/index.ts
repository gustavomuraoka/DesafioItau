// ─── Shared ───────────────────────────────────────────────
export interface ApiError {
    erro: string;
    codigo: string;
}

// ─── Cliente ──────────────────────────────────────────────
export interface ContaGrafica {
    id: number;
    numeroConta: string;
    tipo: "FILHOTE" | "MASTER";
    dataCriacao: string;
}

export interface Cliente {
    clienteId: number;
    nome: string;
    cpf: string;
    email: string;
    valorMensal: number;
    ativo: boolean;
    dataAdesao: string;
    contaGrafica: ContaGrafica;
}

export interface AdesaoRequest {
    nome: string;
    cpf: string;
    email: string;
    valorMensal: number;
}

export interface SaidaResponse {
    clienteId: number;
    nome: string;
    ativo: false;
    dataSaida: string;
    mensagem: string;
}

export interface AlterarValorRequest {
    novoValorMensal: number;
}

export interface AlterarValorResponse {
    clienteId: number;
    valorMensalAnterior: number;
    valorMensalNovo: number;
    dataAlteracao: string;
    mensagem: string;
}

// ─── Carteira ─────────────────────────────────────────────
export interface Ativo {
    ticker: string;
    quantidade: number;
    precoMedio: number;
    cotacaoAtual: number;
    valorAtual: number;
    pl: number;
    plPercentual: number;
    composicaoCarteira: number;
}

export interface ResumoCarteira {
    valorTotalInvestido: number;
    valorAtualCarteira: number;
    plTotal: number;
    rentabilidadePercentual: number;
}

export interface CarteiraResponse {
    clienteId: number;
    nome: string;
    contaGrafica: string;
    dataConsulta: string;
    resumo: ResumoCarteira;
    ativos: Ativo[];
}

export interface HistoricoAporte {
    data: string;
    valor: number;
    parcela: string;
}

export interface EvolucaoCarteira {
    data: string;
    valorCarteira: number;
    valorInvestido: number;
    rentabilidade: number;
}

export interface RentabilidadeResponse {
    clienteId: number;
    nome: string;
    dataConsulta: string;
    rentabilidade: ResumoCarteira;
    historicoAportes: HistoricoAporte[];
    evolucaoCarteira: EvolucaoCarteira[];
}

// ─── Admin - Cesta ────────────────────────────────────────
export interface ItemCesta {
    ticker: string;
    percentual: number;
    cotacaoAtual?: number;
}

export interface CestaRequest {
    nome: string;
    itens: ItemCesta[];
}

export interface CestaDesativada {
    cestaId: number;
    nome: string;
    dataDesativacao: string;
}

export interface CestaResponse {
    cestaId: number;
    nome: string;
    ativa: boolean;
    dataCriacao: string;
    dataDesativacao?: string | null;
    itens: ItemCesta[];
    rebalanceamentoDisparado?: boolean;
    cestaAnteriorDesativada?: CestaDesativada;
    ativosRemovidos?: string[];
    ativosAdicionados?: string[];
    mensagem?: string;
}

export interface HistoricoCestasResponse {
    cestas: CestaResponse[];
}

// ─── Admin - Conta Master ─────────────────────────────────
export interface CustodiaItem {
    ticker: string;
    quantidade: number;
    precoMedio: number;
    valorAtual: number;
    origem: string;
}

export interface ContaMasterCustodiaResponse {
    contaMaster: {
        id: number;
        numeroConta: string;
        tipo: string;
    };
    custodia: CustodiaItem[];
    valorTotalResiduo: number;
}

// ─── Motor de Compra ──────────────────────────────────────
export interface ExecutarCompraRequest {
    dataReferencia: string;
}

export interface OrdemCompra {
    ticker: string;
    quantidadeTotal: number;
    detalhes: { tipo: string; ticker: string; quantidade: number }[];
    precoUnitario: number;
    valorTotal: number;
}

export interface DistribuicaoCliente {
    clienteId: number;
    nome: string;
    valorAporte: number;
    ativos: { ticker: string; quantidade: number }[];
}

export interface ExecutarCompraResponse {
    dataExecucao: string;
    totalClientes: number;
    totalConsolidado: number;
    ordensCompra: OrdemCompra[];
    distribuicoes: DistribuicaoCliente[];
    residuosCustMaster: { ticker: string; quantidade: number }[];
    eventosIRPublicados: number;
    mensagem: string;
}
