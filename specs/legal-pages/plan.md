# Legal text and privacy prompts implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: proposed 2026-11-21.

## Affected modules and contracts

| Module / source path | Planned change | Governing contract / scenario |
|---|---|---|
| `Domain/AccountRole.cs` (`LegalText`) | `CurrentVersion = "2026-11-21"`, plus the date shown on the pages | LP01, LP14 |
| `Account/AccountSetup.cs` (`TaxBuddyClaimsFactory`) | Adds the claim `taxbuddy:terms` with the account's agreed version | LP05, LP06 |
| `Account/TermsAgreement.cs` (new) | `TermsNotRequiredAttribute`, and middleware that redirects GET requests for component pages without that attribute when the claim isn't current | LP05 |
| `Account/AccountService.cs` | `AgreeToCurrentAsync`: stores the version and time, then renews the session | LP06 |
| `Components/Pages/AgreePage.razor` (new) | `/account/agree`, a static named form, from the prototype's `agreeHTML` | LP06, LP07 |
| `Components/Pages/RegisterPage.razor` | New wording and error | LP04 |
| `Components/Pages/TermsPage.razor`, `PrivacyPage.razor` | The full text in the long-document layout; `DraftLegalNotice.razor` removed | LP01–LP03 |
| `Components/Chat/ChatPanes.razor` | The hint above the composer, linked from the text box | LP09 |
| `Components/Pages/ProfileEditorPage.razor` | `Field` gets an optional hint; the title uses it | LP10 |
| `Components/Pages/AccountantProfilePage.razor`, `ReportPage.razor` | Text changes | LP11, LP12 |
| `Styles/app.css` | The prototype's `.legal-*` and `.composer-note` rules, through the existing tokens | LP01, LP09 |
| `Program.cs` | `app.UseTermsAgreement()` after authentication | LP05 |
| `tests/TaxBuddy.Web.Tests/LegalPagesTests.cs` (new), `SignInTests.cs`, `ChatPageTests.cs`, `ProfileEditorTests.cs`, `ProfilePageTests.cs`, `ReportPageTests.cs`; `tests/TaxBuddy.Tests/Domain/LegalTextTests.cs` (new) | Scenario tests | LP01–LP14 |

## Dependencies and approach

The middleware runs after authentication. In a `WebApplication`, routing runs first, so the chosen endpoint and the component's attributes are known. The middleware acts only when all of these hold:
- the request is a GET or HEAD;
- the endpoint is a Razor component page;
- the page isn't marked `[TermsNotRequired]`;
- the user is authenticated, and the `taxbuddy:terms` claim differs from `LegalText.CurrentVersion`.

These pages are marked `[TermsNotRequired]`: Terms, Privacy, Agree, Sign in, Register, Not found and Error. Endpoints such as sign-out, pictures and the theme, and Blazor's own addresses, aren't component pages, so they pass. The redirect keeps the original path and query as `returnUrl`, through `LocalAddress`.

The agree page is a static form like the registration page. It works without script, and a failed submit keeps the page. `RefreshSignInAsync` reissues the cookie with the new claim before the redirect.

There is no new package and no migration.

## Verification

1. Web integration tests on the development database, as for F04–F10. They cover the redirect with an old version written straight into `users`, the agree flow, and a locked account.
2. A unit test reads `docs/legal/*.md` from the repository root and compares the version line.
3. Release build, the whole test suite, the documentation checker and the prototype checker.
4. Headless Edge screenshots of `/terms`, `/privacy` and `/account/agree` on the running app, at phone and desktop widths in both themes.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation |
|---|---|---|
| The legal text exists twice (docs and Razor) | The two could drift | LP14 checks the version. The page header comment names the source file, and a text change needs a new version |
| A redirect on every page for people who haven't agreed | Interactive pages they had open keep working until reload | Acceptable: the gate is a legal prompt, not access control |
| Owner-approved text without a lawyer | Legal risk | Recorded in D10 and the drafting notes; a review before the public launch is advised |

Documentation updates: SDD (sign-in, sessions and authorization; render modes; navigation), DESIGN, STATUS, TEST-PLAN and the docs index.
