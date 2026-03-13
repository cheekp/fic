# F08 Workspace Polish And bUnit Acceptance

## Acceptance Criteria

1. The merchant workspace removes at least one duplicated or confusing action from the main flow.
2. Customer join remains clearly tied to the selected loyalty card rather than acting like a shop-global action.
3. The shop tab continues to show checklist-style setup framing.
4. The test project includes bUnit.
5. Component tests cover the selected programme/join behavior and at least one shop-tab rendering rule.
6. The slice has a scriptable validator and the validator passes.

## Demo Walkthrough

1. Open a merchant workspace.
2. Confirm the shop tab still anchors setup through the checklist area.
3. Switch to `Loyalty Cards` and confirm the card editor is focused on configuration rather than duplicate customer-join actions.
4. Switch to `Customers` and confirm the join surface is tied to the selected card.
5. Run the F08 validator and confirm the build plus UI/state tests pass.
