# F49 Requirements

## Functional

1. Merchant workspace must present all existing programmes as explicit selectable items, not only the currently selected programme.
2. Merchant workspace must allow creation of additional programmes from visible template choices after first-time onboarding is complete.
3. Programme creation UI must make card type and output explicit for each template option.
4. Operate, Configure, and Customers routes must all reflect the currently selected programme without switching to a different visual system.
5. Selected programme surface must include a branded loyalty-card preview that reads as the live customer output of that programme.
6. Customer card management must reuse the same branded card language for table/detail previews rather than generic gradient tiles.

## Non-Functional

1. Existing API/contracts remain the source of truth for multi-programme selection and template creation.
2. Brand treatment must stay merchant-owned for workspace content and not collapse back into generic company styling.
3. Workspace QA must fail if branded programme preview disappears or if creating an additional programme does not produce a second selectable programme item.
