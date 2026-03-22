import Image from "next/image";
import Link from "next/link";
import { ArrowRight, Compass } from "lucide-react";
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
              <div className="space-y-5">
                <h1 className="max-w-4xl font-display text-[3.2rem] leading-[0.92] tracking-tight text-[#f5f3ef] sm:text-[5.55rem]">
                  Designing loyalty that actually works.
                </h1>
                <p className="max-w-2xl text-[1.02rem] leading-8 text-[rgba(245,243,239,0.92)] sm:text-[1.16rem]">
                  North Star helps retail and consumer brands design loyalty and membership programmes with stronger economics, clearer customer value, and a practical route to launch.
                </p>
              </div>

              <div className="flex flex-wrap gap-3 pt-1">
                <Button
                  asChild
                  size="lg"
                  className="min-w-56 border border-[rgba(200,169,106,0.42)] bg-[#c8a96a] text-[#0f1b2a] hover:bg-[#d4b47a]"
                >
                  <Link href="/portal/signup" className="inline-flex items-center gap-2">
                    Start guided setup
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

            </section>

            <aside className="overflow-hidden rounded-[1.8rem] border border-[rgba(245,243,239,0.16)] bg-[rgba(245,243,239,0.06)] p-3 backdrop-blur">
              <img
                src="/images/home-hero.jpeg"
                alt="Customers in an independent coffee shop using loyalty."
                className="h-full min-h-[24rem] w-full rounded-[1.2rem] object-cover"
              />
            </aside>
          </div>

        </div>
      </section>

      <section className="public-premium-band grid gap-6 p-6 lg:grid-cols-[minmax(0,0.95fr)_minmax(0,1.05fr)] lg:p-8">
        <div className="relative z-10 space-y-4">
          <p className="public-micro-label">How North Star Works</p>
          <h2 className="max-w-xl font-display text-[2.2rem] leading-tight text-[#0f1b2a] sm:text-[3rem]">
            From customer strategy to live programme delivery.
          </h2>
          <p className="max-w-xl text-base leading-8 text-[rgba(74,79,85,0.95)]">
            North Star starts with customer behaviour and commercial objectives, then moves into proposition design, launch setup, and day-to-day operation.
          </p>
        </div>

        <div className="relative z-10 grid gap-4 md:grid-cols-3">
          <article className="rounded-[1.5rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,255,255,0.42)] p-5">
            <p className="public-micro-label">Insight</p>
            <h2 className="mt-2 font-display text-[1.75rem] text-[#0f1b2a]">Understand behaviour</h2>
            <p className="mt-3 text-sm leading-7 text-[rgba(74,79,85,0.94)]">Identify the moments, segments, and incentives that shape repeat behaviour.</p>
          </article>
          <article className="rounded-[1.5rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,255,255,0.42)] p-5">
            <p className="public-micro-label">Value</p>
            <h2 className="mt-2 font-display text-[1.75rem] text-[#0f1b2a]">Shape the proposition</h2>
            <p className="mt-3 text-sm leading-7 text-[rgba(74,79,85,0.94)]">Define rewards, membership structure, and value exchange customers will recognise.</p>
          </article>
          <article className="rounded-[1.5rem] border border-[rgba(15,27,42,0.1)] bg-[rgba(255,255,255,0.42)] p-5">
            <p className="public-micro-label">Execution</p>
            <h2 className="mt-2 font-display text-[1.75rem] text-[#0f1b2a]">Launch with control</h2>
            <p className="mt-3 text-sm leading-7 text-[rgba(74,79,85,0.94)]">Set up the programme, publish it, and operate it with clear ownership.</p>
          </article>
        </div>
      </section>

      <section className="rounded-[2rem] border border-[rgba(15,27,42,0.08)] bg-[rgba(255,251,245,0.76)] px-6 py-8 shadow-[0_20px_40px_-34px_rgba(15,27,42,0.18)]">
        <div className="flex flex-wrap items-center justify-center gap-8 sm:gap-12 lg:gap-20">
          {[
            { name: "Facebook", icon: "/images/social/facebook.svg", tileClassName: "bg-[#1877F2]" },
            { name: "X", icon: "/images/social/x.svg", tileClassName: "bg-[#111111]" },
            { name: "TikTok", icon: "/images/social/tiktok.svg", tileClassName: "bg-[#111111]" },
            { name: "Instagram", icon: "/images/social/instagram.svg", tileClassName: "bg-[linear-gradient(135deg,#f58529,#dd2a7b,#8134af,#515bd4)]" },
          ].map((channel) => (
            <span
              key={channel.name}
              className={`flex h-16 w-16 items-center justify-center rounded-2xl shadow-[0_12px_30px_-24px_rgba(15,27,42,0.3)] ${channel.tileClassName}`}
              aria-label={channel.name}
              title={channel.name}
            >
              <Image src={channel.icon} alt={`${channel.name} icon`} width={34} height={34} className="h-[34px] w-[34px]" />
            </span>
          ))}
        </div>
      </section>
    </main>
  );
}
