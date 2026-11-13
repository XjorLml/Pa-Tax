# Tax Buddy software design

**Authority:** architecture and technical contracts. [PRD](PRD.md) owns behavior. [SETUP](SETUP.md) owns chosen tool versions; [DEVELOPMENT](DEVELOPMENT.md) owns coding practices. Status: Proposed — no source exists yet. Reviewed: 2026-11-11 against repository baseline `b9b50f3` (documentation only). Directory address and order updated 2026-11-12 (C03 resolved; D04/D05 provisional). Project layout and per-page render modes updated 2026-11-13 ([ADR 010](adr/010-single-project-per-page-render-modes.md)).

## Architecture and dependencies

Tax Buddy is one ASP.NET Core Blazor Web App on .NET 11 ([ADR 005](adr/005-web-platform-blazor.md)). There is no separate API and no installed client. Business rules, use cases and data access live in the same web project as the pages ([ADR 010](adr/010-single-project-per-page-render-modes.md)). The design has no payments or bookings, and no file storage other than the private documents attached to an accepted service ([R23](PRD.md#requirements); policies D26–D28). None of these projects exist yet; they are created at the scaffold step.

```text
TaxBuddy.slnx
global.json                    # created when the .NET 11 SDK is installed (SETUP)
src/
  TaxBuddy.Web/                # the one Blazor Web App and its composition root
    Program.cs                 # services, sign-in, rate limits, health checks
    Domain/                    # typed IDs, roles, business rules; interfaces for email and the clock
    Application/               # use cases: directory, profiles, conversations, reviews, reports and blocks,
                               #   accounts, notifications, service workspaces
    Data/                      # EF Core context, mappings, migrations, queries
    Components/
      Layout/ Pages/ Shared/   # Razor components
    Account/                   # sign-in, sign-out and provider callback endpoints
    Services/                  # email sender, live chat notifier, background email worker
    wwwroot/                   # styles and static assets
tests/
  TaxBuddy.Tests/              # business rules; data tests against the real database provider
  TaxBuddy.Web.Tests/          # integration tests through WebApplicationFactory; component tests where useful
```

| Component | Responsibility | Allowed dependencies | Source |
|---|---|---|---|
| Domain | Typed IDs, roles, business rules (BR01–BR18), interfaces for email sending and the clock | .NET base library only. No ASP.NET Core, EF Core or Razor types | `src/TaxBuddy.Web/Domain/` (planned) |
| Application | Use-case services. Every authorization rule is enforced here, not in the UI | Domain. No Razor or HTTP types | `src/TaxBuddy.Web/Application/` (planned) |
| Data | EF Core context, migrations, queries, user store | Domain, Application interfaces, EF Core, the chosen database provider | `src/TaxBuddy.Web/Data/` (planned) |
| Pages and components | Rendering, forms, validation display, draft retention | Application services | `src/TaxBuddy.Web/Components/` (planned) |
| Account endpoints | External sign-in challenge and callback, sign-out, cookie session | ASP.NET Core authentication, Application | `src/TaxBuddy.Web/Account/` (planned) |
| Services | Email sender adapter, in-process chat notifier, background email worker | Application, hosting APIs | `src/TaxBuddy.Web/Services/` (planned) |

Dependency rules:
- One production project. Its layers are folders: Domain references nothing project-specific; Application depends on Domain; Data implements their interfaces; Components, Account endpoints and Services call Application.
- Domain and Application never use Razor, HTTP or EF Core types, so the rules stay testable without the web host.
- The compiler does not enforce folder rules. `LayerRulesTests` in `TaxBuddy.Tests` does, on every test run. It checks that Domain and Application use no ASP.NET Core, EF Core or Npgsql types, that Domain doesn't use Application, and that no component injects the data layer. It reads signatures and injected members, not method bodies, so review covers the rest.
- Components call Application services. They never use the database context directly.

### Render modes

The render mode is chosen per page or component ([ADR 010](adr/010-single-project-per-page-render-modes.md); modes per the [render modes reference](REFERENCES.md#implementation-and-hosting)). A page is static unless this table says otherwise. No app-wide interactive mode is set on the router, and Interactive WebAssembly and Auto are not used, so there is no client project.

| Page or component | Render mode | Why |
|---|---|---|
| Directory `/`: browse and search | Static server rendering (the Blazor default) | Search engines and first-time visitors get the complete page in the first response. Search is a plain GET form; sort and page are links ([C03](README.md#history-and-unresolved-conflicts)) |
| Profile `/accountants/{id}`, including portfolio highlights, reviews, review replies and the average rating | Static server rendering | Public and meant to be found; reviews are part of the page itself |
| Sign-in, registration (role choice and agreement), information, not-found and error pages | Static server rendering | Plain links and forms that work before any script loads |
| Signed-in actions on static pages: add to or remove from my accountants, report a profile or a review, the accountant's reply to a review | Static server rendering, as named forms posted to the same page | Visitors never open a server connection. Shown only to signed-in users who may take the action; see the form rules below |
| Review submission `/accountants/{id}/review` | Interactive Server | An action by a signed-in client |
| Conversation list and chat thread `/chats`, `/chats/{id}` | Interactive Server | Private, with live updates over SignalR |
| New chat `/chats/new/{accountantId}` | Interactive Server; open to visitors | The one interactive page a visitor can open. It shows no private data. A visitor can type before signing in (R06); sending requires sign-in, and the draft survives sign-in in the tab's session storage. Until chat is built it is a static "coming soon" page (F01) |
| My accountants (the client's shortlist) and the accountant's client list `/workspace`, `/clients/{id}` | Interactive Server | Private to one user |
| Service request, checklist, progress and documents `/accountants/{id}/request-service`, `/work/{id}` | Interactive Server | State shared by the two participants. File bytes go through endpoints, not the page connection ([private service workspaces](#private-service-workspaces)) |
| Profile editor and account `/account/profile`, `/account`, `/account/delete` | Interactive Server | Rich forms |

Apart from new chat, every interactive page requires sign-in, so only signed-in users hold a server connection (the "circuit"). Cost of this choice: each open interactive page holds one, and it drops when the server restarts. Drafts on interactive forms other than chat depend on open conflict [C04](README.md#history-and-unresolved-conflicts).

Forms on static pages follow the static rendering rules ([forms reference](REFERENCES.md#implementation-and-hosting)):
- each form has a unique name;
- `UseAntiforgery` validates the antiforgery token;
- validation runs on the server after the post;
- the Application service checks the signed-in user, as on any other page.

A successful post redirects back to the page, so a reload does not repeat it. A failed post shows the page again with the user's input and the error.

Possible later additions, each only with a concrete trigger: moving Domain, Application and Data into a class library if the folder rules keep being broken or a second host is approved; a separate API project if a native app is ever approved; a browser test project after repeated regressions; a shared notifier if the site runs on more than one server.

## Technology stack

One table for the whole stack. The owner approved every choice in this table on 2026-11-11 ("stack approved for choices column"). Details that are still open are named in the State column.

| Layer | Choice | State |
|---|---|---|
| Language and runtime | C# on .NET 11 | Decided ([ADR 005](adr/005-web-platform-blazor.md)); reconfirmed 2026-11-11 after Next.js was considered |
| Web framework | ASP.NET Core Blazor Web App | Decided (ADR 005) |
| Live updates | SignalR, which is built into Blazor's interactive server mode | Decided 2026-11-11 |
| Database | Supabase managed PostgreSQL | Decided ([ADR 007](adr/007-supabase-brevo-container-host.md)) |
| Data access | Entity Framework Core with the Npgsql provider | Decided 2026-11-11 |
| Sign-in | Google and Microsoft accounts through ASP.NET Core authentication | Decided ([ADR 006](adr/006-web-sign-in.md)) |
| User store | ASP.NET Core Identity | Decided 2026-11-11 (ADR 006) |
| Styling | Tailwind CSS, with Tax Buddy's semantic colors as theme values. No component library | Stack decided 2026-11-11; current prototype direction applied under the delegated design brief ([DESIGN](DESIGN.md#visual-system)); application acceptance pending |
| Email | Brevo, called from a background service in the application | Decided 2026-11-11 (ADR 007) |
| Background work | ASP.NET Core's built-in hosted services | Decided 2026-11-11 |
| Tests | xUnit; `WebApplicationFactory` for integration tests; bUnit for components where useful | Decided 2026-11-11 |
| Hosting | Render, as a container, in its Singapore region | Decided 2026-11-11 (ADR 007); the plan and its price are still to confirm |
| Editor | VS Code with C# Dev Kit | Owner asked to use it, 2026-11-11 ([SETUP](SETUP.md#reproducibility-record)) |
| CI | GitHub Actions, once the repository has a remote | Decided 2026-11-11 |

Not used: Supabase's sign-in, live-update, storage and web API features; a JavaScript framework; a separate API project.

## Platform capability matrix

Exact SDK versions are in SETUP.

| Platform | Baseline | Needed capabilities | Unavailable behavior | Evidence |
|---|---|---|---|---|
| Desktop browsers: Chrome, Edge, Firefox, Brave, Safari | Current versions (PRD D17). Blazor's supported list names Chrome, Edge, Firefox and Safari ([REFERENCES](REFERENCES.md#toolchain-and-platforms)). Brave is built on the same engine as Chrome and is tested separately, with its tracking protection left on | HTTPS, cookies; JavaScript and WebSockets for interactive pages | Without JavaScript, the directory and profile pages still work and signed-in pages show a notice. Without WebSockets, behavior is to be verified at implementation | Pending ([STATUS](delivery/STATUS.md)) |
| Phone browsers: Chrome and Brave on Android, Safari on iOS | Same | Same, plus a layout that works from the smallest supported width (D15) | Same | Pending |
| Server host | ASP.NET Core on .NET 11, in a container on Render in its Singapore region ([ADR 007](adr/007-supabase-brevo-container-host.md)) | WebSockets, persisted data-protection keys, outbound email, the database | — | Pending |
| Native Android, Windows, iOS, Mac apps | Not targeted ([ADR 005](adr/005-web-platform-blazor.md)) | — | — | Not applicable |

## Data and persistence contracts

The server database is the only store of record.

| Contract / requirement | Definition |
|---|---|
| Identity and mutation | Users, profiles and conversations get server-generated GUIDs. A message gets its GUID when the composer is shown, so a repeated submit stores one message (BR04). Profile updates carry a row version; a stale save gets a conflict state, not a silent overwrite |
| Dates, instants, money | Instants are stored as UTC and converted to Asia/Manila in one display helper (BR07). Experience months, if D04 adds them, are `YYYY-MM` values without a zone. No money fields in release 1 |
| Queries | The directory pages through a stable order with a unique ID tie-breaker (BR02): name A–Z, then ID, by default; "highest rated" orders by the computed average, review count, name, then ID, with unreviewed accountants last (D05, provisional). Messages are ordered by server receive time, then message ID |
| Storage and ownership | Supabase's managed PostgreSQL ([ADR 007](adr/007-supabase-brevo-container-host.md)), through EF Core with the Npgsql provider. Interactive pages take a new context from a factory for each operation, because a context is not thread safe and a circuit lives much longer than a request. In the browser: the session cookie, and unsent chat text in the tab's session storage (Proposed) |
| Schema evolution | EF Core migrations kept with the source. Production changes go through a migration bundle or a reviewed SQL script ([RELEASE](RELEASE.md#deployment)), not automatic migration at startup |
| Failure/downgrade | Each migration step is atomic where the provider allows. A rollback plan is recorded with each release that changes the schema |
| Ratings | One review per client and accountant, enforced by a unique constraint. The average and count are computed from the reviews, never stored by hand. The reviewer's short name is produced when the page is rendered |
| Reports and blocks | A report stores who reported, what was reported, the reason and the time, and emails the site owner (D22). A block is stored per conversation with who set it |
| Retention and backup | Account deletion removes the profile, external sign-in links and email preference (BR06). Chat/service retention per D07/D14; private files and relationship termination per D27. Backup policy belongs to D11 |

For SQL: parameterized queries only, constraints for the rules the database can enforce, and tests that run against the real provider rather than an in-memory substitute.

Supabase is used as a PostgreSQL database only (Proposed in ADR 007). Sign-in stays in the application ([ADR 006](adr/006-web-sign-in.md)), and Supabase's own sign-in, live-update and file-storage services are not used.
- **Connection:** the direct connection, or the session pooler on port 5432 where the host has no IPv6. The transaction pooler on port 6543 is meant for serverless functions and does not support prepared statements, so it is not used ([REFERENCES](REFERENCES.md#hosting-database-and-email-services)).
- **Supabase's web API:** Supabase can publish tables through its own web API. The application does not use it, so it is switched off or pointed at an empty schema, and row-level security is enabled on every table as a second barrier. Confirm both settings when the project is created.
- **Projects:** one Supabase project for development and tests with synthetic data, and a separate one for the test site. The Free plan allows two active projects, pauses a project after a week without activity, and holds 500 MB.

## Sign-in, sessions and authorization

| Decision | Selected approach |
|---|---|
| Sign-in method | Google or Microsoft account through ASP.NET Core external authentication ([ADR 006](adr/006-web-sign-in.md)). The browser is redirected to the provider and back to the site's callback address. No Tax Buddy passwords |
| User store | ASP.NET Core Identity with external sign-ins only (Proposed in ADR 006) |
| Session | A cookie that is HTTPS-only and unreadable by scripts. Lifetime is set at implementation |
| Expired session | The next action sends the user to sign-in and then back to the same address. Unsent chat text is kept |
| Sign-out | Ends the session in that browser |
| Return address after sign-in | Local addresses only, so a crafted link can't send users to another site |

Roles: `client` and `accountant`, chosen at first sign-in (D03 decides whether one account can hold both).

Authorization rules, enforced in Application services on the server:
- Directory and profile reads are public, and return only listed profiles (BR01).
- A profile can be changed only by the accountant who owns it.
- A conversation and its messages are readable and writable only by its client and accountant (BR05).
- Only a client can start a conversation, and only with a listed accountant (BR03).
- A client can write a review only for an accountant who has sent a message in their conversation, once, and only that client can change or delete it (BR09).
- Only the reviewed accountant can reply to a review. Nobody can edit another person's review (BR11).
- Reporting and blocking need a signed-in user. A blocked person cannot add messages to that conversation (R15).
- Saved-accountant lists belong to their authenticated client. A private client list is scoped to its accountant; saving alone never exposes a client's account in the confirmed save/request/accept model.
- Service tasks, updates and files inherit both participant IDs from their engagement. Every read/write/download/removal resolves the parent relationship and current actor, including asynchronous completion. Proposed editor/removal permissions follow the private-workspace table below; UI guards do not enforce security.

A record ID in an address or a form is never proof of access. Hiding a button is not authorization: the service behind it checks again.

Provider client secrets stay on the server: user-secrets in development, the host's secret store in production ([SETUP](SETUP.md#local-development)).

## Private service workspaces

[PRD](PRD.md) R19–R23 / BR14–BR18 own behavior; [F03](../specs/client-services/spec.md) refines scenarios; [ADR 009](adr/009-private-service-workspaces.md) records scope and storage boundaries. This is a proposed implementation contract, not existing .NET source. D24–D28 must be resolved before production implementation/intake.

| Planned entity | Ownership / constraints |
|---|---|
| SavedAccountant | Client ID + accountant ID unique pair; private bookmark independent of shared work |
| ClientRelationship | Client ID + accountant ID; remaining lifecycle/termination under D24. Not created by public profile views, saves or chat alone in the confirmed model |
| ServiceEngagement | Parent participant pair, service name snapshot, requested/active/declined phase, version. Request acceptance is transactional; repeated requests/retries use operation IDs. Prototype deduplicates an existing non-declined service by pair/name; repeat-service policy remains D24 |
| ServiceTask | Engagement FK, title, explicit status, stable ordering and version. Completion/progress per D25; do not store an unrelated percentage |
| ServiceUpdate | Engagement FK, author, UTC instant, shared note or activity record. Exclude contents from logs; ordering uses timestamp plus unique tie-breaker |
| ServiceFile | Engagement FK, optional task FK verified to belong to that engagement, uploader, opaque storage key, original display name, validated media type/length, lifecycle state and operation ID. Original filename is never a storage path or authorization token |

All new IDs are server-generated GUIDs in the application (short fixture IDs in the prototype). Database constraints enforce parent associations and uniqueness; application services enforce the current participant and relationship state on every operation. Proposed permission matrix:

| Operation | Client participant | Accountant participant | Visitor / unrelated actor |
|---|---|---|---|
| Save/remove personal accountant entry | Own list | No | No |
| Request service | Own pair with listed accountant | No; may add work for an existing accepted relationship (D24) | No |
| Accept/decline service | Read result | Own received request | No |
| Read service/tasks/shared updates/files | Yes | Yes | No |
| Add tasks, change statuses, post shared progress note | Read-only progress | Yes, accepted work (D25) | No |
| Attach/download service documents | Yes, accepted work | Yes, accepted work | No |
| Remove document | Own uploaded file only (D27 proposal) | Own uploaded file only (D27 proposal) | No |

Application services are planned in the Application folder (relationship, engagement, progress and document use cases), with EF metadata in the Data folder and thin Razor components. A private file-store interface separates metadata from bytes. The provider and scanning/quarantine implementation are open D28; Supabase's file-storage service is not silently enabled. A database blob approach or separate private byte store requires an explicit decision and operations plan.

Real upload contract: authenticate/authorize before intake; antiforgery on mutating forms; enforce D26 byte/type/quantity limits on the server while streaming; treat client type/name as untrusted; keep incomplete/unvalidated/quarantined bytes unavailable. Metadata becomes Available only after the byte write and required validation/scanning complete. Coordinate partial-write cleanup and retries with operation IDs; a timeout does not prove failure. Downloads resolve current permission and Available state, return an attachment with an escaped/sanitized filename and safe response headers, and never reveal storage credentials or public object URLs. Removal rechecks uploader rights and retention policy, records the event, and coordinates byte/metadata cleanup. Revoked sessions/relationships cannot complete writes under an earlier actor's authorization.

Progress writes use optimistic concurrency; a stale version returns a conflict with the draft retained and current values for review. New shared updates appear on reload/reconnect; live cross-tab refresh can reuse the application's notifier when implemented. No service-progress emails are added to R08. Relationship deletion/blocking implications remain D24/D27; chat block semantics are not silently extended to revoke file access.

Proposed same-site routes: `/workspace` (role-aware private list), `/clients/{clientId}` (accountant-only grouped services), `/accountants/{id}/request-service`, `/work/{engagementId}` and application-owned file form/download endpoints beneath `/work/{engagementId}/files/{fileId}`. These are routes/endpoints of the one web app, not a new public API. URL IDs never grant access. Unauthorized/missing records show a generic unavailable state without other participants' details.

Prototype boundary: all service metadata lives in memory; drafts are separated by actor/service, survive simulated failures within the loaded page, and clear on reload. Upload uses generated sample selections, not a real file picker. Download generates synthetic PDF/CSV/TXT bytes locally. The production lifecycle, persistence and security contract above is not exercised by the HTML artifact.

## Live chat and email notifications

| Contract | Definition |
|---|---|
| Live updates | When a message is saved, an in-process notifier tells the open chat pages of that conversation's two participants, which then re-read from the database. This assumes one server instance; more than one needs a shared notifier and session affinity (decide before scaling out) |
| Missed updates | A page always loads from the database when it renders or reconnects. It never trusts a live event alone |
| Email (R08) | Saving a message also writes an email job for the other participant in the same database transaction. A background worker sends it through the email sender interface, which calls Brevo ([ADR 007](adr/007-supabase-brevo-container-host.md)), and retries on failure. A failed email never fails or loses the message |
| Email content (BR08) | No message text. A link to the conversation, the site name, and a link to turn emails off |
| Email frequency | Proposed: at most one email per conversation until the recipient opens it (D18) |
| Email address | The address from the account's Google or Microsoft sign-in (Proposed) |
| Development | No real emails. A development sender writes them to a local folder or the log |

## Navigation and screen state

Addresses are proposed; the first feature spec refines the directory ones.

| Address | Input contract | Retained state | Back, reload, shared-link behavior |
|---|---|---|---|
| `/` | Query: `q` (search text), `service`, `sort` (`rating`; absent means name A–Z; added with ratings), `page`. Search and service are submitted with a plain GET form; on wide screens each service button submits it. Sort and page are links that carry the current search and filter. No live filtering in the page ([C03](README.md#history-and-unresolved-conflicts), resolved) | All in the address, so Back, reload and shared links show the same list | Unknown or malformed query values are ignored |
| `/accountants/{id}` | GUID | None | Unknown, deleted or unlisted ID shows "no longer available" with HTTP 404 (R03) |
| `/accountants/{id}/review` | GUID; the user must be an eligible client | Stars and text kept through a failed save | An ineligible user sees why, and no form (BR09) |
| `/privacy`, `/terms`, `/about`, `/help`, `/for-accountants` | None | None | Static pages, public (R16, R17) |
| `/email/off` | The signed-in user, or a signed link from an email | None | Turns notification emails off and offers to turn them back on (R08) |
| Any other address | — | — | "Page not found" with HTTP 404; failures show an error page with a support code (R18) |
| `/signin` | Local return address | None | Cancel at the provider returns to the previous page; success continues to the return address |
| `/chats` | Signed-in user | Conversation list | Email links fall back here when the conversation can't be opened |
| `/chats/{id}` | GUID; the user must be a participant | Unsent text kept across a lost connection and a reload of the tab | Not a participant, or unknown ID: "not available", never another user's content |
| `/chats/new/{accountantId}` | GUID of a listed accountant | Unsent text kept through sign-in | Becomes the existing conversation if the pair already has one (per D06) |
| `/account/profile` | Signed-in accountant | Unsaved edits; confirmation on leave | A save that succeeded is reported as saved even if the next navigation fails |
| `/account`, `/account/delete` | Signed-in user | None | Delete needs confirmation; on success the user is signed out and sent to `/`; service/file handling still requires D27 |
| `/workspace`, `/clients/{id}`, `/accountants/{id}/request-service`, `/work/{id}` | Authenticated participant and valid parent IDs; [private-workspace contract](#private-service-workspaces) | Draft request/task/note state retained on failed write; real file reselect/retry strategy decided at implementation | Missing/unauthorized is generic unavailable; proposed routes; durable metadata reloaded from server in the application |

A submit button is disabled while its operation runs, so a double click can't run it twice.

### As built in F01 (2026-11-13)

[Evidence](delivery/evidence/2026-11-13-f01-browse-accountants.md).

**Directory `/`:**
- The query is normalized: search text is trimmed, its spaces collapsed and capped at 100 characters, and unknown values are ignored.
- When the address holds two `service` values, the later one wins and the page redirects to the one-value address. This happens when a service button is pressed on a wide screen, because the form also sends its hidden service menu.
- Page size is 6, a working value (D05).
- A database failure shows "We couldn't load the accountants" with "Try again" and HTTP 503.

**Profile `/accountants/{id}`:**
- An unknown, unlisted or malformed ID calls `NavigationManager.NotFound()`. This Blazor version sends no body with a page-set 404, so the not-found page renders "This profile is no longer available" for `/accountants/…` addresses and "Page not found" for everything else, both with HTTP 404.
- A database failure shows "We couldn't load this profile" with HTTP 503. Status-code pages are switched off for that response so they don't replace the page.

**`/chats/new/{accountantId}`:** a static "coming soon" page until chat is built.

**Tables:** `accountant_profiles`, `accountant_services` and `accountant_experience`, with snake_case names. Services are unique per profile, and both child tables are deleted with their profile. Experience months are stored as the first day of the month in `date` columns.

**Development command:** `dotnet run --project src/TaxBuddy.Web -- seed-development` resets the profiles to the synthetic seed. It runs only in the Development environment.

**Public pages:** every response carries `X-Robots-Tag: noindex, nofollow` and a matching meta tag, unless `Seo:AllowIndexing` is true (D19).

## DI, startup, async work, and lifecycle

| Service / page | Lifetime | Owner and concurrency/disposal contract |
|---|---|---|
| Application services | Scoped | On an interactive page a scope lives as long as the circuit, so services hold no per-request state and take a database context per operation |
| Database context factory | Singleton factory, short-lived contexts | One context per operation, disposed when it ends |
| Chat notifier | Singleton | Thread safe. Pages subscribe when shown and unsubscribe when disposed; updates are marshalled onto the page's own dispatcher |
| Email worker | Hosted background service | Stops with the host. Unsent email jobs stay in the database for the next start |
| Pages and components | Per render or per circuit | Cancel their work and release subscriptions when disposed |

Lost connections: Blazor shows a reconnecting notice and tries to return to the same circuit. If it can't, the page reloads with fresh state from the database, and the chat draft is restored from the tab's session storage. State kept only in server memory is treated as disposable.

Interactive components read the signed-in user from the authentication state they are given, not from the HTTP request, which does not exist during interactive rendering.

## Errors, diagnostics, and security

| Situation | What the user sees |
|---|---|
| Invalid input | Field errors; input kept |
| Not signed in, or session expired | Sign-in, then back to the same address |
| Not allowed | An explanation; no content |
| Missing or deleted record | "No longer available", with a way back |
| Changed by someone else (profile) | The latest version, with the user's edits kept for review |
| Too many requests | A request to wait; no automatic retry loop |
| Server failure | An error page with a support code (correlation ID) and a retry; never an empty list |
| Connection lost on an interactive page | A reconnecting notice; typed text kept (R10) |

- **Diagnostics:** event IDs, error category, duration and correlation ID only. Message text, email addresses, contact details and tokens are never logged. No telemetry service in release 1.
- **Transport and forms:** HTTPS everywhere, with HSTS in production. Antiforgery protection on every form. All input is validated on the server.
- **Abuse limits:** rate limits on sign-in, message sending and directory search, counted per server instance. Message length and profile field lengths are capped so a connection can't make the server hold unbounded data.
- **Keys and secrets:** data-protection keys are persisted outside the app's files, so sessions survive restarts and redeployments. No secrets in the repository or in `appsettings*.json`.
- **Search engines:** public pages are built to be indexed. Each one gives its full content in the first response, with its own title, description and canonical address, and a sitemap can list the listed profiles. Whether indexing is switched on is PRD [D19](PRD.md#open-decisions). Until it is decided, and always on the invite-only test site (D20), public pages ask not to be indexed.

## Traceability and decisions

| Requirement / feature scenario | Technical component | Verification boundary |
|---|---|---|
| R01–R03 / [browse-accountants](../specs/browse-accountants/spec.md) FT01–FT07 | Directory use cases and queries; directory and profile pages | Tests, Web.Tests; TP01, TP05, TP06 |
| R04 | Account endpoints, user store, role choice | Web.Tests; TP10; real providers in TP07 |
| R05 | Profile use cases and editor page | Tests, Web.Tests; TP02 |
| R06, R07, R10 | Conversation use cases, chat pages, chat notifier | Tests, Web.Tests; TP02, TP03, TP08, TP09 |
| R08 | Email jobs, background worker, email sender | Tests, Web.Tests; TP13 |
| R09 | Account deletion use case and page | Tests, Web.Tests; TP14 |
| R11 | Authorization in Application services; directory projection | Tests, Web.Tests; TP10 |
| R12 | Portfolio highlights in the profile use cases and editor page | Tests, Web.Tests; TP02 |
| R13, R14 | Review use cases, rating queries, review and profile pages | Tests, Web.Tests; TP16 |
| R15 | Report and block use cases; report email | Tests, Web.Tests; TP17 |
| R16–R18 | Static pages, sign-in agreement, not-found and error handling | Web.Tests; TP18 |
| R19–R23 / F03 CS01–CS10 | Private relationship/engagement/progress/document use cases, EF metadata and private byte-store boundary | TP19–TP22 application checks; CS prototype checks recorded separately |

Significant rationale: [ADR index](adr/README.md). Conflicts: C04 open; C03 resolved 2026-11-12 ([docs index](README.md#history-and-unresolved-conflicts)). Remaining technical risks, owner lumabanls:
- Restarts and deployments drop interactive connections (TP15 checks what users see).
- Email delivery depends on Brevo and on setting up a sender domain (D18).
- Render's free service sleeps after 15 minutes without traffic, and Supabase's Free plan pauses a project after a week without activity. Either takes the test site down until it wakes, so both need paid plans before testers rely on the site.

Verified state belongs in [STATUS](delivery/STATUS.md).
