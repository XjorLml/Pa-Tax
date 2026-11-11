# Web platform change execution evidence

Procedure definitions: [TEST-PLAN documentation-only checks](../TEST-PLAN.md#documentation-only-checks). Current state: [STATUS](../STATUS.md).

## Identity and attribution

- Date/time and timezone: 2026-11-11 (+08:00).
- Observer: agent-observed (Claude Code session run for lumabanls). Outputs below were read directly from the commands' output.
- Requirements/scenarios/procedures: documentation-only checks, plus a walk-through of the design prototype. No PRD requirement is verified by this record.
- Source revision and working-tree changes: baseline `b9b50f3`, plus uncommitted work:
  - every authority document reworked for the web platform;
  - `docs/API.md` removed;
  - ADRs 005 and 006 added, and ADRs 001–004 marked superseded;
  - `specs/browse-accountants/` and `prototypes/` reworked.
- Artifact: documentation and `prototypes/index.html`; no application artifact exists.
- Environment:
  - Windows 11 Pro 10.0.26300;
  - uv-managed CPython 3.14.4 for the checker, run without writing bytecode;
  - Node 22.12.0 for the syntax check;
  - Microsoft Edge 157.0.4341.42, headless, with a throwaway profile outside the repository.
- Preconditions/fixtures: synthetic data inside the prototype only.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `check_docs.py --ready` after the rework, before fixing links | Errors only for links to removed sections | 9 errors: 7 links from superseded ADRs to removed sections, 2 links to this record before it existed | Pass (expected failure) |
| `check_docs.py` after fixes | PASS | See "Final results" below | Pass |
| `check_docs.py --ready` after fixes | PASS | See "Final results" below | Pass |
| `python -m unittest discover -s tools/tests` | 10 tests OK | See "Final results" below | Pass |
| `grep` for `{{` outside `TEMPLATE.md` and `feature-template` | No matches | No matches | Pass |
| `node --check` on the prototype's script | No syntax errors | No output; exit 0 | Pass |
| Scripted walk-through of `prototypes/index.html` in headless Edge | All checks pass, no page error | 56 passed, 0 failed, 0 errors | Pass |
| Screenshot review: phone and desktop frames, light and dark | Readable, nothing clipped | Four screenshots reviewed; no defects seen | Pass |

Failures and subsequent retries: the first `--ready` run above is the only failing run; later rows are from after the fixes.

### Final results

| Command | Output |
|---|---|
| `check_docs.py` | `PASS: 31 Markdown files; 263 local links; 0 errors.` 81 placeholders, all in reusable templates; exit 0 |
| `check_docs.py --ready` | `PASS: 31 Markdown files; 263 local links; 0 errors.` Readiness enforced; 81 kept in reusable templates; exit 0 |
| `python -m unittest discover -s tools/tests` | `Ran 10 tests` … `OK` |

### Host inventory (observed 2026-11-11)

| Item | Observation |
|---|---|
| .NET SDKs (`dotnet --list-sdks`) | 8.0.307, 8.0.427, 9.0.123, 9.0.319, 9.0.320, 10.0.400, 10.0.403. No .NET 11 SDK |
| ASP.NET Core runtimes | 8.0.11, 8.0.20, 8.0.32, 8.0.33, 9.0.21, 9.0.22, 10.0.11, 10.0.14 |
| Visual Studio (`vswhere`) | Community 2026 18.11.3; Community 2022 17.14.44 |
| HTTPS development certificate | `dotnet dev-certs https --check`: a valid certificate, 2026-08-26 to 2027-08-26. Trust was not checked |

Official pages read on this date are listed in [REFERENCES](../../REFERENCES.md#revalidation-log).

### Design pass on the prototype (later the same day)

Source: the same baseline plus the reworked `prototypes/index.html` and the new `prototypes/DESIGN-DIRECTIONS.md`.

| Procedure / command | Expected result | Actual observation | Outcome |
|---|---|---|---|
| Contrast script over 26 colour pairs in six palettes (three looks, light and dark) | Every pair meets WCAG AA | `ALL PAIRS PASS (26 pairs x 6 palettes)`; lowest ratio 3.4 against a 3.0 minimum | Pass |
| `node --check` on the prototype script | No syntax errors | No output; exit 0 | Pass |
| Scripted walk-through in headless Edge, once per look | All checks pass, no page error | Ledger 83 passed, Neighbourly 83 passed, Clear 83 passed; 0 failed, 0 errors | Pass |
| Screenshot review: three looks, phone and desktop frames, light and dark, and narrow windows (500 px and 700 px) | Readable, nothing clipped | 11 screenshots reviewed. Found and fixed: sample text contradicting the computed years of experience, and dim text on message-idea buttons | Pass after fixes |
| Search of `index.html` for external addresses | None | 0 matches | Pass |
| `check_docs.py --ready` after the documentation updates | PASS | See below | Pass |

Final documentation check after the design pass: `PASS: 32 Markdown files; 273 local links; 0 errors.`

Not checked in the design pass: other browsers, real phones, screen readers, keyboard-only use beyond focus placement, and true browser zoom (narrow windows are a weaker stand-in).

### Stack decisions (later the same day)

Documentation-only change: Supabase, Brevo and the supported browsers were recorded, Vercel was ruled out as the host, and ADR 007 was added.

| Procedure / command | Expected result | Actual observation | Outcome |
|---|---|---|---|
| File versions of the supported browsers on this PC | Recorded | Chrome 157.0.8103.39, Edge 157.0.4341.46, Firefox 159.0.1, Brave 157.1.99.56. This corrected an earlier, unchecked statement that Firefox was not installed | Pass |
| Docker on this PC | Recorded | Not found | Recorded |
| `check_docs.py --ready` after the updates | PASS | `PASS: 33 Markdown files; 302 local links; 0 errors.` | Pass |

Vendor facts behind the decisions are in [REFERENCES](../../REFERENCES.md#hosting-database-and-email-services). The Brevo and SendGrid pricing pages could not be read, so the email comparison rests on third-party pages.

### Second design pass (later the same day)

| Procedure / command | Expected result | Actual observation | Outcome |
|---|---|---|---|
| Screenshots of public reference websites with headless Edge | Pages load | 10 of 14 loaded and were viewed; QuickBooks' directory, Zocdoc, Malt and Clutch returned no usable page | Recorded |
| Contrast script over 26 colour pairs in eight palettes | Every pair meets WCAG AA | All pairs pass; lowest ratio 3.4 against a 3.0 minimum | Pass |
| `node --check` on the prototype script | No syntax errors | No output; exit 0 | Pass |
| Scripted walk-through, once per look | All checks pass | modern 86 passed, 0 failed, 0 errors; ledger 86 passed, 0 failed, 0 errors; neighbourly 86 passed, 0 failed, 0 errors; clear 86 passed, 0 failed, 0 errors | Pass |
| Screenshot review of the Modern look | Readable, nothing clipped | Seven screenshots reviewed; a misaligned heading on the desktop sign-in page was fixed | Pass after fix |
| `check_docs.py --ready` | PASS | `PASS: 33 Markdown files; 312 local links; 0 errors.` | Pass |

### Portfolio and ratings direction (later the same day)

| Procedure / command | Expected result | Actual observation | Outcome |
|---|---|---|---|
| `node --check` on the prototype script | No syntax errors | No output; exit 0 | Pass |
| Scripted walk-through, once per look | All checks pass | Modern, Ledger, Neighbourly and Clear: 132 passed each, 0 failed, 0 errors | Pass |
| Screenshot review of the new pages | Readable, nothing clipped | Nine screenshots reviewed (directory with ratings, review form, reviews with replies, for-accountants page, chat tools). A double full stop on the review form was fixed | Pass after fix |
| Search of `index.html` for external addresses | None | 0 matches | Pass |
| `check_docs.py --ready` | PASS | `PASS: 34 Markdown files; 323 local links; 0 errors.` | Pass |

## Restoration and limits

No system settings were changed. The headless browser used a temporary profile in a scratch folder outside the repository.

Not exercised: any build, test project, the real site, real sign-in, real email, other browsers, real phones, screen readers; external URLs (reported as not executed by the checker). The prototype walk-through shows that the prototype behaves as described in its README. It shows nothing about the future Blazor application. Historical evidence reused: none. Acceptance decision: pending.
