# Google sign-in and registration implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: as built 2026-11-16, uncommitted ([evidence](../../docs/delivery/evidence/2026-11-16-f04-google-sign-in.md)).

## Affected modules and contracts

| Module / source path | Change | Contract / scenario |
|---|---|---|
| `Domain/AccountRole.cs` (new) | `Client` or `Accountant`, one per account | D03, GS01–GS02 |
| `Data/TaxBuddyUser.cs` (new), `Data/TaxBuddyDbContext.cs` | The account: Identity user with a Guid key, display name, role, agreement time and version, creation time. The context becomes an `IdentityUserContext` (users, logins, claims, tokens; no Identity roles tables, because the role is one column) with snake_case table names | ADR 006, GS01 |
| `Data/Migrations/…_AddAccounts` (new) | Creates `users`, `user_logins`, `user_claims` and `user_tokens`. Leaves the existing tables alone | GS01 |
| `Account/AccountSetup.cs` (new) | Identity core with external sign-in only, the application and external cookies, Google when configured, the cancel and failure events | ADR 006, SDD session, GS04, GS08 |
| `Account/AccountEndpoints.cs` (new) | `POST /account/external-signin` (challenge), `GET /account/signin/callback`, `POST /account/signout`. Local-only return addresses through the existing `ThemePreference.IsLocalAddress` rule | SDD, GS03–GS06 |
| `Account/AccountService.cs` (new) | What the pages need: whether Google is available, the pending sign-in, and registration (creates the account, links the Google login, signs in). If two tabs register the same Google account, the second signs in to the first one's account. Pages use this, not Identity's managers, so components never depend on the data layer (layer test) | GS01–GS02 |
| `Components/Pages/SignInPage.razor`, `RegisterPage.razor`, `TermsPage.razor`, `PrivacyPage.razor` (new) | Static pages. Registration is a named form posted to the same page | SDD render modes, GS01–GS08 |
| `Components/Layout/MainLayout.razor`, `Components/Shared/AccountMenu.razor` (new) | Sign in, or name and Sign out; footer legal links | GS06–GS07 |
| `Program.cs`, `_Imports.razor` | Wiring; cascading authentication state | — |
| `tests/TaxBuddy.Web.Tests/SignInTests.cs` (new), `Support/TestExternalSignIn.cs` (new), `DirectoryPageTests.cs` | Integration tests with a test sign-in endpoint; the FT01 assertion is clarified | GS01–GS08 |

## Dependencies and approach

**Packages:**
- `Microsoft.AspNetCore.Authentication.Google` 11.0.0, which has no EF dependency.
- `Microsoft.AspNetCore.Identity.EntityFrameworkCore` 10.0.15. Version 11.0.0 needs EF Core 11, which the pinned Npgsql 10.0.3 provider doesn't support, so the Identity store stays in step with EF Core 10.0.15 (SETUP). Move it to 11 together with EF Core and Npgsql.

**The flow:**
1. The sign-in page posts to the challenge endpoint.
2. Google returns to `/signin-google` (the handler's default), then to the callback.
3. A known login signs in. An unknown login keeps Identity's external cookie and goes to `/register`.
4. Registration creates the user, adds the login, signs in, clears the external cookie and redirects.

All pages are static: no circuit, and they work without script. A scoped context is registered from the existing factory so that Identity's store can use it. Pages and services keep using the factory.

**Return addresses:** carried as `returnUrl`. They are checked when posted, kept in the Google state, and checked again before every redirect.

**Without Google configured** (no `Authentication:Google:ClientId` or `ClientSecret`), the Google handler isn't registered, and the sign-in page says so.

**Data:** an additive migration, run with `dotnet ef database update`. Rollback is `dotnet ef database update InitialDirectory`. Nothing is backfilled.

Not in this slice: persisting data-protection keys to the database. It is needed before the first deployment, so that sessions survive redeploys (SDD keys and secrets; RELEASE).

## Google Cloud setup

The owner does this once; nothing in the repository depends on it:
1. In <https://console.cloud.google.com/>, create or choose a project, for example "Tax Buddy dev".
2. Go to **Google Auth Platform → Branding**: set the app name and support email. Under **Audience**, choose External and keep it in **Testing**, then add your own Google account, and any testers, as test users.
3. Under **Data access**, keep the default scopes only: `openid`, `email`, `profile`.
4. Under **Clients**, create a client of type **Web application**. Authorised redirect URIs: `http://localhost:5234/signin-google` and `https://localhost:7169/signin-google`. Add each deployed site's `https://<host>/signin-google` later.
5. Store the two values in user-secrets in WSL. Read each one in without echoing it, as SETUP does for the database:
   ```bash
   read -rsp 'Client ID: ' V; echo; dotnet user-secrets set --id taxbuddy-web "Authentication:Google:ClientId" "$V"; unset V
   read -rsp 'Client secret: ' V; echo; dotnet user-secrets set --id taxbuddy-web "Authentication:Google:ClientSecret" "$V"; unset V
   ```

## Verification

1. Web integration tests for GS01–GS08. A test-only endpoint signs in to Identity's external scheme as Google would, and the cancel and error tests send Google's callback a real state value created by the challenge.
2. Release build; the whole suite against the development database.
3. Apply the migration to the development database, then a browser check of the sign-in and registration pages and the header at phone and desktop widths in both themes.
4. GS09: a real Google round trip once the owner has stored credentials.
5. Evidence record, STATUS, SDD, PRD D03, DESIGN, SETUP and TEST-PLAN updates.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation / owner |
|---|---|---|
| Identity EF store 10.0.15 on the .NET 11 shared framework | A version mismatch could show up at runtime | The full test suite exercises the store; move to 11 with Npgsql (SETUP) |
| Data-protection keys kept in the default local store | On a host without persistent storage, a redeploy signs everyone out and pending forms fail | Persist keys before the first deployment (RELEASE, G6) |
| Email and name come from Google | Personal data now stored | Never logged (GS10); covered by the D10 privacy review; deletion arrives with R09 |
| The legal text is a draft | People agree to a draft | Only the owner and testers sign in until G13 closes. The agreed version is stored, so re-agreement can be required later |
| The header gets longer on phones | Sign in and the theme switch compete for space | Browser check at 320–1440 px; owner walkthrough (G9) |

Significant rationale: [ADR 006](../../docs/adr/006-web-sign-in.md). Documentation updates: PRD D03, SDD sign-in and navigation, DESIGN states, SETUP secrets, TEST-PLAN, STATUS.
