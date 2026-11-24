# Prototype alignment fixes: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-24, about 13:30–14:45 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user had read the [comparison](2026-11-24-prototype-site-comparison.md) and its suggested order, then said: "yes fix the first two". The first was the two bugs and the four lapsed items; the second, the layout problems (workspace width, the current navigation item, and not-available states keeping their frames).
- Requirements and scenarios: PRD R07, R08, R09, R19–R22, BR15. [F03](../../../specs/client-services/spec.md#prototype-alignment-2026-11-24) CS14–CS17; [F07](../../../specs/chat/spec.md) deviations; [F12](../../../specs/account-deletion/spec.md); DESIGN's site shell, directory, chat, deletion and service-workspace rows.
- Source revision: `ff4f0df` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus earlier uncommitted work and this change.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `src/TaxBuddy.Web/Components/Work/WorkFrame.razor` (new) | `cb509b5e0bb6` |
  | `src/TaxBuddy.Web/Components/Layout/NavSection.cs` (new) | `12c0ef253873` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `cffd2fb931d3` |
  | `src/TaxBuddy.Web/Components/Shared/SiteUser.razor` | `5b3916ceb4d2` |
  | `src/TaxBuddy.Web/Components/Shared/HowItWorksSteps.razor` | `d92c012d9fcd` |
  | `src/TaxBuddy.Web/Components/Chat/ChatPanes.razor` | `3e8ebeed0111` |
  | `src/TaxBuddy.Web/Components/Chat/ChatDisplay.cs` | `4c7ff5f6c98e` |
  | `src/TaxBuddy.Web/Components/Pages/NotFound.razor` | `d448133058b2` |
  | `src/TaxBuddy.Web/Components/Pages/WorkspacePage.razor` | `e63aaa2f51d8` |
  | `src/TaxBuddy.Web/Components/Pages/ServicePage.razor` | `3c99cfbf502f` |
  | `src/TaxBuddy.Web/Components/Pages/ClientServicesPage.razor` | `a9a992cd4f0e` |
  | `src/TaxBuddy.Web/Components/Pages/RequestServicePage.razor` | `0f75df5a7f92` |
  | `src/TaxBuddy.Web/Components/Pages/NewChatPage.razor` | `4d6c073a2e3b` |
  | `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `4fa6826e1eb0` |
  | `src/TaxBuddy.Web/Components/Pages/DeleteAccountPage.razor` | `b0c88bebb420` |
  | `src/TaxBuddy.Web/Components/Pages/DirectoryPage.razor` | `5e743cfc1bba` |
  | `src/TaxBuddy.Web/Styles/app.css` | `10c644a63f75` |

- Environment: WSL2 with .NET SDK 11.0.100; the Supabase development database (throwaway schemas) for the tests; Windows Edge 158.0.4370.33, headless.

## What changed

| Item | Before | After |
|---|---|---|
| Service history times (bug) | Today's entries: "10:42 AM · 10:42 AM" | "10:42 AM" today; "Yesterday · 10:42 AM" or "Nov 16 · 10:42 AM" before (`ChatDisplay.ShortWithTime`) |
| Failed add or remove (bug) | Green success line | The prototype's red `.work-error` alert, on the profile and in My accountants |
| "See how listing works" (lapsed) | Left out until For accountants existed | Under How it works, in the phone toggle and the wide-screen section (`.hero-foot`) |
| Chat list email note (lapsed) | Left out until R08 | Accountants: "New client messages are also emailed to you." or "Email notifications are off." with "Change"; read once per page, left out if it can't be read |
| Deleted-account note in chats (lapsed) | Built in F12, but DESIGN and the chat spec still said it was left out | Documents corrected |
| Delete account list (lapsed) | "Your profile picture." | "Your profile picture and email notifications for this account." |
| Workspace pages' width | The 680 px single column under a heading band | The prototype's frame: 1040 px, breadcrumb, page h1 (`WorkFrame`); on phones the app bar title only |
| Current site bar item | Request a service and review: "Find an accountant"; client and service pages: none; agree: the account link | The prototype's map (`NavSection`): workspace pages and Request a service mark Clients or My accountants; review and agree mark nothing |
| Not-available states | Single centred pages for chats; the review state without its panel; workspace states without a heading; Request a service answered 200 with a page of its own | Chat states in the detail pane beside the list (static); review state in the "Write a review" panel with "Back to profile"; workspace states in the workspace frame; Request a service answers 404 with "This workspace is not available"; visitors at malformed workspace or chat addresses are asked to sign in |

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings | 0 Warning(s), 0 Error(s) | Pass |
| First full run after the changes | Find what the changes break | 1 failure: the deletion test asserted the old "no email" state. Changed to assert the new line | Expected |
| New and changed tests | Pass | Unit: `ChatDisplayTests` (3), `NavSectionTests` (15). Web: the workspace frame, current item, request 404, malformed addresses and red failure (`ClientServicesTests`); the email note and chat panes (`ChatPageTests`); the review panel (`ReviewPageTests`); the directory line (`DirectoryPageTests`); the deletion line (`AccountDeletionTests`) | Pass, after the fixes below |
| Fix found by the tests | — | The navigation links are now plain links, so they carry the layout's scoped-CSS attribute; the tests match it with a pattern. Request a service first set its 404 directly, which sends no body; it now calls `NavigationManager.NotFound()`, and the not-found page draws the workspace frame | Fixed |
| `dotnet test TaxBuddy.slnx -c Release -m:1` | All pass | 374 passed (206 unit, 168 web); 0 failed | Pass |
| Rendered pages in Edge. A temporary test saved signed-in pages through the test host; it was then deleted. They were served locally with the built styles and fonts, in the dark theme: My accountants (client), Service progress (accountant), Chats (accountant), a missing conversation (client), a new chat as an accountant, a missing review page and the directory (visitor). Captured at 1280 px, and at 390 px in a frame | As described above | As expected. Breadcrumb and h1 at full width on desktop; the app-bar title only on phones; the history time shown once; the email note above the list; the chat states in the detail pane beside the list; the review state in its panel; "See how listing works" under the steps; the right site bar item marked | Pass |

## Restoration and limits

- Cleanup: both temporary test files were deleted; the local servers on ports 5312 and 5311 were stopped; Edge's temporary profile was removed. Screenshots stay in the session's scratch folder, outside the repository.
- Not run: a signed-in browser session against the running site, a phone browser, the light theme for these pages, and the live chat connection; the chat states are drawn without one.
- Not changed: the other differences in the comparison (wording, confirmations, the "Accountant added" line's place) wait for the user's decision.
