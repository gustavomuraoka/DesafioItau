import { CestaAtualCard } from "@/components/admin/CestaAtualCard";
import { NovaCestaFormWrapper } from "@/components/admin/NovaCestaFormWrapper";
import { ExecutarCompraForm } from "@/components/admin/ExecutarCompraForm";
import { CustodiaCard } from "@/components/admin/CustodiaCard";

export default function AdminPage() {
    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-gray-900">Painel Administrativo</h1>
                <p className="text-sm text-gray-500 mt-1">
                    Gestão da cesta Top Five e operações do motor de compra
                </p>
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <CestaAtualCard />
                <NovaCestaFormWrapper />
            </div>

            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <ExecutarCompraForm />
                <CustodiaCard />
            </div>
        </div>
    );
}
