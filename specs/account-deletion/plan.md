# Account deletion implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: as built 2026-11-23.

## Affected modules and contracts

| Module / source path | Change | Scenario |
|---|---|---|
| `Data/TaxBuddyUser.cs`, migration `AddAccountDeletion` | `DeletedAt` (`users.deleted_at`, nullable) | AD03 |
| `Domain/AccountRole.cs` | `DeletedAccount.DisplayName = "Deleted user"` | AD03, AD05 |
| `Account/AccountService.cs` | `DeleteAccountAsync`: one transaction, then the picture, then sign-out | AD03–AD09 |
| `Account/AccountSetup.cs` | Security check every minute (`SecurityCheckInterval`) | AD03 |
| `Application/Chat/ChatModels.cs`, `ChatService.cs`, `Data/ChatStore.cs` | `PeerDeleted` from `users.deleted_at` | AD05, AD06 |
| `Components/Chat/ChatPanes.razor` | The deleted note in place of the composer; no Block | AD05, AD06 |
| `Components/Pages/DeleteAccountPage.razor` (new), `AccountPage.razor` | The two steps; the red menu item | AD01–AD03, AD09–AD11 |
| `Styles/app.css` | `.delete-card`, `.mi.danger` | AD01, AD02 |
| Legal pages and documents; `LegalText` | Deletion on the site; version `2026-11-23` | — |
| `tests/TaxBuddy.Web.Tests/AccountDeletionTests.cs` (new) | AD01–AD11 | all |

## Approach

The account row stays, made nameless, instead of being deleted. Messages, conversations and reports keep their links to it, so no foreign key or migration beyond `deleted_at` is needed. "Deleted user" then reaches chats through the existing name lookup. Closing a deleted person's conversations by setting their side's block reuses the existing send rule and its row lock, so a send racing the deletion either lands first or is refused. The picture is deleted after the commit: a storage failure then leaves an unreachable file (logged), never a broken account. No new package.

## Verification

1. Web tests on the development database (throwaway schemas). The failure case reuses a session on a host whose database is unreachable.
2. Release build with 0 warnings; the whole suite; documentation and prototype checkers.
3. Edge: the visitor view of `/account/delete` and no overflow at 320 and 390 px. The signed-in steps need Google sign-in and are covered by web tests.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation |
|---|---|---|
| The row stays with an ID, role and timestamps | It's a pseudonymous record, not full erasure | The Privacy Policy says what remains; there's no name, email, picture or sign-in link |
| Security check every minute | A small account read per signed-in person per minute; when the database is down, a check fails as pages already do | Worth it, so a deleted or closed account can't keep acting for 30 minutes |
| Picture deletion after the commit | An orphan file if storage fails | Event 1403, and a RELEASE step to remove it by hand |
