"use client";

import { useEffect, useState } from "react";
import { clienteService } from "@/service/api";
import { RentabilidadeResponse } from "@/types";
import { Card } from "@/components/ui/Card";

export function RentabilidadeCard({ clienteId }: { clienteId: number }) {
    const [data, setData] = useState<RentabilidadeResponse | null>(null);

    useEffect(() => {
        clienteService
            .consultarRentabilidade(clienteId)
            .then(setData)
            .catch(console.error);
    }, [clienteId]);

    if (!data)
        return (
            <Card>
                <div className="h-32 animate-pulse bg-gray-50 rounded-xl" />
            </Card>
        );

    return (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
                <h2 className="text-lg font-semibold text-gray-800 mb-4">Histórico de Aportes</h2>
                <table className="w-full text-sm">
                    <thead>
                        <tr className="text-gray-400 text-xs uppercase border-b">
                            <th className="pb-2 text-left">Data</th>
                            <th className="pb-2 text-center">Parcela</th>
                            <th className="pb-2 text-right">Valor</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.historicoAportes.map((a, i) => (
                            <tr key={i} className="border-b last:border-0">
                                <td className="py-2">
                                    {new Date(a.data + "T00:00:00").toLocaleDateString("pt-BR")}
                                </td>
                                <td className="py-2 text-center text-gray-500">{a.parcela}</td>
                                <td className="py-2 text-right">
                                    R$ {a.valor.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </Card>

            <Card>
                <h2 className="text-lg font-semibold text-gray-800 mb-4">Evolução da Carteira</h2>
                <table className="w-full text-sm">
                    <thead>
                        <tr className="text-gray-400 text-xs uppercase border-b">
                            <th className="pb-2 text-left">Data</th>
                            <th className="pb-2 text-right">Investido</th>
                            <th className="pb-2 text-right">Carteira</th>
                            <th className="pb-2 text-right">Rent.</th>
                        </tr>
                    </thead>
                    <tbody>
                        {data.evolucaoCarteira.map((e, i) => (
                            <tr key={i} className="border-b last:border-0">
                                <td className="py-2">
                                    {new Date(e.data + "T00:00:00").toLocaleDateString("pt-BR")}
                                </td>
                                <td className="py-2 text-right">
                                    R$ {e.valorInvestido.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}
                                </td>
                                <td className="py-2 text-right">
                                    R$ {e.valorCarteira.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}
                                </td>
                                <td
                                    className={`py-2 text-right font-semibold ${e.rentabilidade >= 0 ? "text-green-600" : "text-red-500"
                                        }`}
                                >
                                    {e.rentabilidade >= 0 ? "+" : ""}
                                    {e.rentabilidade.toFixed(2)}%
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </Card>
        </div>
    );
}
