import {
    AdesaoRequest,
    AlterarValorRequest,
    AlterarValorResponse,
    CarteiraResponse,
    CestaRequest,
    CestaResponse,
    Cliente,
    ContaMasterCustodiaResponse,
    ExecutarCompraRequest,
    ExecutarCompraResponse,
    HistoricoCestasResponse,
    RentabilidadeResponse,
    SaidaResponse
} from "../types";

// Aponta para o dotnet run local em dev, e para o container em produção
const BASE_URL =
    process.env.NEXT_PUBLIC_API_URL ?? "http://localhost:5000";

async function request<T>(
    path: string,
    options?: RequestInit
): Promise<T> {
    const res = await fetch(`${BASE_URL}${path}`, {
        headers: {
            "Content-Type": "application/json",
            Accept: "application/json",
        },
        ...options,
    });

    if (!res.ok) {
        const error = await res.json().catch(() => ({ erro: "Erro desconhecido", codigo: "UNKNOWN" }));
        throw { status: res.status, ...error };
    }

    // 204 No Content
    if (res.status === 204) return undefined as T;

    return res.json() as Promise<T>;
}

// ─── Cliente ──────────────────────────────────────────────
export const clienteService = {
    aderir: (data: AdesaoRequest) =>
        request<Cliente>("/api/clientes/adesao", {
            method: "POST",
            body: JSON.stringify(data),
        }),

    sair: (clienteId: number) =>
        request<SaidaResponse>(`/api/clientes/${clienteId}/saida`, {
            method: "POST",
        }),

    alterarValorMensal: (clienteId: number, data: AlterarValorRequest) =>
        request<AlterarValorResponse>(
            `/api/clientes/${clienteId}/valor-mensal`,
            { method: "PUT", body: JSON.stringify(data) }
        ),

    consultarCarteira: (clienteId: number) =>
        request<CarteiraResponse>(`/api/clientes/${clienteId}/carteira`),

    consultarRentabilidade: (clienteId: number) =>
        request<RentabilidadeResponse>(
            `/api/clientes/${clienteId}/rentabilidade`
        ),
};

// ─── Admin ────────────────────────────────────────────────
export const adminService = {
    cadastrarCesta: (data: CestaRequest) =>
        request<CestaResponse>("/api/admin/cesta", {
            method: "POST",
            body: JSON.stringify(data),
        }),

    consultarCestaAtual: () =>
        request<CestaResponse>("/api/admin/cesta/atual"),

    historicoCestas: () =>
        request<HistoricoCestasResponse>("/api/admin/cesta/historico"),

    consultarCustodiaMaster: () =>
        request<ContaMasterCustodiaResponse>(
            "/api/admin/conta-master/custodia"
        ),
};

// ─── Motor de Compra ──────────────────────────────────────
export const motorService = {
    executarCompra: (data: ExecutarCompraRequest) =>
        request<ExecutarCompraResponse>("/api/motor/executar-compra", {
            method: "POST",
            body: JSON.stringify(data),
        }),
};
