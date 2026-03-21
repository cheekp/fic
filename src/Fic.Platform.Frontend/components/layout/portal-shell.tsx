"use client";

import Link from "next/link";
import { Menu } from "lucide-react";
import { ReactNode, useMemo } from "react";
import type { PortalNavItemContract, PortalNavKey, PortalThemeContract } from "@/types/portal-contracts";
import { ficPortalTheme } from "@/types/portal-contracts";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "@/components/ui/dialog";
import { cn } from "@/lib/utils";

type PortalShellProps = {
  title: string;
  activeKey: PortalNavKey;
  railItems: PortalNavItemContract[];
  children: ReactNode;
  secondary?: ReactNode;
  theme?: PortalThemeContract;
  showRail?: boolean;
  showActiveBadge?: boolean;
};

const apiBaseUrl = process.env.NEXT_PUBLIC_FIC_API_BASE_URL ?? "http://localhost:5276";
const utilityLinks: Array<{ href: string; label: string; external?: boolean }> = [
  { href: "/blogs", label: "Blogs" },
  { href: "/training", label: "Training" },
  { href: "/consultancy", label: "Consultancy" },
  { href: "/account", label: "Account" },
  { href: "/billing", label: "Billing" },
  { href: `${apiBaseUrl}/account/logout`, label: "Log out", external: true },
] as const;
const utilityPrimaryLinks = utilityLinks.slice(0, 2);
const utilityMoreLinks = utilityLinks.slice(2);

function PortalRail({
  activeKey,
  items,
  onNavigate,
}: {
  activeKey: PortalNavKey;
  items: PortalNavItemContract[];
  onNavigate?: () => void;
}) {
  return (
    <nav className="space-y-0.5" aria-label="Portal navigation">
      {items.map((item) => {
        const isActive = item.key === activeKey;
        const className = cn(
          "flex items-center justify-between rounded-xl border px-2.5 py-1.5 text-[13px] transition",
          isActive
            ? "border-[var(--portal-primary)] bg-[color-mix(in_srgb,var(--portal-primary)_10%,white_90%)] text-foreground"
            : item.isComplete
              ? "border-border/70 bg-background/80 text-foreground/84 hover:bg-[color-mix(in_srgb,var(--portal-primary)_8%,white_92%)]"
              : "border-border/70 bg-background/75 text-foreground/72 hover:bg-[color-mix(in_srgb,var(--portal-primary)_8%,white_92%)]",
          item.isDisabled ? "pointer-events-none opacity-45" : "",
        );

        const body = (
          <>
            <span>{item.label}</span>
            {item.badge ? <Badge className="h-4 px-1.5 text-[10px]">{item.badge}</Badge> : null}
          </>
        );

        if (item.isDisabled) {
          return (
            <div key={item.key} className={className} aria-disabled>
              {body}
            </div>
          );
        }

        return (
          <Link key={item.key} href={item.href} className={className} onClick={onNavigate}>
            {body}
          </Link>
        );
      })}
    </nav>
  );
}

export function PortalShell({
  title,
  activeKey,
  railItems,
  children,
  secondary,
  theme = ficPortalTheme,
  showRail = false,
  showActiveBadge = true,
}: PortalShellProps) {
  const activeItem = useMemo(
    () => railItems.find((item) => item.key === activeKey),
    [activeKey, railItems],
  );

  return (
    <section
      className="mx-auto w-full max-w-7xl px-4 pb-12 pt-4 sm:px-6 sm:pt-6"
      style={{
        color: theme.ink,
        ["--portal-primary" as string]: theme.primary,
        ["--portal-accent" as string]: theme.accent,
        ["--portal-surface" as string]: theme.surface,
      }}
    >
      <header className="sticky top-0 z-30 mb-4 flex items-center justify-between rounded-2xl border border-border/70 bg-[var(--portal-surface)] px-3 py-2 backdrop-blur sm:px-4">
        <div className="flex items-center gap-2">
          {showRail ? (
            <Dialog>
              <DialogTrigger asChild>
                <Button size="icon" variant="outline" className="h-9 w-9 md:hidden">
                  <Menu className="h-4 w-4" />
                  <span className="sr-only">Open navigation</span>
                </Button>
              </DialogTrigger>
              <DialogContent className="left-0 top-0 h-full w-[86%] max-w-xs translate-x-0 translate-y-0 rounded-none border-r p-4">
                <DialogHeader>
                  <DialogTitle>{title}</DialogTitle>
                </DialogHeader>
                <PortalRail activeKey={activeKey} items={railItems} />
              </DialogContent>
            </Dialog>
          ) : null}
          <p className="text-sm font-semibold sm:text-base">{title}</p>
          {showActiveBadge && activeItem ? <Badge variant="outline" className="hidden sm:inline-flex">{activeItem.label}</Badge> : null}
        </div>

        <div className="flex items-center gap-2">
          <nav className="hidden items-center gap-2.5 lg:flex" aria-label="Portal utilities">
            {utilityPrimaryLinks.map((item) => (
              item.external ? (
                <a key={item.href} href={item.href} className="text-xs text-foreground/65 transition hover:text-foreground">
                  {item.label}
                </a>
              ) : (
                <Link key={item.href} href={item.href} className="text-xs text-foreground/65 transition hover:text-foreground">
                  {item.label}
                </Link>
              )
            ))}
            <Dialog>
              <DialogTrigger asChild>
                <Button variant="ghost" size="sm" className="h-7 px-2 text-xs text-foreground/70">
                  More
                </Button>
              </DialogTrigger>
              <DialogContent className="sm:max-w-xs">
                <DialogHeader>
                  <DialogTitle>Portal links</DialogTitle>
                </DialogHeader>
                <div className="grid gap-2">
                  {utilityMoreLinks.map((item) => (
                    item.external ? (
                      <a key={item.href} href={item.href} className="rounded-lg border px-3 py-2 text-sm text-foreground/80 transition hover:bg-muted/50">
                        {item.label}
                      </a>
                    ) : (
                      <Link key={item.href} href={item.href} className="rounded-lg border px-3 py-2 text-sm text-foreground/80 transition hover:bg-muted/50">
                        {item.label}
                      </Link>
                    )
                  ))}
                </div>
              </DialogContent>
            </Dialog>
          </nav>

        </div>
      </header>

      <div className={cn(
        "grid grid-cols-1 gap-4",
        showRail
          ? secondary
            ? "md:grid-cols-[minmax(13rem,17rem)_minmax(0,1fr)] xl:grid-cols-[minmax(14rem,18rem)_minmax(0,1fr)_minmax(16rem,20rem)]"
            : "md:grid-cols-[minmax(13rem,17rem)_minmax(0,1fr)]"
          : secondary
            ? "xl:grid-cols-[minmax(0,1fr)_minmax(16rem,20rem)]"
            : "",
      )}>
        {showRail ? (
          <aside className="hidden md:block">
            <div className="glass-panel p-3">
              <PortalRail activeKey={activeKey} items={railItems} />
            </div>
          </aside>
        ) : null}

        <div className="min-w-0">{children}</div>

        {secondary ? <aside className="hidden xl:block">{secondary}</aside> : null}
      </div>
    </section>
  );
}
