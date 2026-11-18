# Pick, preview, save (F06 PP10) execution evidence

## Identity and attribution

- **Date/time and timezone:** 2026-11-19, +08:00, between about 02:00 and 02:40.
- **Observer:** agent-observed (Claude Code).
- **User's instruction:** "implement option e".
- **Scope:** [PP10 scenario](../../../specs/profile-pictures/spec.md#pick-preview-save-2026-11-19-pp10).
- **Source revision:** branch `feature/f02-f04-theme-switch-google-sign-in` at `55e0f12`, with the earlier work of 2026-11-18 and 2026-11-19 uncommitted. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/wwwroot/TaxBuddy.Web.lib.module.js` | `185577c6ff72` |
  | `src/TaxBuddy.Web/Components/Pages/AccountPage.razor` | `bbbd3b7eeed2` |
  | `src/TaxBuddy.Web/Styles/app.css` | `a657845dd45c` |
  | `tests/TaxBuddy.Web.Tests/ProfilePictureTests.cs` | `85460279faf6` |
  | `prototypes/index.html` | `84ce4b1c3907` |
  | `tools/check_prototype.mjs` | `c2b7efd2873f` |

- **Environment:** WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; the Supabase development database for the tests, with the in-memory picture store.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs "<Edge>"` | All checks pass with E as the default design | `PASS: 202 prototype checks` | Pass |
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings, 0 errors | 0 and 0 | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build -m:1` | No failures, nothing skipped | 154 and 105 passed, 0 failed, 0 skipped, on the final sources. The picture test now checks the hooks the script uses and that Save and Remove belong to the page's two forms | Pass |
| Site run on port 5244: `GET /_blazor/initializers` and `GET /TaxBuddy.Web.lib.module.js` | The script is registered and served | `["TaxBuddy.Web.8ci1elo6dh.lib.module.js"]`; 200, `text/javascript` | Pass |
| The script against the rendered Account page in Edge, DOM read after it ran: page load | The picture area is marked ready, the panel hidden, Save and Cancel hidden | `picture-area picture-js`, `data-ready="1"`, the confirm block `hidden` | Pass |
| The same: a PNG put into the file input | The confirm block is shown; no error | Shown; the error banner stays `hidden` | Pass |
| The same: a PNG, then Cancel | The initials avatar is back; the confirm block hidden | `avatar lg t3` with "MS"; `hidden` | Pass |
| The same: a text file | No preview; "That file isn't a JPEG, PNG or WebP photo." | The error banner shown with that text; the confirm block `hidden` | Pass |
| The same: the page after an upload, with a picture | "Remove picture" under the name is shown; the result banner is present | Both, not `hidden` | Pass |
| The same page without the script | The picture area unmarked; the inline "Remove picture" stays hidden; the panel shows as the page's target | As expected | Pass |
| Screenshots ([folder](2026-11-19-f06-picture-option-e/)): preview at 1440 light; after saving at 1440 light; script off at 1440 light | The designs in DESIGN | As expected in the three opened. First attempt: the result banner was broken, because its class `inline` is also Tailwind's `display: inline`; the class was removed and the captures retaken | Pass after a fix |
| Screenshots: preview and error at phone width, dark | The same on a narrow dark page | Saved, not opened after the fix | Not reviewed |

**How the page was rendered:** a temporary test signed in through the test host and saved the HTML of `/account` before and after an upload; it was then deleted. The files were opened in Edge with the built `wwwroot/app.css` and the real script file. A file was put into the input from script with `DataTransfer`, since headless Edge cannot press a file chooser. The stored picture's address was replaced by a placeholder drawing.

## Restoration and limits

The temporary test and the saved pages were deleted. The temporary site instance was stopped and the port checked as closed.

Not exercised:
- **A person using it:** pressing the pencil, the device's file chooser, and Save posting a real file to the running site in a signed-in browser. The file chooser opening from the pencil was not seen; only the code path after a file is chosen was run.
- **Phones:** the camera option in the chooser, and touch.
- **Moves between pages:** that the script runs again after an in-site move to Account was not run; only a full page load was.
- **Keyboard and screen reader:** focus goes to Save after a choice and back to the pencil after Cancel by code, not checked with assistive technology.
- **Other browsers:** only Edge.

Found, not changed: the sign-in page's failure banner carries the same `inline` class.

Acceptance decision: pending the owner's walkthrough (G9).
