import type { MetadataRoute } from "next";

export default function manifest(): MetadataRoute.Manifest {
  return {
    name: "FIC Merchant Workspace",
    short_name: "FIC",
    description: "Premium merchant loyalty workspace powered by FIC.",
    start_url: "/",
    display: "standalone",
    background_color: "#f4f2ec",
    theme_color: "#1f3731",
    icons: [
      {
        src: "/icons/icon-192.svg",
        sizes: "192x192",
        type: "image/svg+xml",
      },
      {
        src: "/icons/icon-512.svg",
        sizes: "512x512",
        type: "image/svg+xml",
      },
    ],
  };
}
