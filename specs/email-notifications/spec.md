# F14: Email notifications

Governing requirements: [PRD](../../docs/PRD.md) R08, BR08, D18 (decided 2026-11-23), G11. Contracts: [SDD](../../docs/SDD.md#live-chat-and-email-notifications). UI: the prototype's `emailRow` (Account) and `infoPage('email-off')`. Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A client or accountant is told by email about a new message while they're away, and can turn these emails off from the email or from Account. The user chose on 2026-11-23 to build it now, before a Brevo account exists, with a development sender.

**Behaviour:**
- **Queuing:** saving a message queues an email for the other person in the same transaction, unless one was already queued since they last opened that conversation (D18).
- **Sending:** after 2 minutes a background worker sends it, unless the recipient has opened the conversation since, turned emails off, or deleted their account.
- **Failures:** a failed send waits (1, 5 and 30 minutes, then 2 hours) and gives up after 6 tries. The message itself is never affected.
- **Content (BR08):** the sender's first name, a link to the conversation, and a signed link that turns emails off without signing in. Never the message text.
- **Senders:** Brevo when `Email:Brevo:ApiKey` and `Email:SenderAddress` are set. Otherwise, in development, a text and an HTML file per email in `artifacts/emails`. On another site without Brevo, jobs are skipped and logged.
- **The setting:** an "Email notifications: On/Off" row on Account. `/email/off` turns emails off from a link and can turn them back on.

**Exclusions:**
- Warnings before deleting unused accounts (D14); the job and email are separate work.
- An email to the owner about reports (D22).
- Microsoft accounts.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| EM01 / D18 | A client sends two messages; the accountant opens the chat; the client sends a third | One job, then a second one; the reply queues one for the client | Web test |
| EM02 / R08, BR08 | A job is due | One email to the account's address: "New message from Olive on Tax Buddy", with both links and none of the message text. The job is Sent, and no address appears in the logs | Web test |
| EM03 / R08 | The recipient opened the conversation first, or turned emails off | The job is Skipped; no email | Web test |
| EM04 / SDD | The email service fails | The job stays pending with 1 attempt and a later due time; the message stays | Web test, unit test |
| EM05 / R08 | A visitor follows the email's link | Emails are off, without signing in; the page doesn't show the address; "Turn emails back on" works and coming back doesn't turn them off again | Web test |
| EM06 / R08 | A signed-in person on Account | The row shows the setting and address; Turn off and Turn on work | Web test |
| EM07 / SDD | A broken link; a post with a broken link and no session | "This link doesn't work any more"; the post is refused | Web test |
| EM08 / R09 | An account with pending emails is deleted | Its pending jobs are gone | Web test |

## Evidence limits

No real email has been sent: there's no Brevo account yet (G11). Results are in [STATUS](../../docs/delivery/STATUS.md).
