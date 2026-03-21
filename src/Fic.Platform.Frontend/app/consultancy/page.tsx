import Link from "next/link";

export default function ConsultancyPage() {
  return (
    <main className="space-y-6">
      <section className="glass-panel p-6">
        <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Consultancy</p>
        <h1 className="mt-2 font-display text-5xl leading-[0.94] tracking-tight">Rollout support</h1>
        <p className="mt-3 max-w-2xl text-foreground/80">Hands-on help for launch planning, staff enablement, and scale-up.</p>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        <article className="glass-panel p-5">
          <h2 className="font-display text-2xl">Starter</h2>
          <p className="mt-2 text-sm text-foreground/75">Launch checklist and setup review.</p>
        </article>
        <article className="glass-panel p-5">
          <h2 className="font-display text-2xl">Growth</h2>
          <p className="mt-2 text-sm text-foreground/75">Operational coaching for multi-site teams.</p>
        </article>
        <article className="glass-panel p-5">
          <h2 className="font-display text-2xl">Enterprise</h2>
          <p className="mt-2 text-sm text-foreground/75">Governance and integration planning support.</p>
        </article>
      </section>

      <Link href="/portal/signup" className="inline-block text-sm font-semibold text-primary underline-offset-4 hover:underline">
        Start merchant setup
      </Link>
    </main>
  );
}
