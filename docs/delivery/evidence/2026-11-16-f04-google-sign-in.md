# F04 Google sign-in and registration execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-16, about 09:55–10:35 +08:00.
- Observer: agent-observed (Claude Code), at the user's instruction "focus on google signin first implementation and requirements". The user decided D03 (one role per account) and chose draft legal pages the same day.
- Requirements/scenarios/procedures: [F04 GS01–GS10](../../../specs/google-sign-in/spec.md#acceptance-scenarios); PRD R04 (Google part), R16; TEST-PLAN TP10 (sign-in part), TP18 (agreement part).
- Source revision and working-tree changes: `af9d3ea60af1cda9ad022322904b76a74e3900cb` plus the uncommitted F02 BD09 work plus F04. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Account/AccountEndpoints.cs` | `48ee1444bb56` |
  | `src/TaxBuddy.Web/Account/AccountService.cs` | `987fe6ec7644` |
  | `src/TaxBuddy.Web/Account/AccountSetup.cs` | `032777a5ca52` |
  | `src/TaxBuddy.Web/Domain/AccountRole.cs` | `533aeb5d20ae` |
  | `src/TaxBuddy.Web/Data/TaxBuddyUser.cs` | `c1c4fb37f778` |
  | `src/TaxBuddy.Web/Data/TaxBuddyDbContext.cs` | `2a94cba95ba8` |
  | `src/TaxBuddy.Web/Data/Migrations/20261116022520_AddAccounts.cs` | `c27c5d68911c` |
  | `src/TaxBuddy.Web/Services/LocalAddress.cs` | `cd7aece97cd8` |
  | `src/TaxBuddy.Web/Services/ThemePreference.cs` (F02, refactored to use `LocalAddress`) | `40f953e43436` |
  | `src/TaxBuddy.Web/Components/Pages/SignInPage.razor` | `5b386e2c506a` |
  | `src/TaxBuddy.Web/Components/Pages/RegisterPage.razor` | `5964866266d4` |
  | `src/TaxBuddy.Web/Components/Pages/TermsPage.razor` | `efd61140055a` |
  | `src/TaxBuddy.Web/Components/Pages/PrivacyPage.razor` | `4fdd26686d34` |
  | `src/TaxBuddy.Web/Components/Shared/AccountMenu.razor` | `1ed6a97cb23d` |
  | `src/TaxBuddy.Web/Components/Shared/DraftLegalNotice.razor` | `9304ec31959f` |
  | `src/TaxBuddy.Web/Components/Shared/ThemeSwitch.razor` (F02, refactored) | `f1dce3ed6d83` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `5795e54b74e1` |
  | `src/TaxBuddy.Web/Components/_Imports.razor` | `c3f7dab56467` |
  | `src/TaxBuddy.Web/Program.cs` | `79c67a49f65e` |
  | `src/TaxBuddy.Web/TaxBuddy.Web.csproj` | `6fdf0038b960` |
  | `src/TaxBuddy.Web/Styles/app.css` | `5acfad1fede0` |
  | `src/TaxBuddy.Web/wwwroot/app.css` (generated, ignored by git) | `2c869c6a206f` |
  | `tests/TaxBuddy.Web.Tests/SignInTests.cs` | `f352fccf9e09` |
  | `tests/TaxBuddy.Web.Tests/Support/TestExternalSignIn.cs` | `b290502a05bd` |
  | `tests/TaxBuddy.Web.Tests/Support/TaxBuddyAppFactory.cs` | `92a16662513a` |
  | `tests/TaxBuddy.Web.Tests/DirectoryPageTests.cs` (FT01 assertion) | `3834051fffec` |
  | `tests/TaxBuddy.Web.Tests/ThemeSwitchTests.cs` (F02, `LocalAddress`) | `88c2676d79df` |

- New packages: `Microsoft.AspNetCore.Authentication.Google` 11.0.0 and `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.15.
- Artifact: Release build output, not retained.
- Environment:
  - WSL2 Ubuntu 26.04 LTS (kernel 6.6.114.1-microsoft-standard-WSL2) with .NET SDK 11.0.100, `libicu78` 78.2 and `dotnet-ef` 10.0.15.
  - Tailwind 4.3.3 run through Windows Node 24.16.0.
  - Headless Microsoft Edge 157.0.4341.60 on Windows 10.0.26300.
  - Supabase development database (Singapore, PostgreSQL 17.11) through the session pooler.
- Preconditions/fixtures:
  - No Google credentials are configured on this machine, so the real app ran in the "not set up" state (GS08).
  - Tests use dummy Google settings and the test sign-in (`TestExternalSignIn`), with synthetic names and emails at `example.test`.
  - Each data test class uses its own throwaway schema.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` (first) | 0 errors | 3 errors: `AccountRoles` not in scope in two components; fixed with `@using TaxBuddy.Web.Domain` | Fail, then fixed |
| `dotnet ef migrations add AddAccounts ... --configuration Release --no-build` | Migration created | Created. Without `--configuration Release --no-build` it failed, because the Debug build runs Tailwind, which this WSL environment can't run. Reviewed: four new tables (`users`, `user_logins`, `user_claims`, `user_tokens`) and their indexes; no change to existing tables | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build`, runs 1–5 | All pass | Test-code defects found and fixed, none in the product code: <br>• the test sign-in set its cookie with `path=/test/external-signin` (`app.Map` path base); <br>• HTTP test clients didn't return Secure cookies, fixed with an HTTPS client; <br>• Blazor redirects are absolute URLs; <br>• the footer markup carries a scoped-CSS attribute; <br>• the registration helper posted to the first form on the page (the theme switch); <br>• two negative cookie checks used the wrong cookie name and would always have passed | Fail, then fixed |
| Same, after a `--no-incremental` rebuild (0 warnings, 0 errors) | No failures, nothing skipped | TaxBuddy.Tests: 44 passed. TaxBuddy.Web.Tests: 60 passed. 0 failed, 0 skipped | Pass |
| `dotnet ef database update --project src/TaxBuddy.Web --configuration Release --no-build` | `AddAccounts` applied | "Applying migration '20261116022520_AddAccounts'. Done." `migrations list` shows both migrations | Pass |
| Windows: `node_modules\.bin\tailwindcss.cmd -i Styles\app.css -o wwwroot\app.css --minify` | The new rules appear in `wwwroot/app.css` | Done in 96 ms; `.choice` and `.notice-danger` present | Pass |
| Edge check script (agent-written; [results](2026-11-16-f04-google-sign-in/results.json)) against `dotnet run` (Development) on the development database: `/`, `/signin`, `/signin?error=failed`, `/terms`, `/privacy` at 320, 390, 760 and 1440 px, light and dark | No overflow; a header "Sign in" of at least 44 px inside the viewport; footer legal links; correct sign-in, failure and draft texts | 152 of 152 passed. Directory header height: 173 px at 320, 127 px at 390 and 760, 75 px at 1440 | Pass |
| GS09, around 11:41 +08:00, after the owner created the Google OAuth client (Testing mode) and stored its ID and secret in WSL user-secrets. The agent checked the stored values by shape only: ID 72 characters ending in `.apps.googleusercontent.com`; secret 35 characters starting with `GOCSPX-`. The owner then ran `dotnet run --launch-profile http -p:SkipTailwind=true` and signed in from a Windows browser at `http://localhost:5234` | A real Google round trip, registration, signed in | Agent-observed before the owner's run: `/signin` showed "Continue with Google", and the challenge went to `accounts.google.com/o/oauth2/v2/auth` with `redirect_uri=http://localhost:5234/signin-google` and the `openid profile email` scopes. The owner's first attempt got Google's `Error 400: redirect_uri_mismatch`, although Google's details showed the expected `redirect_uri` and the client ID matched; the client's redirect settings had not yet taken effect on Google's side, and nothing in the app changed. Then **user-reported**: "google sign is now working". **Agent-observed** in the development database (counts and fields only, no name or email read): 1 account, role Client, agreed version `draft-2026-11-16`, created 2026-11-16 03:41:20Z, no password hash, one `Google` login | Pass (first sign-in and registration). Returning sign-in and sign-out with real Google not reported |
| Screenshot review: sign-in at 390 px (light) and its failure state at 1440 px (dark); terms at 390 px (dark) | Readable, consistent with DESIGN | Reviewed by the agent; as expected. At 320 px the header takes three rows | Pass, with a note |

What the 22 new web tests cover, by scenario:
- **GS01 and GS03:** first sign-in → registration → signed in on the starting page, with role, agreement time and version, a linked Google login and no password stored; registering as an accountant (no listed profile); a returning person skips registration; two tabs registering the same Google account end up with one account.
- **GS02:** a missing agreement, an empty role and an unknown role each get the error, keep the choice and create nothing.
- **GS04:** cancel at Google, using a real state value from the challenge, goes back to the starting page without a session cookie. A Google error leads to the sign-in page with the alert, and only an error category is logged.
- **GS05:** a callback or registration with no sign-in in progress; forged sign-in posts (no token, an unknown or empty provider) get 400; off-site return addresses go home.
- **GS06:** sign-out ends the session.
- **GS07:** both legal pages say they are drafts; every page links to them and offers sign-in.
- **GS08:** without Google there's no button, and a post gets 400.
- **GS10:** a full registration logs neither the email nor the name.
- **Challenge:** the redirect to Google carries the client ID, the HTTPS callback address and the `openid profile email` scopes.

Artifacts/logs: `results.json` beside this record. The scripts, screenshots and app logs stayed in the agent's temporary scratch folder; the app log showed no exceptions. Test data were synthetic only.

## Restoration and limits

- The temporary Edge profile was deleted and the app processes were stopped.
- The development database keeps the four new account tables (the migration) and the owner's real account from GS09. That holds the owner's own name and email from Google; it stays, so the owner can keep using it. Tests created their own schemas and drop them afterwards; the drop was not checked separately.
- Left on purpose: `node_modules` with Windows binaries, and the regenerated `wwwroot/app.css`.

Not run:
- With real Google: a returning sign-in, sign-out and cancel. The tests cover them with the test sign-in.
- Microsoft sign-in (out of scope), other browsers, phones, screen readers and zoom.
- Session expiry and its return (TP10 part).
- A deployed site (TP07, TP12).

Historical evidence reused: none. The F02 BD09 theme switch was refactored (`LocalAddress`), and its 20 tests passed again in this run.

Acceptance decision: pending (owner walkthrough).
