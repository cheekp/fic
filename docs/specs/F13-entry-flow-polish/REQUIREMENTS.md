# F13 Entry Flow Polish Requirements

## Objective

Make the public entry flow cleaner and more direct so merchants understand FIC as the product and move into signup without needing to parse the brand architecture.

## Functional Requirements

### Home Page

- the home page should be product-first
- the primary headline should describe the FIC product outcome rather than the consultancy brand
- there should be one clear primary CTA into `/portal/signup`
- North Star should still be present, but as company backing and support rather than the primary thing being sold
- the home page should avoid multiple competing narrative blocks
- the home page should keep login access visible
- the business-plan quote may remain, but it should be visually subordinate to the product message

### Messaging Hierarchy

- the page should answer these in order:
  - what is this product
  - who is it for
  - what should I do next
- the relationship between FIC and North Star should be clear without needing explanatory paragraphs

### Signup Step

- signup should feel like a short merchant account/shop creation step
- the signup page should not try to explain the whole workspace model
- the next-step message should be short and concrete
- the primary action should clearly move the user forward in the onboarding flow

### Separation Of Concerns

- the merchant workspace remains merchant-owned
- the company layer remains the support and credibility layer
- this slice should not rework the merchant workspace hierarchy

## Non-Goals

- redesigning the merchant workspace
- removing North Star from company-facing surfaces
- implementing real auth or billing
