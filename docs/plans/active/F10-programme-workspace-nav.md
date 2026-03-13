# F10 Programme Workspace Navigation

## Goal

Make the merchant workspace feel anchored around the real day-to-day operating mode: select a programme, run it, and only drop back to shop-level editing when needed.

## Why Now

- `F09` corrected the hierarchy, but the programme experience still feels like a section inside a bigger page rather than the primary working area
- merchants will spend more time opening join QR, stamping visits, and checking programme performance than editing shop details
- the FIC utility chrome still competes slightly with merchant navigation and should recede further

## Scope

- keep `Shop` as the merchant root for profile, brand, and shop-wide insights
- make `Programmes` the default working surface after setup
- group programmes by `active`, `scheduled`, and `expired`
- make the selected programme workspace feel like one focused area with nested `Operate`, `Configure`, and `Insights`
- reduce duplicate explanatory copy and repeated summary cards in the shop and programme surfaces
- tighten the FIC utility chrome into a quieter merchant utility menu

## Planned Outcomes

- the default merchant landing feels like “run the selected programme” rather than “browse the workspace”
- programme selection is clearer because merchants can scan a grouped list by lifecycle state
- shop-level and programme-level metrics stay visibly separated
- the top utility chrome reads as account/help/platform support, not as merchant navigation
- the workspace becomes easier to scan because redundant summary surfaces are removed

## Exit Criteria

- `F10` spec exists with explicit requirements and acceptance
- the merchant workspace defaults to the programme operating path when a programme exists
- the programme list is grouped by lifecycle state
- the selected programme workspace communicates `Operate`, `Configure`, and `Insights` as one coherent nested context
- the FIC utility chrome is quieter than the merchant navigation
- tests and validator pass after the navigation cleanup
