# Prototype and site compared, per user and page: review record

## Identity and attribution

- Date: 2026-11-24, Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). Five read-only reviewers compared the code in parallel, one per page group; their findings were then spot-checked in the code by the main session. The user's request: "check prototype vs site implementation difference per user and page".
- Sources: `ff4f0df` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus the uncommitted work of 2026-11-16 to 2026-11-24. That includes the [Account workspace row and the long-address fix](2026-11-24-account-workspace-row.md).
- Prototype: `prototypes/index.html` (SHA-256 prefix `0209225981cd`), `prototypes/service-workspace.js`, `prototypes/taxbuddy.css`. Site: `src/TaxBuddy.Web`.
- Method: the prototype's render functions were read beside the site's pages for each user: visitor, client and accountant, plus the profile owner and an unrelated user where it matters. Each difference was checked against DESIGN and the feature specs.
- Limits: this is a code comparison. Nothing was run in a browser except the Account page earlier the same day. Items marked ✓ in the tables were re-read in the code by the main session; the rest rest on one reviewer's reading. Line numbers are as of this date.

Classes used below:
- **U:** unrecorded. DESIGN and the feature spec don't mention it.
- **Stale:** recorded as "left out until X is built", but X is built.
- **Docs:** documents that disagree with the code or with each other.

**Fixed later the same day** at the user's "yes fix the first two" ([evidence](2026-11-24-prototype-alignment-fixes.md)):
- the two bugs: history times printed twice, and a failed add or remove shown as success;
- the four Stale items;
- the workspace frame;
- the current site bar item;
- the not-available states: chat panes, the review panel, workspace frames, and Request a service's 404.

The rows below are kept as found.

**Correction (same day):** an earlier version listed DESIGN:66 ("Read their story") as out of date. It isn't: both the prototype and the site add that text through CSS (`.card-foot::before`).

Recorded deviations (R) aren't repeated in the tables; they stay in DESIGN's screen table and the specs. Every page has some, mostly because static pages work without script.

## Summary by user

| User | Most significant unrecorded differences |
|---|---|
| Visitor | Empty directory has no "I'm an accountant" button. "See how listing works" is still left out (Stale). The emails-off page's sign-in wording and title differ. A malformed workspace or chat link says "not available" instead of asking to sign in |
| Client | "Accountant added" lands in the Reviews section, away from the button. A failed add or remove shows in green. Workspace pages are squeezed into the 680 px column on desktop. Signing up as a client from the profile editor ends on "Only accountants have a public profile". The site bar marks the wrong item on request, service and client pages. A message's text stays in the box while it sends. On phones, the profile's bottom bar may cover the end of the page. The editor's not-for-you page is titled "Create your profile" |
| Accountant | The chat list's email note is missing (Stale). A stale accept, decline or save shows "This workspace is not available". The same narrow workspace pages. History times are printed twice for today. Only the latest 200 chat messages load, with no notice |
| Everyone | Not-available states for chats, reviews and workspaces lose the panes or panel and the page heading on desktop. For accountants stacks its three benefits on wide screens. The error page's support code is a long trace ID. Blazor's default yellow error bar is unstyled |

## Directory and profiles

