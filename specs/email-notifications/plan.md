# Email notifications implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: as built 2026-11-23.

| Module / source path | Change | Scenario |
|---|---|---|
| `Domain/Email.cs` (new) | `EmailJob` (pending, sent, skipped, failed), `EmailRules` (2-minute delay, retries, D18's rule) | EM01–EM04 |
| `Domain/Chat.cs` | `Conversation.ReadAtFor` | EM01, EM03 |
| `Data/TaxBuddyUser.cs`, `TaxBuddyDbContext.cs`, migration `AddEmailNotifications` | `users.email_notifications` (existing rows on), `email_jobs` (goes with its conversation) | all |
| `Data/ChatStore.cs` | The job is written in the message's transaction | EM01 |
| `Application/Email/EmailModels.cs` (new) | `IEmailSender`, `EmailMessage` | EM02 |
| `Services/Email/EmailSetup.cs`, `EmailOutbox.cs` (new) | Options; the Brevo, development and not-configured senders; signed links; the outbox and its worker | EM02–EM05, EM07 |
| `Account/EmailPreferences.cs` (new) | The setting and `POST /account/email` | EM05–EM07 |
| `Components/Pages/EmailOffPage.razor` (new), `AccountPage.razor` | `/email/off`; the Account row | EM05, EM06 |
| `Account/AccountService.cs` | Deletion drops pending jobs and turns the setting off | EM08 |
| Legal pages and documents | Emails described; Brevo listed (version `2026-11-23`, still unpublished) | — |
| Tests: `EmailNotificationTests.cs`, `Support/CapturingEmailSender.cs`, `TaxBuddy.Tests/Application/EmailTests.cs`; `FormPosts` pattern widened | EM01–EM08 | all |

## Approach

- **The outbox pattern from the SDD:** the job and the message commit together, so a crash can't lose one or send for an unsaved message. The worker runs every 15 seconds on the one server instance (SDD live updates), 20 jobs at a time.
- **"Not on the site":** approximated by the 2-minute wait. A person with the chat open marks it read, so their job is skipped.
- **Links:** they use `Email:SiteAddress`. The turn-off link carries the account ID protected by the site's data-protection keys, which must persist on a deployed site (SDD keys and secrets). Following it turns emails off at once, matching the prototype; a mail scanner that follows links can only turn emails off, which the person can undo.
- **No new package:** Brevo is called over HTTP.

## Risks and tradeoffs

| Risk | Impact | Mitigation |
|---|---|---|
| No Brevo account yet | No real emails | The development sender; G11 lists what to set |
| Data-protection keys not persisted on the host | Old links stop working after a restart | Already required for sign-in (G16); persist before deployment |
| Two-minute wait | Slightly later emails | It matches "when they are not on the site" without tracking presence |
