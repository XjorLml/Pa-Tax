# Legal text and privacy prompts tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md).

## Definition

- [x] Link the governing requirements and the authorization (the user's decisions of 2026-11-21); record the baseline and exclusions.
- [x] Resolve blocking decisions: text approved by the owner without a lawyer; minor placeholders filled with the recommended defaults.
- [x] Prototype first: the legal layout and the prompts, plus the new `agreeHTML` screen.

## Implementation

- [x] Version constant, session claim, agreement middleware and service method.
- [x] Agree page and registration wording.
- [x] Terms and Privacy pages with the full text and layout; remove the draft notice.
- [x] Chat hint, editor title hint, profile note and report page text.
- [x] Styles ported from the prototype.
- [x] Update SDD, DESIGN, TEST-PLAN, STATUS and the docs index.

## Automated and build verification

- [x] Tests for LP01–LP14 pass, with the whole suite.
- [x] Release build with 0 warnings.
- [x] Documentation and prototype checkers pass.

## Browser verification

- [x] Screenshots of `/terms` and `/privacy` on the running app, at phone and desktop widths in light and dark ([evidence](../../docs/delivery/evidence/2026-11-21-f11-legal-pages.md)).
- [ ] `/account/agree` and the chat hint in a signed-in browser: needs a real Google sign-in. The prototype screen and web tests cover them meanwhile.
- [x] Remove any synthetic accounts the checks create.

## Acceptance

- [ ] The owner's walkthrough (G9) and a decision on publishing without legal review (G13).
