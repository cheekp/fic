# F06 Merchant Brand Engine Requirements

## Objective

Upgrade the current logo-and-colour treatment into a merchant brand engine that automatically themes the merchant workspace, customer join flow, and wallet preview from a shared brand profile.

## Scope

- Keep the product self-serve and white-label.
- Treat uploaded merchant branding as a source for derived presentation tokens, not a pasted decoration.
- Keep the global FIC navigation chrome stable while rebranding the merchant content area, join experience, and wallet preview from the same compiled theme.
- Avoid naive logo tiling, stretched art, or layout choices that fight the uploaded mark.

## Functional Requirements

### Shared Brand Model

- Merchant brand data must include the uploaded logo reference plus enough metadata to choose a cleaner layout treatment.
- PNG uploads must expose width and height metadata so layout heuristics can react to square vs wide marks.
- The platform state must surface the same brand metadata to merchant workspace, join flow, wallet preview, and homepage card summaries.

### Derived Brand Theme

- Introduce a theme compiler that derives presentation tokens from:
  - primary colour
  - accent colour
  - logo dimensions
- The signup flow should suggest site colours from the uploaded logo so merchants are not forced to hand-tune the first draft of their brand theme.
- The compiler must choose a layout variant automatically for at least three visual modes.
- The compiler must return a readable ink colour, surface treatment, accent treatment, and stamp styling that work on both lighter and darker merchant palettes.

### Merchant Shell Theming

- The main merchant content shell must adopt the active merchant brand on:
  - merchant workspace routes
  - customer join routes
  - wallet preview routes
- The left navigation rail should remain recognisably FIC-branded so the product frame stays stable.
- Shell copy and content chrome should shift from platform-generic framing to route-aware merchant framing.
- The uploaded logo should sit in a deliberate logo plate rather than being stretched or tiled into the background.

### Brand Inheritance

- Merchant site branding is the top-level brand source.
- Wallet card styling should inherit merchant site branding by default.
- Future card-level overrides must remain optional and must not force merchants to configure duplicate branding just to launch a card.

### Signup Preview

- The signup flow must preview the compiled brand system before the merchant creates the workspace.
- The preview must show:
  - merchant workspace direction
  - wallet card direction
- The preview must respond to both colours and uploaded logo aspect ratio.

### Wallet Preview Styling

- The wallet preview must use the derived theme rather than a simple primary/accent gradient.
- Stamp treatment, logo plate, pass background, and supporting panel must all align with the same compiled brand.
- The Apple Wallet package service should use the compiled theme when choosing pass background and foreground colours.

## Non-Goals

- No bespoke merchant template editor yet.
- No AI-generated logo analysis.
- No typography uploads or merchant font selection yet.
- No Google Wallet parity in this slice.