| User | Difference | Prototype | Site | Class |
|---|---|---|---|---|
| Visitor | Empty directory: no "I'm an accountant" button | `index.html:983-986` | `DirectoryPage.razor:106-109` | U ✓ |
| Visitor | "Are you an accountant? See how listing works" is missing from How it works. DESIGN keeps it out "until the For accountants page exists", and that page has existed since 2026-11-23 | `index.html:1490` | `DirectoryPage.razor:82-85`; `HowItWorksSteps.razor` | Stale ✓ (DESIGN:123) |
| All | "Clear search and filter" goes to `/` and drops `sort` | `index.html:991, 2156` | `DirectoryPage.razor:113` | U |
| All | Offline wording of the load error ("Can't reach Tax Buddy") is missing | `index.html:979-980` | `DirectoryPage.razor:96-101` | U (low) |
| Client | After Add accountant, "Accountant added to your private list." appears in the Reviews section, usually off-screen; the prototype uses a toast | `service-workspace.js:145` | `AccountantProfilePage.razor:131-133, 396-397` | U ✓ |
| Client | A failed add or remove ("We couldn't change your list…") is styled as success (`save-status ok`, role status) on the profile and on My accountants; the prototype shows a red error | `service-workspace.js:38, 69` | `AccountantProfilePage.razor:132`; `WorkspacePage.razor:63-66, 150` | U ✓ |
| Client (phone) | The hint "Adding is private…" stays in the fixed bottom bar; the prototype hides it on phones | `taxbuddy.css:337` | `AccountantProfilePage.razor:382`; `app.css:1786-1788` | U ✓ |
| Client (phone) | The fixed bar (Message, Add accountant, Request a service, two hints) is about 230 px tall, but the page keeps 104 px of bottom padding, so the end of the page may sit under the bar | `index.html:1726` | `app.css:2307-2320, 3476-3478` | U (from the CSS, not seen) |
| Client | "Request a service" is hidden when the profile has no account; the prototype always shows it | `service-workspace.js:125` | `AccountantProfilePage.razor:378-381` | U (low) |
| All | Experience is sorted current-first, then newest; the prototype keeps the accountant's order | `index.html:1039-1040` | `DirectoryModels.cs:63-66` | U (low) |
| Owner | Reply errors: an inline "Not posted…" line and an empty-reply message, where the prototype shows a toast | `index.html:2121` | `AccountantProfilePage.razor:207-210, 462` | U (low) |
| Client | Review page: the save failure wording and place differ, and there's an extra load-failure state | `index.html:1581, 2102` | `ReviewPage.razor:16-21, 95` | U (low) |
| All (desktop) | Review page "This accountant is no longer available" shows without the "Write a review" panel and breadcrumb; DESIGN says the review page's states sit in the panel | `index.html:1570, 1765` | `NotFound.razor:24-31` | U (contradicts DESIGN:96) |
| Signed in | Report page: extra send-failure and load-failure states | — | `ReportPage.razor:16-21, 201` | U (low) |

## Sign-in, Account, editor, deletion, emails off

| User | Difference | Prototype | Site | Class |
|---|---|---|---|---|
| Visitor → client | Signing up as a client after starting from the profile editor returns to `/account/profile`, which says "Only accountants have a public profile"; the prototype goes to the directory | `index.html:2185` | `RegisterPage.razor:116` | U ✓ |
| Visitor | No "Signed in as …" or "Create your profile…" confirmation after registering; an extra "We couldn't create your account" banner; the role step's back arrow goes to `/signin` | `index.html:2186, 2191, 1731` | `RegisterPage.razor:10, 21-27, 113-117` | U (low) |
| Signed in (desktop) | Agree page: no visible "Updated terms" heading band (its h1 is screen-reader only), and no "Thanks. You can carry on." | `index.html:1783, 2229` | `AgreePage.razor:10-12, 93-94` | U |
| Signed in | Account: no "Signed out." or "Email notifications are on/off." confirmation; when the database is down the email row is silently left out | `index.html:2221, 2231` | `AccountPage.razor:227-230`; `EmailPreferences.cs:69` | U (low) |
| Client | The editor's not-for-you page is titled "Create your profile" in the phone bar and tab; the prototype says "My profile" | `index.html:1307` | `ProfileEditorPage.razor:14-15, 270` | U |
| Accountant | The editor's checklist count, meter and character count update only after a save, not while typing | `index.html:2262-2273, 2308` | `ProfileEditorPage.razor:44-69, 121` | U (low; DESIGN:135 lists other script-only gaps) |
| Signed in | Delete account: the third item drops "and email notifications for this account", though email notifications are built | `index.html:1397` | `DeleteAccountPage.razor:46` | Stale ✓ (DESIGN:143; account-deletion spec:24) |
| Signed in | Delete account: no "Deleting…" state; DESIGN lists it and the SDD asks for buttons disabled while running | `index.html:1405-1406` | `DeleteAccountPage.razor:71` | U (low) |
| Visitor | Emails off without a link: the title is "Sign in to change this setting", where the prototype's title is the on/off state with "Sign in to change this setting." as text. On desktop there's a heading band, not the info panel titled "Emails turned off" | `index.html:1691-1693, 1766` | `EmailOffPage.razor:11-20` | U ✓ |

## Site navigation

