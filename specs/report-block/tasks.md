# Report and block tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md). Evidence: [2026-11-18 record](../../docs/delivery/evidence/2026-11-18-f10-report-block.md).

## Definition

- [x] Review the docs index, PRD (R15, BR12, D22), SDD (render modes, authorization, reports and blocks), DESIGN, STATUS and the prototype (report dialog, `REPORT_REASONS`, report links, `thread-tools`, block dialog, blocked notes).
- [x] Decisions asked and made by the user on 2026-11-18: reports in the database (D22, partly decided); one static report page; block covers messaging and reviews (BR19); one report per item and 10 a day.
- [x] Spec RB01–RB09 and plan.

## Implementation

- [x] `Report` and the block on `Conversation`; table `reports` and two conversation columns (migration `AddReportsAndBlocks`, applied to the development database on 2026-11-18).
- [x] `ReportService` and `ReportStore`; `ChatService.SetBlockAsync`, `SendOutcome.Blocked` and the row lock; `ReviewAccess.Blocked`.
- [x] Report page; report links and thanks lines; chat thread tools, block dialog, notes and Unblock; the blocked review state; "This isn't available".
- [x] Styles and two tokens ported from the prototype; Tailwind rebuilt.
- [x] PRD R15, BR12, BR19, D22; SDD; DESIGN; RELEASE (reading reports); TEST-PLAN TP27; STATUS (gate G12); the docs index.

## Verification

- [x] 16 rule tests and 7 web tests; whole suite 258 passed, 0 failed, 0 skipped with `-m:1`; Release build with 0 warnings.
- [x] Browser: two Edge sessions, including a live block and unblock with the draft kept, and a report without script; screenshots reviewed.
- [ ] The report email (with R08), and the owner's response time and removal steps (D22).
- [ ] Owner walkthrough (G9).
