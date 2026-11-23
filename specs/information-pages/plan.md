# Information pages implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: as built 2026-11-23.

| Module / source path | Change | Scenario |
|---|---|---|
| `Components/Pages/AboutPage.razor`, `HelpPage.razor`, `ForAccountantsPage.razor` (new) | Static pages in the `narrow` panel, like Terms; `[TermsNotRequired]`, as they only inform | IP01–IP03 |
| `Components/Layout/MainLayout.razor`, `Pages/AccountPage.razor` | Footer and Account links | IP04 |
| `Components/Shared/SiteUser.razor` | "For accountants" for visitors, hidden below 900 px | IP04 |
| `Styles/app.css` | `.prose .lead`, `.faq`, `.benefits` from the prototype | IP01 |
| `prototypes/index.html` | About's "Who runs Tax Buddy" with example details; Help's licence question | IP02 |
| `tests/TaxBuddy.Web.Tests/InformationPagesTests.cs` (new) | IP01–IP04 | all |

No data, no new package. Verification: web tests (visitor pages need no database), the whole suite, the prototype checker, and Edge at 320, 390 and 1440 px in both themes.
