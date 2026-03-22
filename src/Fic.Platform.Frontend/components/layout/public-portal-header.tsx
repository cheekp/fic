"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { ChevronRight, Menu } from "lucide-react";
import { Drawer } from "vaul";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

type HeaderLink = {
  label: string;
  href: string;
  isExternal?: boolean;
};

const apiBaseUrl = process.env.NEXT_PUBLIC_FIC_API_BASE_URL ?? "http://localhost:5276";

const productLinks: HeaderLink[] = [
  { label: "Home", href: "/" },
  { label: "Blogs", href: "/blogs" },
  { label: "Training", href: "/training" },
  { label: "Consultancy", href: "/consultancy" },
  { label: "Billing", href: "/billing" },
];

const accountLinks: HeaderLink[] = [
  { label: "Account", href: "/account" },
  { label: "Log in", href: `${apiBaseUrl}/account/login`, isExternal: true },
  { label: "Sign up", href: "/portal/signup" },
];

function PublicHeaderLink({ link, isActive }: { link: HeaderLink; isActive: boolean }) {
  const className = cn(
    "flex items-center justify-between rounded-xl border border-border/70 bg-background/80 px-3 py-2 text-sm transition",
    isActive ? "text-foreground" : "text-foreground/80 hover:bg-background",
  );

  if (link.isExternal) {
    return (
      <a href={link.href} className={className}>
        <span>{link.label}</span>
        <ChevronRight className="h-4 w-4 text-muted-foreground" />
      </a>
    );
  }

  return (
    <Link href={link.href} className={className}>
      <span>{link.label}</span>
      <ChevronRight className="h-4 w-4 text-muted-foreground" />
    </Link>
  );
}

export function PublicPortalHeader() {
  const pathname = usePathname();

  return (
    <header className="sticky top-0 z-40 mb-5 flex items-center justify-between rounded-2xl border border-[rgba(15,27,42,0.12)] bg-[rgba(245,243,239,0.92)] px-3 py-2.5 shadow-[0_18px_40px_-30px_rgba(15,27,42,0.4)] backdrop-blur sm:px-4">
      <div className="flex items-center gap-2">
        <Drawer.Root shouldScaleBackground={false}>
          <Drawer.Trigger asChild>
            <Button variant="outline" size="icon" className="h-9 w-9 border-[rgba(15,27,42,0.16)] bg-transparent text-[#4a4f55] hover:bg-[rgba(15,27,42,0.04)]">
              <Menu className="h-4 w-4" />
              <span className="sr-only">Open site navigation</span>
            </Button>
          </Drawer.Trigger>
          <Drawer.Portal>
            <Drawer.Overlay className="fixed inset-0 z-40 bg-black/35" />
            <Drawer.Content className="fixed inset-x-0 bottom-0 z-50 max-h-[82vh] rounded-t-2xl border border-[rgba(15,27,42,0.12)] bg-[rgba(245,243,239,0.98)] p-4">
              <div className="mx-auto mb-3 h-1.5 w-12 rounded-full bg-border/80" />
              <div className="space-y-4">
                <section className="space-y-2">
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Portal</p>
                  <div className="grid gap-2">
                    {productLinks.map((link) => (
                      <PublicHeaderLink key={link.href} link={link} isActive={!link.isExternal && pathname === link.href} />
                    ))}
                  </div>
                </section>
                <section className="space-y-2">
                  <p className="text-xs uppercase tracking-[0.18em] text-muted-foreground">Account</p>
                  <div className="grid gap-2">
                    {accountLinks.map((link) => (
                      <PublicHeaderLink key={link.href} link={link} isActive={!link.isExternal && pathname === link.href} />
                    ))}
                  </div>
                </section>
              </div>
            </Drawer.Content>
          </Drawer.Portal>
        </Drawer.Root>
        <Link href="/" className="text-sm font-semibold tracking-[0.01em] text-[#0f1b2a] sm:text-base">
          North Star Customer Solutions
        </Link>
      </div>

      <div className="flex items-center gap-2">
        <Button asChild variant="ghost" size="sm" className="h-9 px-3 text-[#0f1b2a] hover:bg-[rgba(15,27,42,0.04)]">
          <a href={`${apiBaseUrl}/account/login`}>Log in</a>
        </Button>
        <Button asChild size="sm" className="h-9 px-4 bg-[#0f1b2a] text-[#f5f3ef] hover:bg-[#1b2d40]">
          <Link href="/portal/signup">Sign up</Link>
        </Button>
      </div>
    </header>
  );
}
