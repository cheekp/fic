import "./globals.css";
import type { Metadata, Viewport } from "next";
import { AppProviders } from "@/components/providers/app-providers";
import { PwaRegister } from "@/components/pwa-register";

export const metadata: Metadata = {
  title: "FIC Merchant Workspace",
  description: "Premium mobile-first loyalty workspace for merchants.",
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
      <body>
        <AppProviders>
          <PwaRegister />
          {children}
        </AppProviders>
      </body>
    </html>
  );
}