| User | Difference | Prototype | Site | Class |
|---|---|---|---|---|
| Signed in (desktop) | On `/clients/{id}` and `/work/{id}` nothing in the site bar is current; the prototype marks Clients or My accountants. `NavLink` matches only the `/workspace` prefix | `index.html:1793` | `MainLayout.razor:20` | U |
| Client (desktop) | On `/accountants/{id}/request-service` "Find an accountant" is current, because every `/accountants/…` path counts as the directory; the prototype marks My accountants | `index.html:1793` | `MainLayout.razor:60-67` | U ✓ |
| All (desktop) | On the review page "Find an accountant" is current, and on `/account/agree` the account link is; the prototype marks neither | `index.html:1793` | `MainLayout.razor:65`; `SiteUser.razor:8` | U (low) |
| Visitor (phone) | Workspace sign-in state shows the tab bar and no back arrow; the prototype shows a back arrow and no tabs | `service-workspace.js:117`; `index.html:1757` | `WorkspacePage.razor:12-13` | U (low) |

## Chats

| User | Difference | Prototype | Site | Class |
|---|---|---|---|---|
| Accountant | The chat list's line "New client messages are also emailed to you." / "Email notifications are off." with "Change" is missing; DESIGN keeps it out "until R08", which is built | `index.html:1064` | `ChatPanes.razor:54-79` | Stale ✓ (DESIGN:139; chat spec:65) |
| Signed in | Only the latest 200 messages of a conversation load, with no notice or "earlier messages" control | `index.html:1083-1101` | `Domain/Chat.cs:13`; `ChatStore.cs:109-113` | U ✓ |
| Signed in | While a message sends, its text stays in the box and also shows as a "Sending…" bubble; the prototype clears the box on Send | `index.html:2000-2001` | `ChatPanes.razor.js:75-93`; `ChatPanes.razor:547-550` | U ✓ |
| Signed in (desktop) | "Only clients can start chats", "This accountant is no longer available" and "This conversation isn't available" are single centred pages; the prototype keeps the conversation list and a pane head beside them | `index.html:1117-1126, 1767-1778` | `NewChatPage.razor:23-28`; `NotFound.razor:16-31` | U |
| Signed in (phone) | "View profile" stays in the pane head that is visually hidden on phones, so keyboard users get an invisible tab stop | `index.html:1122, 1773` | `ChatPanes.razor:95-100`; `app.css:915-922` | U (accessibility) |
| Signed in | Connection banner: drops "Send it again when you're back online", adds "Reconnecting…", a countdown and paused/"Couldn't resume" states | `index.html:902` | `ReconnectModal.razor:9-26` | U (partly recorded, DESIGN:142) |
| Signed in | Smaller: Retry while offline has no "You're still offline" message; the failure announcement differs; after reporting, the Report link is hidden; an unlisted accountant's "View profile" is hidden; the email's button wording differs ("Open the conversation") | `index.html:2199, 2010, 2137, 1130, 1822-1825` | `ChatPanes.razor:186, 617, 120-127, 97-100`; `EmailOutbox.cs:101-108` | U (low) |
| Accountant | The deleted-client note is built, but DESIGN:140 and the chat spec still say it's left out until built | — | `ChatPanes.razor:202-206` | Docs |

## Client services

