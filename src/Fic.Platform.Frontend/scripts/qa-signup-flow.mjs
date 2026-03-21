import fs from "node:fs/promises";
import path from "node:path";
import { chromium } from "@playwright/test";

const frontendBaseUrl = process.env.FIC_QA_FRONTEND_BASE_URL ?? "http://localhost:3001";
const outputRoot = process.env.FIC_QA_OUTPUT_DIR ?? path.resolve(process.cwd(), "../../artifacts/nextjs-qa");
const stamp = process.env.FIC_QA_RUN_ID ?? new Date().toISOString().replace(/[:.]/g, "-");
const runDir = path.join(outputRoot, stamp);

const viewports = [
  { name: "mobile", width: 390, height: 844 },
  { name: "desktop", width: 1440, height: 900 },
];

function assertHttpUrl(value) {
  if (!value.startsWith("http://") && !value.startsWith("https://")) {
    throw new Error(`Invalid URL: ${value}`);
  }
}

async function ensureDir(dir) {
  await fs.mkdir(dir, { recursive: true });
}

async function runScenario(browser, viewport) {
  const context = await browser.newContext({ viewport: { width: viewport.width, height: viewport.height } });
  const page = await context.newPage();
  const scenarioDir = path.join(runDir, viewport.name);
  await ensureDir(scenarioDir);

  const email = `owner${Date.now()}@shop.test`;
  const result = {
    viewport: viewport.name,
    status: "ok",
    findings: [],
    screenshots: [],
  };

  try {
    await page.goto(`${frontendBaseUrl}/`, { waitUntil: "networkidle" });
    const homeShot = path.join(scenarioDir, "00-home.png");
    await page.screenshot({ path: homeShot, fullPage: true });
    result.screenshots.push(homeShot);

    await page.goto(`${frontendBaseUrl}/portal/signup`, { waitUntil: "networkidle" });
    await page.getByText(/Signup roadmap/i).first().waitFor({ state: "visible", timeout: 20000 });
    const loadFailed = page.getByText(/Load failed/i);
    if (await loadFailed.count()) {
      throw new Error("Signup shows 'Load failed'.");
    }
    const signupShot = path.join(scenarioDir, "01-signup.png");
    await page.screenshot({ path: signupShot, fullPage: true });
    result.screenshots.push(signupShot);

    await page.fill('input[id="displayName"]', "Jo's QA Coffee");
    await page.fill('input[id="ownerName"]', "Jo Owner");
    await page.fill('input[id="contactEmail"]', email);
    const submit = page.getByRole("button", { name: /Continue to plan/i });
    await submit.waitFor({ state: "visible", timeout: 10000 });
    const readyAt = Date.now() + 20000;
    while (!(await submit.isEnabled()) && Date.now() < readyAt) {
      await page.waitForTimeout(250);
    }
    if (!(await submit.isEnabled())) {
      throw new Error("Signup submit stayed disabled after entering required fields.");
    }
    await submit.click();

    await page.waitForURL(/\/portal\/signup\/plan\//, { timeout: 30000 });
    const planShot = path.join(scenarioDir, "02-plan.png");
    await page.screenshot({ path: planShot, fullPage: true });
    result.screenshots.push(planShot);

    const continueStarter = page.getByRole("button", { name: /Continue with Starter/i });
    await continueStarter.waitFor({ state: "visible", timeout: 10000 });
    await continueStarter.click();

    await page.waitForURL(/\/portal\/signup\/billing\//, { timeout: 30000 });
    const billingShot = path.join(scenarioDir, "03-billing.png");
    await page.screenshot({ path: billingShot, fullPage: true });
    result.screenshots.push(billingShot);

    await page.fill('input[id="password"]', "jo-demo-setup");
    await page.fill('input[id="confirm-password"]', "jo-demo-setup");
    const confirmOwner = page.getByRole("button", { name: /Confirm owner access|Owner access confirmed/i });
    if (await confirmOwner.count()) {
      await confirmOwner.first().click();
      await page.waitForURL(/stage=billing/, { timeout: 10000 }).catch(() => {});
    }
    const confirmBilling = page.getByRole("button", { name: /Confirm billing details|Billing confirmed/i });
    if (await confirmBilling.count()) {
      await confirmBilling.first().click();
    }
    const continueWorkspace = page.getByRole("button", { name: /Continue to workspace/i });
    await continueWorkspace.click();

    await page.waitForURL(/\/portal\/merchant\//, { timeout: 30000 });
    await page.getByText(/Signup roadmap|Setup tasks|Merchant workspace/i).first().waitFor({ state: "visible", timeout: 15000 });
    const workspaceShot = path.join(scenarioDir, "04-workspace.png");
    await page.screenshot({ path: workspaceShot, fullPage: true });
    result.screenshots.push(workspaceShot);

    const roadmap = page.getByText(/Signup roadmap/i);
    if (!(await roadmap.count())) {
      result.findings.push("Signup roadmap was not visible in workspace onboarding.");
      result.status = "warn";
    }

    const overflow = await page.evaluate(() => ({
      scrollWidth: document.documentElement.scrollWidth,
      viewportWidth: window.innerWidth,
    }));
    if (overflow.scrollWidth > overflow.viewportWidth + 1) {
      result.findings.push(
        `Horizontal overflow detected: scrollWidth=${overflow.scrollWidth}, viewport=${overflow.viewportWidth}`,
      );
      result.status = "warn";
    }
  } catch (error) {
    result.status = "error";
    result.findings.push(error instanceof Error ? error.message : String(error));
    const failureShot = path.join(scenarioDir, "99-failure.png");
    await page.screenshot({ path: failureShot, fullPage: true }).catch(() => {});
    result.screenshots.push(failureShot);
  } finally {
    await context.close();
  }

  return result;
}

async function main() {
  assertHttpUrl(frontendBaseUrl);
  await ensureDir(runDir);

  const browser = await chromium.launch({ headless: true });
  const results = [];

  try {
    for (const viewport of viewports) {
      // run sequentially to avoid API/session race noise in this workflow check
      const result = await runScenario(browser, viewport);
      results.push(result);
    }
  } finally {
    await browser.close();
  }

  const summary = {
    frontendBaseUrl,
    generatedAtUtc: new Date().toISOString(),
    results,
  };

  const summaryPath = path.join(runDir, "summary.json");
  await fs.writeFile(summaryPath, JSON.stringify(summary, null, 2), "utf8");

  // concise console output for CI/local readability
  console.log(`QA artifacts: ${runDir}`);
  for (const result of results) {
    console.log(`- ${result.viewport}: ${result.status}`);
    for (const finding of result.findings) {
      console.log(`  * ${finding}`);
    }
  }
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
