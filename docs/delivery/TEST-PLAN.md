# Verification plan

**Authority:** procedures, environments, expected outcomes, and acceptance gates. [PRD](../PRD.md) owns behavior; [STATUS](STATUS.md) owns results. Procedures are defined for release 1 scope; none has been executed. Rewritten on 2026-11-11 for the web platform; procedure IDs are unchanged, TP11 was retired and TP15 added. TP16–TP18 were added the same day for ratings, reports and the information pages. TP28 was added on 2026-11-25 for the Proposed loading and busy-feedback standard.

## Environments and boundaries

| Boundary | What it establishes | What it cannot establish |
|---|---|---|
| Rule tests (TaxBuddy.Tests) | Validation and authorization rules (BR01–BR18) with controlled inputs and time | Pages, sign-in, browser behavior |
| Data tests (TaxBuddy.Tests) | Real database provider behavior, migrations, paging order | Hosting configuration; behavior on another provider |
| Web integration tests (TaxBuddy.Web.Tests) | Public pages, forms, antiforgery, authorization and status codes through `WebApplicationFactory`, with an isolated test database and a test sign-in scheme. Since F04, `TestExternalSignIn` is a test-only address that leaves Identity's external cookie as Google's handler would. Sign-in tests use an HTTPS client, because the session cookies are Secure outside development | Real Google/Microsoft sign-in, real email delivery, JavaScript and live connection behavior |
| Component tests, where used | Interactive component state changes | The real browser, connection loss |
| Web project build | Compilation and Razor checks | Runtime behavior |
| Browser checks on a local run | Observed behavior in the recorded browser and screen size | Other browsers, real phones, the deployed host |
| Deployed-site checks | Observed behavior of the recorded build on the real host, with real sign-in and email | Other browsers or unchecked scenarios |
| Acceptance decision | Owner's decision against required gates | Untested functionality outside that decision |

### Prototype design checks

