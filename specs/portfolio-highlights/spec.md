# F08: Portfolio highlights

Governing requirements: [PRD](../../docs/PRD.md) R12 and BR13; D23 (limits, provisional below); R11 (no client names without permission). Contracts: [SDD](../../docs/SDD.md) render modes (the profile and the editor are static), data and persistence. [DESIGN](../../docs/DESIGN.md) "Portfolio highlights". Design origin: the prototype's `portfolioFieldset` (editor), the "Portfolio highlights" section of `profileBodyHTML` and the highlight count in `cardHTML` ([CLAUDE.md](../../CLAUDE.md)). Extends [F05](../profile-editing/spec.md), which excluded R12. Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

An accountant adds highlights of past work to their profile: a title, the type of client, what they did, and the result. Highlights appear on the public profile, and directory cards show how many there are.

**Authorization and decisions** (2026-11-17):
- The user said: "implement portfolio highlights", then "proceeed".
- **D23 provisionally decided:** the prototype's proposal of up to 6 highlights (BR13), as with D04. Field limits are working values:
  - title: 100 characters, required;
  - type of client: 60, optional;
  - what you did: 600, required;
  - result: 300, optional.

  The owner can change them. The review part of D23 (a minimum review length) stays open for R13.

**In scope:**
- A "Portfolio highlights" fieldset in the profile editor, after Services, with "Add highlight" and "Remove" on each row. Like the other rows, it is a static form.
- The profile's "Portfolio highlights" section, between Services and Experience, with the note "Described by the accountant. Client names are left out."
- "N highlights" in the card facts.
- Sample highlights for the development seed, taken from the prototype.

**Exclusions:**
- Pictures or files on highlights (BR17: public portfolios get no uploads).
- Reordering, other than removing and adding again.
- Checking whether a client is named: the editor warns, and reporting (R15) is the safeguard.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| PH01 / R12 | An accountant adds a highlight with a title, type of client, what they did and a result, and saves | The public profile shows it under "Portfolio highlights" with the type of client as a tag and "Result" before the result; the directory card shows "1 highlight" | Rule tests, Web.Tests, browser |
| PH02 / BR13 | A row with only some fields filled, or no title or no "what you did" | Nothing is saved; "Highlight 1: add a title." or "Highlight 1: describe what you did." beside the field and in the summary; everything typed stays | Rule tests, Web.Tests |
| PH03 / BR13, D23 | Six highlights; a seventh posted directly; or text over a limit | "Add highlight" is gone at six; a seventh or too-long text is refused with a message | Rule tests |
| PH04 / R12 | An empty row is left, or a row is removed | Empty rows are dropped on save; Remove redraws the form without that row and saves nothing | Web.Tests |
| PH05 / R11 | The editor | "Up to 6. Don't name clients or share their details without their permission." | Web.Tests |
| PH06 / R12 | A profile with no highlights | No section and no card fact, as in the prototype | Web.Tests |
| PH07 / DESIGN | Phone and desktop, light and dark | The prototype's `.pf` cards on the profile and its row layout in the editor | Browser |

## State and dependencies

- Table `accountant_highlights`, owned by the profile like services and experience: title, client type, work, result, position. Deleted with the profile.
- Highlights don't affect listing (BR01).

## Evidence limits

Rule and web tests use the development database; the layout is checked in a browser. Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
