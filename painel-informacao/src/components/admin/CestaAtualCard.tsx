"use client";

import { useEffect, useState } from "react";
import { adminService } from "@/service/api";
import { CestaResponse } from "@/types";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function CestaAtualCard() {
    const [cesta, setCesta] = useState<CestaResponse | null>(null);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        adminService.consultarCestaAtual().then(setCesta).catch((e) => setError(e?.erro ?? "Erro"));
    }, []);

    if (error)
        return (
            <Card>
                <p className="text-red-400 text-sm">{error}</p>
            </Card>
        );

    if (!cesta)
        return (
            <Card className="animate-pulse">
                <div className="h-6 w-48 bg-white/10 rounded-lg mb-4" />
                <div className="space-y-3">
                    {Array(5).fill(null).map((_, i) => (
                        <div key={i} className="h-8 bg-white/5 rounded-lg" />
                    ))}
                </div>
            </Card>
        );

    return (
        <Card glow="blue">
            <div className="flex items-start justify-between mb-6">
                <div>
                    <p className="text-xs text-slate-400 uppercase tracking-widest mb-1">Cesta Ativa</p>
                    <h2 className="text-lg font-bold text-white">{cesta.nome}</h2>
                    <p className="text-xs text-slate-500 mt-0.5">
                        Criada em {new Date(cesta.dataCriacao).toLocaleDateString("pt-BR")}
                    </p>
                </div>
                <Badge variant="green">● Ativa</Badge>
            </div>

            <div className="space-y-3">
                {cesta.itens.map((item) => (
                    <div key={item.ticker} className="flex items-center gap-3">
                        <span className="text-sm font-bold text-blue-400 w-14">{item.ticker}</span>
                        <div className="flex-1 h-2 bg-white/5 rounded-full overflow-hidden">
                            <div
                                className="h-full bg-gradient-to-r from-blue-500 to-blue-400 rounded-full transition-all duration-700"
                                style={{ width: `${item.percentual}%` }}
                            />
                        </div>
                        <span className="text-sm font-semibold text-white w-12 text-right">
                            {item.percentual}%
                        </span>
                        {item.cotacaoAtual != null && (
                            <span className="text-xs text-slate-400 w-20 text-right">
                                R$ {item.cotacaoAtual.toFixed(2)}
                            </span>
                        )}
                    </div>
                ))}
            </div>
        </Card>
    );
}
