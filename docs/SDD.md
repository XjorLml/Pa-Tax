# Tax Buddy software design

**Authority:** architecture and technical contracts. [PRD](PRD.md) owns behavior. [SETUP](SETUP.md) owns chosen tool versions; [DEVELOPMENT](DEVELOPMENT.md) owns coding practices. Status: Proposed — no source exists yet. Reviewed: 2026-11-11 against repository baseline `b9b50f3` (documentation only). Directory address and order updated 2026-11-12 (C03 resolved; D04/D05 provisional). Project layout and per-page render modes updated 2026-11-13 ([ADR 010](adr/010-single-project-per-page-render-modes.md)).

## Architecture and dependencies

Tax Buddy is one ASP.NET Core Blazor Web App on .NET 11 ([ADR 005](adr/005-web-platform-blazor.md)). There is no separate API and no installed client. Business rules, use cases and data access live in the same web project as the pages ([ADR 010](adr/010-single-project-per-page-render-modes.md)). The design has no payments or bookings. It stores files only for profile pictures ([R24](PRD.md#requirements), [ADR 011](adr/011-profile-pictures-supabase-storage.md), since 2026-11-16) and the private documents attached to an accepted service ([R23](PRD.md#requirements); policies D26–D28). None of these projects exist yet; they are created at the scaffold step.

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
| Services | Email sender adapter, in-process chat notifier, background email worker | Application, hosting APIs | `src/TaxBuddy.Web/Services/` (chat notifier since F07; email planned) |

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
| Sign-in, registration (role choice and agreement), agreeing to new terms (`/account/agree`, F11), information, not-found and error pages | Static server rendering | Plain links and forms that work before any script loads |
| Theme toggle in the header (site bar and phone app bar), on every page | Static server rendering: a plain form posted to the endpoint `POST /preferences/theme`, which redirects back to the page | Added 2026-11-16 (F02 BD09) as a footer switch; since 2026-11-23 (BD14) one icon in the header. A choice shared by all pages, so it is posted to an endpoint rather than to each page as a named form. Antiforgery applies as for any form; no script and no server connection. The form holds both buttons (`value="dark"`, `value="light"`), and the stylesheet shows the one that switches away from the current theme |
| Signed-in actions on static pages: add to or remove from my accountants, the accountant's reply to a review | Static server rendering, as named forms posted to the same page | Visitors never open a server connection. Shown only to signed-in users who may take the action; see the form rules below |
| Review page `/accountants/{id}/review` | Static server rendering: a named form posted to the same page | Changed from Interactive Server on 2026-11-17 by the user's choice ([F09](../specs/reviews/spec.md)), as for the profile editor: a failed or invalid save shows the form again with the stars and text, and it works without script. This closes C04 for reviews |
| Report page `/report/{kind}/{id}` (kind `profile`, `review` or `conversation`) | Static server rendering: a named form posted to the same page | F10, 2026-11-18, the user's choice instead of the prototype's dialog: one page for all three kinds, working without script; a failed send keeps the reason and text. A successful send redirects back with `reported=…` |
| Conversation list and chat thread `/chats`, `/chats/{id}` | Static pages holding one Interactive Server chat panel | Private, with live updates over SignalR. Built in F07 (2026-11-16): the static page checks sign-in and access and sets the title and status; only the panel inside is interactive, so a visitor or an outsider never opens a connection |
| New chat `/chats/new/{accountantId}` | Interactive Server; open to visitors | The one interactive page a visitor can open. It shows no private data. A visitor can type before signing in (R06); sending requires sign-in, and the draft survives sign-in in the tab's session storage. Built in F07 like the other chat pages: static, with the interactive panel |
| My accountants (the client's shortlist) and the accountant's client list `/workspace`, `/clients/{id}` | Interactive Server | Private to one user |
| Service request, checklist, progress and documents `/accountants/{id}/request-service`, `/work/{id}` | Interactive Server | State shared by the two participants. File bytes go through endpoints, not the page connection ([private service workspaces](#private-service-workspaces)) |
| Profile editor `/account/profile` | Static server rendering: a named form posted to the same page | Changed from Interactive Server on 2026-11-16 by the owner's choice (C04 for this page, [F05](../specs/profile-editing/spec.md)). A failed or invalid save shows the form again with everything typed. There's no connection to lose, and it works without script. Adding or removing an experience row redraws the form. A stale save is refused through the profile's version |
| Account `/account` | Static server rendering | Built 2026-11-16 (F02 BD10, prototype `meHTML`): menu links and the sign-out form only. Visitors see the visitor card, not a sign-in redirect. Since 2026-11-19 ([F06 PP10](../specs/profile-pictures/spec.md#pick-preview-save-2026-11-19-pp10)) a small script adds the picture preview on top: `wwwroot/TaxBuddy.Web.lib.module.js`, a Blazor JavaScript initializer that `blazor.web.js` loads on every page and runs again after a move between pages. It holds no state and calls no address; Save posts the page's own form. With script off, the page's link and forms do the same job. Since 2026-11-25 (F17) a client's details card; `?edit=details` adds the named form `client-details` posted to the same page ([client details](#client-details)) |
| Picture forms on Account: `POST /account/picture` (multipart) and `POST /account/picture/remove` | Static: plain forms posted to endpoints, which redirect back to `/account?picture=…#picture` | F06, 2026-11-16. The antiforgery token is checked after a 5 MB request limit is set, so an oversized upload is refused before it is read |
| Account deletion `/account/delete` | Static server rendering: step one is the page, step two adds `?step=confirm` and holds a named form posted to the same page | Built in F12 (2026-11-23) as a static form instead of the planned Interactive Server, as with the other account forms (C04): it works without script, and a failure shows the page again |

Apart from new chat, every interactive page requires sign-in, so only signed-in users hold a server connection (the "circuit"). Cost of this choice: each open interactive page holds one, and it drops when the server restarts. Drafts on interactive forms other than chat depend on open conflict [C04](README.md#history-and-unresolved-conflicts).

**Streaming and prerendered state** (written 2026-11-25 with the [loading standard](DESIGN.md#loading-and-busy-feedback); streaming decided the same day as [PRD D32](PRD.md#open-decisions)):
- **No page uses `[StreamRendering]`.** A streamed page sends its first part, status code included, before its data is read. The directory, profiles, the chat pages and the workspace pages could then no longer answer 404 or 503 ([navigation table](#navigation-and-screen-state)). And the streamed content is put in place by script, which [ADR 010](adr/010-single-project-per-page-render-modes.md) rules out for public pages (TP01). [F01 P1](../specs/browse-accountants/tasks.md#polish-follow-ups) reached the same view. A page that needs streaming later comes back as a new decision, recorded in the table above.
- **An interactive component that prerenders carries its first result to the circuit,** so its data is read once and the page isn't drawn twice: `ChatPanes` uses `[PersistentState]` (F07). A component that can't do that has prerendering turned off and shows a skeleton instead, and the table above names it.
- **Noted on 2026-11-25, not changed:** the table's rows for `/workspace`, `/clients/{id}`, `/accountants/{id}/request-service` and `/work/{id}` say Interactive Server. As built they are static pages with named forms ([F03 stage 1](#private-service-workspaces)), and `ChatPanes` is the only interactive component in the source. The loading standard treats them as static. The rows are left for the owner to correct.

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
| Sign-in | Google accounts through ASP.NET Core authentication; Microsoft built but archived on 2026-11-23 | Decided ([ADR 006](adr/006-web-sign-in.md), amended) |
| User store | ASP.NET Core Identity | Decided 2026-11-11 (ADR 006) |
| Styling | Tailwind CSS, with Tax Buddy's semantic colors as theme values. No component library | Stack decided 2026-11-11; current prototype direction applied under the delegated design brief ([DESIGN](DESIGN.md#visual-system)); application acceptance pending |
| Email | Brevo, called from a background service in the application | Decided 2026-11-11 (ADR 007) |
| Background work | ASP.NET Core's built-in hosted services | Decided 2026-11-11 |
| Tests | xUnit; `WebApplicationFactory` for integration tests; bUnit for components where useful | Decided 2026-11-11 |
| Hosting | Docker on an Oracle Cloud Infrastructure Compute VM in Singapore, with Caddy for HTTPS ([ADR 012](adr/012-oci-compute-docker-host.md)) | Decided 2026-11-24, replacing Render (ADR 007); the VM's shape is still to confirm |
| Editor | VS Code with C# Dev Kit | Owner asked to use it, 2026-11-11 ([SETUP](SETUP.md#reproducibility-record)) |
| CI | GitHub Actions, once the repository has a remote | Decided 2026-11-11 |

Not used: Supabase's sign-in, live-update and web API features; a JavaScript framework; a separate API project. Supabase Storage is used for profile pictures ([ADR 011](adr/011-profile-pictures-supabase-storage.md), 2026-11-16) and service documents (D28, 2026-11-23), each in its own private bucket reached from the server.

## Platform capability matrix

Exact SDK versions are in SETUP.

| Platform | Baseline | Needed capabilities | Unavailable behavior | Evidence |
|---|---|---|---|---|
| Desktop browsers: Chrome, Edge, Firefox, Brave, Safari | Current versions (PRD D17). Blazor's supported list names Chrome, Edge, Firefox and Safari ([REFERENCES](REFERENCES.md#toolchain-and-platforms)). Brave is built on the same engine as Chrome and is tested separately, with its tracking protection left on | HTTPS, cookies; JavaScript and WebSockets for interactive pages | Without JavaScript, the directory and profile pages still work and signed-in pages show a notice. Without WebSockets, behavior is to be verified at implementation | Pending ([STATUS](delivery/STATUS.md)) |
| Phone browsers: Chrome and Brave on Android, Safari on iOS | Same | Same, plus a layout that works from the smallest supported width (D15) | Same | Pending |
| Server host | ASP.NET Core on .NET 11, in a Docker container on an OCI Compute VM in Singapore, behind Caddy ([ADR 012](adr/012-oci-compute-docker-host.md); [Dockerfile](../Dockerfile), [deploy/oci](../deploy/oci/compose.yaml)) | WebSockets, persisted data-protection keys, outbound email, the database | — | Pending: the image is written, not built or deployed |
| Native Android, Windows, iOS, Mac apps | Not targeted ([ADR 005](adr/005-web-platform-blazor.md)) | — | — | Not applicable |

## Data and persistence contracts

The server database is the only store of record.

| Contract / requirement | Definition |
|---|---|
| Identity and mutation | Users, profiles and conversations get server-generated GUIDs. A message gets its GUID when the composer is shown, so a repeated submit stores one message (BR04). Profile updates carry a row version; a stale save gets a conflict state, not a silent overwrite |
| Dates, instants, money | Instants are stored as UTC and converted to Asia/Manila in one display helper (BR07). Experience months, if D04 adds them, are `YYYY-MM` values without a zone. No money fields in release 1 |
| Queries | The directory pages through a stable order with a unique ID tie-breaker (BR02): name A–Z, then ID, by default; "highest rated" orders by the computed average, review count, name, then ID, with unreviewed accountants last (D05, provisional). Messages are ordered by server receive time, then message ID |
| Storage and ownership | Supabase's managed PostgreSQL ([ADR 007](adr/007-supabase-brevo-container-host.md)), through EF Core with the Npgsql provider. Interactive pages take a new context from a factory for each operation, because a context is not thread safe and a circuit lives much longer than a request. Profile pictures (F06): a private Supabase Storage bucket `profile-pictures`, one object per upload (`users/{account}/{random}.jpg`), with the current object's name in `users.picture_object`. In the browser: the session cookie, the theme cookie `taxbuddy-theme` (`light` or `dark`, absent for System; one year, HttpOnly, SameSite=Lax; no personal data; F02 BD09, 2026-11-16), and unsent chat text in the tab's session storage (Proposed) |
| Schema evolution | EF Core migrations kept with the source. Production changes go through a migration bundle or a reviewed SQL script ([RELEASE](RELEASE.md#deployment)), not automatic migration at startup |
| Failure/downgrade | Each migration step is atomic where the provider allows. A rollback plan is recorded with each release that changes the schema |
| Ratings | One review per client and accountant, enforced by a unique constraint. The average and count are computed from the reviews, never stored by hand. The reviewer's short name is produced when the page is rendered |
| Reports and blocks | A report stores who reported, what was reported (kind and ID, plus the profile it belongs to), the reason, optional details and the time; one per reporter and item (unique index). Since F10 (2026-11-18) the owner reads the `reports` table in Supabase (D22, partly decided); the email to the owner follows with R08. A block is stored on the conversation as each side's block time (`client_blocked_at`, `accountant_blocked_at`) |
| Retention and backup | Account deletion removes the profile, external sign-in links, email preference and a client's details (BR06; F17). Since 2026-11-21 (D07, D14): chats stay with the other participant, shown as from "Deleted user"; a deleted client's reviews are removed; an account unused for 2 years is deleted after a warning email, which needs a cleanup job and R08 email. Since 2026-11-23 (F03): a deleted account's uploaded service documents are deleted with it, and the rest of the service work stays with the other participant; how long it is kept, and relationship termination, are still D14/D27. Backup policy belongs to D11 |

For SQL: parameterized queries only, constraints for the rules the database can enforce, and tests that run against the real provider rather than an in-memory substitute.

Supabase is used as a PostgreSQL database (Proposed in ADR 007). Since 2026-11-16 its Storage service also holds profile pictures, in a private bucket reached only from the server ([ADR 011](adr/011-profile-pictures-supabase-storage.md)); since 2026-11-23 (D28) a second private bucket holds service documents ([private service workspaces](#private-service-workspaces)). Sign-in stays in the application ([ADR 006](adr/006-web-sign-in.md)), and Supabase's own sign-in and live-update services are not used.
- **Connection:** the direct connection, or the session pooler on port 5432 where the host has no IPv6. The transaction pooler on port 6543 is meant for serverless functions and does not support prepared statements, so it is not used ([REFERENCES](REFERENCES.md#hosting-database-and-email-services)).
- **Supabase's web API:** Supabase can publish tables through its own web API. The application does not use it, so it is switched off or pointed at an empty schema, and row-level security is enabled on every table as a second barrier. Confirm both settings when the project is created.
- **Projects:** one Supabase project for development and tests with synthetic data, and a separate one for the test site. The Free plan allows two active projects, pauses a project after a week without activity, and holds 500 MB.

## Sign-in, sessions and authorization

| Decision | Selected approach |
|---|---|
| Sign-in method | Google account through ASP.NET Core external authentication ([ADR 006](adr/006-web-sign-in.md); Microsoft archived 2026-11-23). The browser is redirected to the provider and back to the site's callback address. No Tax Buddy passwords |
| User store | ASP.NET Core Identity with external sign-ins only (ADR 006). Built in F04, 2026-11-16: `TaxBuddyUser` (Guid key, display name, one role, agreement time and version) in `IdentityUserContext` tables `users`, `user_logins`, `user_claims`, `user_tokens`. No passwords and no Identity role tables |
| Session | A cookie that is HTTPS-only and unreadable by scripts (`SameAsRequest` only in Development, for the HTTP launch profile). Persistent for 14 days, renewed on use: Proposed in F04, 2026-11-16. Identity's external cookie holds a first sign-in for a few minutes until registration completes |
| Expired session | The next action sends the user to sign-in and then back to the same address. Unsent chat text is kept |
| Sign-out | Ends the session in that browser |
| Return address after sign-in | Local addresses only, so a crafted link can't send users to another site |
| Agreeing to the current terms (R16; [F11](../specs/legal-pages/spec.md), 2026-11-21) | Several pieces work together. **Recorded:** registration stores `agreed_terms_version` and `agreed_to_terms_at`. **Carried:** the session carries the version as the claim `taxbuddy:terms`, which Identity's security-stamp check rebuilds every minute (30 minutes until F12). **Checked:** middleware after authentication (`UseTermsAgreement`) redirects a signed-in person's GET or HEAD request for a component page to `/account/agree?returnUrl=…` when the claim isn't `LegalText.CurrentVersion`. A session from before F11 has no claim, so it counts as not current. Pages marked `[TermsNotRequired]` skip the check: Terms, Privacy, Agree, Sign in, Register, Not found, Error, Delete account (F12, so a person can leave without accepting) and the information pages (F13). So do posts, endpoints and Blazor's own addresses: it's a legal prompt, not access control. **Agreed:** the agree page stores the version and time, then renews the session with `RefreshSignInAsync` |
| Closing an account by hand (D22) | `users.lockout_end` in the future refuses the next sign-in (the callback shows the failure); a new `security_stamp` ends open sessions at the next check, within a minute (F12). Steps: [RELEASE](RELEASE.md#acting-on-a-report-d22) |

Roles: `client` or `accountant`, one per account, chosen at first sign-in (D03, decided 2026-11-16). The role is a column on the account and a claim in the session cookie.

Authorization rules, enforced in Application services on the server:
- Directory and profile reads are public, and return only listed profiles (BR01).
- A profile can be changed only by the accountant who owns it.
- A conversation and its messages are readable and writable only by its client and accountant (BR05).
- Only a client can start a conversation, and only with a listed accountant (BR03).
- A client can write a review only for an accountant who has sent a message in their conversation, once, and only that client can change or delete it (BR09).
- Only the reviewed accountant can reply to a review. Nobody can edit another person's review (BR11).
- Reporting and blocking need a signed-in user. Nobody reports their own profile or review, and only a participant reports a conversation (an outsider gets 404). One report per person per item, 10 a day.
- While either participant has blocked the other, neither can add messages to that conversation (R15). Only a participant sets or lifts their own block. A client whom the accountant has blocked cannot write or edit a review of them, but can delete their own (BR19).
- A client's details (F17) are edited only by that client and read only by an accountant on that client's page or a service page where they're the accountant ([client details](#client-details)).
- Saved-accountant lists belong to their authenticated client. A private client list is scoped to its accountant; saving alone never exposes a client's account in the confirmed save/request/accept model.
- Service tasks, updates and files inherit both participant IDs from their engagement. Every read/write/download/removal resolves the parent relationship and current actor, including asynchronous completion. Proposed editor/removal permissions follow the private-workspace table below; UI guards do not enforce security.

A record ID in an address or a form is never proof of access. Hiding a button is not authorization: the service behind it checks again.

**As built in F04 (2026-11-16):**
- `POST /account/external-signin` starts the provider sign-in. It needs an antiforgery token and a registered provider, and gives 400 otherwise.
- The provider returns to `/signin-google`, then to `GET /account/signin/callback`. A known login signs in; a first sign-in goes to `/register`.
- Cancel at the provider returns to the starting page (`OnAccessDenied`). Other provider failures go to `/signin?error=failed` and log only an error category (`OnRemoteFailure`).
- `POST /account/signout` ends the session and goes to `/`.
- Return addresses go through `LocalAddress` (local only), both when posted and before each redirect.
- Google is registered only when its client ID and secret are both configured. Otherwise the sign-in page says it isn't set up.
- **F15 (2026-11-23), archived the same day by the owner:** the code stays but is off. It registers only with `Authentication:Microsoft:Enabled` set to `true` (default off) as well as `Authentication:Microsoft:ClientId` and `ClientSecret`, so stored keys alone change nothing; switching it on also needs the legal text's Microsoft blocks and a new legal version. When on, it is registered through `AddMicrosoftAccount` (the Microsoft identity platform's `common` endpoint: personal, work and school accounts). The sign-in page shows "Continue with Microsoft" under Google, and the pages that name the providers say "Google", "Microsoft" or "Google or Microsoft" from what is registered. The same registration, session, cancel and failure handling apply; the login is stored with provider `Microsoft`. A person who signs in with both providers gets two accounts, because accounts are matched by provider and identifier, not by email.
- Pages use `AccountService`, never Identity's managers, so components don't depend on the data layer.
- Data-protection keys are kept in `DataProtection:KeysPath` when it is set: the container's `/keys` volume (2026-11-24, ADR 012). Locally they stay where ASP.NET Core puts them by default.

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

Real upload contract: authenticate/authorize before intake; antiforgery on mutating forms; enforce D26 byte/type/quantity limits on the server while streaming; treat client type/name as untrusted; keep incomplete/unvalidated/quarantined bytes unavailable. Metadata becomes Available only after the byte write and required validation/scanning complete. Coordinate partial-write cleanup and retries with operation IDs; a timeout does not prove failure. Downloads resolve current permission and Available state (as built, a row exists only once its bytes are stored, so there is no separate state column), return an attachment with an escaped/sanitized filename and safe response headers, and never reveal storage credentials or public object URLs. Removal rechecks uploader rights and retention policy, records the event, and coordinates byte/metadata cleanup. Revoked sessions/relationships cannot complete writes under an earlier actor's authorization.

Progress writes use optimistic concurrency; a stale version returns a conflict with the draft retained and current values for review. New shared updates appear on reload/reconnect; live cross-tab refresh can reuse the application's notifier when implemented. No service-progress emails are added to R08. Relationship deletion/blocking implications remain D24/D27; chat block semantics are not silently extended to revoke file access.

Proposed same-site routes: `/workspace` (role-aware private list), `/clients/{clientId}` (accountant-only grouped services), `/accountants/{id}/request-service`, `/work/{engagementId}` and application-owned file form/download endpoints beneath `/work/{engagementId}/files/{fileId}`. These are routes/endpoints of the one web app, not a new public API. URL IDs never grant access. Unauthorized/missing records show a generic unavailable state without other participants' details.

**As built, F03 stage 1 (2026-11-23):** [evidence](delivery/evidence/2026-11-23-f03-stage1.md).
- **Migration `AddClientServices`:** `saved_accountants` (client and profile, unique; deleted with the profile), `service_engagements` (client, accountant, service name, phase, version; one non-declined service per pair and name through a filtered unique index), `service_tasks` (title, status, position; deleted with the service) and `service_updates` (author, time, text, note or activity; deleted with the service). Links to `users` refuse deletion, because account rows are never deleted (F12). Deleting an account removes the client's saved list; shared service work stays with the other person, as chats do; the rest is still D27.
- **Code:** `WorkService` (Application/Work) checks the participant on every operation, and `WorkStore` (Data) runs each change in one context, with `Version` as the concurrency token. A stale checklist save returns Stale, and the page shows the posted choices with "Saved now: …".
- **Pages, all static with named forms** (C04 closed for service work): `/workspace` (My accountants or Clients), `/clients/{clientId}` (an accountant's services with one client, and adding one), `/accountants/{id}/request-service`, `/work/{id}` (request, checklist, notes, history). Saving posts to `POST /my-accountants` (an endpoint, so the static profile page needs no named form).
- **Unavailable:** an unknown or someone else's service or client answers 404, and the not-found page shows "This workspace is not available" for `/work/` and `/clients/`. Since 2026-11-24 the same goes for `/accountants/{id}/request-service` opened by an accountant or for an unknown or unlisted profile; before that it answered 200 with a page of its own. Visitors are asked to sign in, malformed addresses included.
- **Navigation:** "My accountants" or "Clients" in the site bar and the phone tabs, and since 2026-11-24 a row on Account ("My accountants and service progress" or "Clients and service work"), as in the prototype's `meHTML` and [DESIGN](DESIGN.md#service-workspace-presentation).
- **Not yet:** live updates between open tabs (a reload shows changes).

**As built, F03 stage 2, documents (2026-11-23):** R23 with D26/D28 as decided; [evidence](delivery/evidence/2026-11-23-f03-stage2-f15.md).
- **Migration `AddServiceDocuments`:** `service_files` (service, optional task, uploader, one-time form key, opaque object name, safe display name, content type, length, time). Deleted with the service; a task link is cleared if the task goes; the uploader link refuses deletion (account rows stay). Unique: object name, and uploader plus form key. A check keeps the length between 1 byte and 10 MB.
- **Storage:** `IDocumentStorage` (Application/Work) with `SupabaseDocumentStorage` (Data), the private bucket `Supabase:DocumentBucket` (default `service-documents`), the same server-only key as pictures, no cache, a 60-second timeout. Without Supabase settings, `NoDocumentStorage` switches uploads off and the page says so. Object names are `services/{service}/{file}`; the uploaded name is never part of one.
- **Upload, `POST /work/{id}/files`:** a plain multipart form from the service page, so it works without script. The request limit (10 MB plus 64 KB for the form) is set before the body is read, then the antiforgery token is checked by hand. `ServiceDocuments.AttachAsync` checks that the viewer is a participant of an active service, the form key is new, the file is 1 byte to 10 MB, its content is a PDF (`%PDF-` and an `%%EOF` marker near the end), a JPEG or a PNG (signature, and an image decoder reads its header), the task is the service's, and the service has fewer than 50 documents (a D26 working value). The browser's type and name are not trusted: the stored type is the one found, and the name is cut to its last part, cleaned of control, path and markup characters, kept to 120 characters and given the type's extension. The bytes are written first; the row and a history line ("Attached … to …") follow in one transaction that locks the service row and checks again: participant, account not deleted, active, task, room, key. If that refuses, the bytes are deleted; if the database fails, they are deleted only when the row surely wasn't written. The result goes back as `/work/{id}?file=…#documents`, with the task kept for a retry. No virus scanning (D28).
- **Download, `GET /work/{id}/files/{fileId}`:** signed-in participants of that service only; anyone else, a guessed ID or a file of another service gets 404. Always an attachment with the safe name, `X-Content-Type-Options: nosniff`, `Cache-Control: private, no-store` and a sandboxing content security policy. A storage failure returns to the page with a message.
- **Removal:** the uploader only, from `/work/{id}?remove={fileId}` (a confirmation step, then a named form). The row goes with a history line ("Removed … from this service."), then the bytes; a storage failure logs event 1602 and leaves an unlisted object for the operator.
- **Logs:** events 1601 (storage failed) and 1602 (left in storage) carry the error type only: no names, IDs or file details (BR18).
- **Lists:** each service card shows its number of documents, as in the prototype.

Prototype boundary: all service metadata lives in memory; drafts are separated by actor/service, survive simulated failures within the loaded page, and clear on reload. Upload uses generated sample selections, not a real file picker. Download generates synthetic PDF/CSV/TXT bytes locally. The production lifecycle, persistence and security contract above is not exercised by the HTML artifact.

## Client details

[PRD R25](PRD.md#requirements) owns the behavior and [F17](../specs/client-details/spec.md) the scenarios; built on 2026-11-25.

- **Data:** on the account row: `phone_number` (Identity's column, limited to 20 characters), `city`, `business_name`, `business_type` and `industry`, all nullable; the name is `display_name`. Migration `AddClientDetails`. A blank field is stored as null. The checks are in `Domain/ClientDetails.cs` (`ClientDetailsRules`).
- **Page:** a client's Account (static) shows the "Your details" card. With `?edit=details` the card holds the named form `client-details`, posted to the same page.
  - A valid save updates the account through `ClientDetailsService` and renews the session (`RefreshSignInAsync`), so a new name is in the header claim. It then redirects to `/account?details=saved#details`.
  - A refused or failed save shows the form again with everything typed (BR18).
- **Who sees them:**
  - only the client edits them;
  - an accountant reads them only on pages they can already open for that client: their page for the client (`/clients/{id}`, which needs a service) and a service page where they're the accountant (`WorkStore` gives them to the accountant only);
  - nothing is added to public pages, chat or any new address;
  - the form names the accountants who can see them: those with a service.
- **Deletion:** account deletion clears all five fields with the name and email (BR06).
- **Diagnostics:** events 1701 (saved) and 1702 (save failed), with the error type or Identity's codes only, never the details.

## Live chat and email notifications

| Contract | Definition |
|---|---|
| Live updates | When a message is saved, an in-process notifier tells the open chat pages of that conversation's two participants, which then re-read from the database. This assumes one server instance; more than one needs a shared notifier and session affinity (decide before scaling out) |
| Missed updates | A page always loads from the database when it renders or reconnects. It never trusts a live event alone |
| Email (R08) | Saving a message also writes an email job for the other participant in the same database transaction. A background worker sends it through the email sender interface, which calls Brevo ([ADR 007](adr/007-supabase-brevo-container-host.md)), and retries on failure. A failed email never fails or loses the message |
| Email content (BR08) | No message text. A link to the conversation, the site name, and a link to turn emails off |
| Email frequency | At most one email per conversation until the recipient opens it (D18, decided 2026-11-23). An email waits 2 minutes and is skipped if the recipient has read the conversation by then, the site's stand-in for "not on the site" |
| Email address | The address from the account's Google sign-in at registration |
| Turning emails off | `users.email_notifications` (on by default). Account has a switch; each email has a signed link to `/email/off` that turns emails off without signing in (the account ID protected by the site's data-protection keys), and the page can turn them back on. The form posts to `POST /account/email` with antiforgery |
| As built (F14, 2026-11-23) | Migration `AddEmailNotifications`: `users.email_notifications` (existing rows on) and `email_jobs` (recipient, conversation, created and due times, status, attempts, finished time; deleted with the conversation). The job is written in `ChatStore.AddMessageAsync`'s transaction. `EmailOutbox` (Services/Email) runs every 15 seconds in `EmailOutboxWorker`, 20 jobs at a time. It retries after 1, 5 and 30 minutes, then every 2 hours, and gives up after 6 tries; finished jobs are removed after 30 days. Senders: `BrevoEmailSender` when `Email:Brevo:ApiKey` and `Email:SenderAddress` are set; `DevelopmentEmailSender` in development; otherwise `NotConfiguredEmailSender`, which skips jobs. Logs: events 1501–1505, no addresses or names |
| Development | No real emails. `DevelopmentEmailSender` writes each one as a `.txt` and an `.html` file in `artifacts/emails` (ignored by git), or `Email:DevelopmentFolder` |

## Navigation and screen state

Addresses are proposed; the first feature spec refines the directory ones.

| Address | Input contract | Retained state | Back, reload, shared-link behavior |
|---|---|---|---|
| `/` | Query: `q` (search text), `service`, `sort` (`rating`; absent or any other value means name A–Z; built in F09), `page`. The search form carries `sort` in a hidden field, so a new search keeps the order. Search and service are submitted with a plain GET form; on wide screens each service button submits it. Sort and page are links that carry the current search and filter. No live filtering in the page ([C03](README.md#history-and-unresolved-conflicts), resolved) | All in the address, so Back, reload and shared links show the same list | Unknown or malformed query values are ignored |
| `/accountants/{id}` | GUID. Query, F09: `reply` (a review ID; opens the owner's reply box) and `saved` (`review-posted`, `review-updated`, `review-deleted` or `reply`, the status after a redirect) and, F10, `reported` (`profile` or `review`, the thanks line) | None | Unknown, deleted or unlisted ID shows "no longer available" with HTTP 404 (R03). Unknown query values are ignored |
| `/report/{kind}/{id}` | `profile`, `review` or `conversation`, and a GUID; a signed-in user (F10) | Reason and details kept through a failed send | A visitor is asked to sign in. Unknown items, unlisted profiles and other people's conversations: "This isn't available" with HTTP 404. Own content: 403. Already reported: a message instead of the form |
| `/accountants/{id}/review` | GUID; the user must be an eligible client | Stars and text kept through a failed save | An ineligible user sees why, and no form (BR09). An unlisted or unknown accountant: "This accountant is no longer available" with HTTP 404 |
| `/privacy`, `/terms`, `/about`, `/help`, `/for-accountants` | None | None | Static pages, public (R16, R17) |
| `/email/off` | The signed-in user, or a signed link from an email | None | Turns notification emails off and offers to turn them back on (R08) |
| Any other address | — | — | "Page not found" with HTTP 404; failures show an error page with a support code (R18) |
| `POST /preferences/theme` | Form: `theme` (`light`, `dark` or `system`), `returnUrl`, antiforgery token | A cookie for light or dark; System deletes it | Redirects to `returnUrl` when it is an address on this site, otherwise to `/`. Missing token or unknown theme: 400, nothing stored (F02 BD09) |
| `/signin` | Local return address; `error=failed` shows the failure alert | None | Cancel at the provider returns to the previous page; success continues to the return address. Built in F04 |
| `/register` | Local return address; a first sign-in in progress (Identity's external cookie), otherwise redirect to `/signin` | The role chosen and the agreement box are kept through a failed submit | Named form posted to the same page. Built in F04 |
| `/account/agree` | `returnUrl` (local; the agree page itself becomes `/`); a signed-in person, otherwise redirect to `/signin` and back | The tick is kept through a failed submit | Named form posted to the same page, plus the sign-out form. An account already at the current version (agreed in another browser) has its session renewed and goes straight back. Built in F11 (2026-11-21) |
| `/chats` | Signed-in user; a visitor sees "Sign in to see your chats" | Conversation list | Email links fall back here when the conversation can't be opened |
| `/chats/{id}` | GUID; the user must be a participant | Unsent text kept across a lost connection and a reload of the tab | Not a participant, or unknown ID: "This conversation isn't available" with HTTP 404, never another user's content (F07) |
| `/chats/new/{accountantId}` | GUID of a listed accountant | Unsent text kept through sign-in | Redirects to the existing conversation if the pair has one (D06). An accountant gets "Only clients can start chats"; a listed profile with no account, "This accountant can't receive messages yet"; an unknown or unlisted one, 404 "This accountant is no longer available" (F07) |
| `/account/profile` | Signed-in accountant, owner from the session only; `saved=1` after a save | Unsaved edits are kept through a failed, invalid or stale save. Deviation (F05): no confirmation when leaving with unsaved changes, and Save is not disabled while it runs, since both need script. A second submit is refused by the version check. Services are rows with a dropdown, as in the prototype (BD10) | A successful save redirects to `?saved=1`, so a reload doesn't save again. A visitor goes to sign-in and back; a client gets 403 "Only accountant accounts have a profile" |
| `/account` | Anyone; content depends on who is signed in. F17 (2026-11-25): `edit=details` opens a client's details form; `details=saved` after a save | A client's typed details are kept through a refused or failed save | Visitor card, or the account menu with sign-out (BD10); for a client, the "Your details" card ([client details](#client-details)) |
| `/pictures/{accountId}` | GUID; `v` = the picture version | None | An accountant's picture for anyone (`public, immutable` when `v` matches); a client's picture for that client and the accountants they have a conversation with (`private`, F07); otherwise 404. JPEG with `nosniff` (F06) |
| `/account/delete` | Signed-in user; `step=confirm` for the second step; `done=1` for the result after sign-out | None | Two steps; on success the user is signed out and sees "Your account was deleted" (`?done=1`); a failure keeps them signed in with the page and a banner. Built in F12; service files will follow D27 |
| `/workspace`, `/clients/{id}`, `/accountants/{id}/request-service`, `/work/{id}` | Authenticated participant and valid parent IDs; [private-workspace contract](#private-service-workspaces) | Draft request/task/note state retained on failed write. Documents (2026-11-23): after a failed upload the task choice is kept (`?task=`) and the file is chosen again, as browsers don't keep a file choice across pages; resending the same form stores one document | Missing/unauthorized is generic unavailable; built 2026-11-23, with `/work/{id}/files` (upload) and `/work/{id}/files/{fileId}` (download) |

On interactive pages a submit button is disabled while its operation runs, so a double click can't run it twice. On static forms a double click sends the form twice, and the server makes the repeat change nothing ([PRD D31](PRD.md#open-decisions), decided 2026-11-25; checks in [DEVELOPMENT](DEVELOPMENT.md#loading-and-busy-feedback)). Until then this sentence named every submit button.

### As built in F01 (2026-11-13)

[Evidence](delivery/evidence/2026-11-13-f01-browse-accountants.md).

**Directory `/`:**
- The query is normalized: search text is trimmed, its spaces collapsed and capped at 100 characters, and unknown values are ignored.
- When the address holds two `service` values, the later one wins and the page redirects to the one-value address. This happens when a service button is pressed on a wide screen, because the form also sends its hidden service menu.
- Page size is 6, a working value (D05).
- A database failure shows "We couldn't load the accountants" with "Try again" and HTTP 503.

**Profile `/accountants/{id}`:**
- An unknown, unlisted or malformed ID calls `NavigationManager.NotFound()`. This Blazor version sends no body with a page-set 404, so the not-found page renders "This profile is no longer available" for `/accountants/…` addresses and "Page not found" for everything else, both with HTTP 404.
- The not-found page has one variant per kind of address (updated 2026-11-24): profile, review, request a service, new chat, conversation, report, workspace (`/work/`, `/clients/`) and "Page not found". The workspace and request variants are drawn in the workspace frame. The chat variants are drawn statically in the chat panes, beside the viewer's conversation list, with no live connection. The review variant is drawn in the "Write a review" panel. A visitor at a workspace, request or conversation address is asked to sign in instead.
- A database failure shows "We couldn't load this profile" with HTTP 503. Status-code pages are switched off for that response so they don't replace the page.

**`/chats/new/{accountantId}`:** a static "coming soon" page until chat was built; replaced in F07 (below).

**Tables:** `accountant_profiles`, `accountant_services` and `accountant_experience`, with snake_case names (and `accountant_highlights` since F08, 2026-11-17: title, type of client, what was done, result and position; at most 6 per profile, deleted with it). Services are unique per profile, and both child tables are deleted with their profile. Experience months are stored as the first day of the month in `date` columns.

**Development command:** `dotnet run --project src/TaxBuddy.Web -- seed-development` resets the profiles to the synthetic seed. It runs only in the Development environment.

**Theme (F02 BD09, 2026-11-16):** `App.razor` writes `data-theme` on `<html>` from the theme cookie, so the first response already has the chosen colours. `Program.cs` calls `UseAntiforgery()`; the theme endpoint is the first form that needs it. The switch's return address is the current path and query, or on a not-found or error page the original address.

**Public pages:** every response carries `X-Robots-Tag: noindex, nofollow` and a matching meta tag, unless `Seo:AllowIndexing` is true (D19, decided 2026-11-21: true on the public site from launch, off on the test site).

### As built in F07 (2026-11-16)

[Evidence](delivery/evidence/2026-11-16-f07-chat.md); scenarios in [F07](../specs/chat/spec.md).

**Tables (migration `AddChat`):**
- `conversations`: client and accountant account, created and last-message times, and each side's read time. The pair is unique (D06).
- `messages`: an ID made by the composer, the conversation, the sender, the text (2,000 characters at most) and the time sent. Messages are deleted with their conversation.
- Foreign keys from both tables to `users` refuse deletion. Since F12 the account row is never deleted, only made nameless ("As built in F12" below), so the conversation stays with the other participant as D07 decided.

**Rules:**
- A message is unread for you when the other side sent it after your read time. Opening a thread moves your read time to its latest message from the other side.
- Sending is idempotent through the message ID. A save with an unknown outcome is followed by a check ("Checking whether it was sent…").
- An account may send 20 messages a minute across all its conversations.

**Pages and the live panel:**
- The three chat pages are static. They hold one `ChatPanes` component in Interactive Server mode, which receives only IDs and takes the viewer from the sign-in.
- The panel is prerendered, and its data is carried to the live connection with `[PersistentState]`.
- `ChatNotifier` (a singleton) tells the open panels of both participants to reload from the database. It assumes one server.

**Drafts:**
- A small script owns the composer: its text and the message ID are kept in the tab's session storage.
- A visitor's draft moves into the conversation once it exists.
- A send gets 20 seconds to answer. After that the text stays, with "You're offline…".
- Nothing is sent without a press of Send or Enter.

**Connection:**
- The reconnect notice is a non-modal banner.
- When the server no longer knows the page, it reloads, and the draft comes back from the tab.

**Navigation:** "Chats" with the unread count, read once per request (`UnreadChats`); the chat list itself updates live.

**Known limits:**
- An open chat page keeps the sign-in it started with until it reloads.
- The navigation count changes on the next page load.
### As built in F09 (2026-11-17)

[Evidence](delivery/evidence/2026-11-17-f09-reviews.md); scenarios in [F09](../specs/reviews/spec.md).

**Table (migration `AddReviews`):** `reviews` holds:
- the profile, the client account, and the stars (checked 1–5);
- the text (1,000 characters at most), and the created and updated times;
- the reply text (600 at most) and its time.

The profile and client pair is unique (BR09). Reviews are deleted with the profile. The foreign key to `users` refuses deletion; F12 removes a deleting client's reviews explicitly (D07).

**Rules:**
- **Who may review:** `ReviewService` checks it on every request: a signed-in client whose conversation with this accountant holds a message from the accountant.
- **One review per client:** posting again updates the client's one review. Two inserts at once are settled by the unique index.
- **Replies:** only the profile's owner posts or edits the reply. A reply can't be deleted, only edited, as in the prototype.
- **Names:** reviewers are shown as "First L." (D21), from `users.display_name` at render time.

**Pages:**
- The review page and the owner's reply form are static named forms.
- A post for a form the page didn't draw is refused by Blazor with 400 before the page runs, and the service would refuse it too.
- After a save, the page redirects to the profile with `saved=…`.
- "Highest rated" is ordered in SQL: accountants with reviews first, then the average and count descending, then name and ID.

### As built in F10 (2026-11-18)

[Evidence](delivery/evidence/2026-11-18-f10-report-block.md); scenarios in [F10](../specs/report-block/spec.md).

**Tables (migration `AddReportsAndBlocks`):**
- `reports` holds:
  - the reporter;
  - the kind, stored as text: `Profile`, `Review` or `Conversation`;
  - the item ID and the profile it belongs to;
  - the reason, stored as text: one of the prototype's five;
  - the details (600 characters at most) and the time.

  It is unique per reporter, kind and item, with no foreign key to the item. Its foreign key to `users` refuses deletion. Since F12 a deleted person's reports stay, linked to the nameless account row, and are kept as the Privacy Policy says.
- `conversations` gains `client_blocked_at` and `accountant_blocked_at`.

**Rules:**
- **The report service:** checks the reporter on every request. It logs only "Report received" with the kind (event 1301), never names, IDs, reasons or text.
- **Blocking:** happens in the chat panel through `ChatService.SetBlockAsync`, which tells both panels through the notifier.
- **Row lock:** storing a message and storing a block both lock the conversation row (`SELECT … FOR UPDATE`), so no message is stored after a block. A send refused that way returns `Blocked`, and the draft stays in the tab.
- **Reviews:** `ReviewService` returns `Blocked` for a client whom the accountant has blocked (BR19).

**Pages:**
- The report page is static.
- **Profile page:** "Report this profile" for everyone except the owner (a visitor signs in on the report page), and "Report this review" for signed-in users other than its author.
- **Chat panel:** shows Report and Block above the messages, and asks before blocking in a sheet that Escape closes. It replaces the composer with the blocked notes, and re-attaches the composer script after Unblock.

### As built in F12 (2026-11-23)

[Evidence](delivery/evidence/2026-11-23-f12-f13.md); scenarios in [F12](../specs/account-deletion/spec.md).

**Migration `AddAccountDeletion`:** a nullable `users.deleted_at`.

**One transaction** (`AccountService.DeleteAccountAsync`):
- the accountant profile is deleted, and its services, experience, highlights and reviews cascade;
- the client's own reviews are deleted;
- each conversation gets this side's block time, so sends are refused both ways. A conversation whose other person was already deleted is deleted, with its messages;
- sign-in links, claims and tokens are deleted;
- the user row is made nameless: the display name becomes "Deleted user"; email, normalized email and picture become null; `deleted_at` is set; lockout is set to the maximum; and the security and concurrency stamps change.

**After the commit:**
- open chat panels are told the conversations changed, so they show "This client/accountant deleted their account…";
- the picture object is deleted from storage. A failure logs event 1403 without IDs, and the object is then removed by hand;
- since F03 stage 2, the service documents the account uploaded are deleted from storage (their rows and a history line per service went in the transaction). A failure logs event 1404 with a count only, and the objects are removed by hand ([RELEASE](RELEASE.md));
- the person is signed out, and their other sessions end at the next security check.

**Sessions:** `SecurityStampValidatorOptions.ValidationInterval` is 1 minute (`AccountSetup.SecurityCheckInterval`).

**Chat:** `ThreadView.PeerDeleted` (from `users.deleted_at`) shows the deleted note instead of the composer, and hides Block.

**Logs:** event 1401 "Account deleted", 1402 deletion failed (error type only) and 1403 picture left in storage. None of them records IDs, names or emails.

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

- **Diagnostics:** `System.Net.Http.HttpClient` logs at Warning, so picture storage addresses (with account IDs) are not logged (F06). Event IDs, error category, duration and correlation ID only. Message text, email addresses, contact details and tokens are never logged. No telemetry service in release 1.
- **Transport and forms:** HTTPS everywhere, with HSTS in production. Antiforgery protection on every form. All input is validated on the server.
- **Abuse limits:** rate limits on sign-in, message sending and directory search, counted per server instance. Message length and profile field lengths are capped so a connection can't make the server hold unbounded data.
- **Keys and secrets:** data-protection keys are persisted outside the app's files, so sessions survive restarts and redeployments. As built (2026-11-24): a Docker volume on the VM, unencrypted at rest on the private VM. No secrets in the repository or in `appsettings*.json`; the deployed settings live in `deploy/oci/.env.web`, which git ignores.
- **Behind the proxy:** the container sets `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, so the app trusts Caddy's `X-Forwarded-Proto` and `X-Forwarded-For` and builds `https` callback addresses. The app's port isn't published, so only Caddy can send those headers.
- **Health:** `GET /health` answers `{"status":"ok","version":"<git commit>"}` without touching the database (`Site:Version`, set when the image is built).
- **Search engines:** public pages are built to be indexed. Each one gives its full content in the first response, with its own title, description and canonical address, and a sitemap can list the listed profiles. Whether indexing is switched on is PRD [D19](PRD.md#open-decisions). Until it is decided, and always on the invite-only test site (D20), public pages ask not to be indexed.

## Traceability and decisions

| Requirement / feature scenario | Technical component | Verification boundary |
|---|---|---|
| R01–R03 / [browse-accountants](../specs/browse-accountants/spec.md) FT01–FT07 | Directory use cases and queries; directory and profile pages | Tests, Web.Tests; TP01, TP05, TP06 |
| R04 | Account endpoints, user store, role choice | Web.Tests; TP10; real providers in TP07 |
| R05 | Profile use cases and editor page | Tests, Web.Tests; TP02 |
| R06, R07, R10 | Conversation use cases, chat pages, chat notifier | Tests, Web.Tests; TP02, TP03, TP08, TP09 |
| R08 | Email jobs, background worker, email sender | Tests, Web.Tests; TP13 |
| R09 | Account deletion use case and page (built in [F12](../specs/account-deletion/spec.md)) | Web.Tests (`AccountDeletionTests`); TP14 |
| R11 | Authorization in Application services; directory projection | Tests, Web.Tests; TP10 |
| R12 | Portfolio highlights in the profile use cases and editor page (F08) | Tests, Web.Tests; TP25 |
| R13, R14 | Review use cases, rating queries, review and profile pages | Tests, Web.Tests; TP16 |
| R15 | Report and block use cases (F10); report email with R08 | Tests, Web.Tests; TP17, TP27 |
| R16–R18 | Static pages, sign-in agreement, not-found and error handling | Web.Tests; TP18 |
| R19–R23 / F03 CS01–CS10 | Private relationship/engagement/progress/document use cases, EF metadata and private byte-store boundary | TP19–TP22 application checks; CS prototype checks recorded separately |

Significant rationale: [ADR index](adr/README.md). Conflicts: C04 open; C03 resolved 2026-11-12 ([docs index](README.md#history-and-unresolved-conflicts)). Remaining technical risks, owner lumabanls:
- Restarts and deployments drop interactive connections (TP15 checks what users see).
- Email delivery depends on Brevo and on setting up a sender domain (D18).
- Supabase's Free plan pauses a project after a week without activity, which takes the test site down until it is resumed, so it needs a paid plan before testers rely on the site. (Render's sleeping free service no longer applies: the host is an OCI VM, ADR 012.)

Verified state belongs in [STATUS](delivery/STATUS.md).
