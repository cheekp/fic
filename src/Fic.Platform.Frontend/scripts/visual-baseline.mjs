import fs from "node:fs/promises";
import path from "node:path";
import { spawn } from "node:child_process";
import { createRequire } from "node:module";

const require = createRequire(import.meta.url);
const { PNG } = require("pngjs");

const mode = process.argv[2] ?? "compare";
const frontendBaseUrl = process.env.FIC_QA_FRONTEND_BASE_URL ?? "http://localhost:3003";
const outputRoot = process.env.FIC_QA_OUTPUT_DIR ?? path.resolve(process.cwd(), "../../artifacts/nextjs-qa");
const baselineRoot = process.env.FIC_VISUAL_BASELINE_DIR ?? path.resolve(process.cwd(), "./visual-baseline");
const runId = process.env.FIC_QA_RUN_ID ?? "visual-current";
const runDir = path.join(outputRoot, runId);
const diffRoot = path.join(runDir, "diff");

const viewports = ["mobile", "desktop"];
const screens = ["00-home.png", "01-signup.png", "02-plan.png", "03-billing.png", "04-workspace.png"];
const maxDiffRatio = Number(process.env.FIC_VISUAL_MAX_DIFF_RATIO ?? "0.008");

function runQaFlow() {
  return new Promise((resolve, reject) => {
    const child = spawn("npm", ["run", "qa:signup-flow"], {
      cwd: process.cwd(),
      env: {
        ...process.env,
        FIC_QA_FRONTEND_BASE_URL: frontendBaseUrl,
        FIC_QA_OUTPUT_DIR: outputRoot,
        FIC_QA_RUN_ID: runId,
      },
      stdio: "inherit",
    });

    child.on("exit", (code) => {
      if (code === 0) {
        resolve();
      } else {
        reject(new Error(`qa:signup-flow failed with code ${code}`));
      }
    });
  });
}

async function ensureDir(dir) {
  await fs.mkdir(dir, { recursive: true });
}

async function readQaSummary() {
  const summaryPath = path.join(runDir, "summary.json");
  const raw = await fs.readFile(summaryPath, "utf8");
  const parsed = JSON.parse(raw);
  const bad = (parsed.results ?? []).filter((item) => item.status === "error");
  if (bad.length > 0) {
    throw new Error(`qa:signup-flow reported errors for ${bad.map((x) => x.viewport).join(", ")}. See ${summaryPath}`);
  }
}

async function copyBaseline() {
  await ensureDir(baselineRoot);
  for (const viewport of viewports) {
    const destDir = path.join(baselineRoot, viewport);
    await ensureDir(destDir);
    for (const screen of screens) {
      const source = path.join(runDir, viewport, screen);
      const dest = path.join(destDir, screen);
      await fs.copyFile(source, dest);
    }
  }
}

async function compare() {
  const { default: pixelmatch } = await import("pixelmatch");
  await ensureDir(diffRoot);

  let failed = 0;
  const summary = [];

  for (const viewport of viewports) {
    for (const screen of screens) {
      const baselinePath = path.join(baselineRoot, viewport, screen);
      const currentPath = path.join(runDir, viewport, screen);
      const diffPath = path.join(diffRoot, `${viewport}-${screen}`);

      const [baselineBuffer, currentBuffer] = await Promise.all([
        fs.readFile(baselinePath),
        fs.readFile(currentPath),
      ]);

      const baselinePng = PNG.sync.read(baselineBuffer);
      const currentPng = PNG.sync.read(currentBuffer);

      if (baselinePng.width !== currentPng.width || baselinePng.height !== currentPng.height) {
        failed += 1;
        summary.push({
          viewport,
          screen,
          status: "failed",
          reason: `dimension mismatch baseline(${baselinePng.width}x${baselinePng.height}) current(${currentPng.width}x${currentPng.height})`,
        });
        continue;
      }

      const diffPng = new PNG({ width: baselinePng.width, height: baselinePng.height });
      const diffPixels = pixelmatch(
        baselinePng.data,
        currentPng.data,
        diffPng.data,
        baselinePng.width,
        baselinePng.height,
        { threshold: 0.1 },
      );
      const totalPixels = baselinePng.width * baselinePng.height;
      const diffRatio = diffPixels / totalPixels;

      await fs.writeFile(diffPath, PNG.sync.write(diffPng));

      const status = diffRatio <= maxDiffRatio ? "ok" : "failed";
      if (status === "failed") {
        failed += 1;
      }

      summary.push({
        viewport,
        screen,
        status,
        diffPixels,
        totalPixels,
        diffRatio,
      });
    }
  }

  const summaryPath = path.join(runDir, "visual-compare-summary.json");
  await fs.writeFile(summaryPath, JSON.stringify(summary, null, 2), "utf8");

  console.log(`Visual comparison summary: ${summaryPath}`);
  for (const item of summary) {
    const ratio = typeof item.diffRatio === "number" ? item.diffRatio.toFixed(4) : "n/a";
    console.log(`- ${item.viewport}/${item.screen}: ${item.status} (${ratio})`);
  }

  if (failed > 0) {
    throw new Error(`Visual baseline comparison failed (${failed} screen(s) exceeded diff ratio ${maxDiffRatio}).`);
  }
}

async function main() {
  await runQaFlow();
  await readQaSummary();

  if (mode === "capture") {
    await copyBaseline();
    console.log(`Baseline captured at ${baselineRoot}`);
    return;
  }

  if (mode === "compare") {
    await compare();
    return;
  }

  throw new Error(`Unknown mode '${mode}'. Use 'capture' or 'compare'.`);
}

main().catch((error) => {
  console.error(error);
  process.exitCode = 1;
});
