import Link from "next/link";
import { ArrowRight, CheckCircle2, Compass, ShieldCheck, Wallet } from "lucide-react";
import { buildBrandCssVars } from "@/lib/brand";
import { PublicPortalHeader } from "@/components/layout/public-portal-header";
import { Button } from "@/components/ui/button";
import { northStarPortalTheme } from "@/types/portal-contracts";

export default function HomePage() {
  return (
    <main className="space-y-8" style={buildBrandCssVars(northStarPortalTheme)}>
      <PublicPortalHeader />

      <section
        className="relative overflow-hidden rounded-3xl border p-5 shadow-[0_28px_80px_-42px_rgba(15,27,42,0.48)] sm:p-8"
        style={{
          borderColor: "rgba(200, 169, 106, 0.28)",
          background:
            "radial-gradient(circle at top left, rgba(200,169,106,0.18), transparent 26%), linear-gradient(140deg, #0f1b2a 0%, #152739 54%, #20344a 100%)",
        }}
      >
        <div
          aria-hidden
          className="pointer-events-none absolute -right-16 -top-20 h-72 w-72 rounded-full bg-[rgba(200,169,106,0.16)] blur-3xl"
        />
        <div
          aria-hidden
          className="pointer-events-none absolute -left-16 bottom-0 h-56 w-56 rounded-full bg-[rgba(245,243,239,0.08)] blur-3xl"
        />

        <div className="relative grid gap-7">
          <div className="grid gap-6 lg:grid-cols-[1.06fr_0.94fr]">
            <section className="space-y-6">
              <div className="inline-flex items-center gap-2 rounded-full border border-[rgba(200,169,106,0.22)] bg-[rgba(245,243,239,0.06)] px-3 py-1 text-[11px] uppercase tracking-[0.24em] text-[rgba(200,169,106,0.92)]">
                <Compass className="h-3.5 w-3.5" />
                North Star Customer Solutions
              </div>
              <h1 className="max-w-4xl font-display text-[2.9rem] leading-[0.94] tracking-tight text-[#f5f3ef] sm:text-[5.25rem]">
                Designing loyalty that actually works.
              </h1>
              <p className="max-w-2xl text-[1.02rem] leading-8 text-[rgba(245,243,239,0.82)] sm:text-[1.16rem]">
                North Star helps retailers and consumer brands shape loyalty and membership strategies with commercial rigour, customer insight, and practical execution.
              </p>

              <div className="flex flex-wrap gap-3 pt-1">
                <Button
                  asChild
                  size="lg"
                  className="min-w-56 border border-[rgba(200,169,106,0.42)] bg-[#c8a96a] text-[#0f1b2a] hover:bg-[#d4b47a]"
                >
                  <Link href="/portal/signup" className="inline-flex items-center gap-2">
                    Start platform setup
                    <ArrowRight className="h-4 w-4" />
                  </Link>
                </Button>
                <Button
                  asChild
                  size="lg"
                  variant="outline"
                  className="min-w-48 border-[rgba(245,243,239,0.24)] bg-transparent text-[#f5f3ef] hover:bg-[rgba(245,243,239,0.08)]"
                >
                  <a href="/consultancy">Talk to us</a>
                </Button>
              </div>

              <ul className="grid gap-2 text-sm text-[rgba(245,243,239,0.88)] sm:grid-cols-3">
                <li className="flex items-center gap-2">
                  <Wallet className="h-4 w-4 text-[#c8a96a]" />
                  Membership strategy
                </li>
                <li className="flex items-center gap-2">
                  <CheckCircle2 className="h-4 w-4 text-[#c8a96a]" />
                  Customer insight
                </li>
                <li className="flex items-center gap-2">
                  <ShieldCheck className="h-4 w-4 text-[#c8a96a]" />
                  Commercial execution
                </li>
              </ul>
            </section>

            <aside className="overflow-hidden rounded-2xl border border-[rgba(245,243,239,0.16)] bg-[rgba(245,243,239,0.06)] backdrop-blur">
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
        <article className="glass-panel border-[rgba(15,27,42,0.08)] bg-[rgba(255,250,244,0.88)] p-5">
          <p className="text-xs uppercase tracking-[0.16em] text-[rgba(74,79,85,0.86)]">Insight</p>
          <h2 className="mt-2 font-display text-2xl text-[#0f1b2a]">Understand customer behaviour</h2>
          <p className="mt-2 text-sm text-[rgba(74,79,85,0.96)]">Build loyalty around the moments, segments, and value drivers that actually influence repeat behaviour.</p>
        </article>
        <article className="glass-panel border-[rgba(15,27,42,0.08)] bg-[rgba(255,250,244,0.88)] p-5">
          <p className="text-xs uppercase tracking-[0.16em] text-[rgba(74,79,85,0.86)]">Value</p>
          <h2 className="mt-2 font-display text-2xl text-[#0f1b2a]">Design propositions customers value</h2>
          <p className="mt-2 text-sm text-[rgba(74,79,85,0.96)]">Move beyond points and discounts with membership and loyalty mechanics grounded in genuine customer benefit.</p>
        </article>
        <article className="glass-panel border-[rgba(15,27,42,0.08)] bg-[rgba(255,250,244,0.88)] p-5">
          <p className="text-xs uppercase tracking-[0.16em] text-[rgba(74,79,85,0.86)]">Execution</p>
          <h2 className="mt-2 font-display text-2xl text-[#0f1b2a]">Turn strategy into operating loyalty</h2>
          <p className="mt-2 text-sm text-[rgba(74,79,85,0.96)]">Use the platform to move from proposition thinking into merchant-ready delivery, testing, and daily operation.</p>
        </article>
      </section>
    </main>
  );
}
