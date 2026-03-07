import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Permite chamadas server-side para a API C# sem restrição de domínio
  async rewrites() {
    return [
      {
        source: "/api/backend/:path*",
        destination: `${process.env.NEXT_PUBLIC_API_URL}/api/:path*`,
      },
    ];
  },
};

export default nextConfig;