For F02, [BD01–BD06](../../specs/brand-design-refresh/spec.md#acceptance-scenarios) define the prototype outcomes. Run the [prototype checker](../../tools/README.md#check-the-prototype), inspect representative screenshots, check the inline script with `node --check`, parse SVG assets, and run the documentation checker. Expected outcomes: usable directory/recovery and existing simulated flows, no horizontal clipping at recorded widths, readable light/dark semantic color pairs, keyboard access to the skip link, intact local assets, and preserved historical records. The 4.5:1 text and 3:1 control checks are implementation checks, not a decision on PRD D15 or full accessibility certification. BD09, the application theme switch, is covered by `ThemeSwitchTests` in Web.Tests and by TP06 in each browser ([first run](evidence/2026-11-16-f02-theme-switch.md)).

These observations are separate from TP01–TP18 application execution. The prototype contains no server, real sign-in, real delivery or persistence, so a passing prototype check cannot complete those gates. Current observations are linked from [STATUS](STATUS.md).

Use isolated test databases, controllable clocks and synthetic accountant and client data. Do not rely on an in-memory database substitute for persistence rules. Emails in tests go to a capturing test sender, never to real addresses.

## Scenario mapping

| Procedure / requirement | Preconditions and action | Expected observable result | Boundary / environment |
|---|---|---|---|
| TP01 / R01, R03 | As a visitor, open the directory, open a profile, reload, and open the profile's address in a new tab | List and profile load without sign-in; every published profile field is shown; the address alone reproduces the page. The first HTML response already holds the listed accountants, the profile fields, the reviews and each page's title and canonical address, with no script needed ([ADR 010](../adr/010-single-project-per-page-render-modes.md)) | Web.Tests + browser |
| TP02 / R05, R06, BR04 (profile part: [F05](../../specs/profile-editing/spec.md) PE02–PE05, PE07) | Empty or whitespace message; incomplete profile; repeated clicks on Send and Save; a controlled save failure, then retry | No invalid or duplicate message or profile change; typed text kept; truthful error, then success on retry | Rule and integration tests + browser |
| TP03 / R10 | On a chat page with typed text, cut the network, wait for the notice, restore it; repeat and reload the tab instead | A connection-lost notice appears; the text is still there after reconnecting and after a reload; nothing was sent meanwhile | Browser |
| TP04 / server schema | Apply each new migration to a database at the previous schema with synthetic data; make a migration fail part-way | Data kept; a failed migration leaves the previous schema usable | Data tests with isolated database |
| TP05 / R01, R06, R07 | Back and Forward through directory, profile and chat; share a filtered directory address; keep the same account open in two tabs and send from one | Back and shared addresses show the same list; the second tab shows the new message without a duplicate | Browser |
| TP06 / DESIGN, F02 BD09, BD14 | Light and dark themes chosen with the header's theme icon, and the System default with the browser setting both ways; browser zoom to the D15 target, a screen reader, keyboard only, the smallest supported width | All actions reachable and readable; labels and focus order correct; no clipped or overlapping content | Each browser in the matrix |
| TP07 / RELEASE | Deploy a recorded build to the test site; check the health address, HTTPS, sign-in with Google, one message, one email (Microsoft is archived) | The deployed revision matches; Google sign-in returns to the site; the message and email arrive | Deployed site |
| TP08 / R10 | Stop the server, or go offline, during a directory load and during a message send; restore and retry | Error or connection-lost state shown, never an empty list; typed text kept; retry succeeds; nothing is sent automatically later | Web.Tests + browser |
| TP09 / R06, BR04 | Make the response to a send get lost after the message is saved, then retry | Exactly one message exists; the site reports the real outcome | Rule and integration tests |
| TP10 / R04, R11, BR05 (sign-in parts: [F04](../../specs/google-sign-in/spec.md) GS01–GS06; archived Microsoft: [F15](../../specs/microsoft-sign-in/spec.md) MS00–MS06, MS07 dropped) | Let the session expire, then act; sign in as a second client and a second accountant; request another user's conversation address directly | Sign-in, then back to the same page with the draft kept; no user can read or post to a conversation they are not part of; client details never appear on public pages | Web.Tests + browser |
| TP11 | Retired 2026-11-11. It tested an older installed app against a newer API, which no longer exists | — | — |
| TP12 / RELEASE | Inspect the deployed configuration | HTTPS redirect and HSTS on; secure session cookie; detailed error pages off; no development secrets, test sign-in scheme or development email sender; public pages follow the D19 indexing decision | Deployed site |
| TP13 / R08, BR08 ([F14](../../specs/email-notifications/spec.md) EM01–EM08) | A user is not on the site; the other person sends two messages; repeat for a client and for an accountant, with emails turned off, through the email's own "turn off" link, and with the email service failing | One email per the D18 frequency rule, with no message text and a working link; none when turned off; with the service failing, the message is still saved and the email is retried | Web.Tests with capturing sender + deployed site |
| TP14 / R09, BR06 ([F12](../../specs/account-deletion/spec.md) AD01–AD11) | Delete an accountant account and a client account with existing conversations | Confirmation required. Afterwards the profile is gone from the directory and from old links, signing in again starts a fresh registration, and no more emails are sent. Per D07 (2026-11-21): the other participant still sees the conversation, shown as from "Deleted user", and can't reply to a deleted accountant; a deleted client's reviews are gone and the ratings recalculated | Rule and integration tests + browser |
| TP15 / RELEASE | Restart or redeploy the server while a chat page with typed text is open | A reconnecting notice, then a working page with the typed text restored; no message lost or duplicated | Browser against a local or deployed run |
| TP16 / R13, R14, BR09–BR11 | Try to review as a visitor, as a client with no reply from the accountant, as an accountant, and twice as an eligible client; change and delete a review; reply as the accountant and as someone else | Only the eligible client can post, once; the average and count follow every change; only the reviewed accountant can reply, and nobody can alter another person's review | Rule and integration tests + browser |
| TP17 / R15, BR12 | Report a profile, a review and a conversation; block and unblock in a chat; try to message while blocked | A report without a reason is refused; a sent report reaches the owner's inbox with what was reported; the blocked person cannot send and is not told why | Rule and integration tests with a capturing sender + browser |
| TP18 / R16–R18 (agreement part: F04 GS02, GS07) | Finish a first sign-in without agreeing; open each information page signed out; request an unknown address; force a server failure | Registration cannot finish without agreement; every page loads without sign-in; unknown addresses return HTTP 404 with the not-found page; failures show the error page with a support code | Web.Tests + browser |
| TP18a / R16 ([F11](../../specs/legal-pages/spec.md) LP01–LP14) | Open `/terms` and `/privacy` at 320, 390 and 1440 px in both themes and follow a contents link. Sign in with an account whose stored version is older, agree, then sign in with one closed by the RELEASE steps | Full text with the current version and no horizontal scroll; tables stack on phones. The older account is sent to `/account/agree` and returns after agreeing; the closed account lands on the sign-in failure. Versions in `LegalText` and `docs/legal` match | Web.Tests + Tests + browser |
| TP23 / R24, R11 (F06) | Upload a phone photo with location data as an accountant and as a client; replace it; remove it; open each picture address as a visitor and as another user; upload a non-image and a file over 5 MB; break storage during an upload | Stored as a 400 × 400 JPEG with no metadata, upright; the accountant's picture is public, the client's only for the client; initials come back after removal; clear messages; the old picture is kept on failure | Rule tests + Web.Tests (in-memory store) + browser with real Supabase Storage |
| TP24 / R06, R07, R10, BR03–BR05 ([F07](../../specs/chat/spec.md) CH01–CH10) | As a visitor, type to an accountant, reload, sign in and send; reply as the accountant in a second browser; open someone else's conversation address; stop the server with text typed, press Send, start it again; send 21 messages in a minute | The draft survives the reload and sign-in; one message per send, shown live to the other side with unread marks that clear when opened; outsiders get "isn't available" with nothing shown; while disconnected the text stays and nothing is sent later; the 21st message is refused | Rule tests, Web.Tests, browser with two sessions |
| TP25 / R12, BR13 ([F08](../../specs/portfolio-highlights/spec.md) PH01–PH07) | Add, fill, half-fill, remove and save highlights; try a seventh and over-long text; view the profile and the card; a profile with none | Saved highlights appear between Services and Experience with tag and result; cards count them; half-filled rows are refused with field messages and nothing is lost; six is the most; no section or fact without highlights | Rule tests, Web.Tests, browser |
| TP26 / R13, R14, BR02, BR09–BR11 ([F09](../../specs/reviews/spec.md) RV01–RV09) | TP16's steps on the built pages, plus: post a review with no stars and with page script switched off; open a review page for an unknown accountant; sort the directory by "Highest rated", then search and page | Ineligible users see the reason and no form, and forged posts store nothing; a missing rating is refused with the text kept; the static form works without script; the average and count update on the profile and the card; replies show under the review and can be edited; reviewed accountants come first, unreviewed last, and links keep the order | Rule tests, Web.Tests, browser |
| TP27 / R15, BR12, BR19 ([F10](../../specs/report-block/spec.md) RB01–RB09) | TP17's steps on the built pages, plus: report as a visitor, without a reason, twice, an 11th time in a day, your own profile and someone else's conversation; block in one browser while the other has text typed, try "Message" from the profile, then unblock; open the review page as a client the accountant blocked; send a report with page script switched off | Visitors are asked to sign in; reports need a reason, keep the text on failure, are stored once and thank the reporter; own content and others' chats are refused without revealing anything; the block shows live on both sides, neither can send, the typed text survives and returns after unblocking; the blocked client can't review but can delete; the static report form works without script | Rule tests, Web.Tests, browser |
| TP28 / [loading and busy feedback](../DESIGN.md#loading-and-busy-feedback) (Proposed 2026-11-25), R01, R10, BR18 | DevTools with Slow 4G throttling and the cache off. (1) Load each release 1 page ([DESIGN scope](../DESIGN.md#scope-notice)) and make each move between them, stepping through the recorded frames. (2) Hold a write's answer back (a breakpoint in its Application service), double-click its button, then let it answer with an error. (3) Repeat a load and a refresh without throttling. (4) Make each list's read fail (TP08 covers the directory). (5) Open a chat page with EF Core's command log on and count its reads. (6) On Slow 4G, press Send, Block and Retry before the chat panel is interactive. (7) Cut the network on a chat page with typed text (TP03). (8) Turn on reduced motion and a screen reader, and repeat a wait | (1) Every step from frame to frame is "no change" or "content arrives": nothing moves, resizes, or disappears and comes back; the chrome stays put; no general skeleton is replaced by a specific one. (2) On an interactive page one request goes out, and the button stays busy (disabled, `aria-busy`, same size) until the held answer; on a static form the second post changes nothing more (D31). After the error everything typed is still there and the page says what failed (TP02 owns the no-duplicate result). (3) No skeleton, bar or dimming for an answer under 300 ms. (4) The error with Try again, never the empty message. (5) No skeleton → content → skeleton cycle, and one read per page load. (6) The controls are disabled until the panel is interactive; no click is lost silently. (7) Writes are disabled while the notice shows, and the typed text stays. (8) Skeletons and spinners are still; the screen reader reads the wait's name and announces its end | Browser, in each browser of the matrix; screen reader as in TP06. Timings and looks as decided in PRD D29–D34 (2026-11-25); the page-move bar (D33) is checked once built |

Additional application procedures for the service-work expansion (defined, not executed):

| Procedure / requirement | Preconditions and action | Expected observable result | Boundary |
|---|---|---|---|
| TP19 / R19–R21, BR14–BR15 | Save/remove an accountant, request a service repeatedly, accept/decline as the relevant accountant and unrelated accounts; inspect both private lists | Private saving shares no client data under the confirmed model; only scoped requests/engagements appear; duplicate retries are controlled; malformed/unrelated IDs disclose no records | Real server/data tests + browser, after D24. Web tests: `ClientServicesTests` (F03 stage 1, 2026-11-23) |
| TP20 / R21–R22, BR16/BR18 | Change tasks, add a note, fail a save, retry, race two versions and reconnect as each participant | Client cannot write accountant progress; saved values/history agree with completed/total; zero tasks not complete; failed/conflicting drafts retained; no repeated update on retry | Rule, data and integration tests + browser, after D25. Web tests: `ClientServicesTests` (F03 stage 1, 2026-11-23) |
| TP21 / R23, BR15/BR17–BR18 | Stream valid/invalid/oversize files to different services/tasks, interrupt upload, switch/revoke account access, retry an uncertain write, request a guessed file ID | Server validates limits/content, parent association and current actor; unfinished/quarantined bytes unavailable; no duplicate/public object URL or cross-participant download; cleanup observed | Server + real private store + browser, after D26/D28. 2026-11-23: server side covered by web tests with an in-memory store (`ServiceDocumentsTests`); the real bucket and the browser are not run |
| TP22 / R09, R23 | Remove own/other file, terminate relationship, delete each account, fail byte/metadata cleanup and recover | D24/D27 permission/retention policy holds; confirmed operations truthful and recoverable; history and real bytes handled consistently | Data/storage integration + browser, after D24/D27/D28. 2026-11-23: own/other removal, account deletion and a failed write covered by web tests; relationship ending isn't built (D24/D27) |

Prototype CS01–CS10 checks are described in [F03](../../specs/client-services/spec.md#acceptance-scenarios). Use the [prototype checker](../../tools/README.md#check-the-prototype), but do not mark TP19–TP22 passed from HTML simulation.

Domain-specific boundaries to add as features are specified:
- directory ties with an equal sort key (BR02);
- message ordering when two messages arrive at the same time;
- display of times around midnight in Asia/Manila;
- profile edits from two browsers (conflict state).

Not applicable: device data migrations, app packaging and signing, and money precision (no money fields).

## Execution and browser matrix

Commands and versions: [SETUP](../SETUP.md). The supported browsers are Chrome, Edge, Firefox, Brave and Safari (PRD D17, decided 2026-11-11).

| Browser | Device | Screen | State on 2026-11-11 |
|---|---|---|---|
| Google Chrome | This development PC, Windows 11 Pro 10.0.26300 | Desktop window, and a narrow window | Installed (157.0.8103.39); nothing to run yet |
| Microsoft Edge | Same PC | Same | Installed (157.0.4341.46) |
| Mozilla Firefox | Same PC | Same | Installed (159.0.1). An earlier version of this table said it was not installed; that was not checked and was wrong |
| Brave | Same PC | Same, with tracking protection left on | Installed (157.1.99.56) |
| Safari on macOS | A Mac | Desktop window | No device recorded |
| Chrome and Brave on Android | A real phone | Phone | No device recorded |
| Safari on iOS | A real iPhone | Phone | No device recorded |

A narrow desktop window or the browser's device emulation checks layout only. It does not establish behavior on a real phone browser.

For each run, record the browser and version, the build or revision, expected and actual results, and the restoration of anything changed (network settings, zoom, theme). Screenshots must not show real users' chats or contact details.

## Gate definitions

| Gate | Required evidence | Decision owner |
|---|---|---|
| Feature implementation | Required scenarios implemented, reviewed against contracts | lumabanls |
| Automated validation | Relevant tests pass and the web project builds on recorded source | lumabanls |
| Browser acceptance | Required browser procedures pass in each browser of the agreed matrix | lumabanls |
| Release acceptance (test site) | Required gates closed, deployed revision verified (TP07, TP12), limitations and decision recorded | lumabanls |

Optional checks must be labeled optional; do not let absent optional features block unrelated scope. Exceptions need a recorded decision, not an assumed pass.

## Documentation-only checks

Check local Markdown paths and heading anchors, authority/requirement traceability, template placeholder consistency, history preservation, and the final diff. Record existing unrelated broken links separately from new defects. Application tests/builds are unnecessary when only documentation changes.
