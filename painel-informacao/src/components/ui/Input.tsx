export function Input({
    label,
    ...props
}: { label?: string } & React.InputHTMLAttributes<HTMLInputElement>) {
    return (
        <div className="space-y-1.5">
            {label && (
                <label className="text-xs text-slate-400 uppercase tracking-widest font-medium block">
                    {label}
                </label>
            )}
            <input
                {...props}
                className="w-full bg-white/5 border border-white/10 rounded-xl px-4 py-2.5 text-sm text-white placeholder-slate-500 focus:outline-none focus:ring-2 focus:ring-blue-500/50 focus:border-blue-500/50 transition-all"
            />
        </div>
    );
}
