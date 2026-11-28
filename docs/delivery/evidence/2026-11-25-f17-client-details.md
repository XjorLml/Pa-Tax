# F17 client details, and the picture-upload setup: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-25, about 15:00–16:20 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user sent a screenshot of "Picture upload isn't set up on this site yet. Your initials are shown instead." and wrote: "implement to site and also how to resolve profile change/upload in the build?"
- Requirement and scenario: [PRD R25](../../PRD.md#requirements) (added that day), [F17](../../../specs/client-details/spec.md) CD01–CD08; R11, R24, BR06, BR18, D21. Resolves conflict C05. Picture setup: [F06](../../../specs/profile-pictures/spec.md) and [SETUP](../../SETUP.md#troubleshooting-and-maintenance).
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`. Migration `20261125074243_AddClientDetails`.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Domain/ClientDetails.cs` | `3a2025059b7f` |
  | `src/TaxBuddy.Web/Account/ClientDetailsService.cs` | `90d93c508a0e` |
  | `src/TaxBuddy.Web/Components/Pages/AccountPage.razor` | `8ef0774133cf` |
  | `src/TaxBuddy.Web/Components/Work/AboutClient.razor` | `06927d8cfc3b` |
  | `src/TaxBuddy.Web/Data/WorkStore.cs` | `d9e2677dd039` |
  | `src/TaxBuddy.Web/Styles/app.css` | `d3885ca70a17` |
  | `src/TaxBuddy.Web/Components/Pages/PrivacyPage.razor` | `455a64f7f5ba` |
  | `src/TaxBuddy.Web/Data/Migrations/20261125074243_AddClientDetails.cs` | `8bb1e7dc99e5` |
  | `tests/TaxBuddy.Tests/Domain/ClientDetailsRulesTests.cs` | `a640d3b6adf6` |
  | `tests/TaxBuddy.Web.Tests/ClientDetailsTests.cs` | `2ce7b875ffdf` |
  | `prototypes/index.html` | `036b82fe2a0c` |
  | `prototypes/service-workspace.js` | `a60cf744fe4f` |

- Environment: WSL2 with .NET SDK 11.0.100 and `dotnet-ef` 10.0.15; the Supabase development database (throwaway schemas for the tests; the migration applied to the shared schema); Windows Edge 158.0.4370.33, headless; Windows Node 22.12.0 for the prototype checker.

## Picture upload "isn't set up"

| Procedure | Expected result | Actual observation | Outcome |
|---|---|---|---|
| Read the code: `SupabaseStorageApi.IsConfigured` | Uploads turn on only with `Supabase:Url` and `Supabase:SecretKey` | Confirmed; otherwise `NoPictureStorage` and the message the user saw | Cause narrowed |
| List the user-secret **names**, then each value's length and prefix (never the values) | Both keys present and filled | `Supabase:Url` present but empty. No `Supabase:SecretKey`. Instead an entry whose **name** was a 41-character `sb_secret_` key, with an empty value. **The name listing printed that key into the session's output** | Cause found |
| Supabase Storage `GET /storage/v1/bucket/{name}` with that key, for both buckets | Both exist and are private | `profile-pictures`: exists, private. `service-documents`: exists, private | Pass |
| Fix the secrets file with a script that prints nothing secret | `Supabase:Url` and `Supabase:SecretKey` set; the misnamed entry gone | Names afterwards: `Authentication:Google:ClientId`, `Authentication:Google:ClientSecret`, `ConnectionStrings:TaxBuddy`, `Supabase:SecretKey`, `Supabase:Url` | Pass |

**Not observed:** an upload on the running site. That needs the user's Google sign-in, and the site reads these settings only at start-up.

**Owner action:** the key was exposed in the session, so rotate it (SETUP has the steps).

## F17 execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` (with Tailwind v4.3.3) | 0 warnings | 0 Warning(s), 0 Error(s); the new rules are in `wwwroot/app.css` | Pass |
| `dotnet ef migrations add AddClientDetails` | Four nullable columns; `phone_number` limited to 20 | As expected. EF warned about narrowing `phone_number`. No code wrote that column before F17, and PostgreSQL refuses to narrow rather than cut a longer value, so nothing can be lost | Pass |
| `dotnet ef database update` on the development database | Applied | `ALTER TABLE users ADD …` ×4 and the type change; `20261125074243_AddClientDetails` listed | Pass |
| New unit tests `ClientDetailsRulesTests` | All pass | 17 passed | Pass |
| New web tests `ClientDetailsTests`:<br>• the card for a client only;<br>• the edit form;<br>• a save, with the renewed session and blanks stored as null;<br>• a refused save keeps what was typed;<br>• the accountant's client page and request see the details, while a stranger and the public profile don't;<br>• deletion clears them | All pass | 6 passed. The first run failed one test, which found two real faults, fixed before the next run: a literal `<text>` tag in the sharing line, and "Phone(optional)" with no space | Pass after fix |
| `dotnet test TaxBuddy.slnx -c Release -m:1` | All pass | 400 passed (223 unit, 177 web); 0 failed, 0 skipped. Includes the F11 legal-version tests against `2026-11-25` | Pass |
| Rendered pages in Edge. A temporary test signed in a client and an accountant through the test host and saved the HTML; it was then deleted. The HTML was served locally with the built `app.css`, the scoped styles, the fonts and the logos | Matches the prototype's design B | Viewed:<br>• Account with details;<br>• the edit form with the sharing line naming the accountant;<br>• a refused phone, with a red border and message, everything else kept;<br>• the accountant's "About Maria" on the client page and, compact, in the request.<br>Desktop 1280 px light and dark, and a 390 px phone frame. As designed | Pass |
| The prototype after aligning the name limit (100), the phone rule (at least 7 digits) and the sharing line (accountants with a service): `node tools/check_prototype.mjs` on a Windows temp copy | All pass | `PASS: 212 prototype checks` | Pass |

## Restoration and limits

- **Cleanup:**
  - the temporary page-saving test was deleted;
  - the local file server on port 5317 was stopped;
  - Edge's profile and the checker's Windows copy were removed;
  - the screenshots stay in the session's scratch folder, outside the repository.
- **Changed outside the repository:**
  - the development database gained the migration;
  - the user-secrets file was corrected, with the old key name removed.
- **Not run:**
  - the running site with a real Google sign-in;
  - a database failure during a save (CD05 is a code path only);
  - screen readers, real phones, or contrast calculations for the new styles (they reuse the existing tokens).
- **Legal:** version `2026-11-25` makes every signed-in person agree again on their next visit. Like `2026-11-23.2`, it awaits the owner's approval (G13).
