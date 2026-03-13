# F09 Merchant Workspace Polish

## Goal

Make the merchant workspace feel calmer and more intentional by clarifying shop-level setup, card-level management, and where customer operations belong.

## Why Now

- the merchant flow is now test-backed, which makes this the right moment to simplify the workspace without flying blind
- the current workspace still mixes readiness, editing, and analytics in ways that feel noisy
- the product promise is self-serve simplicity for independent coffee shops, so the workspace should reflect that directly

## Scope

- tighten the merchant workspace information architecture without changing the underlying domain model
- separate FIC utility chrome more clearly from merchant-owned workspace content
- make the shop tab a true setup and ownership area
- make the loyalty cards tab a true programme management area
- make customer join and stamping unambiguously card-specific
- sharpen the distinction between shop-level insights and selected-card insights
- convert setup checklist framing into a slimmer onboarding roadmap that can be collapsed or dismissed

## Planned Outcomes

- the top of the workspace separates FIC utility actions, shop-level meta, and loyalty-programme content more clearly
- the workspace uses a horizontal onboarding roadmap instead of a large checklist panel
- the shop tab shows shop details without redundant “status card” clutter
- the cards tab makes multiple loyalty cards feel like a real supported concept
- the customers tab clearly operates on the selected loyalty card
- insights read cleanly at both shop and selected-card level
- the proof loop continues to cover the key workspace behaviors

## Exit Criteria

- `F09` spec exists with explicit requirements and acceptance
- the workspace tabs communicate clearer boundaries between shop, card, customers, and insights
- at least one redundant workspace surface is removed or consolidated
- tests and validator pass after the workspace cleanup
