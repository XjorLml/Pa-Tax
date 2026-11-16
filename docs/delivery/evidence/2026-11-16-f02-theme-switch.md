# F02 BD09 theme switch execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-16, about 07:55–08:22 +08:00.
- Observer: agent-observed (Claude Code), at the user's instruction "implement f02", with the scope the user chose on 2026-11-16: add a theme switch, check name availability, close F02 records.
- Requirements/scenarios/procedures: [F02 BD09](../../../specs/brand-design-refresh/spec.md#theme-switch-in-the-application-2026-11-16), [DESIGN visual system](../../DESIGN.md#visual-system), [SDD forms and browser storage](../../SDD.md#render-modes).
- Source revision and working-tree changes: `af9d3ea60af1cda9ad022322904b76a74e3900cb` plus uncommitted changes. Leading 12 hex digits of SHA-256 at the time of the checks:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Services/ThemePreference.cs` (new) | `5ec6928ef48b` |
  | `src/TaxBuddy.Web/Components/Shared/ThemeSwitch.razor` (new) | `97256d813344` |
  | `src/TaxBuddy.Web/Components/Shared/BrandLogo.razor` | `316cf8b03e44` |
  | `src/TaxBuddy.Web/Components/Shared/Icon.razor` | `72c35716f4c8` during the browser check; `6cd283881813` after a whitespace-only fix (`["list"] =` regained its space). After the fix, the WSL build (0 warnings) and the full test run (58 passed, 22 skipped) were repeated; the browser check was not |
  | `src/TaxBuddy.Web/Components/App.razor` | `00d00a16b7b7` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `12330256ba28` |
  | `src/TaxBuddy.Web/Components/_Imports.razor` | `f66f17bb9bd9` |
  | `src/TaxBuddy.Web/Program.cs` | `c5a7e738d59f` |
  | `src/TaxBuddy.Web/Styles/app.css` | `4bf1ecee4284` |
  | `src/TaxBuddy.Web/wwwroot/app.css` (generated, ignored by git) | `3ea96ff2f97b` |
  | `tests/TaxBuddy.Web.Tests/ThemeSwitchTests.cs` (new) | `6970aa72b5e9` |

- Artifact: Release build output in each project's `bin/Release/net11.0`, not retained.
- Environment:
  - Windows 10.0.26300 x64 with .NET SDK 11.0.100 (`dotnet.exe`), Node 24.16.0 and npm 11.13.0.
  - WSL2 Ubuntu 26.04 LTS (kernel 6.6.114.1-microsoft-standard-WSL2) with .NET SDK 11.0.100 and runtimes 11.0.0, installed for this check in `~/.dotnet` with Microsoft's `dotnet-install.sh --version 11.0.100`.
  - Headless Microsoft Edge 157.0.4341.60 on Windows, driven over the DevTools protocol.
- Preconditions/fixtures: no database. The app ran with an unreachable connection string (`Host=127.0.0.1;Port=1`), so the directory shows its "couldn't load" state and the not-found page needs no data. No user-secrets were present in this WSL environment. Isolated, temporary Edge profile. No personal data.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Windows: `npm ci` in `src/TaxBuddy.Web` | Tailwind installed | 0 vulnerabilities; exit 0 | Pass |
| Windows: `dotnet.exe build TaxBuddy.slnx -c Release` | 0 warnings, 0 errors; Tailwind writes `wwwroot/app.css` with the theme rules | Build succeeded; generated CSS contains the `[data-theme=dark]` token block and the logo and `.theme-option` rules | Pass |
| Windows: `dotnet.exe test ... --filter ThemeSwitch` (first run) | All pass | 9 of 20 failed: the tests read the 404 not-found page with `GetStringAsync`, which throws on 404. Test defect, fixed by reading the response and asserting 404 | Fail, then fixed |
| Windows: rerun the tests, then a full rebuild and rerun, then `dotnet run` | Tests run; app starts | Each attempt refused: "An Application Control policy has blocked this file" (0x800711C7) for the new `TaxBuddy.Web.Tests.dll` and for `TaxBuddy.Web.exe`. The first test run in the same session had loaded the earlier build. Not bypassed | Blocked |
| WSL: first `dotnet build` | Build | Stopped: "Couldn't find a valid ICU package". `libicu` needs `sudo`, which asks for a password | Blocked |
| WSL, with `DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1`: `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true --no-incremental` | 0 warnings, 0 errors | Build succeeded, 0 warnings, 0 errors. Tailwind was skipped because WSL has no Linux Node; the stylesheet from the Windows build above was used | Pass |
| WSL: `dotnet test TaxBuddy.slnx -c Release --no-build` | No failures | TaxBuddy.Tests: 36 passed, 8 skipped. TaxBuddy.Web.Tests: 22 passed, 14 skipped. 0 failed. The skipped tests are the database tests (no connection string) | Pass for what ran |
| WSL: `dotnet test tests/TaxBuddy.Web.Tests ... --filter ThemeSwitch` | All 20 pass | 20 passed, 0 skipped | Pass |
| Edge check script (agent-written; [results](2026-11-16-f02-theme-switch/results.json)) against `dotnet run` in WSL on `http://localhost:5299` | All checks pass | 181 of 181 passed (details below) | Pass |
| Rerun at about 09:05 +08:00, after the user installed `libicu78` 78.2-2ubuntu1 in WSL, without invariant mode and with `Icon.razor` at `6cd283881813`: `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true --no-incremental`, `dotnet test TaxBuddy.slnx -c Release --no-build`, then the Edge check against a fresh `dotnet run` | Same results as above | Build: 0 warnings, 0 errors. Tests: 36 passed and 8 skipped, plus 22 passed and 14 skipped; 0 failed. Edge: 181 of 181. The app log's exceptions were all "connection refused" from 8 directory loads against the deliberately unreachable database | Pass |
| About 09:45 +08:00, after the user stored the development database connection string in WSL user-secrets: `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true`, then `dotnet test TaxBuddy.slnx -c Release --no-build` (with ICU) | Every test runs, including the database tests | First attempt: exit 0, but no output and no tests run, because `obj/` had been restored from Windows in the meantime. After a WSL rebuild (0 warnings, 0 errors): TaxBuddy.Tests 44 passed; TaxBuddy.Web.Tests 38 passed; 0 failed, 0 skipped. The data tests create and drop their own `test_` schemas in the development database (Supabase, Singapore); the drop was not checked separately | Pass |
| Screenshot review: directory at 390 and 1440 px in four stored/browser combinations, and keyboard focus at 1440 px | Readable; switch, logo and colours agree | Reviewed by the agent. Colours, logo and pressed option agree in every image; focus ring visible. At 390 px the switch wraps to its own row, and the header grows from about 75 to 127 px | Pass, with a note |

The 20 web tests cover:
- no choice gives no `data-theme` and System pressed;
- choosing light or dark stores an HttpOnly, SameSite=Lax cookie for a year and redirects to the same address, including the query, from the not-found page;
- choosing System deletes the cookie;
- a post without the antiforgery token gets 400 and no cookie;
- an unknown value gets 400;
- off-site return addresses (`https://…`, `//…`, `/\…`, empty) send the visitor home;
- unit cases for the local-address rule, including CR/LF;
- the two dark token blocks in `Styles/app.css` are identical.

The 181 browser checks cover three stored choices (none, light, dark), two browser settings (light, dark) and widths of 320, 390, 760 and 1440 px. Each combination checks:
- `data-theme` and the page background colour;
- that exactly one logo is shown, and that it matches the theme;
- the pressed option;
- no horizontal overflow, with the switch inside the viewport;
- buttons of at least 44 px;
- text labels from 760 px up and icons only below.

The script also pressed Dark, Light and System for real, checking each time that the browser returned to `/no-such-page?x=1` with the right theme and cookie. Finally it tabbed from the top of the page to the switch and found a 3 px solid focus outline.

Artifacts/logs: `results.json` beside this record is from the first browser run; the ICU rerun matched it, 181 of 181. The script, screenshots and app log stayed in the agent's temporary scratch folder and are not retained. No sensitive data.

## Restoration and limits

- The temporary Edge profile was deleted; its absence was checked afterwards. The app process was stopped.
- Left in place on purpose:
  - `src/TaxBuddy.Web/node_modules`, Windows binaries, ignored by git;
  - .NET SDK 11.0.100 in WSL `~/.dotnet`, installed at the user's choice. Afterwards, at the user's request, `~/.bashrc` gained `DOTNET_ROOT` and `PATH` lines. To remove both: `rm -rf ~/.dotnet` and delete those three lines.
  - `libicu78` in WSL, installed by the user.

Not run:
- Chrome, Firefox, Brave, Safari, real phones, screen readers and zoom;
- the profile page and the populated directory, because there was no data;
- nothing for ICU any more: the first runs used invariant globalization, and the rerun with ICU gave the same results.

Historical evidence reused: none. The F01 results are not carried over to this source.

Acceptance decision: pending. The user's walkthrough is gate G9.
