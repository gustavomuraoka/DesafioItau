import { clsx } from "clsx";

export function Card({
    children,
    className,
    glow,
}: {
    children: React.ReactNode;
    className?: string;
    glow?: "blue" | "green" | "orange";
}) {
    const glowColors = {
        blue: "shadow-blue-500/20 shadow-xl",
        green: "shadow-green-500/20 shadow-xl",
        orange: "shadow-orange-500/20 shadow-xl",
    };

    return (
        <div
            className={clsx(
                "glass rounded-2xl p-6",
                glow && glowColors[glow],
                className
            )}
        >
            {children}
        </div>
    );
}
