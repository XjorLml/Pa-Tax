# Chat implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: built 2026-11-16, uncommitted ([evidence](../../docs/delivery/evidence/2026-11-16-f07-chat.md)).

## Affected modules

| Module | Change | Scenario |
|---|---|---|
| `Domain/Chat.cs` (new) | `Conversation`, `ChatMessage`, `ChatLimits` (2,000 characters; 20 per minute), message text rule | CH01–CH04 |
| `Application/Chat/` (new) | `ChatService`: start or find, send idempotently, list, thread, mark read, unread total, rate limit. `IChatStore`; `IChatNotifier` | CH01–CH09 |
| `Data/ChatStore.cs` (new), migration `AddChat` | Tables, the unique pair, queries for the list with peer names and pictures | CH01–CH03, CH07 |
| `Services/ChatNotifier.cs` (new) | Singleton per-account subscriptions; thread-safe; pages marshal onto their own dispatcher | CH03 |
| `Components/Chat/ChatPanes.razor` (new, Interactive Server) | List and thread panes; live reload; send states; composer; draft in `ProtectedSessionStorage`; small JS module (`ChatPanes.razor.js`) for Enter-to-send and auto-grow | CH01–CH06, CH10 |
| `Components/Pages/ChatsPage.razor`, `ChatThreadPage.razor`, `NewChatPage.razor` (replaces `ChatComingSoon`) | Static pages that check access, set the phone bar and title, and hold the live island | CH05, CH07, CH08 |
| Layout: `MainLayout`, `PhoneTabs`, `SiteUser` | "Chats" in the site navigation and the phone tab bar, with the unread count | CH01, CH03 |
| `Application/Pictures`, `Data/AccountPictureStore.cs` | A client's picture is also visible to accountants who share a conversation with them | R24 |
| `Components/Layout/ReconnectModal.*` | Wording and styles in the prototype's voice (connection lost; your text is kept) | CH06 |

## Approach

**Render modes:** pages stay static, which allows access checks, 404-style states and the page title. The conversation area is one `@rendermode InteractiveServer` component that receives only IDs. It is prerendered, so the first response already shows the list and messages, and then it goes live. Visitors get a live connection only on the new-chat page (SDD).

**Sending:**
1. The message ID is created when the composer is shown.
2. On Send, the message appears with "Sending…".
3. The service stores it. If that ID is already stored, it returns the stored message, so a retry can't create a duplicate.
4. On an error, the panel checks whether the message is stored ("Checking…"), then shows sent, or "Not sent. Retry".
5. The draft in session storage is cleared only once a send is confirmed.

**Visitor on a new chat:** "Sign in to send" saves the draft under the new-chat key, then goes to `/signin?returnUrl=/chats/new/{id}`. After sign-in the panel restores the draft. If the pair already has a conversation, the page moves to it, and the draft moves with it.

**Data:** additive migration `AddChat`. Rollback: `dotnet ef database update AddProfilePicture`.

**As built, beyond the plan:**
- The composer script, not the circuit, owns the text and the message's ID, keeping both in the tab's session storage (`taxbuddy:chat-draft:thread:{id}` or `new:{profileId}`). So text typed while disconnected is what gets sent, and sending a restored draft again still stores it once. A send that gets no answer in 20 seconds says "You're offline…" and keeps the text.
- `ProtectedSessionStorage` wasn't used: the draft is the user's own text in their own tab, and the visitor's draft must survive the sign-in round trip.
- The prerendered data is carried to the live connection (`[PersistentState]`), so the panel doesn't flash empty while it loads again.
- Chat 404s go through the not-found page (`NavigationManager.NotFound()`), as the profile page does.
- Database foreign keys from chats to accounts refuse deletion until D07 decides what deletion does to the other person's copy.

## Verification

1. Rule tests: message text rule; start, find and the pair rule; idempotent send; participant checks; unread counts; rate limit; picture visibility for a client's accountants.
2. Web tests: pages for each role and state; a non-participant gets "isn't available"; the navigation count.
3. Browser: two sessions (client and accountant) exchanging messages live; the visitor's draft through sign-in; a reload keeping the draft; phone and desktop, light and dark.
4. Evidence, STATUS, PRD D06, SDD, DESIGN, TEST-PLAN.
