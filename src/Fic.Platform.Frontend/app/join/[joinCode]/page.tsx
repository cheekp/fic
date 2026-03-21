"use client";

import Image from "next/image";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { getJoinExperience, joinProgramme } from "@/lib/api";
import type { JoinExperienceSnapshot, WalletCardSnapshot } from "@/types/contracts";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";

export default function JoinPage() {
  const params = useParams<{ joinCode: string }>();
  const joinCode = Array.isArray(params.joinCode) ? params.joinCode[0] : params.joinCode;
  const [experience, setExperience] = useState<JoinExperienceSnapshot | null>(null);
  const [card, setCard] = useState<WalletCardSnapshot | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [isJoining, setIsJoining] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    let cancelled = false;
    getJoinExperience(joinCode)
      .then((next) => {
        if (!cancelled) {
          setExperience(next);
          setError(null);
        }
      })
      .catch((err) => {
        if (!cancelled) {
          setError(err instanceof Error ? err.message : "Unable to load join experience.");
          setExperience(null);
        }
      })
      .finally(() => {
        if (!cancelled) {
          setIsLoading(false);
        }
      });

    return () => {
      cancelled = true;
    };
  }, [joinCode]);

  async function handleJoin() {
    setIsJoining(true);
    setError(null);
    try {
      const nextCard = await joinProgramme(joinCode);
      setCard(nextCard);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Unable to join right now.");
    } finally {
      setIsJoining(false);
    }
  }

  if (isLoading) {
    return (
      <main className="mx-auto min-h-screen w-full max-w-3xl px-4 py-10 sm:px-6">
        <Card>
          <CardContent className="p-6">Loading join flow...</CardContent>
        </Card>
      </main>
    );
  }

  if (!experience) {
    return (
      <main className="mx-auto min-h-screen w-full max-w-3xl px-4 py-10 sm:px-6">
        <Card>
          <CardHeader>
            <CardTitle>Join unavailable</CardTitle>
            <CardDescription>{error ?? "This join code is no longer available."}</CardDescription>
          </CardHeader>
        </Card>
      </main>
    );
  }

  return (
    <main className="mx-auto min-h-screen w-full max-w-3xl px-4 py-8 sm:px-6">
      <section
        className="rounded-[2rem] border border-white/30 p-6 text-white shadow-[0_24px_60px_-30px_rgba(20,33,29,0.6)] sm:p-8"
        style={{
          background: `linear-gradient(120deg, ${experience.brandProfile.primaryColor} 0%, color-mix(in srgb, ${experience.brandProfile.primaryColor} 45%, ${experience.brandProfile.accentColor} 55%) 55%, ${experience.brandProfile.accentColor} 100%)`,
        }}
      >
        <p className="text-xs uppercase tracking-[0.22em] text-white/80">Customer join flow</p>
        <h1 className="mt-3 font-display text-5xl leading-[0.95] sm:text-6xl">{experience.merchant.displayName}</h1>
        <p className="mt-4 max-w-2xl text-base text-white/90 sm:text-lg">
          Join {experience.programme.templateLabel.toLowerCase()} and start collecting progress immediately.
        </p>

        <div className="mt-6 rounded-2xl bg-white/12 p-4 backdrop-blur">
          <p className="text-sm text-white/80">Reward</p>
          <p className="text-2xl font-semibold">{experience.programme.rewardCopy}</p>
          <p className="mt-1 text-sm text-white/80">
            {experience.programme.rewardThreshold} visits to unlock {experience.programme.rewardItemLabel.toLowerCase()}.
          </p>
        </div>

        <div className="mt-6 flex flex-wrap items-center gap-3">
          {card ? (
            <Button className="bg-white text-[color:var(--join-ink,#14211d)] hover:bg-white/90" disabled>
              Joined: {card.cardCode}
            </Button>
          ) : (
            <Button className="bg-white text-[color:var(--join-ink,#14211d)] hover:bg-white/90" onClick={handleJoin} disabled={isJoining}>
              {isJoining ? "Joining..." : "Join now"}
            </Button>
          )}
          <span className="text-sm text-white/85">Code: {experience.programme.joinCode}</span>
        </div>
      </section>

      {card ? (
        <Card className="mt-5">
          <CardHeader>
            <CardTitle>You are in</CardTitle>
            <CardDescription>Your first wallet card is ready.</CardDescription>
          </CardHeader>
          <CardContent className="space-y-3 text-sm text-foreground/80">
            <p>Card code: <span className="font-semibold text-foreground">{card.cardCode}</span></p>
            <p>Progress: <span className="font-semibold text-foreground">{card.progressDisplayText}</span></p>
            <p>Status: <span className="font-semibold text-foreground">{card.customerCardStatusLabel}</span></p>
          </CardContent>
        </Card>
      ) : null}

      {experience.brandProfile.logoUrl ? (
        <div className="mt-4 text-right">
          <Image
            src={experience.brandProfile.logoUrl}
            alt={`${experience.merchant.displayName} logo`}
            width={experience.brandProfile.logoWidth || 96}
            height={experience.brandProfile.logoHeight || 96}
            className="inline-block rounded-md border border-border/60 bg-white/70 p-1"
            unoptimized
          />
        </div>
      ) : null}

      {error ? <p className="mt-4 text-sm text-destructive">{error}</p> : null}
    </main>
  );
}
