# Verification plan

**Authority:** procedures, environments, expected outcomes, and acceptance gates. [PRD](../PRD.md) owns behavior; [STATUS](STATUS.md) owns results. Procedures are defined for release 1 scope; none has been executed. Rewritten on 2026-11-11 for the web platform; procedure IDs are unchanged, TP11 was retired and TP15 added. TP16–TP18 were added the same day for ratings, reports and the information pages.

## Environments and boundaries

| Boundary | What it establishes | What it cannot establish |
|---|---|---|
| Domain/unit tests (Core.Tests) | Validation and authorization rules (BR01–BR08) with controlled inputs and time | Pages, sign-in, browser behavior |
| Data tests (Core.Tests) | Real database provider behavior, migrations, paging order | Hosting configuration; behavior on another provider |
| Web integration tests (Web.Tests) | Public pages, forms, antiforgery, authorization and status codes through `WebApplicationFactory`, with an isolated test database and a test sign-in scheme | Real Google/Microsoft sign-in, real email delivery, JavaScript and live connection behavior |
| Component tests, where used | Interactive component state changes | The real browser, connection loss |
| Web project build | Compilation and Razor checks | Runtime behavior |
| Browser checks on a local run | Observed behavior in the recorded browser and screen size | Other browsers, real phones, the deployed host |
| Deployed-site checks | Observed behavior of the recorded build on the real host, with real sign-in and email | Other browsers or unchecked scenarios |
| Acceptance decision | Owner's decision against required gates | Untested functionality outside that decision |

### Prototype design checks

