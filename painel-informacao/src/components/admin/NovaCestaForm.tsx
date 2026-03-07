"use client";

import { useState } from "react";
import { adminService } from "@/service/api";
import { CestaResponse, ItemCesta } from "@/types";
import { Card } from "@/components/ui/Card";
import { Button } from "@/components/ui/Button";
import { Input } from "@/components/ui/Input";

const TICKERS = ["PETR4", "VALE3", "ITUB4", "DEXP4", "WEGE3", "ABEV3", "RENT3", "MGLU3", "ALOS3", "TTEN3"];

interface Props {
    onSuccess: (cesta: CestaResponse) => void;
}

export function NovaCestaForm({ onSuccess }: Props) {
    const [nome, setNome] = useState("");
    const [itens, setItens] = useState<ItemCesta[]>(
        Array(5).fill(null).map(() => ({ ticker: "", percentual: 0 }))
    );
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const total = itens.reduce((acc, i) => acc + Number(i.percentual), 0);
    const totalOk = Math.abs(total - 100) < 0.01;

    function updateItem(index: number, field: keyof ItemCesta, value: string | number) {
        setItens((prev) => prev.map((item, i) => (i === index ? { ...item, [field]: value } : item)));
    }

    async function handleSubmit(e: React.FormEvent) {
        e.preventDefault();
        setError(null);
        setLoading(true);
        try {
            const result = await adminService.cadastrarCesta({ nome, itens });
            onSuccess(result);
            setNome("");
            setItens(Array(5).fill(null).map(() => ({ ticker: "", percentual: 0 })));
        } catch (err: unknown) {
            const apiErr = err as { erro?: string };
            setError(apiErr?.erro ?? "Erro ao cadastrar cesta");
        } finally {
            setLoading(false);
        }
    }

    return (
        <Card>
            <p className="text-xs text-slate-400 uppercase tracking-widest mb-1">Nova Cesta</p>
            <h2 className="text-lg font-bold text-white mb-6">Cadastrar Top Five</h2>

            <form onSubmit={handleSubmit} className="space-y-5">
                <Input
                    label="Nome da Cesta"
                    placeholder="Top Five — Abril 2026"
                    value={nome}
                    onChange={(e) => setNome(e.target.value)}
                    required
                />

                <div className="space-y-2">
                    <div className="grid grid-cols-2 gap-3 text-xs text-slate-500 uppercase tracking-widest px-1">
                        <span>Ticker</span>
                        <span>Percentual</span>
                    </div>
                    {itens.map((item, i) => (
                        <div key={i} className="grid grid-cols-2 gap-3">
                            <select
                                value={item.ticker}
                                onChange={(e) => updateItem(i, "ticker", e.target.value)}
                                required
                                className="bg-white/5 border border-white/10 rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50 transition-all"
                            >
                                <option value="" className="bg-slate-900">Selecione...</option>
                                {TICKERS.map((t) => (
                                    <option key={t} value={t} className="bg-slate-900">{t}</option>
                                ))}
                            </select>
                            <input
                                type="number"
                                min={0}
                                max={100}
                                step={0.01}
                                value={item.percentual}
                                onChange={(e) => updateItem(i, "percentual", parseFloat(e.target.value) || 0)}
                                required
                                className="bg-white/5 border border-white/10 rounded-xl px-3 py-2.5 text-sm text-white focus:outline-none focus:ring-2 focus:ring-blue-500/50 transition-all"
                            />
                        </div>
                    ))}
                </div>

                {/* Total indicator */}
                <div className="glass rounded-xl p-3 flex items-center justify-between">
                    <span className="text-sm text-slate-400">Total alocado</span>
                    <div className="flex items-center gap-2">
                        <div className="w-24 h-1.5 bg-white/10 rounded-full overflow-hidden">
                            <div
                                className={`h-full rounded-full transition-all duration-300 ${totalOk ? "bg-green-400" : total > 100 ? "bg-red-400" : "bg-orange-400"
                                    }`}
                                style={{ width: `${Math.min(total, 100)}%` }}
                            />
                        </div>
                        <span className={`text-sm font-bold ${totalOk ? "text-green-400" : "text-orange-400"}`}>
                            {total.toFixed(0)}%
                        </span>
                    </div>
                </div>

                {error && (
                    <div className="bg-red-500/10 border border-red-500/20 rounded-xl p-3">
                        <p className="text-red-400 text-sm">{error}</p>
                    </div>
                )}

                <Button type="submit" loading={loading} disabled={!totalOk} className="w-full justify-center">
                    Cadastrar Cesta
                </Button>
            </form>
        </Card>
    );
}
