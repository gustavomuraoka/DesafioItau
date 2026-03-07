"use client";

import { useEffect, useState } from "react";
import { adminService } from "@/service/api";
import { ContaMasterCustodiaResponse } from "@/types";
import { Card } from "@/components/ui/Card";
import { Badge } from "@/components/ui/Badge";

export function CustodiaCard() {
    const [data, setData] = useState<ContaMasterCustodiaResponse | null>(null);

    useEffect(() => {
        adminService.consultarCustodiaMaster().then(setData).catch(console.error);
    }, []);

    if (!data)
        return (
            <Card>
                <p className="text-gray-400 text-sm animate-pulse">Carregando custódia master...</p>
            </Card>
        );

    return (
        <Card>
            <div className="flex items-center justify-between mb-4">
                <h2 className="text-lg font-semibold text-gray-800">Custódia Conta Master</h2>
                <Badge variant="blue">{data.contaMaster.numeroConta}</Badge>
            </div>
            <table className="w-full text-sm">
                <thead>
                    <tr className="text-gray-400 text-xs border-b uppercase">
                        <th className="pb-2 text-left">Ticker</th>
                        <th className="pb-2 text-right">Qtd</th>
                        <th className="pb-2 text-right">Preço Médio</th>
                        <th className="pb-2 text-right">Valor Atual</th>
                    </tr>
                </thead>
                <tbody>
                    {data.custodia.map((item) => (
                        <tr key={item.ticker} className="border-b last:border-0">
                            <td className="py-2 font-medium text-[#003087]">{item.ticker}</td>
                            <td className="py-2 text-right">{item.quantidade}</td>
                            <td className="py-2 text-right">R$ {item.precoMedio.toFixed(2)}</td>
                            <td className="py-2 text-right">R$ {item.valorAtual.toFixed(2)}</td>
                        </tr>
                    ))}
                </tbody>
            </table>
            <div className="mt-4 pt-4 border-t flex justify-between text-sm">
                <span className="text-gray-500">Total Resíduo</span>
                <span className="font-bold text-gray-800">
                    R$ {data.valorTotalResiduo.toLocaleString("pt-BR", { minimumFractionDigits: 2 })}
                </span>
            </div>
        </Card>
    );
}