For F02, [BD01–BD06](../../specs/brand-design-refresh/spec.md#acceptance-scenarios) define the prototype outcomes. Run the [prototype checker](../../tools/README.md#check-the-prototype), inspect representative screenshots, check the inline script with `node --check`, parse SVG assets, and run the documentation checker. Expected outcomes: usable directory/recovery and existing simulated flows, no horizontal clipping at recorded widths, readable light/dark semantic color pairs, keyboard access to the skip link, intact local assets, and preserved historical records. The 4.5:1 text and 3:1 control checks are implementation checks, not a decision on PRD D15 or full accessibility certification.

These observations are separate from TP01–TP18 application execution. The prototype contains no server, real sign-in, real delivery or persistence, so a passing prototype check cannot complete those gates. Current observations are linked from [STATUS](STATUS.md).

Use isolated test databases, controllable clocks and synthetic accountant and client data. Do not rely on an in-memory database substitute for persistence rules. Emails in tests go to a capturing test sender, never to real addresses.

## Scenario mapping

| Procedure / requirement | Preconditions and action | Expected observable result | Boundary / environment |
|---|---|---|---|
| TP01 / R01, R03 | As a visitor, open the directory, open a profile, reload, and open the profile's address in a new tab | List and profile load without sign-in; every published profile field is shown; the address alone reproduces the page | Web.Tests + browser |
| TP02 / R05, R06, BR04 | Empty or whitespace message; incomplete profile; repeated clicks on Send and Save; a controlled save failure, then retry | No invalid or duplicate message or profile change; typed text kept; truthful error, then success on retry | Core/Web tests + browser |
| TP03 / R10 | On a chat page with typed text, cut the network, wait for the notice, restore it; repeat and reload the tab instead | A connection-lost notice appears; the text is still there after reconnecting and after a reload; nothing was sent meanwhile | Browser |
| TP04 / server schema | Apply each new migration to a database at the previous schema with synthetic data; make a migration fail part-way | Data kept; a failed migration leaves the previous schema usable | Data tests with isolated database |
| TP05 / R01, R06, R07 | Back and Forward through directory, profile and chat; share a filtered directory address; keep the same account open in two tabs and send from one | Back and shared addresses show the same list; the second tab shows the new message without a duplicate | Browser |
| TP06 / DESIGN | Light and dark themes, browser zoom to the D15 target, a screen reader, keyboard only, the smallest supported width | All actions reachable and readable; labels and focus order correct; no clipped or overlapping content | Each browser in the matrix |
| TP07 / RELEASE | Deploy a recorded build to the test site; check the health address, HTTPS, sign-in with Google and with Microsoft, one message, one email | The deployed revision matches; both providers sign in and return to the site; the message and email arrive | Deployed site |
| TP08 / R10 | Stop the server, or go offline, during a directory load and during a message send; restore and retry | Error or connection-lost state shown, never an empty list; typed text kept; retry succeeds; nothing is sent automatically later | Web.Tests + browser |
| TP09 / R06, BR04 | Make the response to a send get lost after the message is saved, then retry | Exactly one message exists; the site reports the real outcome | Core/Web tests |
| TP10 / R04, R11, BR05 | Let the session expire, then act; sign in as a second client and a second accountant; request another user's conversation address directly | Sign-in, then back to the same page with the draft kept; no user can read or post to a conversation they are not part of; client details never appear on public pages | Web.Tests + browser |
| TP11 | Retired 2026-11-11. It tested an older installed app against a newer API, which no longer exists | — | — |
| TP12 / RELEASE | Inspect the deployed configuration | HTTPS redirect and HSTS on; secure session cookie; detailed error pages off; no development secrets, test sign-in scheme or development email sender; public pages follow the D19 indexing decision | Deployed site |
| TP13 / R08, BR08 | A user is not on the site; the other person sends two messages; repeat for a client and for an accountant, with emails turned off, through the email's own "turn off" link, and with the email service failing | One email per the D18 frequency rule, with no message text and a working link; none when turned off; with the service failing, the message is still saved and the email is retried | Web.Tests with capturing sender + deployed site |
| TP14 / R09, BR06 | Delete an accountant account and a client account with existing conversations | Confirmation required. Afterwards the profile is gone from the directory and from old links, signing in again starts a fresh registration, and no more emails are sent. Chat handling follows D07 | Core/Web tests + browser |
| TP15 / RELEASE | Restart or redeploy the server while a chat page with typed text is open | A reconnecting notice, then a working page with the typed text restored; no message lost or duplicated | Browser against a local or deployed run |
| TP16 / R13, R14, BR09–BR11 | Try to review as a visitor, as a client with no reply from the accountant, as an accountant, and twice as an eligible client; change and delete a review; reply as the accountant and as someone else | Only the eligible client can post, once; the average and count follow every change; only the reviewed accountant can reply, and nobody can alter another person's review | Core/Web tests + browser |
| TP17 / R15, BR12 | Report a profile, a review and a conversation; block and unblock in a chat; try to message while blocked | A report without a reason is refused; a sent report reaches the owner's inbox with what was reported; the blocked person cannot send and is not told why | Core/Web tests with a capturing sender + browser |
| TP18 / R16–R18 | Finish a first sign-in without agreeing; open each information page signed out; request an unknown address; force a server failure | Registration cannot finish without agreement; every page loads without sign-in; unknown addresses return HTTP 404 with the not-found page; failures show the error page with a support code | Web.Tests + browser |

Additional application procedures for the service-work expansion (defined, not executed):

| Procedure / requirement | Preconditions and action | Expected observable result | Boundary |
|---|---|---|---|
| TP19 / R19–R21, BR14–BR15 | Save/remove an accountant, request a service repeatedly, accept/decline as the relevant accountant and unrelated accounts; inspect both private lists | Private saving shares no client data under the confirmed model; only scoped requests/engagements appear; duplicate retries are controlled; malformed/unrelated IDs disclose no records | Real server/data tests + browser, after D24 |
| TP20 / R21–R22, BR16/BR18 | Change tasks, add a note, fail a save, retry, race two versions and reconnect as each participant | Client cannot write accountant progress; saved values/history agree with completed/total; zero tasks not complete; failed/conflicting drafts retained; no repeated update on retry | Core/Web/data tests + browser, after D25 |
| TP21 / R23, BR15/BR17–BR18 | Stream valid/invalid/oversize files to different services/tasks, interrupt upload, switch/revoke account access, retry an uncertain write, request a guessed file ID | Server validates limits/content, parent association and current actor; unfinished/quarantined bytes unavailable; no duplicate/public object URL or cross-participant download; cleanup observed | Server + real private store + browser, after D26/D28 |
| TP22 / R09, R23 | Remove own/other file, terminate relationship, delete each account, fail byte/metadata cleanup and recover | D24/D27 permission/retention policy holds; confirmed operations truthful and recoverable; history and real bytes handled consistently | Data/storage integration + browser, after D24/D27/D28 |

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
