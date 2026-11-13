# Browse accountants tasks

**Implemented 2026-11-13; browser matrix and acceptance pending.** Scope/scenarios: [spec](spec.md). Sequence/dependencies: [plan](plan.md). Evidence: [F01 record](../../docs/delivery/evidence/2026-11-13-f01-browse-accountants.md).

## Definition

- [x] Link governing requirements and authorization; record baseline and exclusions (spec, 2026-11-10; reworked for the web 2026-11-11).
- [x] Review the directory and profile pages in the [design prototype](../../prototypes/README.md) (STATUS gate G9). On 2026-11-13 the user chose the Paper & Forest palette and instructed starting F01. A recorded walkthrough of the whole prototype remains part of gate G9.
- [x] Resolve blocking decisions D04 (profile fields) and D05 (search, filters, order), or agree on provisional seed fields: adopted provisionally by the user's instruction, 2026-11-12 ([PRD](../../docs/PRD.md#open-decisions)).
- [x] Resolve conflict C03 (directory interaction): static contract kept, prototype adjusted, 2026-11-12 ([evidence](../../docs/delivery/evidence/2026-11-12-c03-directory.md)).
- [x] Identify source/test dependencies and meaningful checks: confirmed 2026-11-13 after the SDK install and database set-up ([SETUP](../../docs/SETUP.md#reproducibility-record)).

## Implementation

- [x] Domain model, data model with the first migration (`InitialDirectory`), seed data, and the directory use case.
- [x] Directory and profile pages, with "not available" and error pages.
- [x] Failure, not-available and address-keeps-state behavior (FT05–FT07).
- [x] Update SDD (as built) and SETUP (database, ports, commands); ADR 010 records the single-project layout.

## Automated and build verification

- [x] TaxBuddy.Tests and TaxBuddy.Web.Tests for FT01–FT06 pass: 62 passed, 0 skipped, on uncommitted source fingerprint `eae0d6ae…` in WSL2 ([evidence](../../docs/delivery/evidence/2026-11-13-f01-browse-accountants.md)).
- [x] The web project builds on this PC: SDK 11.0.100, Release, 0 warnings.
- [x] Documentation links, traceability and diff are checked (STATUS, 2026-11-13).

## Browser verification

- [ ] TP01, TP05 (FT07) and TP08 pass in the browsers of the agreed matrix, with synthetic data. Done so far: headless Edge 157 at emulated 320, 390, 760 and 1440 px, in light and dark. Remaining: Chrome, Firefox, Brave, Safari, real phones, and a real Back/Forward/reload and offline walk.
- [ ] TP06 accessibility and layout checks pass for the directory and profile pages. Layout and 44 px targets passed in emulated Edge. Remaining: screen reader, keyboard only, zoom to the D15 target.
- [x] Restore changed settings: no browser, network or device settings were changed. Temporary browser profiles were deleted, and the app was stopped.

## Acceptance

- [ ] Required gates are satisfied or exceptions explicitly decided.
- [ ] Acceptance decision is recorded with its owner (lumabanls) and scope.
- [x] [STATUS](../../docs/delivery/STATUS.md) links the evidence and remaining limitations.

## Polish follow-ups

Recorded 2026-11-13 at the user's request ("record it for now for polishing updates"). Not started, and not needed for F01 acceptance unless the owner decides otherwise.

- [ ] **P1. Feedback when following links inside the site.**
  - Problem: cards, Next/Previous and "All accountants" use Blazor's enhanced navigation, which fetches the next page in the background. As far as checked, Blazor shows no indicator for this, so on a slow connection the old page stays with no sign that anything is happening. A full page load (first visit, reload, Search) shows the browser's own indicator.
  - Option A: a thin progress bar that appears only when a navigation takes longer than about 300 ms (a few lines of script around Blazor's navigation events).
  - Option B: turn off enhanced navigation for those links (`data-enhance-nav="false"`) so the browser's indicator shows.
  - Confirm .NET 11's current behaviour first, and record the chosen state in [DESIGN](../../docs/DESIGN.md#screen-and-component-states).
  - A skeleton is not the answer here: streaming a placeholder would send the response before the 404/503 status is known (FT05, FT06) and would put content behind script ([ADR 010](../../docs/adr/010-single-project-per-page-render-modes.md)).
- [ ] **P2. Shorter database timeouts.**
  - Problem: with Npgsql's defaults (15 s to connect, 30 s per query), a slow or paused database can keep a visitor waiting up to about 30 s before the "Try again" page (FT06).
  - Fix: set shorter values, about 5 s, in the connection settings. A paused free Supabase project makes this more likely.
  - Verify: FT06 still returns 503 promptly.
- [ ] **P3. Name the environment in the start-up error.**
  - Problem: when `ConnectionStrings:TaxBuddy` is missing because the app did not start as Development (for example, a mistyped launch profile), the message should name the environment the app ran in, so the cause is obvious.
  - Fix: touches `Program.cs` only.

Blocked/not-applicable tasks:
- Phone-browser checks are blocked until devices are recorded (STATUS gate G10).
- Owner: lumabanls.

Implementation completion alone does not complete the later sections. A checkbox must not imply a test or browser result that has not occurred.
