# F10 Programme Workspace Navigation

## Goal

Make the merchant workspace feel anchored around the real day-to-day operating mode: stay inside the shop, move into programmes when needed, and treat the loyalty card as a programme output rather than a peer concept.

## Why Now

- `F09` corrected the hierarchy, but the programme experience still feels like a section inside a bigger page rather than the primary working area
- the current workspace still treats `Shop` and `Programmes` like peers, which makes the shop root feel cluttered and conceptually unstable
- merchants will spend more time opening join QR, stamping visits, and checking programme performance than editing shop details
- the FIC utility chrome still competes slightly with merchant navigation and should recede further

## Scope

- keep `Shop` as the merchant root
- move `Programmes` underneath the shop rather than treating it as a peer scope
- keep the shop root focused on shop identity, shop-wide insight, and clear handoff into programmes
- group programmes by `active`, `scheduled`, and `expired`
- make the selected programme workspace feel like one focused area with nested `Operate`, `Configure`, and `Insights`
- make the loyalty card clearly live inside programme configuration instead of behaving like a top-level object
- reduce duplicate explanatory copy and repeated summary cards in the shop and programme surfaces
- tighten the FIC utility chrome into a quieter merchant utility menu

## Planned Outcomes

- the default merchant landing feels like “this is my shop, and programmes are where loyalty runs”
- programme selection is clearer because merchants can scan a grouped list by lifecycle state
- shop-level and programme-level metrics stay visibly separated
- the shop overview becomes calmer because it stops repeating programme operation and insight content
- the top utility chrome reads as account/help/platform support, not as merchant navigation
- the workspace becomes easier to scan because redundant summary surfaces are removed

## Exit Criteria

- `F10` spec exists with explicit requirements and acceptance
- the merchant workspace defaults into the shop's `Programmes` working path when a programme exists
- the workspace navigation makes `Programmes` a section under `Shop`, not a peer root tab
- the programme list is grouped by lifecycle state
- the selected programme workspace communicates `Operate`, `Configure`, and `Insights` as one coherent nested context
- the loyalty card is expressed as part of programme configuration
- the FIC utility chrome is quieter than the merchant navigation
- tests and validator pass after the navigation cleanup
