import "./globals.css";
import type { Metadata, Viewport } from "next";
import { buildBrandCssVars } from "@/lib/brand";
import { AppProviders } from "@/components/providers/app-providers";
import { PwaRegister } from "@/components/pwa-register";
import { ficPortalTheme } from "@/types/portal-contracts";

export const metadata: Metadata = {
  title: "FIC | Merchant loyalty workspace",
  description: "Mobile-first loyalty setup and operations for merchants.",
  manifest: "/manifest.webmanifest",
  applicationName: "FIC",
  appleWebApp: {
    capable: true,
    statusBarStyle: "default",
    title: "FIC",
  },
};

export const viewport: Viewport = {
  themeColor: "#1f3731",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body style={buildBrandCssVars(ficPortalTheme)}>
        <AppProviders>
          <PwaRegister />
          {children}
        </AppProviders>
      </body>
    </html>
  );
}
