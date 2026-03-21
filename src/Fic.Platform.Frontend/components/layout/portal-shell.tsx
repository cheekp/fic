"use client";

import Link from "next/link";
import { motion, useReducedMotion } from "framer-motion";
import { Menu } from "lucide-react";
import { ReactNode, useMemo } from "react";
import { Drawer } from "vaul";
import type {
  PortalNavItemContract,
  PortalNavKey,
  PortalThemeContract,
  PortalUtilityLinkContract,
} from "@/types/portal-contracts";
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
  utilityLinks?: PortalUtilityLinkContract[];
  showRail?: boolean;
  showActiveBadge?: boolean;
  headerMode?: "workspace" | "onboarding";
};

const apiBaseUrl = process.env.NEXT_PUBLIC_FIC_API_BASE_URL ?? "http://localhost:5276";
const fallbackUtilityLinks: PortalUtilityLinkContract[] = [
  { key: "blogs", href: "/blogs", label: "Blogs", isExternal: false },
  { key: "training", href: "/training", label: "Training", isExternal: false },
  { key: "consultancy", href: "/consultancy", label: "Consultancy", isExternal: false },
  { key: "account", href: "/account", label: "Account", isExternal: false },
  { key: "billing", href: "/billing", label: "Billing", isExternal: false },
  { key: "logout", href: "/account/logout", label: "Log out", isExternal: true },
] as const;

function resolveUtilityHref(link: PortalUtilityLinkContract) {
  if (!link.isExternal) {
    return link.href;
  }

  if (link.href.startsWith("http://") || link.href.startsWith("https://")) {
    return link.href;
  }

  return `${apiBaseUrl}${link.href}`;
}

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
  utilityLinks,
  showRail = false,
  showActiveBadge = true,
  headerMode = "workspace",
}: PortalShellProps) {
  const prefersReducedMotion = useReducedMotion();
  const activeItem = useMemo(
    () => railItems.find((item) => item.key === activeKey),
    [activeKey, railItems],
  );
  const resolvedUtilityLinks = useMemo(() => {
    const source = utilityLinks && utilityLinks.length > 0 ? utilityLinks : fallbackUtilityLinks;
    return source.map((link) => ({
      ...link,
      href: resolveUtilityHref(link),
    }));
  }, [utilityLinks]);
  const isOnboardingHeader = headerMode === "onboarding";

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
          {!isOnboardingHeader ? (
            <Drawer.Root shouldScaleBackground={false}>
              <Drawer.Trigger asChild>
                <Button variant="outline" size="icon" className="h-9 w-9">
                  <Menu className="h-4 w-4" />
                  <span className="sr-only">Open portal links</span>
                </Button>
              </Drawer.Trigger>
              <Drawer.Portal>
                <Drawer.Overlay className="fixed inset-0 z-40 bg-black/35" />
                <Drawer.Content className="fixed inset-x-0 bottom-0 z-50 max-h-[78vh] rounded-t-2xl border border-border/80 bg-[var(--portal-surface)] p-4">
                  <div className="mx-auto mb-3 h-1.5 w-12 rounded-full bg-border/80" />
                  <p className="mb-3 text-sm font-semibold">Portal links</p>
                  <div className="grid gap-2 pb-2">
                    {resolvedUtilityLinks.map((item) => (
                      item.isExternal ? (
                        <a key={item.key} href={item.href} className="rounded-lg border px-3 py-2 text-sm text-foreground/82 transition hover:bg-muted/50">
                          {item.label}
                        </a>
                      ) : (
                        <Link key={item.key} href={item.href} className="rounded-lg border px-3 py-2 text-sm text-foreground/82 transition hover:bg-muted/50">
                          {item.label}
                        </Link>
                      )
                    ))}
                  </div>
                </Drawer.Content>
              </Drawer.Portal>
            </Drawer.Root>
          ) : null}

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
          {isOnboardingHeader ? (
            <Link href="/" className="text-sm font-semibold sm:text-base">
              {title}
            </Link>
          ) : (
            <p className="text-sm font-semibold sm:text-base">{title}</p>
          )}
          {showActiveBadge && activeItem ? <Badge variant="outline" className="hidden sm:inline-flex">{activeItem.label}</Badge> : null}
        </div>

        <div className="flex items-center gap-2">
          {isOnboardingHeader ? (
            <>
              <Button asChild variant="ghost" size="sm" className="h-8 px-2.5 text-xs">
                <Link href="/">Home</Link>
              </Button>
              <Button asChild variant="outline" size="sm" className="h-8 px-3 text-xs">
                <a href={`${apiBaseUrl}/account/login`}>Log in</a>
              </Button>
            </>
          ) : (
            <nav className="hidden items-center gap-2.5 lg:flex" aria-label="Portal utilities">
              {resolvedUtilityLinks.map((item) => (
                item.isExternal ? (
                  <a key={item.key} href={item.href} className="text-xs text-foreground/65 transition hover:text-foreground">
                    {item.label}
                  </a>
                ) : (
                  <Link key={item.key} href={item.href} className="text-xs text-foreground/65 transition hover:text-foreground">
                    {item.label}
                  </Link>
                )
              ))}
            </nav>
          )}

        </div>
      </header>

      <motion.div
        className={cn(
          "grid grid-cols-1 gap-4",
          showRail
            ? secondary
              ? "md:grid-cols-[minmax(13rem,17rem)_minmax(0,1fr)] xl:grid-cols-[minmax(14rem,18rem)_minmax(0,1fr)_minmax(16rem,20rem)]"
              : "md:grid-cols-[minmax(13rem,17rem)_minmax(0,1fr)]"
            : secondary
              ? "xl:grid-cols-[minmax(0,1fr)_minmax(16rem,20rem)]"
              : "",
        )}
        initial={prefersReducedMotion ? false : { opacity: 0, y: 10 }}
        animate={prefersReducedMotion ? undefined : { opacity: 1, y: 0 }}
        transition={prefersReducedMotion ? undefined : { duration: 0.24, ease: [0.22, 0.9, 0.2, 1] }}
      >
        {showRail ? (
          <aside className="hidden md:block">
            <div className="glass-panel p-3">
              <PortalRail activeKey={activeKey} items={railItems} />
            </div>
          </aside>
        ) : null}

        <div className="min-w-0">{children}</div>

        {secondary ? <aside className="hidden xl:block">{secondary}</aside> : null}
      </motion.div>
    </section>
  );
}
