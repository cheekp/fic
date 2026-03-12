# F06 Merchant Brand Engine Acceptance

## Acceptance Criteria

1. A merchant upload plus primary/accent colours produces a compiled theme, not just two raw colour assignments.
2. Uploading a merchant logo suggests usable site colours automatically, while still allowing manual edits.
3. Merchant workspace, join page, and wallet preview all visibly inherit the same merchant brand treatment.
4. The left navigation rail remains FIC-branded rather than becoming merchant-branded.
5. Wallet card styling inherits the merchant site brand by default.
6. Wide logos remain legible and contained inside logo plates without cropping or background tiling.
7. Signup shows a richer live preview that communicates how the merchant shell and wallet preview will look.
8. Homepage remains a FIC-level entry surface rather than a heavy multi-merchant control plane.
9. The Apple Wallet package builder uses the derived theme for pass colours.
10. The solution builds successfully after the new brand engine is introduced.

## Demo Walkthrough

1. Open `/portal/signup`.
2. Enter a merchant name, reward rule, primary colour, accent colour, and upload a PNG logo.
3. Verify the signup preview updates into a coherent merchant shell and wallet direction.
4. Create the merchant and land in the workspace.
5. Verify the shell, workspace panels, and logo plate feel merchant-specific.
6. Open the join page and confirm the join experience carries the same theme.
7. Open the wallet preview and confirm the pass styling matches the merchant workspace.
