#!/usr/bin/env python3
"""Render a CSS file with local @import entries inlined."""

from __future__ import annotations

import argparse
import re
import sys
from pathlib import Path

IMPORT_RE = re.compile(r'^\s*@import\s+(?:url\()?\s*["\']([^"\')]+)["\']\s*\)?\s*;\s*$', re.IGNORECASE)


def render(path: Path, stack: list[Path]) -> str:
    resolved = path.resolve()
    if resolved in stack:
        cycle = " -> ".join(str(p) for p in stack + [resolved])
        raise RuntimeError(f"CSS import cycle detected: {cycle}")

    if not resolved.is_file():
        raise FileNotFoundError(f"missing CSS file: {resolved}")

    stack.append(resolved)
    lines: list[str] = []

    for line in resolved.read_text(encoding="utf-8").splitlines():
        match = IMPORT_RE.match(line)
        if not match:
            lines.append(line)
            continue

        target = match.group(1).strip()
        if target.startswith(("http://", "https://", "//")):
            lines.append(line)
            continue

        imported = (resolved.parent / target).resolve()
        lines.append(f"/* begin import: {target} */")
        lines.append(render(imported, stack))
        lines.append(f"/* end import: {target} */")

    stack.pop()
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("entry", help="entry CSS file path")
    args = parser.parse_args()

    entry = Path(args.entry)

    try:
        sys.stdout.write(render(entry, []))
    except Exception as exc:  # pragma: no cover
        print(str(exc), file=sys.stderr)
        return 1

    return 0


if __name__ == "__main__":
    raise SystemExit(main())
