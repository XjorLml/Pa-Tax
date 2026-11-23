# Tax Buddy product requirements

**Authority:** approved product behavior, business rules, scope, and acceptance outcomes. Technical implementation belongs in [SDD](SDD.md); presentation in [DESIGN](DESIGN.md); evidence in [STATUS](delivery/STATUS.md).

Status: Draft — scope answers recorded, not yet approved for release. Decision owner: lumabanls (project owner). Last reviewed: 2026-11-11.

Tax Buddy is the current name in the prototype and planned project names. Name history: on 2026-11-11 the user authorized executing the design brief, including delegating the name/logo choice, and the agent selected Kuwenta to replace the working name AccApp; on 2026-11-12 the user instructed renaming the app to Tax Buddy ([F02](../specs/brand-design-refresh/spec.md)). These are branding changes, not formal approval of this PRD or release. Domain selection remains [D01](#open-decisions).

Platform change, 2026-11-11: the owner changed Tax Buddy from a .NET MAUI app for Android and Windows to a .NET web application ([ADR 005](adr/005-web-platform-blazor.md)). Requirement IDs are unchanged. R08 changed from push notifications to email, and decisions D02 and D13 were closed.

Stack decisions, 2026-11-11 ([ADR 007](adr/007-supabase-brevo-container-host.md)): the database is Supabase, the email service is Brevo, and the supported browsers are Chrome, Edge, Firefox, Brave and Safari. Vercel was ruled out as the host, and the owner then approved Render with the rest of the technology stack ([SDD](SDD.md#technology-stack)).

Product direction, 2026-11-11 ([ADR 008](adr/008-portfolio-ratings-no-approval.md)): Tax Buddy is a portfolio site for accountants with client ratings, and accountants are still not approved or verified. Requirements R12–R18 and rules BR09–BR13 were added, and R08 now covers clients.

Service-work expansion, 2026-11-11: the user authorized updating documentation and the prototype for client lists, added accountants, per-client service checklists, shared progress and service documents ([ADR 009](adr/009-private-service-workspaces.md), [F03](../specs/client-services/spec.md)). R19–R23 are approved feature categories; the owner subsequently confirmed private saving followed by a service request and accountant acceptance. Remaining lifecycle and file policies stay open in D24–D28. No production file-storage provider or release is approved by this task.

Directory decisions, 2026-11-12: the user instructed adopting the prototype's proposals for [D04 and D05](#open-decisions) "for now", and following the review recommendation for conflict C03 ([docs index](README.md#history-and-unresolved-conflicts)): the directory is a static page whose search and service filter are submitted as a plain form. Both decisions are provisional and can be revisited. They do not approve this PRD.

Legal and privacy decisions, 2026-11-21: the user answered the open questions raised by the [legal drafts](legal/drafting-notes.md) in a question session. D07, D09 and D19 were decided, and D10, D14 and D22 partly; the rows below were updated.

The approved product changes are built in the prototype first and in the site in a follow-up:
- The registration box reads "I agree to the Terms of use and have read the Privacy policy".
- A hint above the chat box warns against sharing TINs, tax returns or ID numbers.
- The profile editor's title hint discourages licence numbers.
- Signed-in users agree to a materially changed version at their next sign-in, after 30 days' notice.
- An account unused for 2 years is deleted after a warning email (needs R08).

Other decisions:
- Accounts are for people 18 and older.
- The operator will be a sole proprietorship registered with the DTI, and its owner the Data Protection Officer.
- The site is free now; any future fee is announced in advance and charged only with agreement.
- Testers' accounts carry over to the public site.

These decisions don't approve this PRD.

Later on 2026-11-21 the user gave the operator details and more answers:
- The operator is the owner as an individual based in Manila; there is no DTI registration, which replaces the sole-proprietorship answer above.
- Documents someone uploaded to a service are deleted with their account (D27, partly).
- Simple steps for acting on reports were requested (D22).
- There is no lawyer, so the owner approved the legal text as version `2026-11-21` without a legal review (D10).
- The site follow-up was requested ([F11](../specs/legal-pages/spec.md)).

Contact details live in the [legal text](legal/privacy-policy.md), not here.

Decisions of 2026-11-23, after account deletion (F12) and the information pages (F13) were built. The user chose the recommended option in four questions:
- **Email (R08, D18):** one email per conversation until the recipient opens it. There's no Brevo account yet, so email is built now with a development sender that writes to a local folder; real sending starts when the Brevo key and sender are added (G11).
- **Client services (D24, D25):** the prototype's proposals are adopted for now.
- **Service documents (D26, D28):** PDF, JPEG and PNG, checked by content, up to 10 MB, in a private Supabase Storage bucket, with no virus scanning yet.

Later on 2026-11-23, after Microsoft sign-in was built ([F15](../specs/microsoft-sign-in/spec.md)), the user said: "archive the microsoft signin feature. google signin is enough but since it already develop just hide it". R04 is now Google only. The Microsoft code stays in the site, switched off (`Authentication:Microsoft:Enabled`), and the legal text keeps its Microsoft blocks off.

## Problem, users, and success

People looking for an accountant have no simple way to compare accountants and contact one. Accountants have no simple channel to present their experience and services and to receive enquiries. Tax Buddy is a portfolio and directory site for accountants, with messaging and client ratings. Accountants publish a profile with their services, experience and portfolio highlights, with no approval step. Clients browse, chat with the accountant they choose, and rate the accountants who have replied to them. Both sides are told by email about new messages. Clients also keep a private list of added accountants; accountants organize client services, and both participants follow shared checklists/progress and service documents. Service progress does not add email events in this change.

| User | Who | What they can do in release 1 |
|---|---|---|
| Visitor | Anyone who opens the site and has not signed in | Browse the directory and view accountant profiles, portfolios and reviews |
| Client | A registered user looking for an accountant | Everything a visitor can do, plus start and continue chats, review accountants who have replied to them, add accountants, request services, view shared progress, attach service documents, and report or block |
| Accountant | A registered accountant | Create and edit their own public profile and portfolio; read and reply to client chats; reply to reviews; report or block; manage their client list, services, checklists and shared updates; attach service documents; receive message email notifications |

Success measures: not yet defined ([D12](#open-decisions)). No adoption, performance, or response-time targets are approved.

## Release scope

| Decision | Selected scope / unresolved question |
|---|---|
| Target platforms | A web application for phone and desktop browsers ([ADR 005](adr/005-web-platform-blazor.md)). Supported browsers: Chrome, Edge, Firefox, Brave and Safari, in their current versions. |
| Offline and connectivity | Online only. If the connection drops, the site says so, keeps typed text, and queues nothing. |
| Data and accounts | Accounts through Google sign-in ([ADR 006](adr/006-web-sign-in.md); Microsoft sign-in archived on 2026-11-23, see R04). Accountant profiles, portfolio highlights and reviews are public. Chats and service work/documents are private to the relevant client and accountant; saved-accountant lists are private to their client. Account deletion on the site (R09). Retention periods are open ([D14](#open-decisions)). |
| Server and older versions | One ASP.NET Core web application serves the pages and holds the data. Nothing is installed, so every user gets the current version on their next page load. |
| Languages and locales | English only. No RTL. |
| Date, time, and money | Times are instants, displayed in Asia/Manila time. No money values in release 1: services are listed without prices. If prices are added later, the currency is PHP. |
| Distribution | A hosted website. Internal testing first: the site runs at a test address where only invited testers can sign in ([D20](#open-decisions)). A public launch is a later decision. |
| Explicit exclusions | Native mobile and desktop apps. A public API for other clients. Browser push notifications. Prices, payments and bookings. Chat attachments (chat remains text only; private service documents are included separately under R23). Group chats. Offline use. Admin approval or verification of accountants. An admin page: reports are handled by hand outside the site ([D22](#open-decisions)). Posts, follows, likes and comments. Localization beyond English. (Profile photos were excluded until 2026-11-16, when R24 added profile pictures.) |

Browser and hosting capabilities are recorded in the [SDD platform matrix](SDD.md#platform-capability-matrix).

## Requirements

IDs stay stable when wording changes. R19–R23 come from the current user request and execution instruction; earlier rows come from owner answers in the setup sessions of 2026-11-10 and 2026-11-11.

| ID | User need and required behavior | Acceptance outcome | Priority / decision state |
|---|---|---|---|
| R01 | A visitor or signed-in user browses a list of listed accountants, each showing name, a short career summary and services offered | The list loads without sign-in. An empty directory, a failed load and a loaded list are visibly different | Required / Approved scope |
| R02 | A user narrows the directory by searching or filtering | Matching accountants are shown; "no match" is distinct from an empty directory. Fields and filters: [D05](#open-decisions) | Required / Approved scope, details open |
| R03 | A user opens an accountant's profile: career summary, experience, contact details and services offered (no prices) | Every published profile field is shown. A profile that was removed shows a "no longer available" state, not an error loop | Required / Approved scope |
| R04 | A person signs in with a Google account and, on first sign-in, registers as a client or an accountant. Until 2026-11-23 this also said "or Microsoft"; the owner then archived Microsoft sign-in ("Google sign-in is enough") | After sign-in the user stays signed in in that browser until they sign out or the session ends. One role per account ([D03](#open-decisions), decided 2026-11-16). Google sign-in built in [F04](../specs/google-sign-in/spec.md) on 2026-11-16; Microsoft sign-in built in [F15](../specs/microsoft-sign-in/spec.md) on 2026-11-23 and archived the same day: the code stays, switched off and hidden | Required / Approved scope; Microsoft removed from scope 2026-11-23 |
| R05 | An accountant creates and edits their own profile | Once the required fields are complete, the profile appears in the directory with no approval step. Only its owner can change it. Required fields: [D04](#open-decisions). Built in [F05](../specs/profile-editing/spec.md) on 2026-11-16 | Required / Approved scope, details open |
| R06 | A signed-in client starts a text chat with an accountant from that accountant's profile, and continues it later | The client's message reaches the accountant once. Visitors are asked to sign in or register before chatting, and text they typed is kept | Required / Approved scope |
| R07 | An accountant sees their client conversations, reads new messages and replies | Replies appear for the client. Unread conversations are marked | Required / Approved scope |
| R08 | A client or an accountant is told by email about a new message when they are not on the site | An email arrives at the account's address; its link opens that conversation, after sign-in if needed. The user can turn these emails off, from a link in the email or in their account. Unread marks work either way | Required / Approved scope (email chosen 2026-11-11; clients included the same day); built in [F14](../specs/email-notifications/spec.md) on 2026-11-23 with a development sender (real sending waits for Brevo, G11) |
| R09 | A user deletes their own account on the site | After confirmation the account can no longer be used, an accountant's profile leaves the directory, and no further notification emails are sent. Existing chats stay with the other participant, shown as from "Deleted user", and the user's reviews are removed ([D07](#open-decisions), decided 2026-11-21). Built in [F12](../specs/account-deletion/spec.md) on 2026-11-23 | Required / Approved scope |
| R10 | A user who loses the connection is told so, and does not lose typed text | A connection-lost notice is shown; unsent text stays in the field; retry works when the connection returns. Nothing is sent later in the background | Required / Approved scope |
| R11 | Personal information is shown only where its owner expects it | Accountant contact details are public, by the accountant's choice when publishing the profile. Client accounts are never publicly listed; private accountant client lists are allowed under R19. A review shows only its author's first name and last initial ([D21](#open-decisions)). A conversation is visible only to its client and accountant | Required / Approved scope |
| R12 | An accountant adds portfolio highlights to their profile: a title, the type of client, what they did, and the result | Highlights appear on the public profile. They carry no client names unless the accountant has permission | Required / Approved scope ([ADR 008](adr/008-portfolio-ratings-no-approval.md)); limits provisionally decided ([D23](#open-decisions)); built in [F08](../specs/portfolio-highlights/spec.md) |
| R13 | A client rates an accountant from 1 to 5 stars, with an optional written review | The profile and the directory show the average rating and the number of reviews. Reviews are listed on the profile, newest first. An accountant with no reviews says so | Required / Approved scope (ADR 008); built in [F09](../specs/reviews/spec.md) (2026-11-17) |
| R14 | An accountant replies publicly to a review | One reply per review, shown under it. The accountant can edit the reply | Required / Approved scope (ADR 008); built in [F09](../specs/reviews/spec.md) (2026-11-17) |
| R15 | A signed-in user reports a profile, a review or a conversation, and blocks someone from messaging them | A report with a reason reaches the site owner. A blocked person can no longer send messages in that conversation and is not told why. The blocker can undo it | Required / Approved scope (ADR 008); built in [F10](../specs/report-block/spec.md) (2026-11-18); handling partly open ([D22](#open-decisions)) |
| R16 | The site has a privacy policy and terms of use, and a person agrees to them at first sign-in | Both pages are reachable from every page. Registration cannot finish without agreement | Required / Approved scope; text needs legal review ([D10](#open-decisions)) |
| R17 | The site explains itself: About, Help and contact, and a For accountants page | Each page is reachable from every page without signing in | Required / Approved scope; built in [F13](../specs/information-pages/spec.md) on 2026-11-23 |
| R18 | A wrong address or a server failure shows a clear page | "Page not found" for unknown addresses, and an error page with a support code for failures, each with a way back | Required / Approved scope |
| R19 | An accountant views a private client list and each client's services | Only that accountant's requesting or engaged clients are shown, grouped by client, with service status; empty/loading/error states differ | Required / Approved feature scope; relationship lifecycle D24 open; built in [F03](../specs/client-services/spec.md) stage 1 on 2026-11-23 |
| R20 | A signed-in client adds accountants to a private personal list | Added accountants are reachable with profiles, chats and service work; repeated adding does not duplicate a row; saving alone shares no client details in the confirmed save/request/accept model | Required / Approved feature scope; private save/request/accept confirmed; remaining D24 details open; built in [F03](../specs/client-services/spec.md) stage 1 on 2026-11-23 |
| R21 | Services engaged for a client have a checklist, organized under the relevant accountant and client | Each service has its own named tasks and statuses; a request is distinct from accepted work; who creates/accepts services and edits tasks follows D24–D25 | Required / Approved feature scope; save/request/accept model confirmed; other D24/D25 policies open; built in [F03](../specs/client-services/spec.md) stage 1 on 2026-11-23 |
| R22 | Client and accountant track shared service progress and updates | Saved task progress, shared notes, author and time are visible to both participants; unsaved/failed changes are not presented as saved; client sign-off remains D25 | Required / Approved feature scope; status/progress policy proposed; built in [F03](../specs/client-services/spec.md) stage 1 on 2026-11-23 |
| R23 | Both participants attach and download private documents associated with a service and optionally a checklist task | Correct service/task association, uploading/available/failed/retry states, validation errors and participant-only access. Production intake requires D26–D28 policies/storage | Required / Approved feature scope; stage 2 of F03 built 2026-11-23 with the D26/D28 choices; the per-service count is a working value, and quotas, retention and the privacy impact assessment (legal issue 2.10) are open |
| R24 | Every signed-in user can add, change and remove their own profile picture. Without one, their avatar is their initials | An accountant's picture is public: header, directory card, profile. A client's is visible to that client and to the accountants they have a conversation with ([F07](../specs/chat/spec.md)). Uploads are JPEG, PNG or WebP up to 5 MB, saved as a 400 × 400 square with location and camera data removed ([ADR 011](adr/011-profile-pictures-supabase-storage.md), [F06](../specs/profile-pictures/spec.md)) | Required / Added 2026-11-16 at the owner's instruction |


## Business rules

| Rule ID | Requirement | Rule and edge cases | Decision evidence |
|---|---|---|---|
| BR01 | R01, R03, R05 | A profile is listed only while its account exists and its required fields are complete. There is no admin approval in release 1. Deleted or incomplete profiles never appear, including through an old link | Owner answer 2026-11-10 ("self-register, listed at once") |
| BR02 | R01, R02 | Directory order is stable, with a unique tie-breaker, so paging never repeats or skips an entry. Default order: name A–Z, then ID. "Highest rated", added with ratings (R13), orders by average rating, then review count, then name, then ID; accountants without reviews come last ([D05](#open-decisions), provisional) | Rule from the kit's query guidance; order adopted provisionally 2026-11-12 (D05) |
| BR03 | R06 | Only a signed-in client can start a conversation, and only with a listed accountant. A client and an accountant have one ongoing conversation; "Message" again opens it ([D06](#open-decisions)) | Owner answer 2026-11-10; pairing decided 2026-11-16 (D06) |
| BR04 | R06, R07 | A message is plain text. It is rejected if it is empty or whitespace-only after trimming, or longer than 2,000 characters after trimming ([D06](#open-decisions)). Sending the same message twice because of a retry creates one message | Owner answer 2026-11-10 ("text only"); limit decided 2026-11-16 (D06) |
| BR05 | R11 | Only the conversation's client and accountant can read or add messages. A record ID in a link or form never proves access ([SDD](SDD.md#sign-in-sessions-and-authorization)) | Owner answer 2026-11-10 |
| BR06 | R09 | Deletion needs an explicit confirmation step. After deletion the account's profile, sign-in and notification settings are removed. Chats stay with the other participant, shown as from "Deleted user". A client's reviews are removed and ratings recalculated; an accountant's reviews and replies go with the profile ([D07](#open-decisions), 2026-11-21) | Owner answer 2026-11-10 ("in-app account deletion") |
| BR07 | All | Times are stored as UTC instants and displayed in Asia/Manila time. There are no calendar-date-only fields in release 1 except experience start and end months ([D04](#open-decisions), provisional) | Owner answer 2026-11-10 ("PHP, English, Asia/Manila") |
| BR08 | R08 | A notification email never contains the message text. At most one email per conversation until the recipient opens it ([D18](#open-decisions), decided 2026-11-23) | Proposed 2026-11-11; pending owner confirmation |
| BR09 | R13 | A client can review an accountant only after that accountant has sent at least one message in their conversation. One review per client and accountant. The client can change or delete it. An accountant cannot review anyone | Owner answer 2026-11-11 ("Clients they've chatted with") |
| BR10 | R13 | The average is shown to one decimal with the number of reviews, from the first review on. Zero reviews shows "No reviews yet", never a zero score | Proposed 2026-11-11; adopted 2026-11-17 in [F09](../specs/reviews/spec.md) |
| BR11 | R14 | Only the reviewed accountant can reply, once per review. An accountant cannot edit, hide or delete a review | Owner answer 2026-11-11 ("Stars + review + reply") |
| BR12 | R15 | A report needs a reason. The reported person is not told who reported them. Reported content stays visible until the owner acts | Owner answer 2026-11-11 ("Report and block only"); handling in D22. Built in F10 (2026-11-18) with one report per person per item and at most 10 a day (the user's choice) |
| BR13 | R12 | A highlight needs a title and a description of the work. Up to six highlights. The type of client and the result are optional | Proposed 2026-11-11; provisionally adopted 2026-11-17 (D23) |
| BR14 | R19–R21 | Confirmed: saving an accountant is private; a client requests a service and accountant acceptance starts the shared relationship. A request explicitly shares the client name/service with that accountant for a response; saving or chatting alone does not. Prototype proposals: deduplicate active services, decline requests and allow accountant-added services for existing accepted relationships | Owner selected the private-save/request/accept option on 2026-11-11; remaining D24 details proposed |
| BR15 | R11, R19–R23 | Each list, service, task, update and document is authorized using the authenticated participant and its parent relationship. A guessed record/file ID or hiding a control is not authorization. No public client list or public service document | User's expanded brief, 2026-11-11 |
| BR16 | R21–R22 | Proposed: accountant manages tasks/statuses and shared notes; client sees read-only progress. Statuses: Not started, In progress, Waiting for client, Completed. Progress is completed tasks / total tasks, with zero tasks never treated as complete. Completion has no client sign-off yet | Prototype proposal, D25 |
| BR17 | R23 | A document belongs to exactly one service and optionally one of its tasks, visible only to that service's two participants. Text-only chat and public portfolios do not gain uploads. Demo uses generated samples only; production size/type/scanning, ownership/removal and retention depend on D26–D28 | Authorized service-file category; policy details proposed |
| BR18 | R10, R21–R23 | Failed or offline changes retain drafts/selection and report the actual outcome. Nothing queues in the background; retries must not duplicate work or files. Progress updates carry author/time; no file names, bytes, notes or private client details in logs | User's expanded brief and existing privacy/retry boundaries |
| BR19 | R13, R15 | While either participant has blocked the other, neither can send in that conversation. A client whom the accountant has blocked cannot write or edit a review of that accountant; they can still delete their own review, which stays visible until then. A block set by a client does not change their own review rights | The user's choice, 2026-11-18 ("Messaging and reviews", F10) |


No soft-delete, fixed categories, or other rules are inherited from the app the starter kit was extracted from.

## Workflows and recovery

| Workflow / requirement | Successful result | Failure / cancellation / restart result |
|---|---|---|
| Browse and view a profile / R01–R03 | List and profile shown from the server | A failed load shows an error page with a retry, never an empty directory; a removed profile shows "no longer available" |
| Sign in and register / R04 | Signed-in session kept in that browser | Cancelling at Google returns to the site unchanged; a failed sign-in can be retried |
| Edit own profile / R05 | Saved changes appear in the directory | A failed save keeps the edited fields; leaving with unsaved changes asks for confirmation |
| Send a chat message / R06, R07 | Message appears once for both participants | A failed or timed-out send keeps the text and lets the user retry. If the outcome is unknown, the site checks before claiming success or failure; a retry never duplicates the message |
| Email notification / R08 | The email's link opens the conversation | If the user is not signed in, sign-in comes first and then the conversation opens. If the conversation is gone, the site explains and opens the conversation list. A failed email never blocks or loses the message |
| Delete account / R09 | Account removed and user signed out | If deletion fails, nothing is partly deleted from the user's point of view; the site reports the failure and stays signed in |
| Write or change a review / R13 | The review and the new average appear on the profile | A failed save keeps the stars and text. A client who is not eligible sees why, and no form |
| Report or block / R15 | The reporter is thanked; the blocked person can no longer message | A failed report keeps the reason and text for a retry. Cancelling changes nothing |
| Add accountant / R20 | Accountant saved privately once | Failure keeps the page intact; removing a saved entry does not erase existing service history (remaining lifecycle D24) |
| Request/accept service / R19–R21 | Client requests a service; accountant accepts; starting checklist appears (remaining lifecycle D24) | Pending and declined requests remain distinct from active work; unrelated users see unavailable |
| Update service progress / R21–R22 | Accountant saves tasks or posts a note; both participants see saved progress and history | Failed/conflicting save keeps draft changes; client has no status-edit controls (Proposed D25) |
| Attach service document / R23 | File validated and associated with service/task before it becomes downloadable | Failed/offline upload preserves selection for explicit retry; unavailable/rejected/quarantined files cannot be downloaded; actor changes cannot finish an upload as a different user |


Unsent message text survives a lost connection and a page reload in the same browser tab. Unsaved profile edits survive a lost connection that recovers; leaving the page asks for confirmation. Both are proposals to confirm with the D04 and D06 decisions.

## Non-functional outcomes

- **Privacy:** chat text, service notes/file contents and metadata, tokens and personal details are kept out of logs ([SDD](SDD.md#errors-diagnostics-and-security)). A privacy and legal review is open ([D10](#open-decisions)). Search engines may index public profiles from the public launch ([D19](#open-decisions), decided 2026-11-21).
- **Accessibility:** screen-reader labels, keyboard use, and browser zoom are required. Exact targets are open ([D15](#open-decisions)) and procedures are in [TEST-PLAN](delivery/TEST-PLAN.md) TP06.
- **Reliability, data size, performance:** no measured targets are approved. Directory paging exists so the list does not depend on directory size.

## Decisions and acceptance

### Open decisions

All owned by lumabanls unless reassigned. Each row names the work it blocks.

| ID | Question | Affects |
|---|---|---|
| D01 | Partly decided. Name: Tax Buddy, by the user's instruction of 2026-11-12. It replaces Kuwenta, which the agent selected on 2026-11-11 under the user's delegated design brief to replace AccApp. The original logo symbol was kept and the wordmark redrawn; on 2026-11-18 the user chose a new logo, which replaced both ([DESIGN](DESIGN.md#visual-system), [F02 BD12](../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12)). Earlier [name sketches](../prototypes/brand-options.html) remain historical alternatives. Domain selection and name/domain availability checks remain open. A preliminary web survey on 2026-11-16 ([record](delivery/evidence/2026-11-16-name-availability.md)) found an active TaxBuddy BIR-filing product in the Philippines (taxbuddyph.com), India's TaxBuddy (taxbuddy.com, also in the Philippine app stores) and a live US TAXBUDDIES mark for tax services. It rated the conflict risk high. IPOPHL, WIPO and registrars were not searched directly. A second survey on 2026-11-19 ([record](delivery/evidence/2026-11-19-f02-logo-outlines-hero.md)) read domain registration records: taxbuddy.app, .ai, .net and .org are registered; no record was found for taxbuddy.io or .co; taxbuddy.ph and .com.ph could not be confirmed either way. It also searched for marks like the new logo and found none in tax or accounting, without seeing the two existing TaxBuddy logos. Not clearance; the owner decides | RELEASE identity and public launch; current planned project names use `TaxBuddy` |
| D02 | Closed 2026-11-11: sign-in on Windows desktop no longer applies, because there is no Windows app ([ADR 006](adr/006-web-sign-in.md)) | — |
| D03 | Closed 2026-11-16: one role per account. The owner chose "One role per account" (the prototype's proposal), and the role is chosen once at first sign-in ([F04](../specs/google-sign-in/spec.md)) | — |
| D04 | Provisionally decided 2026-11-12: the user instructed going with the prototype's proposal for now. Profile fields: title, city, career summary, experience (role, company, start and end month), services from a fixed list (Bookkeeping, Tax filing, Payroll, Financial statements, Audit, Business registration, Tax advisory) each with a short description, email, phone and website. To be listed, a profile needs a name, title, summary, email, at least one service and consent to show contact details. Cards and profiles show years of experience, counted from the earliest start month. Field length limits decided 2026-11-16 by the owner with R05: F01's working values (name 100, title 100, city 80, summary 1000, email 254, phone 30, website 200, service description 200, experience role and company 100 characters each) and at most 10 experience entries ([F05](../specs/profile-editing/spec.md)) | R05, R03, data model |
| D05 | Provisionally decided 2026-11-12: the user instructed going with the prototype's proposal for now. Text search on name, title, city and service; one service filter (a menu on phones, buttons on wide screens); name A–Z by default, and a "Highest rated" order added with ratings (R13, built in F09 on 2026-11-17); order rules in [BR02](#business-rules). Still open: page size. The prototype and the F01 implementation (2026-11-13) use 6 as a working value | R01, R02, BR02 |
| D06 | Closed 2026-11-16: one ongoing conversation per client and accountant, and messages up to 2,000 characters (the prototype's proposal). The agent recommended both and the user approved: "proceed to implementing f07 then go for your recommendations" ([F07](../specs/chat/spec.md)) | — |
| D07 | Decided 2026-11-21 for chats and reviews, by the user's choice of the recommended option. The other participant keeps the conversation, shown as from "Deleted user". A deleted client's reviews are removed and ratings recalculated; a deleted accountant's profile, reviews and replies go with it. Service work and documents still follow D27 | R09, BR06 |
| D08 | Closed 2026-11-11: yes, clients also get an email when an accountant replies | — |
| D09 | Partly decided 2026-11-11 ([ADR 008](adr/008-portfolio-ratings-no-approval.md)): no approval or verification; ratings, reports and blocking are the safeguards. Decided 2026-11-21: no licence checks before a public launch. Profiles keep the note that Tax Buddy doesn't check licences or qualifications, which now also points clients to the PRC | Public launch, RELEASE |
| D10 | Privacy and legal review of public contact details, chats, private service progress/documents, notification emails and the site's cookies (session; theme preference since 2026-11-16, no personal data), for example against the Philippines' Data Privacy Act of 2012. Partly decided 2026-11-21 ([drafting notes](legal/drafting-notes.md)): [terms](legal/terms-of-use.md) and [privacy](legal/privacy-policy.md) drafts exist, and the policy choices in the paragraph above were made. The same day the owner approved them as version `2026-11-21` without a lawyer's review, because there is no lawyer. The site shows them from F11. A legal review before the public launch is still advised, and is the owner's risk if skipped | R11, retention, RELEASE |
| D11 | Decided 2026-11-11 ([ADR 007](adr/007-supabase-brevo-container-host.md)): the database is Supabase and the host is Render. Still open: which Render plan, and whether production gets its own environment | SDD, SETUP, RELEASE |
| D12 | Success measures | This document |
| D13 | Closed 2026-11-11: support for older installed app versions no longer applies to a website | — |
| D14 | Partly decided 2026-11-21: an account unused for 2 years is deleted after a warning email, and its chats are handled as in D07. This needs R08 email and a cleanup job before the first accounts reach 2 years. Still open: how long service work and documents are kept (D27), and the periods for logs, reports and backups | R09, SDD retention |
| D15 | Accessibility targets: zoom level, smallest supported screen width | DESIGN, TP06 |
| D16 | License for the source code | README |
| D17 | Closed 2026-11-11: Chrome, Edge, Firefox, Brave and Safari, in their current versions | — |
| D18 | Partly decided 2026-11-11: the email service is Brevo. Still open: the sender address and its domain, and how often emails are sent per conversation Decided 2026-11-23 by the user: one email per conversation until the recipient opens it. There is no Brevo account yet, so email is built with a development sender (F14); the sender address and domain stay open until Brevo is set up (G11) | R08, BR08, SETUP, RELEASE |
| D19 | Decided 2026-11-21: yes. From the public launch, search engines may index public pages (profiles, highlights, reviews and replies); the invite-only test site keeps asking not to be indexed. The user's architecture note of 2026-11-13 had called search-engine discoverability critical, and public pages are built to be indexable ([ADR 010](adr/010-single-project-per-page-render-modes.md)) | R11, SDD, D10 |
| D20 | How the test site is limited to invited testers | Distribution, RELEASE |
| D21 | Provisionally decided 2026-11-17: the user chose the prototype's proposal, first name and last initial ("Liza F."), from the account's display name ([F09](../specs/reviews/spec.md)). The owner can change it | R11, R13 |
| D22 | Partly decided 2026-11-18 ([F10](../specs/report-block/spec.md)): reports are stored in the database, and the owner reads them in Supabase's Table Editor ([RELEASE](RELEASE.md)); an email to the owner follows with R08 once Brevo is set up (G11). On 2026-11-21 the user chose to review every report within 10 working days, and asked for simple steps for removing content and closing accounts by hand. They are in [RELEASE](RELEASE.md#acting-on-a-report-d22). Decided | R15, RELEASE |
| D23 | Highlights provisionally decided 2026-11-17: the user said "implement portfolio highlights" and "proceeed", so the prototype's proposal applies for now, as with D04. Up to 6 highlights; title 100 characters, type of client 60, what you did 600, result 300 (working values, [F08](../specs/portfolio-highlights/spec.md)). The owner can change them. Review part provisionally decided 2026-11-17 by the user's choice of the prototype's values: no minimum length; a review's text is optional, up to 1,000 characters; a reply up to 600 ([F09](../specs/reviews/spec.md)) | R12, R13 |
| D24 | Partly decided 2026-11-11: the owner selected private saving, then a client service request, with a shared relationship on accountant acceptance. Still open: accountant-added services, decline/repeat-service details, unlisting, termination and saved-entry removal effects. Prototype demonstrates these remaining details On 2026-11-23 the user adopted the prototype's proposals for now (as for D04/D05): deduplicated active services, declined requests, and accountant-added services within an accepted relationship. They can change later | R19–R21; service data model |
| D25 | Task/status/progress rules, task limits, who posts updates, client confirmation of completion and reopen policy. Prototype uses BR16 and accountant-only shared notes On 2026-11-23 the user adopted the prototype's proposal for now: BR16 (the accountant manages tasks, statuses and notes; the client reads; no client sign-off yet) | R21–R22 |
| D26 | Allowed document types, byte/count limits and per-user/service quotas. Prototype uses generated PDF/CSV/TXT and a 5 MiB demo limit; not a production decision Partly decided 2026-11-23 by the user: PDF, JPEG and PNG, checked by their content, up to 10 MB each. Count limits and quotas are still open. Built 2026-11-23 with a working value of 50 documents per service, for the owner to confirm | R23, upload validation |
| D27 | Uploader/counterpart removal rights, retained history, file retention and account/relationship deletion. Prototype allows uploader-only confirmed removal in memory. Partly decided 2026-11-21 by the user: documents someone uploaded are deleted with their account. Still open: removal rights, history, retention of the rest of the service work, and relationship ending. Built 2026-11-23 with the prototype's proposals: only the uploader removes a document, after a confirmation, and the history records it; the rest of the service work stays with the other person when an account is deleted | R09, R23, privacy, RELEASE |
| D28 | Private byte-storage provider, quarantine/scanning implementation, cleanup/recovery and operational costs. No provider selected for service documents. Since 2026-11-16, profile pictures (R24) use a private Supabase Storage bucket ([ADR 011](adr/011-profile-pictures-supabase-storage.md)), which is a candidate for documents too Partly decided 2026-11-23 by the user: a private Supabase Storage bucket, as for profile pictures, downloadable only by the two participants; no virus scanning yet. Built 2026-11-23 as the bucket `service-documents` ([SDD](SDD.md#private-service-workspaces)) | R23, SDD storage, RELEASE |


The R19–R23 scope expansion is authorized for documentation and prototype by the current user request. Detailed policy proposals remain open. Earlier scope answers come from the setup sessions; formal approval of the full PRD and release remains pending.

Acceptance uses [TEST-PLAN](delivery/TEST-PLAN.md), with outcomes recorded in [STATUS](delivery/STATUS.md). Approval of this document does not imply implementation or release acceptance.
