# F15 Programme Workspace Focus Requirements

## Objective

Reduce visual and conceptual density in the merchant programme workspace so merchants can operate and configure programmes without the UI repeatedly explaining itself.

## Functional Requirements

### Shop Header And Navigation

- shop settings should be available from one clear shop-level entrypoint
- shop settings should not appear as duplicate actions in both the header and overview content
- the top-level shop navigation should remain `Overview`, `Programmes`, and `Insights`

### Programme Rail

- the programme rail should act as navigation, not as a content panel
- the programme rail should contain:
  - the programmes heading
  - the create action
  - lifecycle-grouped programme lists
- the programme rail should not include explanatory prose about what a programme is
- creating a programme should still make the current delivery output explicit

### Selected Programme Work Surface

- the selected-programme header should show only current context:
  - programme name
  - status / validity
  - current customer output
- the selected-programme header should not explain what the sub-tabs do
- `Operate`, `Configure`, and `Insights` should remain the nested programme sections

### Programme Configure

- programme configure should focus on:
  - programme rule
  - schedule
  - current customer output
- the wallet loyalty card should be presented as the current output of the programme
- the separate delivery-model explainer panel should not return
- the preview should stay available, but as a supporting panel rather than a competing narrative block

## Non-Goals

- introducing new customer delivery types
- changing the underlying programme data model
- changing the public home, signup, or billing entry flow
