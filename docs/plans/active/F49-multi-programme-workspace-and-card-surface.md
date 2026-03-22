# F49 Multi Programme Workspace And Card Surface

## Goal

Bring the Next merchant workspace up to parity with the product model by supporting multiple loyalty programmes, template-led creation, and a branded loyalty-card surface that anchors operate/configure/customers.

## Scope

- Replace single-programme summary treatment with a reusable programme workspace deck.
- Show existing programmes as a selectable rail in the merchant workspace.
- Allow merchants to create additional programmes from visible template/card-type choices.
- Introduce a glossy branded loyalty-card preview for selected programme and customer-card detail surfaces.
- Extend workspace QA so multi-programme creation/selection and branded preview rendering are checked automatically.

## Proof

- `cd src/Fic.Platform.Frontend && npm run build`
- `cd src/Fic.Platform.Frontend && FIC_QA_FRONTEND_BASE_URL=http://localhost:3301 npm run qa:workspace-slices`