| User | Difference | Prototype | Site | Class |
|---|---|---|---|---|
| Signed in (desktop) | All four workspace pages sit in `.single-inner` (680 px) under a plain title band. The prototype uses the 1040 px container with a breadcrumb ("Clients / …") and a page h1. On `/work` the main column is about 356 px beside the 300 px history | `index.html:1761`; `taxbuddy.css:252-253` | `WorkspacePage.razor:16`, `ServicePage.razor:25`, `ClientServicesPage.razor:15`, `RequestServicePage.razor:17`; `app.css:952-955` | U ✓ |
| Both | History entries from today show the time twice ("10:42 AM · 10:42 AM"): the short date is already the time for today, and the time is added again | `service-workspace.js:113` | `ServicePage.razor:276`; `ChatDisplay.cs:34-36` | U ✓ (bug) |
| Accountant | A stale accept or decline (already answered in another tab), or a save, task or note after the phase changed, ends on "This workspace is not available" (404); the prototype ignores it | `service-workspace.js:149-153` | `ServicePage.razor:455-469`; `WorkService.cs:147-179` | U (404 branch ✓; the stale cases are from code reading) |
| Accountant | A 31st checklist task also ends on "not available": the site caps tasks at 30 but still shows the form | — | `Domain/Work.cs:371`; `WorkService.cs:204` | U |
| Accountant, unknown | Request a service for an accountant, or for an unknown or unlisted profile, says "This isn't available / Only clients can request services…" with HTTP 200. DESIGN records the generic "This workspace is not available" 404 | `service-workspace.js:83, 44` | `RequestServicePage.razor:28-33` | U (contradicts DESIGN:161) |
| All | "Not available" for `/work` and `/clients` is drawn by the not-found page: no h1, no title band, and the phone title is "Service progress" on `/clients` too. A visitor with a malformed ID gets "not available" instead of the sign-in prompt (SDD:214) | `service-workspace.js:44, 116-117` | `NotFound.razor:8-15, 76-77` | U |
| Client | No "Message accountant" link in the service summary when there's no chat yet | `service-workspace.js:53, 109` | `ServicePage.razor:49-55` | U |
| Both | A declined request shows an extra "Service request" panel; the accountant's button says "Decline" (prototype: "Decline request") | `service-workspace.js:110` | `ServicePage.razor:66, 75-78` | U (low) |
| Client | My accountants: an accountant with past work but no longer saved has no "Service history" badge; an unlisted accountant shows their name instead of "Accountant no longer listed"; the Remove button sits in the actions row | `service-workspace.js:67` | `WorkspacePage.razor:85-90, 117-126` | U (low) |
| Accountant | Clients list: "awaiting your response" (prototype: "awaiting acceptance"), the line of service names is missing, the empty-state text differs | `service-workspace.js:61` | `WorkspacePage.razor:39, 51` | U (low) |
| Both | Smaller: load failures have no Try again on the client and request pages; the client page's service menu hides open services; errors sit above the buttons; "No checklist changes to save" isn't shown; panel headings are 22 px (prototype 24 px) | `service-workspace.js:41, 77, 91, 118, 151`; `taxbuddy.css:273` | `ClientServicesPage.razor:24, 43, 60-64`; `RequestServicePage.razor:26, 50-55`; `ServicePage.razor:90`; `app.css:1478-1481` | U (low) |

## Information pages, errors

| User | Difference | Prototype | Site | Class |
|---|---|---|---|---|
| All (desktop) | For accountants: the three benefits are one stacked column; the prototype shows three columns on wide desktops | `index.html:499` | `app.css:1870-1890` (no column rule) | U ✓ |
| All | For accountants: "free for now. If fees are ever introduced, you'll be told in advance…" vs "free in this release" | `index.html:1688` | `ForAccountantsPage.razor:22` | U |
| All | Help: the service-tracking answer is reworded ("Once the accountant accepts, you both see…") and links to My accountants and Account were added | `index.html:1672-1677` | `HelpPage.razor:19, 29, 33` | U (low) |
| All | Error page: the support code is a W3C trace ID of about 55 characters; the prototype shows a short code ("7F3K-21") | `index.html:1694` | `Error.razor:16-19, 37` | U |
| All | When an interactive page crashes, Blazor's default yellow "An unhandled error has occurred" bar shows; it doesn't use the site's tokens and has no prototype design | — | `MainLayout.razor:53-57`; `MainLayout.razor.css` | U |
| All | State panels on Not found and Error keep 8 px bottom padding (prototype 40 px) | `index.html:197` | `app.css:3327` | U (trivial) |

The legal pages match the prototype's structure (section titles and order, short version, contents, tables). Privacy shows version `2026-11-23.2`, which still awaits the owner's approval (STATUS G13).

## Documents out of date

Found during the comparison, not changed:
- DESIGN:123: "See how listing works" left out until For accountants exists (it exists).
- DESIGN:139 and chat spec:65: the email note left out until R08 (built).
- DESIGN:140 and chat spec:65: the deleted-account note left out until built (built in F12).
- DESIGN:143 and account-deletion spec:24: the email clause left out until R08 (built).
- DESIGN:147: "The 'emails off' page waits for R08" (built in F14, DESIGN:141).
- report-block spec:36 (RB01): "Sign in to report"; DESIGN:97 and the site say "Sign in to send a report".
- SDD:59-60: workspace pages listed as Interactive Server; SDD:213 and the code have them static.
- SDD:282: the not-found page has two variants; it has seven.
- SDD:427: the error page has "a retry"; neither the prototype nor the site has one.
- information-pages spec:16, 20: service and email Help answers excluded; they're built (IP02).
- `AboutPage.razor:5-6` comment: the prototype's operator section is "to be written"; the prototype has it.

## Not checked

- Nothing here was seen in a browser; the phone-bar overlap on the profile is worked out from the CSS.
- Whether the Chats badge still counts a thread just opened, and how a send or Retry behaves while the connection is down, need a browser run.
- Whether `NavLink` sets `aria-current` as well as the `active` class wasn't confirmed.
