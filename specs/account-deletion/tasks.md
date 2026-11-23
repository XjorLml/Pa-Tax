# Account deletion tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md).

## Definition

- [x] Link R09, BR06, D07 and D27 (partly), and the user's request of 2026-11-23; record exclusions.
- [x] Prototype first: `deleteHTML` and the deleted-account notes exist since 2026-11-21.

## Implementation

- [x] `deleted_at` and the migration, applied to the development database on 2026-11-23.
- [x] The deletion transaction, picture removal, sign-out and the one-minute security check.
- [x] Chat: the deleted note, and no Block.
- [x] The delete page and the Account menu item.
- [x] Legal text: deletion on the site, version `2026-11-23`.
- [x] SDD, DESIGN, PRD, RELEASE, TEST-PLAN and STATUS.

## Automated and build verification

- [x] AD01–AD11 pass, with the whole suite ([evidence](../../docs/delivery/evidence/2026-11-23-f12-f13.md)).
- [x] Release build with 0 warnings; documentation and prototype checkers pass.

## Browser verification

- [x] The visitor view in Edge; no overflow at 320 and 390 px.
- [ ] The signed-in steps in a real browser with Google sign-in.

## Acceptance

- [ ] The owner's walkthrough (G9).
