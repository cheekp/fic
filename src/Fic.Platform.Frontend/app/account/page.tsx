import { PublicPortalHeader } from "@/components/layout/public-portal-header";

const apiBaseUrl = process.env.NEXT_PUBLIC_FIC_API_BASE_URL ?? "http://localhost:5276";

export default function AccountPage() {
  return (
    <main className="space-y-6">
      <PublicPortalHeader />

      <section className="glass-panel p-6">
        <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Account</p>
        <h1 className="mt-2 font-display text-5xl leading-[0.94] tracking-tight">Merchant account</h1>
        <p className="mt-3 max-w-2xl text-foreground/80">Sign in, manage your session, and access workspace routes.</p>
      </section>

      <section className="grid gap-4 md:grid-cols-2">
        <a href={`${apiBaseUrl}/account/login`} className="glass-panel block p-5 transition hover:border-primary/50">
          <h2 className="font-display text-3xl">Log in</h2>
          <p className="mt-2 text-sm text-foreground/75">Open the existing authentication flow.</p>
        </a>
        <a href={`${apiBaseUrl}/account/logout`} className="glass-panel block p-5 transition hover:border-primary/50">
          <h2 className="font-display text-3xl">Log out</h2>
          <p className="mt-2 text-sm text-foreground/75">End current session and return to entry.</p>
        </a>
      </section>
    </main>
  );
}
