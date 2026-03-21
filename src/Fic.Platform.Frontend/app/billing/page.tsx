import Link from "next/link";

export default function BillingPage() {
  return (
    <main className="space-y-6">
      <section className="glass-panel p-6">
        <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Billing</p>
        <h1 className="mt-2 font-display text-5xl leading-[0.94] tracking-tight">Subscription and invoices</h1>
        <p className="mt-3 max-w-2xl text-foreground/80">Manage plans and billing records as these surfaces mature.</p>
      </section>

      <section className="glass-panel p-5">
        <h2 className="font-display text-3xl">Current status</h2>
        <p className="mt-2 text-sm text-foreground/75">Starter onboarding is active. Additional billing controls continue in upcoming slices.</p>
        <Link href="/portal/signup" className="mt-4 inline-block text-sm font-semibold text-primary underline-offset-4 hover:underline">
          Go to signup flow
        </Link>
      </section>
    </main>
  );
}
