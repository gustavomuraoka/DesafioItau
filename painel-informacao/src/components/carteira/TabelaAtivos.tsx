"use client";

import { useEffect, useState } from "react";
import { clienteService } from "@/service/api";
import { Ativo } from "@/types";
import { Card } from "@/components/ui/Card";

export function TabelaAtivos({ clienteId }: { clienteId: number }) {
    const [ativos, setAtivos] = useState<Ativo[]>([]);

    useEffect(() => {
        clienteService
            .consultarCarteira(clienteId)
            .then((c) => setAtivos(c.ativos))
            .catch(console.error);
    }, [clienteId]);

    if (!ativos.length)
        return (
            <Card>
                <div className="space-y-3 animate-pulse">
                    {Array(5).fill(null).map((_, i) => (
                        <div key={i} className="h-10 bg-white/5 rounded-lg" />
                    ))}
                </div>
            </Card>
        );

    return (
        <Card>
            <p className="text-xs text-slate-400 uppercase tracking-widest mb-1">Posição</p>
            <h2 className="text-lg font-bold text-white mb-5">Custódia de Ativos</h2>

            <div className="overflow-x-auto -mx-6">
                <table className="w-full text-sm">
                    <thead>
                        <tr className="text-slate-500 text-xs uppercase border-b border-white/10">
                            {["Ticker", "Qtd", "Preço Médio", "Cotação Atual", "Valor Atual", "P&L", "% Carteira"].map(
                                (h) => (
                                    <th
                                        key={h}
                                        className={`px-6 py-3 font-medium tracking-wider ${h === "Ticker" ? "text-left" : "text-right"
                                            }`}
                                    >
                                        {h}
                                    </th>
                                )
                            )}
                        </tr>
                    </thead>
                    <tbody>
                        {ativos.map((a) => (
                            <tr
                                key={a.ticker}
                                className="border-b border-white/5 last:border-0 hover:bg-white/5 transition-colors group"
                            >
                                <td className="px-6 py-4">
                                    <span className="font-black text-blue-400 group-hover:text-blue-300 transition-colors">
                                        {a.ticker}
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-right text-slate-300">{a.quantidade}</td>
                                <td className="px-6 py-4 text-right text-slate-300">
                                    R$ {a.precoMedio.toFixed(2)}
                                </td>
                                <td className="px-6 py-4 text-right text-white font-medium">
                                    R$ {a.cotacaoAtual.toFixed(2)}
                                </td>
                                <td className="px-6 py-4 text-right text-white font-semibold">
                                    R$ {a.valorAtual.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}
                                </td>
                                <td className="px-6 py-4 text-right">
                                    <span
                                        className={`font-bold ${a.pl >= 0 ? "text-green-400" : "text-red-400"}`}
                                    >
                                        {a.pl >= 0 ? "▲" : "▼"} {Math.abs(a.plPercentual).toFixed(2)}%
                                    </span>
                                </td>
                                <td className="px-6 py-4 text-right">
                                    <div className="flex items-center justify-end gap-2">
                                        <span className="text-slate-300 text-xs">
                                            {a.composicaoCarteira.toFixed(1)}%
                                        </span>
                                        <div className="w-20 h-1.5 bg-white/10 rounded-full overflow-hidden">
                                            <div
                                                className="h-full bg-gradient-to-r from-blue-500 to-blue-400 rounded-full"
                                                style={{ width: `${a.composicaoCarteira}%` }}
                                            />
                                        </div>
                                    </div>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>
        </Card>
    );
}
