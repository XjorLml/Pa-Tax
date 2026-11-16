# Google sign-in and registration tasks

Scope and scenarios: [spec](spec.md). Sequence and dependencies: [plan](plan.md). Evidence: [2026-11-16 record](../../docs/delivery/evidence/2026-11-16-f04-google-sign-in.md).

## Definition

- [x] Link the governing requirements (R04 Google part, R16) and the authorization; record the baseline and exclusions (spec, 2026-11-16).
- [x] Resolve blocking decisions: D03, one role per account, and draft legal pages (owner, 2026-11-16). The 14-day session is Proposed.
- [x] Define scenarios GS01–GS10 and how each is checked.

## Implementation

- [x] Account entity, Identity user store and an additive `AddAccounts` migration; applied to the development database on 2026-11-16.
- [x] Google sign-in, callback, registration, sign-out; cancel and failure handling; running without Google configured.
- [x] Sign-in, registration, terms and privacy pages; header account menu; footer legal links.
- [x] Shared local-address rule (`LocalAddress`), now also used by the F02 theme switch.
- [x] Update PRD D03, SDD, DESIGN, SETUP, TEST-PLAN, STATUS and the F01 FT01 clarification.

## Automated and build verification

- [x] 22 new tests; whole suite 104 passed, 0 failed, 0 skipped on the development database; Release build with 0 warnings ([evidence](../../docs/delivery/evidence/2026-11-16-f04-google-sign-in.md)).
- [x] Documentation links and traceability checked.

## Browser verification

- [x] Headless Edge 157: sign-in page, its failure state, terms, privacy and the directory header at 320–1440 px in both themes: 152 checks passed; screenshots reviewed.
- [x] GS09: a real Google round trip with registration as a client, on 2026-11-16. Reported by the owner, and the account was checked in the database by the agent ([evidence](../../docs/delivery/evidence/2026-11-16-f04-google-sign-in.md)). The first attempt hit Google's `redirect_uri_mismatch` until the client's settings took effect.
- [ ] With real Google: a returning sign-in, sign-out and cancel. So far only Web.Tests cover them.
- [ ] Other browsers in the D17 matrix, phones, a screen reader and zoom (TP06).

## Acceptance

- [ ] Owner walkthrough: sign-in, registration as each role, sign-out, cancel; whether the header's height on phones is acceptable (three rows at 320 px).
- [ ] Before real users: final legal text (G13), data-protection keys persisted outside the app (RELEASE), Google consent screen published or test users listed.
- [ ] [STATUS](../../docs/delivery/STATUS.md) links the evidence and the limits.
