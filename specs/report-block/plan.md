# Report and block plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: built 2026-11-18, uncommitted.

## Affected modules

| Module | Change | Scenario |
|---|---|---|
| `Domain/Report.cs`, `Domain/Chat.cs` | `Report`, `ReportKind`, `ReportReason`, `ReportLimits`; `Conversation.Block`, `Unblock`, `IsBlocked`, `IsBlockedBy` | RB02, RB06 |
| `Application/Reports` (`ReportModels`, `ReportService`), `Data/ReportStore.cs` | Who may report what, validation, limits, the return address; the event "Report received" with the kind only | RB01–RB05 |
| `Application/Chat`, `Data/ChatStore.cs` | `SetBlockAsync`; `SendOutcome.Blocked`; the row lock when storing a message or a block; block flags on `ThreadView` | RB06 |
| `Application/Reviews`, `Data/ReviewStore.cs` | `ReviewAccess.Blocked` from the accountant's block; delete still allowed | RB08 |
| `TaxBuddyDbContext`, migration `AddReportsAndBlocks` | Table `reports`; two conversation columns | RB02, RB06 |
| `Components/Pages/ReportPage.razor`, `NotFound.razor` | The report page and "This isn't available" | RB01–RB05 |
| `AccountantProfilePage.razor`, `ReviewPage.razor`, `ChatThreadPage.razor` | Report links, thanks lines, the blocked review state | RB02, RB08 |
| `Components/Chat/ChatPanes.razor` (+ `.razor.js`) | Thread tools, the block dialog, the notes, the composer re-attached after Unblock | RB06, RB07 |
| `Styles/app.css` | `.reasons`, `.chip.reason`, `.report-row`, `.rev-report`, `.thread-tools`, `.overlay`, `.sheet`, `.btn-danger`; tokens `--on-danger` and `--overlay` from the prototype | RB09 |

## Approach

- **Report page:** like the review page (F09), a static named form. A successful send redirects to where the report came from.
- **Blocking:** happens inside the live chat panel. The service checks the participant, stores the block under a row lock, and tells both panels through the notifier.
- **Sending:** locks the same row, so a send racing a block is refused, and the draft stays in the tab.

**Data:** additive migration. Rollback: `dotnet ef database update AddReviews`.

## Verification

- **Rule tests:** validation, access, limits, blocking, sending while blocked, the review rule, and a failed save.
- **Web tests:** the report page states and send, the links, and the prerendered blocked states.
- **Browser check:** two Edge sessions for the live block, plus a report without script.
- **Afterwards:** the docs and the evidence.
