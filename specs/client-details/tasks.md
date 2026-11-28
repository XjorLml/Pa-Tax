# F17 tasks: Client details

[Spec](spec.md) · [plan](plan.md)

- [x] PRD R25 recorded; C05 resolved by the user's instruction of 2026-11-25 ("implement to site").
- [x] Domain rules, data columns and the `AddClientDetails` migration; account deletion clears the details.
- [x] `ClientDetailsService`, and the Account card and form.
- [x] The "About" card for accountants on the client page and in service requests.
- [x] Privacy policy and legal version `2026-11-25`; SDD, DESIGN, SETUP and the documentation index.
- [x] Tests:
  - 17 unit tests for the rules, and 6 web tests (CD01–CD04, CD06, CD07);
  - the whole suite: 400 pass (223 unit, 177 web);
  - the Release build with styles: 0 warnings.
- [x] Pages from the test host viewed in Edge with the built styles: a client and an accountant, desktop in light and dark, and a 390 px phone frame ([evidence](../../docs/delivery/evidence/2026-11-25-f17-client-details.md)).
- [x] `AddClientDetails` applied to the development database on 2026-11-25.
- [x] The prototype aligned with what was built: the name limit, the phone rule and the sharing line (F02 BD15).
- [ ] **Owner:** approve the privacy text, version `2026-11-25` (STATUS G13).
- [ ] **Owner:** try it on the running site with a real Google sign-in: save details as a client, then view them as the accountant of a request.
