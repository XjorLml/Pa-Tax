# F05 accountant profile editing execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-16, about 12:10–13:20 +08:00.
- Observer: agent-observed (Claude Code), at the user's instruction "implement R05, accountant profile editing (it would be F05)". The user chose the same day: a static form (C04 for this page), and F01's limits as D04 plus a cap of 10 experience entries.
- Requirements/scenarios/procedures: [F05 PE01–PE08](../../../specs/profile-editing/spec.md#acceptance-scenarios); PRD R05, BR01, D04, R11; TEST-PLAN TP02 (profile part).
- Source revision and working-tree changes: branch `feature/f02-f04-theme-switch-google-sign-in` at `af9d3ea` with the F02 and F04 changes staged, plus F05 unstaged. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Domain/AccountantProfile.cs` | `797a999a8970` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileEditorService.cs` | `ea8e86b642d9` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileForm.cs` | `d18bbb65491c` |
  | `src/TaxBuddy.Web/Application/Profiles/ProfileValidator.cs` | `1e9a2294137e` |
  | `src/TaxBuddy.Web/Data/ProfileStore.cs` | `5d451b15ece7` |
  | `src/TaxBuddy.Web/Data/TaxBuddyDbContext.cs` | `19d6a6c074f5` |
  | `src/TaxBuddy.Web/Data/Migrations/20261116044023_AddProfileOwner.cs` | `c6f100a9fda1` |
  | `src/TaxBuddy.Web/Components/Pages/ProfileEditorPage.razor` (after the browser check; see below) | `909dc22217b3` |
  | `src/TaxBuddy.Web/Components/Pages/RegisterPage.razor` | `d0a1bd1b9329` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `8131cc8df10b` |
  | `src/TaxBuddy.Web/Account/AccountService.cs` | `f7da6906352d` |
  | `src/TaxBuddy.Web/Account/AccountSetup.cs` | `920296f578a4` |
  | `src/TaxBuddy.Web/Program.cs` | `7ad5420f4d77` |
  | `src/TaxBuddy.Web/Styles/app.css` | `b7edd2584ca8` |
  | `tests/TaxBuddy.Tests/Application/ProfileValidatorTests.cs` | `2b4252da4c9b` |
  | `tests/TaxBuddy.Tests/Domain/ProfileEditingRulesTests.cs` | `cea98cb3a401` |
  | `tests/TaxBuddy.Web.Tests/ProfileEditorTests.cs` | `d55b5cd3f58d` |
  | `tests/TaxBuddy.Web.Tests/Support/FormPosts.cs` (helpers moved from `SignInTests`) | `bd4f0d34d29f` |
  | `tests/TaxBuddy.Web.Tests/SignInTests.cs` | `302fcbfea54d` |

- Artifact: Release build output, not retained.
- Environment:
  - WSL2 Ubuntu 26.04 LTS with .NET SDK 11.0.100, ICU 78 and `dotnet-ef` 10.0.15.
  - Tailwind 4.3.3 through Windows Node 24.16.0.
  - Headless Microsoft Edge 157.0.4341.60 on Windows 10.0.26300.
  - The Supabase development database.
- Preconditions/fixtures: synthetic names and `example.test` emails only. Web tests use the F04 test sign-in and their own throwaway schemas.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` | 0 warnings, 0 errors | 0 and 0 (the final run used `--no-incremental`) | Pass |
| `dotnet ef migrations add AddProfileOwner ... --configuration Release --no-build`, then a review | Additive only | `owner_account_id` (uuid, null), `version` (int, default 0), a unique index, and an FK to `users` with on-delete cascade | Pass |
| Rule tests (validator, domain), first run | All pass | 32 of 32 passed | Pass |
| Web tests, runs 1–4 | All pass | Defects found and fixed: <br>• **Product:** the editor rendered `Form` before it loaded (500; fixed by not rendering until it exists). <br>• **Test code:** the helpers didn't post the visible name field; the nav link was matched with exact whitespace; the directory check ran while signed in; the session-reuse test was missing the name field | Fail, then fixed |
| Edge check script (agent-written), run 1, against the temporary test host | Functional and layout checks pass | **Product defect:** adding the first experience row on a new profile gave a 500, because form binding set the empty list to null. Fixed in `ProfileForm`, with the regression test `A_new_profile_with_no_experience_rows_can_add_its_first_row`. The layout results of that run were invalid: the test host served pages without the stylesheet | Fail, then fixed |
| Edge check, run 2 | Pass | Browsed `localhost:5234`, which was the owner's own running instance (port already in use), not this build. Only page requests and one failed submit; nothing changed through it. Results discarded | Not applicable |
| `dotnet test TaxBuddy.slnx -c Release --no-build` (final) | No failures, nothing skipped | TaxBuddy.Tests 76 passed; TaxBuddy.Web.Tests 72 passed; 0 failed, 0 skipped | Pass |
| `dotnet ef database update` | `AddProfileOwner` applied | "Applying migration '20261116044023_AddProfileOwner'. Done." | Pass |
| Edge check, run 3 ([results](2026-11-16-f05-profile-editing/results.json)): sign-in through a temporary test host on port 5301 against the development database; the real app (`dotnet run`, Development) on port 5240, sharing the session | All pass | 42 of 42: registration lands on the editor; no overflow, every field labelled and buttons at least 44 px at 320–1440 px in both themes; add-row redraws at `#experience`; an invalid save gives a 5-problem summary linked to marked fields, with the input kept; Enter in a field saves rather than removing a row; a complete profile is listed and shown on its public page and in the directory. No exceptions in the app log | Pass |
| Screenshot review: errors at 390 px (dark), saved at 1440 px (light), empty at 390 and 1440 px | As DESIGN | As expected. Two findings: the consent label showed "(PRD R11)", which was removed afterwards (`ProfileEditorPage.razor` then got the hash above, and the full test suite was rerun: 148 passed). The header wraps to two rows at 1440 px once "My profile" is added | Pass, with notes |

The 12 web tests cover:
- **PE01:** complete, listed and public.
- **PE02:** unfinished, unlisted, with what's missing named.
- **PE03:** invalid input saves nothing and keeps everything.
- **PE04:** add and remove rows; editing; the first row on a new profile.
- **PE05:** a stale tab gets 409, keeps its edits, then overwrites.
- **PE06:** a visitor is redirected to sign-in; a client gets 403 with no form; each accountant edits only their own profile; no token gets 400; an accountant registering from home lands on the editor.
- **PE07:** an unavailable database gives 503 on load and on save, with the input kept.

## Restoration and limits

- The temporary test host and diagnostic tests were deleted; none are committed. The temporary Edge profiles were deleted and the app processes stopped.
- **Development database:** the browser check created two synthetic accounts (`andrea.browser@example.test`), one with a profile. They were deleted afterwards (`DELETE FROM users WHERE email = 'andrea.browser@example.test'`: 2 rows; the profile went with them through the cascade). Afterwards: 2 accounts (not the agent's), 0 owned profiles, 9 seed profiles.
- The migration remains applied.
- The owner's own app instance on port 5234 was left running.

Not run:
- Editing with a real Google accountant account.
- Firefox and Safari, where month fields fall back to text boxes.
- Phones, screen readers and zoom.

Historical evidence reused: none.

Acceptance decision: pending (owner walkthrough).
