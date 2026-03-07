import { clsx } from "clsx";

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: "primary" | "ghost" | "danger";
    loading?: boolean;
}

export function Button({
    variant = "primary",
    loading,
    children,
    className,
    ...props
}: ButtonProps) {
    const styles = {
        primary:
            "bg-gradient-to-r from-blue-600 to-blue-500 hover:from-blue-500 hover:to-blue-400 text-white shadow-lg shadow-blue-500/25",
        ghost:
            "bg-white/5 hover:bg-white/10 text-slate-300 hover:text-white border border-white/10",
        danger:
            "bg-gradient-to-r from-red-600 to-red-500 hover:from-red-500 hover:to-red-400 text-white shadow-lg shadow-red-500/25",
    };

    return (
        <button
            {...props}
            disabled={loading || props.disabled}
            className={clsx(
                "px-5 py-2.5 rounded-xl text-sm font-semibold transition-all duration-200 disabled:opacity-40 disabled:cursor-not-allowed flex items-center gap-2",
                styles[variant],
                className
            )}
        >
            {loading && (
                <span className="w-4 h-4 border-2 border-current border-t-transparent rounded-full animate-spin" />
            )}
            {children}
        </button>
    );
}
