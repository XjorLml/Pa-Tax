# F11 legal text and privacy prompts: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-21, about 14:40–15:20 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The decisions and the request to build are the user's ([drafting notes](../../legal/drafting-notes.md#decisions-made-on-2026-11-21)).
- Requirements and scenarios: PRD R16, R05, R06, R15; D09, D10, D19, D22. [F11](../../../specs/legal-pages/spec.md) LP01–LP14.
- Source revision: `1565e69` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus uncommitted F11 and documentation changes.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Account/TermsAgreement.cs` (new) | `4e503feb041b` |
  | `src/TaxBuddy.Web/Account/AccountService.cs` | `e2490297205e` |
  | `src/TaxBuddy.Web/Components/Pages/AgreePage.razor` (new) | `62041e229986` |
  | `src/TaxBuddy.Web/Components/Pages/TermsPage.razor` | `35176728f9ed` |
  | `src/TaxBuddy.Web/Components/Pages/PrivacyPage.razor` | `eac8a737185d` |
  | `src/TaxBuddy.Web/Styles/app.css` | `35284dddcdc7` |
  | `prototypes/index.html` | `fb525a7b5c74` |
  | `prototypes/taxbuddy.css` | `126f004f5837` |

- Environment: WSL2 with .NET SDK 11.0.100 and the Supabase development database (throwaway test schemas). Tailwind ran through the build. Headless Microsoft Edge 158.0.4370.33, driven from Windows Node v22.12.0.
- Preconditions and fixtures: synthetic accounts created by the tests in throwaway schemas. For the screenshots, the running app used the development database's synthetic seed. No real accounts were changed.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings, 0 errors; Tailwind writes the new rules | "Build succeeded. 0 Warning(s) 0 Error(s)"; `legal-short`, `legal-table`, `composer-note` and `agree-signout` found in `wwwroot/app.css` | Pass |
| `dotnet test TaxBuddy.slnx -c Release -m:1 --no-build` | Whole suite passes, including the new LP tests | TaxBuddy.Tests 156 passed, 0 failed, 0 skipped; TaxBuddy.Web.Tests 112 passed, 0 failed, 0 skipped. That is 268 in all, against 259 before; 9 are new | Pass |
| The app on `http://localhost:5234` (Development); `curl` of `/terms`, `/privacy` and `/account/agree` as a visitor | 200, 200; agree redirects to sign-in and back | 200; the version line "Version 2026-11-21 · Last updated 21 November 2026"; 302 to `/signin?returnUrl=%2Faccount%2Fagree%3FreturnUrl%3D%252F` | Pass |
| Edge via DevTools: horizontal overflow of `/terms` and `/privacy` at 320 and 390 px | 0 | 0 px in all four cases | Pass |
| Screenshots viewed: `/terms` on the phone, light (top and contents); `/terms` section 7 on the phone, dark; `/privacy` cookie table on the phone, light; `/privacy` purposes table on desktop, light; `/terms` on desktop, dark | Prototype layout; tables stacked on phones | As expected. A first attempt with Edge's command-line screenshot rendered wider than 390 px and followed Windows' dark mode; that was the tool, not the site, and the DevTools viewport was used instead | Pass (visual) |
| Prototype: syntax check and `node tools/check_prototype.mjs` with Edge | Pass | `PASS: 202 prototype checks; Edg/158.0.4370.33` on the final file | Pass |
| Prototype screenshots of the new "Updated terms" screen (Jump to → Updated terms), desktop light and phone dark with the error | Screen renders; error shown without the tick | The first attempt found a real defect: the new route had no fake address ("PATHS[r.name] is not a function"). Fixed by adding `agree: '/account/agree'`; afterwards both states render as designed | Pass after fix |

## Restoration and limits

- Cleanup: the local app was stopped. The Windows temporary folders `taxbuddy-site-shots` and `taxbuddy-proto-check` were deleted. Test schemas are dropped by the test fixtures.
- Not exercised in a browser on the site:
  - the agree page itself, since it needs a signed-in session and Google sign-in can't run headless (its markup, redirect, store and renew steps are covered by web tests);
  - the chat hint, since the seeded accountant has no account and shows "can't receive messages" (covered by a web test);
  - the editor hint and the report page wording (web tests only).
- Not run: keyboard use of the contents links, screen readers, zoom, Safari, Firefox, Brave and real phones.
- The owner's development account, registered on 2026-11-16 with `draft-2026-11-16`, will be asked to agree on its next visit. That's expected, but it hasn't been observed with a real Google sign-in.

Acceptance: pending the owner's walkthrough (G9). The legal text was approved by the owner without a lawyer's review (G13, D10).
