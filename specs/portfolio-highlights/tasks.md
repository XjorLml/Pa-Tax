# Portfolio highlights tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md). Evidence: [2026-11-17 record](../../docs/delivery/evidence/2026-11-17-f08-portfolio-highlights.md).

## Definition

- [x] Review the PRD (R12, BR13, D23), SDD, DESIGN, F05 and the prototype (`portfolioFieldset`, `profileBodyHTML`, `cardHTML`, `PORTFOLIO_SEED`).
- [x] D23 provisionally decided on 2026-11-17 at the user's "proceeed": up to 6 highlights (the prototype); working field limits 100, 60, 600 and 300.
- [x] Spec PH01–PH07 and plan.

## Implementation

- [x] `PortfolioHighlight` on the profile, with limits; table `accountant_highlights` (migration `AddPortfolioHighlights`, applied to the development database on 2026-11-17).
- [x] Form rows, validation with the prototype's messages, save with the profile.
- [x] Editor fieldset after Services; profile section between Services and Experience; "N highlights" on cards.
- [x] The prototype's sample highlights in the development seed (Andrea, Benjie, Daniel, Elaine).
- [x] PRD BR13 and D23; SDD; DESIGN; TEST-PLAN TP25; STATUS; the docs index; the F05 spec.

## Verification

- [x] 5 rule tests and 2 web tests; whole suite 197 passed, 0 failed, 0 skipped with `-m:1`; Release build with 0 warnings.
- [x] Browser: profile at 390 and 1440 px, light and dark; card; editor at 1440 light and 390 dark (7 of 7).
- [ ] The seed's sample highlights in a browser: rerunning the seed would delete the owner's own profile, so it wasn't run.
- [ ] Owner walkthrough (G9).
