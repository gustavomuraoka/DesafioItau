"use client";

import { useEffect, useState } from "react";
import { clienteService } from "@/service/api";
import { CarteiraResponse } from "@/types";
import { StatCard } from "@/components/ui/StatCard";
import { Badge } from "@/components/ui/Badge";
import {
    TrendingUp,
    Wallet,
    BarChart3,
    DollarSign,
} from "lucide-react";

export function CarteiraResumo({ clienteId }: { clienteId: number }) {
    const [carteira, setCarteira] = useState<CarteiraResponse | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        clienteService
            .consultarCarteira(clienteId)
            .then(setCarteira)
            .catch((e) => setError(e?.erro ?? "Erro ao carregar carteira"));
    }, [clienteId]);

    if (error) return <p className="text-red-400">{error}</p>;

    if (!carteira)
        return (
            <div className="space-y-4 animate-pulse">
                <div className="h-8 w-64 bg-white/10 rounded-lg" />
                <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                    {Array(4).fill(null).map((_, i) => (
                        <div key={i} className="h-28 bg-white/5 rounded-2xl" />
                    ))}
                </div>
            </div>
        );

    const lucro = carteira.resumo.plTotal >= 0;
    const fmt = (v: number) =>
        `R$ ${v.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}`;

    return (
        <div className="space-y-4">
            <div className="flex items-center gap-3">
                <div>
                    <h2 className="text-xl font-bold text-white">{carteira.nome}</h2>
                    <p className="text-sm text-slate-400 mt-0.5">
                        {new Date(carteira.dataConsulta).toLocaleDateString("pt-BR", {
                            dateStyle: "long",
                        })}
                    </p>
                </div>
                <Badge variant="blue">{carteira.contaGrafica}</Badge>
            </div>

            <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
                <StatCard
                    label="Valor Investido"
                    value={fmt(carteira.resumo.valorTotalInvestido)}
                    icon={<DollarSign size={16} />}
                />
                <StatCard
                    label="Valor Atual"
                    value={fmt(carteira.resumo.valorAtualCarteira)}
                    icon={<Wallet size={16} />}
                />
                <StatCard
                    label="P&L Total"
                    value={fmt(carteira.resumo.plTotal)}
                    positive={lucro}
                    icon={<TrendingUp size={16} />}
                />
                <StatCard
                    label="Rentabilidade"
                    value={`${carteira.resumo.rentabilidadePercentual.toFixed(2)}%`}
                    positive={lucro}
                    icon={<BarChart3 size={16} />}
                />
            </div>
        </div>
    );
}
