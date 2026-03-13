# F09 Merchant Workspace Polish

## Goal

Correct the merchant workspace hierarchy so the product reads as `Shop -> Programmes -> Operate/Configure/Insights` instead of a flat set of competing tabs.

## Why Now

- the merchant flow is now test-backed, which makes this the right moment to simplify the workspace without flying blind
- the current workspace still mixes shop concerns, programme concerns, customer operations, and analytics at the same level
- the product promise is self-serve simplicity for independent coffee shops, so the workspace must reflect the user’s actual working modes

## Scope

- tighten the merchant workspace information architecture without changing the underlying domain model
- separate FIC utility chrome more clearly from merchant-owned workspace content
- make `Shop` and `Programmes` the primary workspace scopes
- treat a loyalty card as the customer-facing artifact of a loyalty programme rather than the top-level object
- move customer join and stamping into programme-level `Operate`
- split insights into shop-level and programme-level sections
- keep onboarding guidance present but subordinate once setup is complete

## Planned Outcomes

- the top of the workspace separates FIC utility actions, shop-level context, and programme work more clearly
- `Shop` becomes the merchant root with `Overview`, `Edit Shop`, and `Insights`
- `Programmes` becomes the daily-use root with `Operate`, `Configure`, and `Insights`
- the onboarding roadmap becomes a horizontal, collapsible guide on shop overview rather than a dominant content block everywhere
- multiple programmes feel like a real concept, with one selected programme driving join, stamping, and wallet behaviour
- shop-level insights and programme-level insights are no longer mixed in a single view
- the proof loop continues to cover the key workspace behaviors

## Exit Criteria

- `F09` spec exists with explicit requirements and acceptance
- the workspace communicates a clear hierarchy between shop-level and programme-level work
- customer join and stamping are available only within programme `Operate`
- programme configuration is available only within programme `Configure`
- shop-level and programme-level insights are rendered in separate scopes
- tests and validator pass after the workspace cleanup
