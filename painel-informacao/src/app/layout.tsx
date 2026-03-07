import type { Metadata } from "next";
import { Geist } from "next/font/google";
import Link from "next/link";
import "./globals.css";

const geist = Geist({ subsets: ["latin"] });

export const metadata: Metadata = {
    title: "Itaú Corretora — Compra Programada",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
    return (
        <html lang="pt-BR">
            <body className={geist.className}>
                {/* Background */}
                <div className="fixed inset-0 -z-10 bg-slate-950">
                    <div className="absolute top-0 left-1/4 w-96 h-96 bg-blue-600/20 rounded-full blur-3xl" />
                    <div className="absolute bottom-0 right-1/4 w-96 h-96 bg-orange-500/10 rounded-full blur-3xl" />
                </div>

                {/* Navbar */}
                <nav className="sticky top-0 z-50 glass border-b border-white/10 px-6 py-4">
                    <div className="max-w-7xl mx-auto flex items-center justify-between">
                        <div className="flex items-center gap-3">
                            <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-blue-500 to-blue-700 flex items-center justify-center text-white font-black text-sm shadow-lg shadow-blue-500/30">
                                i
                            </div>
                            <div>
                                <span className="font-bold text-white text-sm">Itaú Corretora</span>
                                <span className="text-slate-400 text-xs block leading-none">
                                    Compra Programada de Ações
                                </span>
                            </div>
                        </div>
                        <div className="flex items-center gap-1">
                            <Link
                                href="/admin"
                                className="px-4 py-2 rounded-lg text-sm text-slate-300 hover:text-white hover:bg-white/10 transition-all"
                            >
                                Admin
                            </Link>
                            <Link
                                href="/carteira/1"
                                className="px-4 py-2 rounded-lg text-sm text-slate-300 hover:text-white hover:bg-white/10 transition-all"
                            >
                                Carteira
                            </Link>
                        </div>
                    </div>
                </nav>

                <main className="max-w-7xl mx-auto px-4 py-8">{children}</main>
            </body>
        </html>
    );
}
