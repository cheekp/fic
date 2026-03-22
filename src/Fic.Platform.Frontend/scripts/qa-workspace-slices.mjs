import fs from "node:fs/promises";
import path from "node:path";
import { chromium } from "@playwright/test";

const frontendBaseUrl = process.env.FIC_QA_FRONTEND_BASE_URL ?? "http://localhost:3002";
const outputRoot = process.env.FIC_QA_OUTPUT_DIR ?? path.resolve(process.cwd(), "../../artifacts/nextjs-qa");
const stamp = process.env.FIC_QA_RUN_ID ?? new Date().toISOString().replace(/[:.]/g, "-");
const runDir = path.join(outputRoot, `${stamp}-workspace-slices`);

const viewports = [
  { name: "mobile", width: 390, height: 844 },
  { name: "desktop", width: 1440, height: 900 },
];

async function ensureDir(dir) {
  await fs.mkdir(dir, { recursive: true });
}

function isDarkNavy(color) {
  return /rgb\(\s*(1[0-9]|2[0-9])\s*,\s*(2[0-9]|3[0-9])\s*,\s*(3[0-9]|4[0-9])\s*\)/.test(color);
}

async function bootstrapMerchant(page) {
  const email = `owner${Date.now()}@shop.test`;
  const logoPath = path.resolve(process.cwd(), "../Fic.Platform.Web/wwwroot/wallet-assets/icon.png");

  await page.goto(`${frontendBaseUrl}/portal/signup`, { waitUntil: "networkidle" });
  await page.fill('input[id="displayName"]', "Jo's QA Coffee");
  await page.fill('input[id="ownerName"]', "Jo Owner");
  await page.fill('input[id="contactEmail"]', email);
  const continueToPlan = page.getByRole("button", { name: /Continue to plan/i });
  await continueToPlan.waitFor({ state: "visible", timeout: 10000 });
  const signupReadyAt = Date.now() + 20000;
  while (!(await continueToPlan.isEnabled()) && Date.now() < signupReadyAt) {
    await page.waitForTimeout(250);
  }
  if (!(await continueToPlan.isEnabled())) {
    throw new Error("Workspace bootstrap signup submit stayed disabled after entering required fields.");
  }
  await continueToPlan.click();

  await page.waitForURL(/\/portal\/signup\/plan\//, { timeout: 30000 });
  await page.getByRole("button", { name: /Continue with Starter/i }).click();

  await page.waitForURL(/\/portal\/signup\/billing\//, { timeout: 30000 });
  await page.fill('input[id="password"]', "jo-demo-setup");
  await page.fill('input[id="confirm-password"]', "jo-demo-setup");
  const confirmOwner = page.getByRole("button", { name: /Confirm owner access|Owner access confirmed|Save owner access and continue/i });
  if (await confirmOwner.count()) {
    await confirmOwner.first().click();
    await page.waitForURL(/stage=billing/, { timeout: 10000 }).catch(() => {});
  }
  const confirmBilling = page.getByRole("button", { name: /Confirm billing|Billing confirmed/i });
  if (await confirmBilling.count()) {
    await confirmBilling.first().click();
  }
  await page.getByRole("button", { name: /Continue to workspace/i }).click();

  await page.waitForURL(/\/portal\/merchant\//, { timeout: 30000 });
  const merchantMatch = page.url().match(/\/portal\/merchant\/([^/?#]+)/);
  const merchantId = merchantMatch?.[1];

  if (!merchantId) {
    throw new Error("Unable to resolve merchant id from workspace URL.");
  }

  await page.waitForLoadState("networkidle");
  await page.getByText(/Shop details|Programme template|Merchant workspace/i).first().waitFor({ state: "visible", timeout: 10000 }).catch(() => {});

  const openShopSetup = page.getByRole("button", { name: /Open shop setup/i });
  const editShopDetails = page.getByRole("button", { name: /Edit shop details/i });
  const createProgrammeButton = page.getByTestId("create-programme");
  const inlineShopSaveButton = page.getByRole("button", { name: /Save shop details/i });

  let attemptsRemaining = 5;
  while (attemptsRemaining > 0 && (await openShopSetup.count()) === 0 && (await createProgrammeButton.count()) === 0 && (await inlineShopSaveButton.count()) === 0) {
    attemptsRemaining -= 1;
    await page.waitForTimeout(200);
  }

  const needsShopSetup = (await inlineShopSaveButton.count()) > 0 || (await openShopSetup.count()) > 0;
  let shopDetailsSaved = !needsShopSetup;

  if (needsShopSetup) {
    if ((await openShopSetup.count()) > 0 && (await inlineShopSaveButton.count()) === 0) {
      await openShopSetup.first().click();
    }

    const shopTypeTrigger = page.locator('button[id="shop-type"], button[id="shop-type-inline"]');
    if (await shopTypeTrigger.count()) {
      await shopTypeTrigger.first().click();
      const coffeeOption = page.getByRole("option", { name: /Coffee shop/i });
      if (await coffeeOption.count()) {
        await coffeeOption.first().click();
      }
    }

    const town = page.locator('input[id="shop-town"], input[id="shop-town-inline"]');
    const postcode = page.locator('input[id="shop-postcode"], input[id="shop-postcode-inline"]');
    await town.first().waitFor({ state: "visible", timeout: 10000 }).catch(() => {});
    await postcode.first().waitFor({ state: "visible", timeout: 10000 }).catch(() => {});
    if (await town.count()) {
      await town.fill("Bristol");
    }
    if (await postcode.count()) {
      await postcode.fill("BS1 4DJ");
    }

    const logoInput = page.locator('input[type="file"]');
    if (await logoInput.count()) {
      await logoInput.first().setInputFiles(logoPath);
      const uploadLogo = page.getByRole("button", { name: /Upload logo/i }).first();
      if (await uploadLogo.isEnabled()) {
        await uploadLogo.click();
        await page.getByText(/Brand logo updated/i).first().waitFor({ state: "visible", timeout: 15000 }).catch(() => {});
      }
    }

    const saveShopDetails = page.getByRole("button", { name: /Save shop details/i });
    if (await saveShopDetails.count()) {
      const saveButton = saveShopDetails.first();
      if (await saveButton.isEnabled()) {
        await saveButton.click();
        await page.getByText(/Shop details saved/i).first().waitFor({ state: "visible", timeout: 15000 }).catch(() => {});
        await page.keyboard.press("Escape").catch(() => {});
        await page.waitForTimeout(250);
        shopDetailsSaved = true;
      }
    }
  }

  if (await createProgrammeButton.count()) {
    const createButton = createProgrammeButton.first();
    const readyAt = Date.now() + 10000;
    while (!(await createButton.isEnabled()) && Date.now() < readyAt) {
      await page.waitForTimeout(200);
    }
    if (await createButton.isEnabled()) {
      await createButton.click();
      await page.waitForTimeout(350);
    }
  }

  const stillBlockedByShopSetup = await openShopSetup.count();
  if (!shopDetailsSaved || stillBlockedByShopSetup > 0) {
    throw new Error("Shop details were not saved in bootstrap; workspace remains blocked in setup tasks.");
  }

  if (await editShopDetails.count()) {
    await page.keyboard.press("Escape").catch(() => {});
  }

  return merchantId;
}

async function updateShopBrand(page) {
  const editShopDetails = page.getByRole("button", { name: /Edit shop|Edit shop details/i }).first();
  if (!(await editShopDetails.count())) {
    return;
  }

  await editShopDetails.click();

  const shopName = page.locator('input[id="shop-name"]');
  const shopEmail = page.locator('input[id="shop-email"]');
  const shopPrimary = page.locator('input[id="shop-primary"]');
  const shopAccent = page.locator('input[id="shop-accent"]');

  await shopName.fill("North Star Coffee House");
  await shopEmail.fill(`brand${Date.now()}@shop.test`);
  if (await shopPrimary.count()) {
    await shopPrimary.fill("#10263a");
  }
  if (await shopAccent.count()) {
    await shopAccent.fill("#d3ab5b");
  }

  const saveShopDetails = page.getByRole("button", { name: /Save shop details/i }).last();
  await saveShopDetails.click();
  await page.getByText(/Shop details saved/i).first().waitFor({ state: "visible", timeout: 15000 }).catch(() => {});
  await page.keyboard.press("Escape").catch(() => {});
  await page.waitForTimeout(250);
}

async function runScenario(browser, viewport) {
  const context = await browser.newContext({ viewport: { width: viewport.width, height: viewport.height } });
  const page = await context.newPage();
  const scenarioDir = path.join(runDir, viewport.name);
  await ensureDir(scenarioDir);

  const result = {
    viewport: viewport.name,
    status: "ok",
    findings: [],
    screenshots: [],
  };

  try {
    const merchantId = await bootstrapMerchant(page);
    await updateShopBrand(page);
    const routes = [
      { name: "operate", url: `${frontendBaseUrl}/portal/merchant/${merchantId}?programmeSection=operate` },
      { name: "configure", url: `${frontendBaseUrl}/portal/merchant/${merchantId}?programmeSection=configure` },
      { name: "customers", url: `${frontendBaseUrl}/portal/merchant/${merchantId}?programmeSection=customers` },
    ];

    for (const route of routes) {
      await page.goto(route.url, { waitUntil: "networkidle" });
      await page.waitForTimeout(250);
      const clientError = page.getByText(/Application error: a client-side exception has occurred/i);
      if (await clientError.count()) {
        throw new Error(`Client exception rendered on ${route.name} route.`);
      }
      if (route.name === "operate") {
        const heading = page.getByText(/Programme workspace|Coffee stamp card|Shop details/i).first();
        await heading.waitFor({ state: "visible", timeout: 10000 }).catch(() => {});
        const primaryButton = page.getByRole("button", { name: /Create programme|Create demo customer join|Save shop details/i }).first();
        if (await primaryButton.count()) {
          const color = await primaryButton.evaluate((element) => getComputedStyle(element).backgroundColor);
          if (!isDarkNavy(color)) {
            result.findings.push(`Operate primary button drifted from North Star navy: ${color}`);
            result.status = "warn";
          }
        }

        const selectedProgrammeCard = page.getByTestId("selected-programme-card");
        if (!(await selectedProgrammeCard.count())) {
          throw new Error("Selected programme branded card preview did not render on operate route.");
        }
        const operateHeading = page.getByRole("heading", { name: /North Star Coffee House/i });
        if (!(await operateHeading.count())) {
          throw new Error("Updated shop name did not propagate into workspace branding.");
        }

        const templateOptions = page.getByTestId("template-option");
        if (await templateOptions.count() > 1) {
          const existingProgrammeCount = await page.getByTestId("programme-rail-item").count();
          await templateOptions.nth(1).click();

          const createProgramme = page.getByTestId("create-programme");
          if (await createProgramme.isEnabled()) {
            await createProgramme.click();
            await page.waitForTimeout(500);

            const afterCreateCount = await page.getByTestId("programme-rail-item").count();
            if (afterCreateCount <= existingProgrammeCount) {
              throw new Error("Creating an additional programme did not increase visible programme rail items.");
            }
          }
        }

        const programmeRailItems = page.getByTestId("programme-rail-item");
        if (await programmeRailItems.count() > 1) {
          const firstCurrent = await page.getByText(/Current/i).count();
          await programmeRailItems.nth(1).click();
          await page.waitForTimeout(350);
          const currentAfterSwitch = await page.getByText(/Current/i).count();
          if (currentAfterSwitch === 0 && firstCurrent === 0) {
            throw new Error("Programme switch did not expose an active item in the programme rail.");
          }
        }
      }
      const shot = path.join(scenarioDir, `${route.name}.png`);
      await page.screenshot({ path: shot, fullPage: true });
      result.screenshots.push(shot);
    }
  } catch (error) {
    result.status = "error";
    result.findings.push(error instanceof Error ? error.message : String(error));
    const failureShot = path.join(scenarioDir, "failure.png");
    await page.screenshot({ path: failureShot, fullPage: true }).catch(() => {});
    result.screenshots.push(failureShot);
  } finally {
    await context.close();
  }

  return result;
}

async function main() {
  await ensureDir(runDir);
  const browser = await chromium.launch({ headless: true });
  const results = [];

  try {
    for (const viewport of viewports) {
      results.push(await runScenario(browser, viewport));
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

  console.log(`Workspace slice artifacts: ${runDir}`);
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
