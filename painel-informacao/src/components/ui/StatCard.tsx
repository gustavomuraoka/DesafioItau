import { clsx } from "clsx";

interface StatCardProps {
    label: string;
    value: string;
    sub?: string;
    positive?: boolean;
    icon?: React.ReactNode;
}

export function StatCard({ label, value, sub, positive, icon }: StatCardProps) {
    return (
        <div className="glass rounded-2xl p-5 flex flex-col gap-3">
            <div className="flex items-center justify-between">
                <span className="text-xs text-slate-400 uppercase tracking-widest font-medium">
                    {label}
                </span>
                {icon && (
                    <div className="w-8 h-8 rounded-lg bg-white/5 flex items-center justify-center text-slate-400">
                        {icon}
                    </div>
                )}
            </div>
            <div>
                <span
                    className={clsx(
                        "text-2xl font-bold",
                        positive === undefined && "text-white",
                        positive === true && "text-green-400",
                        positive === false && "text-red-400"
                    )}
                >
                    {value}
                </span>
                {sub && <p className="text-xs text-slate-500 mt-0.5">{sub}</p>}
            </div>
        </div>
    );
}
