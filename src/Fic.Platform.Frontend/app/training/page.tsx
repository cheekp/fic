import Link from "next/link";

export default function TrainingPage() {
  return (
    <main className="space-y-6">
      <section className="glass-panel p-6">
        <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Training</p>
        <h1 className="mt-2 font-display text-5xl leading-[0.94] tracking-tight">Merchant training</h1>
        <p className="mt-3 max-w-2xl text-foreground/80">Onboarding sessions and playbooks for operators and teams.</p>
      </section>

      <section className="grid gap-4 md:grid-cols-2">
        <article className="glass-panel p-5">
          <h2 className="font-display text-3xl">Launch session</h2>
          <p className="mt-3 text-sm text-foreground/75">45-minute operator training for first deployment.</p>
        </article>
        <article className="glass-panel p-5">
          <h2 className="font-display text-3xl">Daily operations</h2>
          <p className="mt-3 text-sm text-foreground/75">Runbook for card support, rewards, and in-store troubleshooting.</p>
        </article>
      </section>

      <Link href="/consultancy" className="inline-block text-sm font-semibold text-primary underline-offset-4 hover:underline">
        Need guided rollout? Speak to consultancy
      </Link>
    </main>
  );
}
