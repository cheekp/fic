# F37 Acceptance

- [ ] `GET /api/v1/portal/navigation/signup` returns HTTP 200 and includes signup nav contract shape.
- [ ] `GET /api/v1/merchants/{merchantId}/portal/navigation` returns HTTP 401 for anonymous access.
- [ ] Authenticated owner receives workspace nav contract with expected route slice keys.
- [ ] Next.js signup/workspace pages render rail items from API nav contract payload.
- [ ] Frontend build passes.
- [ ] Web API tests pass.
