import { CarteiraResumo } from "@/components/carteira/CarteiraResumo";
import { TabelaAtivos } from "@/components/carteira/TabelaAtivos";
import { RentabilidadeCard } from "@/components/carteira/RentabilidadeCard";

interface Props {
    params: Promise<{ clienteId: string }>;
}

export default async function CarteiraPage({ params }: Props) {
    const { clienteId } = await params;
    const id = Number(clienteId);

    return (
        <div className="space-y-6">
            <div>
                <h1 className="text-2xl font-bold text-white">Carteira do Cliente</h1>
                <p className="text-sm text-slate-400 mt-1">ID: #{id}</p>
            </div>
            <CarteiraResumo clienteId={id} />
            <TabelaAtivos clienteId={id} />
            <RentabilidadeCard clienteId={id} />
        </div>
    );
}
