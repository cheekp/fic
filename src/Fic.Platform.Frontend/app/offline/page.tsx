import Link from "next/link";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";

export default function OfflinePage() {
  return (
    <main>
      <Card className="mx-auto max-w-xl">
        <CardHeader>
          <CardTitle>You are offline</CardTitle>
          <CardDescription>The workspace shell is still available. Reconnect to sync live merchant data.</CardDescription>
        </CardHeader>
        <CardContent>
          <Button asChild>
            <Link href="/">Return to workspace home</Link>
          </Button>
        </CardContent>
      </Card>
    </main>
  );
}
