# Google mark on the sign-in button: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-24, about 09:20–09:50 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code), at the user's request "add google logo on the sigin button".
- Requirement and scenario: PRD R04; [F04](../../../specs/google-sign-in/spec.md) GS10a; [DESIGN](../../DESIGN.md) sign-in row (a deviation from the prototype's `signinHTML`, which has no mark, by the owner's request).
- Source revision: `ff4f0df` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus this uncommitted change.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Components/Shared/GoogleMark.razor` (new) | `bf1b24385399` |
  | `src/TaxBuddy.Web/Components/Pages/SignInPage.razor` | `c92b73730b12` |
  | `src/TaxBuddy.Web/Styles/app.css` | `7dfad410bdb7` |
  | `tests/TaxBuddy.Web.Tests/SignInTests.cs` | `016df6b71859` |

- Environment: WSL2 with .NET SDK 11.0.100; Windows Edge (headless) driven by Windows Node 22.12.0 through the DevTools protocol, from a scratch script outside the repository.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings | 0 Warning(s), 0 Error(s) | Pass |
| `dotnet test --filter SignInWithoutDatabaseTests\|MicrosoftSignInPageTests` | All pass, with the new check: the button holds the mark, hidden from screen readers, before the text | 19 passed | Pass |
| `dotnet test --filter LayerRules` | All pass | 4 passed | Pass |
| `dotnet test --filter SignIn` (with database tests) | All pass | Every database test failed at fixture start: `NpgsqlException: The operation has timed out`. On this network the database host resolves (IPv4 and NAT64 IPv6), TCP to 5432 and 6543 opens and HTTPS to the same host answers in 0.2 s, but the Postgres TLS handshake stalls (`openssl s_client -starttls postgres` hung until stopped), and IPv6 is unreachable from WSL | Not run: network |
| The same handshake, retried later on 2026-11-24 | Completes | `CONNECTED`, TLSv1.3; the host now resolves to IPv4 addresses only (another network) | Pass |
| `dotnet test TaxBuddy.slnx -c Release -m:1` | All pass | TaxBuddy.Tests 188 passed. TaxBuddy.Web.Tests 147 of 161: all 14 tests of `ProfilePageTests` and `ReportPageTests` failed in about 1 ms at fixture start; the error text wasn't kept | Partial |
| `dotnet test --filter ProfilePageTests\|ReportPageTests` straight after | All pass | 14 passed. With the run above: 349 of 349 passed. The first failure wasn't reproduced and its cause is unknown | Pass on rerun |
| A copy of the site on `http://localhost:5299` (Development, a dummy Google key, an unreachable database, email worker off), `/signin` captured at 390 × 844 light and dark and 1280 × 800 light | The mark before the text, aligned, in both themes | As expected in all three; the user's own site on port 5234 wasn't touched | Pass |

## Restoration and limits

- Cleanup: the copy on port 5299 was stopped (the port no longer answers) and Edge's temporary profile was removed. Screenshots stay in the session's scratch folder, outside the repository.
- Not run: a phone browser. The database-backed tests ran once the network allowed it (above).
- The mark follows Google's sign-in branding rules (its own colours, unchanged); no Google asset file is added, only the inline mark.
