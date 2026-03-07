"use client";

import { useState } from "react";
import { motorService } from "@/service/api";
import { ExecutarCompraResponse } from "@/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";
import { Badge } from "@/components/ui/Badge";

export function ExecutarCompraForm() {
    const [data, setData] = useState(new Date().toISOString().split("T")[0]);
    const [result, setResult] = useState<ExecutarCompraResponse | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setError(null);
        setResult(null);
        setLoading(true);
        try {
            setResult(await motorService.executarCompra({ dataReferencia: data }));
        } catch (err: unknown) {
            const apiErr = err as { erro?: string };
            setError(apiErr?.erro ?? "Erro ao executar compra");
        } finally {
            setLoading(false);
        }
    }

    return (
        <Card>
            <p className="text-xs text-slate-400 uppercase tracking-widest mb-1">Motor de Compra</p>
            <h2 className="text-lg font-bold text-white mb-6">Execução Manual</h2>

            <form onSubmit={handleSubmit} className="flex gap-3 items-end mb-5">
                <div className="flex-1">
                    <Input
                        label="Data de Referência"
                        type="date"
                        value={data}
                        onChange={(e) => setData(e.target.value)}
                        required
                    />
                </div>
                <Button type="submit" loading={loading}>
                    Executar
                </Button>
            </form>

            {error && (
                <div className="bg-red-500/10 border border-red-500/20 rounded-xl p-3 mb-4">
                    <p className="text-red-400 text-sm">{error}</p>
                </div>
            )}

            {result && (
                <div className="space-y-4 animate-in fade-in slide-in-from-bottom-2 duration-300">
                    <div className="bg-green-500/10 border border-green-500/20 rounded-xl p-4">
                        <p className="text-green-400 text-sm font-medium">✓ {result.mensagem}</p>
                    </div>

                    <div className="grid grid-cols-3 gap-3">
                        {[
                            { label: "Clientes", value: result.totalClientes },
                            {
                                label: "Total",
                                value: `R$ ${result.totalConsolidado.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}`,
                            },
                            { label: "Eventos IR", value: result.eventosIRPublicados },
                        ].map((s) => (
                            <div key={s.label} className="glass rounded-xl p-3">
                                <p className="text-xs text-slate-500">{s.label}</p>
                                <p className="text-base font-bold text-white mt-1">{s.value}</p>
                            </div>
                        ))}
                    </div>

                    <div className="glass rounded-xl overflow-hidden">
                        <div className="px-4 py-2 border-b border-white/10">
                            <p className="text-xs text-slate-400 uppercase tracking-widest">Ordens de Compra</p>
                        </div>
                        <table className="w-full text-sm">
                            <thead>
                                <tr className="text-slate-500 text-xs border-b border-white/5">
                                    <th className="px-4 py-2.5 text-left">Ticker</th>
                                    <th className="px-4 py-2.5 text-right">Qtd</th>
                                    <th className="px-4 py-2.5 text-right">Preço</th>
                                    <th className="px-4 py-2.5 text-right">Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {result.ordensCompra.map((o) => (
                                    <tr key={o.ticker} className="border-b border-white/5 last:border-0 hover:bg-white/5 transition-colors">
                                        <td className="px-4 py-3 font-bold text-blue-400">{o.ticker}</td>
                                        <td className="px-4 py-3 text-right text-slate-300">{o.quantidadeTotal}</td>
                                        <td className="px-4 py-3 text-right text-slate-300">R$ {o.precoUnitario.toFixed(2)}</td>
                                        <td className="px-4 py-3 text-right font-semibold text-white">
                                            R$ {o.valorTotal.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    <div className="flex items-center gap-2 flex-wrap">
                        <span className="text-xs text-slate-500">Resíduos Master:</span>
                        {result.residuosCustMaster.map((r) => (
                            <Badge key={r.ticker} variant="orange">
                                {r.ticker} +{r.quantidade}
                            </Badge>
                        ))}
                    </div>
                </div>
            )}
        </Card>
    );
}
