# Chat tasks

Scope and scenarios: [spec](spec.md). Sequence: [plan](plan.md). Evidence: [2026-11-16 record](../../docs/delivery/evidence/2026-11-16-f07-chat.md).

## Definition

- [x] Review the PRD (R06, R07, R10, BR03–BR05, BR18), the SDD (render modes, live chat, navigation, DI), DESIGN (conversation, thread and connection states) and the prototype (`convListHTML`, `messagesHTML`, `composerHTML`, `threadParts`, desktop panes).
- [x] D06 decided on 2026-11-16: one conversation per client and accountant; messages up to 2,000 characters. Email (R08) left for its own feature.
- [x] Spec CH01–CH10 and plan.

## Implementation

- [x] Domain: `Conversation`, `ChatMessage`, `ChatLimits`, the message text rule.
- [x] `ChatService` (start or find, idempotent send, list, thread, mark read, unread count, 20 messages a minute) and `ChatStore`; migration `AddChat`, applied to the development database on 2026-11-16.
- [x] `ChatNotifier` (in-process, one server) and the live `ChatPanes` panel with its composer script: draft and message ID kept in the tab, Enter to send, message ideas, send states, Retry, the check after an unknown outcome.
- [x] Static pages `/chats`, `/chats/{id}` and `/chats/new/{accountantId}` (replacing the "coming soon" page), with access checks and the not-found page's chat states.
- [x] "Chats" in the site bar and phone tab bar, with the unread count; "View profile" in the phone app bar.
- [x] Connection notice restyled as the prototype's offline banner, not a modal.
- [x] Client pictures visible to the accountants they chat with (R24).
- [x] PRD D06, BR03, BR04, R24; SDD; DESIGN; TEST-PLAN TP24; SETUP; STATUS; the docs index; the F06 spec.

## Verification

- [x] 16 rule tests (6 of them for the text rule) and 6 web tests; whole suite 190 passed, 0 failed, 0 skipped with `-m:1`; Release build with 0 warnings.
- [x] Browser, two Edge sessions against the real app: 14 of 14 (visitor draft through reload and sign-in, send, live reply, unread marks, thread draft through a reload, server restart with text kept and nothing sent later); screenshots at 390 and 1440 px, light and dark.
- [ ] Real phones, other browsers and a screen reader; the on-screen keyboard and the send button (DESIGN).
- [ ] Two tabs of the same account (TP05) and an expired session during a chat (TP10).
- [ ] Owner walkthrough (G9).
