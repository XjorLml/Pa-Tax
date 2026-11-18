# F10: Report and block

Governing requirements: [PRD](../../docs/PRD.md) R15, BR12, BR19 (new) and D22 (partly decided below); BR05 (conversations are private), BR09 (reviews). Contracts: [SDD](../../docs/SDD.md) render modes (the report page is static; Block lives in the chat panel), authorization, "Reports and blocks". [DESIGN](../../docs/DESIGN.md) "Report and block". Design origin: the prototype's report dialog (`overlaysHTML`, `REPORT_REASONS`), "Report this profile" (`profileBodyHTML`), "Report this review" (`reviewsHTML`), the chat's `thread-tools`, the block dialog and the blocked notes (`threadParts`) ([CLAUDE.md](../../CLAUDE.md)). Builds on [F07](../chat/spec.md) and [F09](../reviews/spec.md). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A signed-in user reports a profile, a review or a conversation to the site owner, with a reason. Either participant in a chat can block the other and undo it.

**Authorization and decisions** (2026-11-18):
- **The request:** the user said "proceeed on implementing r15, provide recommendations and choices onn the decisions needed".
- **What the user chose:**
  - **D22, partly:** reports are stored in the database and read by the owner in Supabase's Table Editor ([RELEASE](../../docs/RELEASE.md)). There is no admin page, as the PRD excludes one. Email to the owner comes with R08 once Brevo is set up (G11). Response times and removal steps stay open.
  - **Report form:** one static report page for profiles, reviews and conversations, instead of the prototype's dialog.
  - **Block scope:** messaging **and reviews** (BR19).
    - While either side has blocked the other, neither can send in that conversation.
    - A client the accountant has blocked can't write or edit a review of that accountant. They can still delete one.
  - **Limits:** one report per person per item, and at most 10 reports per account per day.
- **Adopted from the prototype:** the five reasons, "Anything to add?" up to 600 characters, and the wording.

**In scope:**
- The report page `/report/{profile|review|conversation}/{id}`, with "Report this profile", "Report this review" and the chat's "Report".
- Block in the chat panel: a confirmation, the blocked notes, Unblock, and live updates on both sides.
- The review rule (BR19).

**Exclusions:**
- The owner's handling and removal steps (D22).
- Email (R08).
- A list of blocked people.
- Blocking from a profile without a conversation.
- Hiding reported content: it stays visible until the owner acts (BR12).

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| RB01 / R15 | A visitor presses "Report this profile" | "Sign in to report", which returns to the report page after sign-in. For conversations nothing about the chat is shown | Rule tests, Web.Tests, browser |
| RB02 / R15, BR12 | A signed-in user picks a reason, optionally adds text, and sends | Back on the profile, review or chat with "Thanks. Your report was sent to the Tax Buddy team."; the report is stored with reporter, kind, item, profile, reason, text and time; the reported person sees nothing | Rule tests, Web.Tests, browser |
| RB03 / R15 | No reason, or text over 600 characters | "Choose a reason." or the length message; everything typed stays | Rule tests, Web.Tests, browser |
| RB04 / F10 limits | The same item again; an 11th report in a day | "You've already reported this"; "You've sent a lot of reports today…" with the text kept | Rule tests, Web.Tests |
| RB05 / BR12, BR05 | Own profile or review; someone else's conversation; an unknown item | "You can't report your own …" (403); "This isn't available" (404) with nothing revealed | Rule tests, Web.Tests |
| RB06 / R15 | A participant presses Block, then confirms | The dialog asks first and Escape or Cancel changes nothing. Then the blocker sees "You blocked {first}. They can't message you." with Unblock, and the other side sees "You can't send messages in this conversation." live, without a reason. Neither can send, including through "Message" | Rule tests, Web.Tests, browser |
| RB07 / R15, R10 | The blocked side had text typed; later Unblock | The text stays in the tab; after Unblock the composer comes back with it on both sides | Browser |
| RB08 / BR19 | The accountant blocked a client who reviewed them | "You can't review {first}" with no form; the existing review stays visible and can be deleted; a block set by the client changes nothing about their reviewing | Rule tests, Web.Tests, browser |
| RB09 / DESIGN | Phone and desktop, light and dark | The prototype's reason chips, report rows, thread tools, dialog sheet and notes | Browser |

## Deviations from the prototype

Recorded in [DESIGN](../../docs/DESIGN.md):
- **Reporting:** a page rather than a dialog, so it works without script. The reasons are real radio inputs drawn as the prototype's chips. The prototype's toast becomes a "Thanks" line on the page the reporter returns to.
- **The blocked client's review page** (BR19) has no prototype counterpart. It uses the state-message layout.

## State and dependencies

- **New table `reports`:**
  - reporter, kind, item ID, profile ID, reason, details and time;
  - unique per reporter, kind and item;
  - no foreign key to the item, so a report outlives the content;
  - the foreign key to the reporter is Restrict until D07.
- **Conversations:** `client_blocked_at` and `accountant_blocked_at`. Sending and blocking lock the conversation row, so no message lands after a block.
- **No content snapshot:** the owner reads the item from its own table (D22).

## Evidence limits

Rule and web tests use the development database. The live block, the draft and the no-script report are checked in a browser. Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
