"use client";

import { Toaster as SonnerToaster } from "sonner";

export function Toaster() {
  return (
    <SonnerToaster
      position="top-right"
      richColors
      closeButton
      toastOptions={{
        classNames: {
          toast: "border border-border/80 bg-card text-foreground shadow-luxe",
          description: "text-muted-foreground",
        },
      }}
    />
  );
}
