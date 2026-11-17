# F08 portfolio highlights execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-17, about 12:20–12:50 +08:00.
- Observer: agent-observed (Claude Code). User's instructions: "implement portfolio highlights", then "proceeed". D23 adopted provisionally from the prototype (up to 6), with working field limits.
- Requirements/scenarios: PRD R12, BR13, D23 (provisional); [F08 PH01–PH07](../../../specs/portfolio-highlights/spec.md#acceptance-scenarios).
- Source revision: branch `feature/f02-f04-theme-switch-google-sign-in` at `37c5ae7`, with F07 and F08 uncommitted. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Domain/AccountantProfile.cs` | `33b9c7a94161` |
  | `src/TaxBuddy.Web/Data/TaxBuddyDbContext.cs` | `61a2cfe31b2f` |
  | `src/TaxBuddy.Web/Data/Migrations/20261117043746_AddPortfolioHighlights.cs` | `21d846807cb7` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileForm.cs` | `29f34898e02d` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileValidator.cs` | `79f7c69d6e5d` |
  | `src/TaxBuddy.Web/Data/ProfileStore.cs` | `66651c1ced45` |
  | `src/TaxBuddy.Web/Application/Directory/DirectoryModels.cs` | `457da4f5b214` |
  | `src/TaxBuddy.Web/Application/Directory/DirectoryService.cs` | `ad58ecb97d36` |
  | `src/TaxBuddy.Web/Data/DirectoryStore.cs` | `212facc09641` |
  | `src/TaxBuddy.Web/Data/DevelopmentSeed.cs` | `b7af233ac9c4` |
  | `src/TaxBuddy.Web/Data/Seed/development-profiles.json` | `43495b3a97ff` |
  | `src/TaxBuddy.Web/Components/Pages/ProfileEditorPage.razor` | `167f20bb7647` |
  | `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `bca34e6ff19f` |
  | `src/TaxBuddy.Web/Components/Shared/AccountantCardItem.razor` | `cf4d962d55dd` |
  | `src/TaxBuddy.Web/Components/Shared/Display.cs` | `c9cb98cf7678` |
  | `src/TaxBuddy.Web/Styles/app.css` | `4fdf63f1c291` |
  | `tests/TaxBuddy.Tests/Application/HighlightTests.cs` | `45407b872a3c` |
  | `tests/TaxBuddy.Web.Tests/ProfileEditorTests.cs` | `ca09eee2ca0e` |

- Environment: WSL2 Ubuntu 26.04 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; the Supabase development database.
- Fixtures: synthetic accounts and profiles with `example.test` emails; highlight text from the prototype's invented samples.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` | 0 warnings, 0 errors | 0 and 0 | Pass |
| `dotnet ef migrations add AddPortfolioHighlights`, then a review | Additive | One table, `accountant_highlights`, owned by the profile and deleted with it | Pass |
| Whole suite, first run | No failures | 1 web test failed: the test's remove post still carried an empty second row, which became row 1. The test was corrected; the product behaved as designed | Fail, then pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build -m:1` (final) | No failures, nothing skipped | TaxBuddy.Tests 107 passed; TaxBuddy.Web.Tests 90 passed; 0 failed, 0 skipped | Pass |
| `dotnet ef database update` | Applied | "Applying migration '20261117043746_AddPortfolioHighlights'. Done." | Pass |
| Edge check ([results](2026-11-17-f08-portfolio-highlights/results.json)): the real app (`dotnet run`, Development, port 5240), a temporary sign-in host on 5301, a synthetic accountant with two highlights | PH01, PH05–PH07 | 7 of 7. The profile shows the sections About, Services, Portfolio highlights, Experience at 390 and 1440 px, light and dark, with a "Food retail" tag and one "Result" line, and no sideways scrolling. The card shows "2 highlights". The editor's legends are About you, Experience, Services, Portfolio highlights, Contact details, with two rows labelled Title, Type of client, What you did, Result, then "Add highlight" and the hint. App log: no exceptions | Pass |
| Screenshot review: profile section (1440 light), card (1440), editor fieldset (390 dark) | The prototype's `.pf` cards and row layout | As expected | Pass |

**What the tests cover:**
- **Rule tests:**
  - complete rows kept in order, blank rows dropped;
  - a half-filled row names the missing title and "what you did";
  - a seventh highlight and over-long text are refused;
  - the domain refuses a seventh highlight or one without a title or work;
  - highlights don't change listing.
- **Web tests:**
  - "Add highlight" draws a row;
  - saving shows the highlight on the profile with its tag, its result and the note, before Experience;
  - the card shows "1 highlight";
  - removing the row and saving leaves no section and no fact;
  - a half-filled row returns 422 with linked messages, keeps the typed value and saves nothing.

## Restoration and limits

- **Development database:**
  - the check created 1 synthetic account with its profile and 2 highlights; they were deleted afterwards. Afterwards: 2 accounts, 0 `example.test`, 10 profiles, 0 highlights;
  - the development seed was **not** rerun. It replaces every profile, including the owner's own, so the seed's sample highlights appear only after the owner reruns it.
- The temporary host, script and Edge profile were deleted; none are committed.
- Not run: other browsers, phones, screen readers.

Acceptance: pending.
