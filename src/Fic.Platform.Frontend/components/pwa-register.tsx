"use client";

import { useEffect } from "react";

export function PwaRegister() {
  useEffect(() => {
    if (typeof window === "undefined" || !("serviceWorker" in navigator)) {
      return;
    }

    const pwaEnabled = process.env.NEXT_PUBLIC_ENABLE_PWA === "true";
    const isProduction = process.env.NODE_ENV === "production";

    if (!pwaEnabled || !isProduction) {
      navigator.serviceWorker.getRegistrations().then((registrations) => {
        registrations.forEach((registration) => {
          registration.unregister().catch(() => {
            // Ignore cleanup failures.
          });
        });
      });
      if ("caches" in window) {
        caches.keys().then((keys) => {
          keys.filter((key) => key.startsWith("fic-next-shell-")).forEach((key) => {
            caches.delete(key).catch(() => {
              // Ignore cleanup failures.
            });
          });
        });
      }
      return;
    }

    navigator.serviceWorker.register("/sw.js").catch(() => {
      // Keep shell resilient when service worker registration fails.
    });
  }, []);

  return null;
}
