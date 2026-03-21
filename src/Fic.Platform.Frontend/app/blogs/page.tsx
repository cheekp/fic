import Link from "next/link";

const posts = [
  {
    title: "How to launch a wallet loyalty programme in one shift",
    excerpt: "A practical launch checklist for independent shops.",
  },
  {
    title: "Five QR join patterns that improve first-week adoption",
    excerpt: "Real operator patterns for quicker customer onboarding.",
  },
  {
    title: "What merchants measure in week one",
    excerpt: "The few metrics that actually matter in early rollout.",
  },
];

export default function BlogsPage() {
  return (
    <main className="space-y-6">
      <section className="glass-panel p-6">
        <p className="text-xs uppercase tracking-[0.16em] text-muted-foreground">Blogs</p>
        <h1 className="mt-2 font-display text-5xl leading-[0.94] tracking-tight">Operator insights</h1>
        <p className="mt-3 max-w-2xl text-foreground/80">Concise guidance for launching and running loyalty in-store.</p>
      </section>

      <section className="grid gap-4 md:grid-cols-3">
        {posts.map((post) => (
          <article key={post.title} className="glass-panel p-5">
            <h2 className="font-display text-3xl leading-tight">{post.title}</h2>
            <p className="mt-3 text-sm text-foreground/75">{post.excerpt}</p>
            <Link href="/portal/signup" className="mt-4 inline-block text-sm font-semibold text-primary underline-offset-4 hover:underline">
              Start setup
            </Link>
          </article>
        ))}
      </section>
    </main>
  );
}
