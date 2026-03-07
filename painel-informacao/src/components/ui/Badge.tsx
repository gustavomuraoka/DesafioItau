const variants = {
    green: "bg-green-500/15 text-green-400 border border-green-500/30",
    red: "bg-red-500/15 text-red-400 border border-red-500/30",
    blue: "bg-blue-500/15 text-blue-400 border border-blue-500/30",
    gray: "bg-slate-500/15 text-slate-400 border border-slate-500/30",
    orange: "bg-orange-500/15 text-orange-400 border border-orange-500/30",
};

export function Badge({
    children,
    variant = "gray",
}: {
    children: React.ReactNode;
    variant?: keyof typeof variants;
}) {
    return (
        <span className={`inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${variants[variant]}`}>
            {children}
        </span>
    );
}
