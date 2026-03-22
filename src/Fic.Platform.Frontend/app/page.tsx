import Link from "next/link";
import { ArrowRight, CheckCircle2, Compass, ShieldCheck, Wallet } from "lucide-react";
import { buildBrandCssVars } from "@/lib/brand";
import { PublicPortalHeader } from "@/components/layout/public-portal-header";
import { Button } from "@/components/ui/button";
import { northStarPortalTheme } from "@/types/portal-contracts";

export default function HomePage() {
  return (
    <main className="public-editorial-shell space-y-8" style={buildBrandCssVars(northStarPortalTheme)}>
      <PublicPortalHeader />

      <section
        className="relative overflow-hidden rounded-[2.2rem] border p-5 shadow-[0_28px_80px_-42px_rgba(15,27,42,0.48)] sm:p-8 lg:p-10"
        style={{
          borderColor: "rgba(200, 169, 106, 0.28)",
          background:
            "radial-gradient(circle at top left, rgba(200,169,106,0.16), transparent 24%), radial-gradient(circle at bottom right, rgba(245,243,239,0.06), transparent 24%), linear-gradient(140deg, #0f1b2a 0%, #152739 54%, #20344a 100%)",
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

        <div className="relative grid gap-8">
          <div className="grid items-stretch gap-8 lg:grid-cols-[minmax(0,1.12fr)_minmax(24rem,0.88fr)]">
            <section className="space-y-7">
              <div className="inline-flex items-center gap-2 rounded-full border border-[rgba(200,169,106,0.22)] bg-[rgba(245,243,239,0.06)] px-3 py-1 text-[11px] uppercase tracking-[0.24em] text-[rgba(200,169,106,0.92)]">
                <Compass className="h-3.5 w-3.5" />
                North Star Customer Solutions
              </div>
              <div className="grid gap-6 xl:grid-cols-[minmax(0,1fr)_13rem] xl:items-end">
                <div className="space-y-5">
                  <h1 className="max-w-4xl font-display text-[3.2rem] leading-[0.92] tracking-tight text-[#f5f3ef] sm:text-[5.55rem]">
                    Designing loyalty that customers choose to return to.
                  </h1>
                  <p className="max-w-2xl text-[1.02rem] leading-8 text-[rgba(245,243,239,0.82)] sm:text-[1.16rem]">
                    North Star helps retailers and consumer brands shape loyalty and membership strategies with commercial rigour, customer insight, and practical execution.
                  </p>
                </div>
                <div className="grid gap-3 xl:pb-2">
                  <div className="premium-stat">
                    <p className="text-[11px] uppercase tracking-[0.2em] text-[rgba(200,169,106,0.85)]">Approach</p>
                    <p className="mt-2 text-sm leading-6 text-[rgba(245,243,239,0.82)]">Insight-led proposition design with merchant-ready delivery.</p>
                  </div>
                  <div className="premium-stat">
                    <p className="text-[11px] uppercase tracking-[0.2em] text-[rgba(200,169,106,0.85)]">Use case</p>
                    <p className="mt-2 text-sm leading-6 text-[rgba(245,243,239,0.82)]">Launch Apple Wallet loyalty with a clearer operating model.</p>
                  </div>
                </div>
              </div>

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

              <ul className="grid gap-3 text-sm text-[rgba(245,243,239,0.88)] sm:grid-cols-3">
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

            <aside className="overflow-hidden rounded-[1.8rem] border border-[rgba(245,243,239,0.16)] bg-[rgba(245,243,239,0.06)] p-3 backdrop-blur">
              <img
                src="/images/home-hero.jpeg"
                alt="Customers in an independent coffee shop using loyalty."
                className="h-full min-h-[24rem] w-full rounded-[1.2rem] object-cover"
              />
            </aside>
          </div>

          <div className="public-insight-grid">
            <article className="rounded-[1.7rem] border border-[rgba(200,169,106,0.18)] bg-[rgba(245,243,239,0.08)] p-6 text-[rgba(245,243,239,0.88)]">
              <p className="text-xs uppercase tracking-[0.18em] text-[rgba(200,169,106,0.88)]">Strategic focus</p>
              <p className="mt-3 max-w-3xl font-display text-[2rem] leading-tight text-[rgba(245,243,239,0.96)] sm:text-[2.35rem]">
                Reward behaviour, not just transactions.
              </p>
              <p className="mt-3 max-w-3xl text-base leading-8 text-[rgba(245,243,239,0.84)]">
                North Star brings together customer insight, proposition design, and practical rollout so loyalty becomes a commercially credible operating system rather than a tactical discount device.
              </p>
            </article>

            <div className="grid gap-3 sm:grid-cols-3 lg:grid-cols-1">
              <div className="rounded-[1.4rem] border border-[rgba(245,243,239,0.12)] bg-[rgba(245,243,239,0.06)] p-4 text-[rgba(245,243,239,0.9)]">
                <p className="text-[11px] uppercase tracking-[0.18em] text-[rgba(200,169,106,0.86)]">Pillar</p>
                <p className="mt-2 font-display text-2xl">Insight</p>
                <p className="mt-2 text-sm leading-6 text-[rgba(245,243,239,0.74)]">Segmentation, behaviour, and the moments that shape repeat choice.</p>
              </div>
              <div className="rounded-[1.4rem] border border-[rgba(245,243,239,0.12)] bg-[rgba(245,243,239,0.06)] p-4 text-[rgba(245,243,239,0.9)]">
                <p className="text-[11px] uppercase tracking-[0.18em] text-[rgba(200,169,106,0.86)]">Pillar</p>
                <p className="mt-2 font-display text-2xl">Value</p>
                <p className="mt-2 text-sm leading-6 text-[rgba(245,243,239,0.74)]">Propositions customers genuinely benefit from, not just price-led mechanics.</p>
              </div>
              <div className="rounded-[1.4rem] border border-[rgba(245,243,239,0.12)] bg-[rgba(245,243,239,0.06)] p-4 text-[rgba(245,243,239,0.9)]">
                <p className="text-[11px] uppercase tracking-[0.18em] text-[rgba(200,169,106,0.86)]">Pillar</p>
                <p className="mt-2 font-display text-2xl">Execution</p>
                <p className="mt-2 text-sm leading-6 text-[rgba(245,243,239,0.74)]">Operational setup that turns strategy into a live merchant programme.</p>
              </div>
            </div>
          </div>
        </div>
      </section>

      <section className="public-premium-band grid gap-6 p-6 lg:grid-cols-[minmax(0,0.95fr)_minmax(0,1.05fr)] lg:p-8">
        <div className="relative z-10 space-y-4">
          <p className="public-micro-label">Operating model</p>
          <h2 className="max-w-xl font-display text-[2.2rem] leading-tight text-[#0f1b2a] sm:text-[3rem]">
            Strategy, setup, and execution in one coherent lane.
          </h2>
          <p className="max-w-xl text-base leading-8 text-[rgba(74,79,85,0.95)]">
            The platform carries the same North Star principles through the journey: insight first, disciplined proposition design, then a merchant setup flow that gets the programme live without unnecessary friction.
          </p>
        </div>

        <div className="relative z-10 grid gap-4 md:grid-cols-3">
          <article className="rounded-[1.5rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,255,255,0.42)] p-5">
            <p className="public-micro-label">Insight</p>
            <h2 className="mt-2 font-display text-[1.75rem] text-[#0f1b2a]">Understand behaviour</h2>
            <p className="mt-3 text-sm leading-7 text-[rgba(74,79,85,0.94)]">Build around the moments, segments, and value drivers that influence repeat behaviour.</p>
          </article>
          <article className="rounded-[1.5rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,255,255,0.42)] p-5">
            <p className="public-micro-label">Value</p>
            <h2 className="mt-2 font-display text-[1.75rem] text-[#0f1b2a]">Shape the proposition</h2>
            <p className="mt-3 text-sm leading-7 text-[rgba(74,79,85,0.94)]">Design membership and loyalty mechanics grounded in genuine customer benefit.</p>
          </article>
          <article className="rounded-[1.5rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,255,255,0.42)] p-5">
            <p className="public-micro-label">Execution</p>
            <h2 className="mt-2 font-display text-[1.75rem] text-[#0f1b2a]">Launch with control</h2>
            <p className="mt-3 text-sm leading-7 text-[rgba(74,79,85,0.94)]">Move from proposition thinking into a merchant-ready programme and daily operation.</p>
          </article>
        </div>
      </section>
    </main>
  );
}
