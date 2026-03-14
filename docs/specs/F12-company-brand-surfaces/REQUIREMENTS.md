# F12 Company Brand Surfaces Requirements

## Objective

Bring the company-facing layer of FIC in line with Jo's North Star brand guidance while keeping merchant-owned workspace surfaces separate.

## Functional Requirements

### Brand Sources

- company-layer tone, palette, and messaging should derive from [docs/business/brand-guidlines.txt](/Users/paulcheek/dev/fic/docs/business/brand-guidlines.txt)
- the company layer should present itself as `North Star Customer Solutions`
- the supplied company logo image should not be required for this slice
- the implementation should rely on typography, palette, and copy rather than the provided logo sheet

### Home Page

- the home page should present FIC as a company-backed product rather than a generic signup stub
- the home page should:
  - keep a clear signup CTA
  - keep login access visible
  - introduce North Star company positioning
  - use calmer consultancy-style tone
- the home page should not look like the merchant workspace

### Account And Recovery Surfaces

- login and forgot-password should use the company brand layer
- they should feel like support/account surfaces rather than raw MVP forms
- these surfaces may remain functional stubs, but the messaging should feel intentional and commercially grounded

### Utility Chrome And Support

- merchant utility chrome should read as company support, not primary app navigation
- the merchant utility menu should expose working destinations for:
  - training and consultancy
  - billing
  - account
- the loyalty agent launcher should remain discreet
- agent wording should reflect the current surface:
  - sales/consultancy on the public side
  - setup/billing help during onboarding
  - loyalty support inside the merchant workspace

### Support Pages

- the repo should include lightweight routed stubs for consultancy, billing, and account support surfaces where the utility menu sends people
- these should reflect the company layer visually and in tone

### Separation Of Concerns

- merchant workspace themes must stay merchant-owned
- shop and programme views must not be re-skinned into the company brand
- company branding should live in:
  - home
  - account/recovery
  - consultancy/support/billing
  - utility chrome

## Non-Goals

- implementing real auth
- implementing real billing
- applying the company brand to merchant-owned shop/programme surfaces
- extracting final production logo assets from the supplied logo montage
