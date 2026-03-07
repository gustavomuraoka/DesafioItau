"use client";

import { useState } from "react";
import { NovaCestaForm } from "./NovaCestaForm";
import { CestaResponse } from "@/types";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function NovaCestaFormWrapper() {
    const [ultimaResposta, setUltimaResposta] = useState<CestaResponse | null>(null);

    return (
        <div className="space-y-4">
            <NovaCestaForm onSuccess={setUltimaResposta} />
            {ultimaResposta && (
                <Card className="border-green-200 bg-green-50">
                    <div className="flex items-center justify-between mb-2">
                        <p className="text-sm font-semibold text-green-800">
                            Cesta #{ultimaResposta.cestaId} cadastrada com sucesso
                        </p>
                        <Badge variant="green">{ultimaResposta.ativa ? "Ativa" : "Inativa"}</Badge>
                    </div>
                    {ultimaResposta.rebalanceamentoDisparado && (
                        <p className="text-xs text-green-700">
                            ♻️ Rebalanceamento disparado — {ultimaResposta.mensagem}
                        </p>
                    )}
                    {ultimaResposta.ativosAdicionados?.length ? (
                        <p className="text-xs text-green-700">
                            Adicionados: {ultimaResposta.ativosAdicionados.join(", ")}
                        </p>
                    ) : null}
                    {ultimaResposta.ativosRemovidos?.length ? (
                        <p className="text-xs text-orange-600">
                            Removidos: {ultimaResposta.ativosRemovidos.join(", ")}
                        </p>
                    ) : null}
                </Card>
            )}
        </div>
    );
}
