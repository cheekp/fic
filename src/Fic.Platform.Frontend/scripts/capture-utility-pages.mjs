import fs from "node:fs/promises";
import path from "node:path";
import { chromium } from "@playwright/test";

const runDir = process.env.FIC_QA_UTILITY_DIR ?? path.resolve(process.cwd(), "../../artifacts/nextjs-qa/utility-desktop");
await fs.mkdir(runDir, { recursive: true });

const routes = [
  ["blogs", "/blogs"],
  ["training", "/training"],
  ["consultancy", "/consultancy"],
  ["account", "/account"],
  ["billing", "/billing"],
];

const browser = await chromium.launch({ headless: true });
const context = await browser.newContext({ viewport: { width: 1440, height: 900 } });
const page = await context.newPage();

for (const [name, route] of routes) {
  await page.goto("http://localhost:3001" + route, { waitUntil: "networkidle" });
  await page.screenshot({ path: path.join(runDir, name + ".png"), fullPage: true });
}

await context.close();
await browser.close();
console.log(runDir);
