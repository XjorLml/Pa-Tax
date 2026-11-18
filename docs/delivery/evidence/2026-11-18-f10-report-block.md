# F10 report and block execution evidence

## Identity and attribution

- **Date/time and timezone:** 2026-11-18, +08:00. The migration was created at 22:30; the final Edge check ran at 22:41.
- **Observer:** agent-observed (Claude Code).
- **User's instruction:** "proceeed on implementing r15, provide recommendations and choices onn the decisions needed". The user chose:
  - reports stored in the database (D22, partly decided);
  - one static report page;
  - blocking covers messaging and reviews (BR19);
  - one report per item and 10 a day.

  The user then approved the plan.
- **Requirements/scenarios:** PRD R15, BR12, BR19, D22 (partly decided); [F10 RB01–RB09](../../../specs/report-block/spec.md#acceptance-scenarios).
- **Source revision:** branch `feature/f02-f04-theme-switch-google-sign-in` at `55e0f12`, with F10 uncommitted. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Domain/Report.cs` | `13cd5db6651b` |
  | `src/TaxBuddy.Web/Domain/Chat.cs` | `b1d51f71b5a8` |
  | `src/TaxBuddy.Web/Application/Reports/ReportModels.cs` | `b1323291adce` |
  | `src/TaxBuddy.Web/Application/Reports/ReportService.cs` | `62f1f41e4f48` |
  | `src/TaxBuddy.Web/Application/Chat/ChatModels.cs` | `d9c4599692e8` |
  | `src/TaxBuddy.Web/Application/Chat/ChatService.cs` | `7d55794dd295` |
  | `src/TaxBuddy.Web/Application/Reviews/ReviewService.cs` | `5167d8f4cc3c` |
  | `src/TaxBuddy.Web/Data/ReportStore.cs` | `b24246d84ccf` |
  | `src/TaxBuddy.Web/Data/ChatStore.cs` | `07f83ec0da3f` |
  | `src/TaxBuddy.Web/Data/ReviewStore.cs` | `5bead439e142` |
  | `src/TaxBuddy.Web/Data/TaxBuddyDbContext.cs` | `dbca47882fde` |
  | `src/TaxBuddy.Web/Data/Migrations/20261118143026_AddReportsAndBlocks.cs` | `55d5fdad5943` |
  | `src/TaxBuddy.Web/Components/Pages/ReportPage.razor` | `67cc35b747a3` |
  | `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `05f992576902` |
  | `src/TaxBuddy.Web/Components/Pages/ReviewPage.razor` | `4919f6580f8d` |
  | `src/TaxBuddy.Web/Components/Chat/ChatPanes.razor` | `3cd6cfb210ee` |
  | `src/TaxBuddy.Web/Components/Chat/ChatPanes.razor.js` | `9ce673953b08` |
  | `src/TaxBuddy.Web/Styles/app.css` | `094281789abb` |
  | `tests/TaxBuddy.Tests/Application/ReportServiceTests.cs` | `5d8765957537` |
  | `tests/TaxBuddy.Web.Tests/ReportPageTests.cs` | `71f89758f1d3` |

- **Environment:**
  - WSL2 Ubuntu 26.04 with .NET SDK 11.0.100;
  - Tailwind 4.3.3 through Windows Node 24.16.0;
  - two headless Edge 157.0.4341.60 sessions;
  - the Supabase development database.
- **Fixtures:** synthetic accounts with `example.test` emails, and invented message, review and report text.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` | 0 warnings, 0 errors | 0 and 0 (after a naming clash between the page `ReportPage` and a record of the same name, renamed `ReportFormView`) | Pass |
| `dotnet ef migrations add AddReportsAndBlocks`, then a review | Additive | `conversations` gains two nullable columns. Table `reports` gets a unique index (reporter, kind, item), an index on (reporter, created at), an index on created at, and a Restrict foreign key to `users` | Pass |
| `dotnet ef migrations has-pending-model-changes` | None | "No changes have been made to the model since the last migration." | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build -m:1` | No failures, nothing skipped | First run: TaxBuddy.Tests 154 passed; TaxBuddy.Web.Tests 104 passed; 0 failed, 0 skipped | Pass |
| `dotnet ef database update` | Applied | `20261118143026_AddReportsAndBlocks` recorded. "Done." | Pass |
| Edge check, three runs ([final results](2026-11-18-f10-report-block/results.json)) | RB01–RB03, RB05–RB09 | Run 1: 13 of 14. The blocker-side check read the panel 400 ms after pressing Block, before the save to the remote database finished; the block itself worked (the other side's note arrived live, and Unblock later found the blocker's note). The check now waits, as the others do. Run 2: 14 of 14. Screenshot review then found Report and Block at different heights at 390 px; fixed in CSS, and a check was added. Run 3: 15 of 15. App log: no exceptions | Fail, then pass |
| Screenshot review | The prototype's chips, thread tools, dialog sheet and notes | As expected after the alignment fix. In the 390 px Reviews picture the right edge is cut by the screenshot frame; the overflow check passed | Pass |

**Final Edge check:**
- **Visitor:** sees "Report this profile", and the report page asks to sign in.
- **Report form at 390:** five reason chips. Picking one marks it.
- **No reason:** "Choose a reason.", with the text kept.
- **Sent:** back on the profile with the thanks line.
- **Chat, two sessions:**
  - Report and Block show on both sides and line up.
  - Block asks "Block Liza?" with focus on Cancel; Escape closes it.
  - After confirming, the blocker sees "You blocked Liza. They can't message you." with Unblock.
  - The client sees "You can't send messages in this conversation." live, with no reason given.
- **The blocked client's review page:** "You can't review Nina", with a delete button and no form.
- **Unblock:** the client's composer returns live, with the text typed before the block.
- **The accountant reports the conversation:** back in the chat with the thanks line.
- **With page script disabled:** the accountant reports the client's review, and the thanks line shows under Reviews.

**What the tests cover:**
- **Rule tests:**
  - **Validation:** reasons are required and limited to the list; details are at most 600 characters.
  - **Reasons:** the prototype's five, in order.
  - **Blocking:** each side blocks and unblocks only their own block.
  - **Reporting a profile:** stored once with its reason, and a repeat is refused.
  - **Access:** own profile and own review are refused; the reviewed accountant may report a review; visitors are asked to sign in, and nothing about a conversation is revealed.
  - **Conversations:** only participants can report them; unknown items aren't available.
  - **Limits:** an invalid report stores nothing; 10 a day, with the 11th refused until a day later.
  - **Sending while blocked:** neither side can send in the thread, nor through "Message"; old messages stay readable; a block can't be lifted by the other side; unblocking restores sending.
  - **Reviews (BR19):** a client the accountant blocked can't review but can delete; the client's own block doesn't change their reviewing.
  - **Failures:** a failed save reports "failed".
- **Web tests:**
  - **Visitors:** the report link, and the sign-in state.
  - **Reporting a profile:** a missing reason (422, text kept), the redirect and thanks, the stored row, and "already reported".
  - **The owner:** no link, and a 403.
  - **Unknown items and others' conversations:** 404, with nothing revealed.
  - **"Report this review":** shown to the right users, and sent.
  - **A blocked conversation:** each side's note, no composer, and no reason given to the blocked side.
  - **The chat:** the thanks line after a report.
  - **The blocked client's review:** the review state, with delete through the form.

## Restoration and limits

- **Development database:**
  - The migration `AddReportsAndBlocks` was applied.
  - The three runs' synthetic accounts were deleted afterwards: 6 accounts with their profiles, 3 conversations, 6 messages, 3 reviews and 9 reports.
  - Afterwards: 2 accounts, 0 `example.test`, 10 profiles, 0 conversations, 0 reviews, 0 reports.
- The temporary host, scripts and Edge profiles were deleted; none are committed.
- **Not run:**
  - other browsers, phones and screen readers;
  - a send racing a block at the same instant. The row lock is in the code, but no test forces the race.
  - the report email (R08).

Acceptance: pending.
