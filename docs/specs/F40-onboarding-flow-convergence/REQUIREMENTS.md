# F40 Requirements

## Functional

1. Merchant workspace first-time onboarding renders one setup taskboard when onboarding is incomplete.
2. The setup taskboard shows remaining tasks for shop details and programme creation using completion state from workspace setup checklist.
3. Workspace onboarding task surfaces avoid explicit "Step N" heading copy where roadmap already defines sequence.
4. Merchant workspace route accepts `setup=shop` and opens shop setup modal when shop details are incomplete.
5. Billing completion route navigates to merchant workspace with setup intent query so first action is clear after signup completion.
6. Existing shop details save + logo upload flow remains functional in centered modal composition.
7. QA signup and workspace scripts validate onboarding flow using the new taskboard markers.

## Non-Functional

1. Desktop and mobile layout must remain free of horizontal overflow through signup and workspace onboarding routes.
2. Onboarding primary action must remain visually prominent and keyboard accessible.
