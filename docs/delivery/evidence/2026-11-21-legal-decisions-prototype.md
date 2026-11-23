# Legal decisions in the prototype: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-21, about 14:15 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The decisions are the user's answers to 16 questions in the same session ([drafting notes](../../legal/drafting-notes.md#decisions-made-on-2026-11-21)).
- Requirements and decisions: PRD R16, R09, R15; D07, D09, D19 decided; D10, D14, D22 partly decided ([PRD](../../PRD.md#open-decisions)); [DESIGN](../../DESIGN.md#screen-and-component-states) rows "Information pages" and "Legal and privacy prompts".
- Source revision: `1565e69` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus uncommitted documentation and prototype changes.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `e8d2e340fc1a` |
  | `prototypes/taxbuddy.css` | `126f004f5837` |
  | `prototypes/service-workspace.js` | `8f7d9c797695` (unchanged) |
  | `tools/check_prototype.mjs` | `666bc747a865` (unchanged) |

- Environment: WSL2 on Windows. The checker ran with Windows Node v22.12.0 and headless Microsoft Edge 158.0.4370.33, against a copy of `prototypes/` and `tools/` in the Windows temporary folder, as [tools/README](../../../tools/README.md#check-the-prototype) describes for WSL.
- Preconditions and fixtures: the prototype's synthetic data only. No real accounts, files or personal data.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Extract the inline script and run `node --check`; run `node --check prototypes/service-workspace.js` | No syntax errors | "syntax OK", "module OK" (WSL Node v24.16.0) | Pass |
| `node tools/check_prototype.mjs "<Edge>" "<artifacts>"` on the final files | All existing checks pass | `PASS: 202 prototype checks; Edg/158.0.4370.33`. An earlier run, before the guide-panel text edits, also passed 202 | Pass |
| Headless Edge screenshots of `#/terms` and `#/privacy`, desktop light and phone light and dark, viewed by the agent | Draft note, version line, short version box, contents list (two columns on desktop, one on phones), numbered sections | As expected. The page title stays the `h1` on desktop, and sections are `h2` with the 24 px style | Pass (visual) |
| Screenshots scrolled to Privacy section 8: phone light, desktop dark | Cookie table stacked into labelled rows on phones, a 4-column table on desktop | As expected; readable in dark mode | Pass (visual) |
| Screenshot of `#/compose/a1` on the phone | A one-line hint with a lock icon above the message box; the composer still fits | "Don't share TINs, tax returns or ID numbers in chat." shown above the box and send button | Pass (visual) |

The screenshots used a temporary copy of the page with a scroll script added. They were viewed and not kept.

## Restoration and limits

The Windows temporary folder `taxbuddy-proto-check`, which held the copy, the checker's artifacts, the screenshots and a headless Edge profile, was deleted after the run. Nothing in the repository was changed by the checks.

Not exercised:
- The 202 checks predate these changes and don't assert the new text or layout. The new elements were checked only by the screenshots above and the syntax check.
- Not looked at: the registration wording, the editor's title hint, the deleted-account notes and the report page wording.
- Not run: keyboard use of the contents links, screen readers, zoom, and other browsers.
- The site (`src/TaxBuddy.Web`) is unchanged; building the approved changes there is a follow-up.

Acceptance: pending the owner's walkthrough (G9) and the lawyer's review of the legal text (G13).
