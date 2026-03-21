import Link from "next/link";
import { ArrowRight, CheckCircle2, ShieldCheck, Wallet } from "lucide-react";
import { PublicPortalHeader } from "@/components/layout/public-portal-header";
import { Button } from "@/components/ui/button";

export default function HomePage() {
  return (
    <main className="space-y-8">
      <PublicPortalHeader />

      <section className="relative overflow-hidden rounded-3xl border border-border/70 bg-[linear-gradient(145deg,#1c342f,#275248)] p-5 text-slate-100 shadow-luxe sm:p-8">
        <div
          aria-hidden
          className="pointer-events-none absolute -right-16 -top-20 h-72 w-72 rounded-full bg-amber-300/20 blur-3xl"
        />
        <div
          aria-hidden
          className="pointer-events-none absolute -left-16 bottom-0 h-56 w-56 rounded-full bg-emerald-300/20 blur-3xl"
        />

        <div className="relative grid gap-7">
          <div className="grid gap-6 lg:grid-cols-[1.06fr_0.94fr]">
            <section className="space-y-5">
              <p className="text-xs uppercase tracking-[0.18em] text-amber-200/90">Launch in minutes</p>
              <h1 className="max-w-4xl font-display text-[2.7rem] leading-[0.93] tracking-tight text-slate-50 sm:text-[5rem]">
                Set up Apple Wallet loyalty. Fast.
              </h1>
              <p className="max-w-2xl text-[1.06rem] leading-8 text-slate-200/95 sm:text-[1.2rem]">
                Launch a live programme your customers can join and use immediately.
              </p>

              <div className="flex flex-wrap gap-3 pt-1">
                <Button asChild size="lg" className="min-w-56 bg-[#f4c15d] text-[#14211d] hover:bg-[#f2cc7c]">
                  <Link href="/portal/signup" className="inline-flex items-center gap-2">
                    Start setup
                    <ArrowRight className="h-4 w-4" />
                  </Link>
                </Button>
                <Button asChild size="lg" variant="outline" className="min-w-48 border-white/35 bg-transparent text-slate-100 hover:bg-white/10">
                  <a href="/consultancy">Talk to us</a>
                </Button>
              </div>

              <ul className="grid gap-2 text-sm text-slate-200/90 sm:grid-cols-3">
                <li className="flex items-center gap-2">
                  <Wallet className="h-4 w-4 text-amber-300" />
                  Apple Wallet first
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle2 className="h-4 w-4 text-amber-300" />
                  Instant QR join
                </li>
                <li className="flex items-center gap-2">
                  <ShieldCheck className="h-4 w-4 text-amber-300" />
                  Daily operations ready
                </li>
              </ul>
            </section>

            <aside className="overflow-hidden rounded-2xl border border-white/20 bg-white/10 backdrop-blur">
              <img
                src="/images/home-hero.jpeg"
                alt="Customers in an independent coffee shop using loyalty."
                className="h-full min-h-[20rem] w-full object-cover"
              />
            </aside>
          </div>
        </div>
      </section>

      <section className="grid gap-4 sm:grid-cols-3">
        <article className="glass-panel p-5">
          <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Setup</p>
          <h2 className="mt-2 font-display text-2xl">Start in minutes</h2>
          <p className="mt-2 text-sm text-foreground/75">Create your shop, pick a plan, and publish your first programme.</p>
        </article>
        <article className="glass-panel p-5">
          <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Customers</p>
          <h2 className="mt-2 font-display text-2xl">Join without an app</h2>
          <p className="mt-2 text-sm text-foreground/75">Scan QR, add pass to Apple Wallet, and start collecting visits.</p>
        </article>
        <article className="glass-panel p-5">
          <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Operate</p>
          <h2 className="mt-2 font-display text-2xl">Run from one workspace</h2>
          <p className="mt-2 text-sm text-foreground/75">Manage programme setup, customer cards, and rewards in one place.</p>
        </article>
      </section>
    </main>
  );
}
