# F07: Chat between clients and accountants

Governing requirements: [PRD](../../docs/PRD.md) R06, R07, R10, with BR03, BR04, BR05 and BR07; R24 (client pictures shown to their accountants). Contracts: [SDD](../../docs/SDD.md) render modes, live chat, navigation, DI and lifecycle. [DESIGN](../../docs/DESIGN.md) conversation, thread and connection states. Design origin: the prototype's `convListHTML`, `messagesHTML`, `composerHTML` and `threadParts`, plus its desktop panes ([CLAUDE.md](../../CLAUDE.md)). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A signed-in client starts a text chat with an accountant from the accountant's profile and continues it later. The accountant sees their conversations, reads new messages and replies. New messages appear without reloading, and unread conversations are marked.

**Authorization and decisions** (2026-11-16):
- The user said: "proceed to implementing f07 then go for your recommendations".
- D06 is decided: **one ongoing conversation per client and accountant**, and messages up to **2,000 characters** (the prototype's proposal).
- **Email (R08) is a separate feature** after Brevo (G11).

**In scope:**
- `/chats`: the conversation list, with the thread beside it on desktop.
- `/chats/{id}`: the thread with its composer.
- `/chats/new/{accountantId}`: a new chat. Visitors can type; "Sign in to send" keeps the text through sign-in.
- Live updates for both participants; unread counts in the list and the navigation.
- Send states: sending, sent, failed with retry, checking.
- A repeated send never duplicates a message.
- The draft is kept in the tab across a reload and a lost connection.
- A rate limit on sending.
- Client pictures visible to their accountants.

**Exclusions:**
- Email (R08).
- Report and block (R15), which the prototype shows as "Report" and "Block".
- What happens when an account is deleted (D07).
- Attachments (excluded by the PRD).
- Editing or deleting messages.
- Typing indicators and read receipts.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| CH01 / R06, BR03 | A signed-in client opens "Message" on a listed accountant's profile and sends a message | The conversation is created and the message shows as sent. The accountant sees it in their list with a "1 new" badge and in the navigation count | Rule and Web.Tests, browser |
| CH02 / R06, D06 | The client opens "Message" again for the same accountant | They go to the existing conversation; there is never a second one | Rule tests, Web.Tests |
| CH03 / R07 | The accountant opens the conversation and replies | Unread clears for the accountant; the reply appears for the client without a reload; the client's list shows it as new | Rule tests, browser (two sessions) |
| CH04 / BR04 | Empty or whitespace text, over 2,000 characters, or the same message sent twice by a retry | Send is unavailable or refused with a message; a retry stores one message | Rule tests |
| CH05 / R06 | A visitor types on the new-chat page and presses "Sign in to send" | Sign-in; after it, the same page has the text, ready to send | Browser |
| CH06 / R10, BR18 | The save fails, the connection drops, or the tab is reloaded with text typed | A failed send shows "Not sent. Retry", and retry works. Text typed and not yet sent survives a reload. Nothing is sent later in the background | Rule tests, browser |
| CH07 / BR05, R11 | Someone opens a conversation address they aren't part of, or uses an unknown ID | "This conversation isn't available", with no names or messages | Web.Tests |
| CH08 / BR03 | An accountant opens a new-chat page; or the accountant's profile is unlisted or has no account | "Only clients can start chats"; or "This accountant can't receive messages" | Web.Tests |
| CH09 / SDD abuse limits | More than 20 messages in a minute from one account | "You're sending messages quickly. Wait a moment and try again"; nothing is stored | Rule tests |
| CH10 / DESIGN | Phone and desktop, light and dark, keyboard | The prototype's layout: list and thread panes on desktop, one screen at a time on phones; day separators; bubbles; Enter sends and Shift+Enter adds a new line; the count shows; states are announced | Browser |

## State, dependencies, and unresolved decisions

**Data:**
- `conversations`: client account, accountant account, created time, last-message time, and each side's last-read time. Unique per pair (D06).
- `messages`: an ID made when the composer is shown (so a retry stores one message), the conversation, the sender, the text and the time sent.
- Unread messages are the other side's messages after your last-read time.

**Live updates:** an in-process notifier tells the open chat panels of both participants to reload from the database (SDD: one server instance).

**Still open:** R08 email and D18; R15 report and block; D07 deletion; scaling beyond one instance (shared notifier).

## Deviations from the prototype (recorded in DESIGN)

- A failed message shows "Not sent. Retry" as in the prototype, and its text also stays saved in the tab, so a reload puts it back in the field (BR18).
- On wide screens the panes fill the window below the site bar; the site footer (Privacy, Terms: R16) stays below them instead of being left out.
- The connection notice is the prototype's offline banner, fixed to the top of the window.
- New state, not in the prototype: a listed profile with no account behind it (the development seed) says "This accountant can't receive messages yet".
- Left out until their features exist: the accountant's email note in the list (R08), "Report" and "Block" (R15), the deleted-accountant note (D07).

## Evidence limits

Rule and web tests use the development database; live updates and the session-storage draft are checked in a browser. Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
