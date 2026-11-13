# Delivery status

**Authority:** current implementation, verification, and acceptance state. Behavior belongs in [PRD](../PRD.md); procedures in [TEST-PLAN](TEST-PLAN.md).

Last update: 2026-11-13. Repository HEAD: `5ceee374da2f2622930fa4a1e5b69601852d22f3` on `main` (merge of pull request #1 from `develop`). It holds the F02/F03 work and the 2026-11-12 rename to Tax Buddy, palette options and prototype review record. Uncommitted on top of it, all to be committed by the user after F01:
- the 2026-11-12 C03 resolution and provisional D04/D05 decisions;
- the 2026-11-13 architecture update ([ADR 010](../adr/010-single-project-per-page-render-modes.md));
- the Supabase setup records;
- the F01 application source and tests.

Earlier evidence retains its own exact source identity and the names of its day. Current focus: F01 acceptance (owner walkthrough and browser matrix), then the next feature. Release readiness: **Not assessed**.

## Feature state

Application source exists since 2026-11-13 (F01). The other rows are at their starting state.

| Requirement / feature | Implementation | Automated verification | Build | Browser verification | Acceptance |
|---|---|---|---|---|---|
| R01–R03 / [F01 browse-accountants](../../specs/browse-accountants/spec.md) | Implemented 2026-11-13 (uncommitted) | Pass: 62 tests, 0 skipped, including data tests on PostgreSQL ([evidence](evidence/2026-11-13-f01-browse-accountants.md)) | Pass: Release, 0 warnings | Partial: Edge 157 emulated at 320–1440 px, light and dark, 0 layout problems. Chrome, Firefox, Brave, Safari, real phones, screen reader and zoom not run | Pending |
| R04 sign-in and registration | Not started (no spec) | Not run | Not run | Not run | Pending |
| R05 accountant profile editing | Not started (no spec) | Not run | Not run | Not run | Pending |
| R06, R07 chat | Not started (no spec) | Not run | Not run | Not run | Pending |
| R08 email notifications | Not started (no spec) | Not run | Not run | Not run | Pending |
| R09 account deletion | Not started (no spec) | Not run | Not run | Not run | Pending |
| R10, R11 connection loss and privacy | Not started; covered by each feature | Not run | Not run | Not run | Pending |
| R12 portfolio highlights | Not started (no spec) | Not run | Not run | Not run | Pending |
| R13, R14 ratings, reviews and replies | Not started (no spec) | Not run | Not run | Not run | Pending |
| R15 report and block | Not started (no spec) | Not run | Not run | Not run | Pending |
| R16–R18 legal, information and error pages | Not started (no spec) | Not run | Not run | Not run | Pending |
| R19–R23 / [F03 client services](../../specs/client-services/spec.md) | Application not started; documentation/prototype task separate below | Application tests not run | Not run | Application not run | Pending; policies D24–D28 open |

States:
- Implementation: not started, in progress, or implemented.
- Verification: not run, pass, fail, blocked, or not applicable with a reason.
- Acceptance: pending, accepted, or rejected, with decision evidence.

Use separate browser rows when outcomes differ. A blocked check requires a concrete reason and next action.

### Prototype work, separate from application implementation

| Work | Implementation | Verification | Acceptance |
|---|---|---|---|
| [C03 directory contract](../README.md#history-and-unresolved-conflicts) | Applied locally 2026-11-12: the prototype directory is a submitted form with sort and page links, all in the address, and no per-user card tag | 186 Edge checks passed, including 9 new directory checks; syntax and documentation checks pass; desktop, page-2 and 320 px screenshots agent-reviewed ([evidence](evidence/2026-11-12-c03-directory.md)) | Resolved by the user's instruction; owner walkthrough pending (G9) |
| [F02 rename to Tax Buddy (BD07)](../../specs/brand-design-refresh/spec.md#rename-to-tax-buddy-2026-11-12) | Applied locally 2026-11-12: display name, redrawn wordmark, renamed stylesheet/logo files, look id, example domain, planned project names, current documentation | 142 Edge checks passed on the renamed sources, including 34 contrast pairs; syntax, SVG and documentation checks pass; header screenshots and logo render agent-reviewed ([evidence](evidence/2026-11-12-tax-buddy-rename.md)) | Name set by the user's instruction; name/trademark/domain availability unchecked (D01); owner walkthrough and release acceptance pending |
| [F02 palette options (BD08)](../../specs/brand-design-refresh/spec.md#palette-options-2026-11-12) | Applied locally 2026-11-12: four colour proposals selectable in the prototype beside the current palette; colours only | 178 Edge checks passed, including 36 new palette checks and 17 contrast pairs per option and theme; syntax and documentation checks pass; directory screenshots agent-reviewed ([evidence](evidence/2026-11-12-palette-options.md)) | 2026-11-13: the user chose Paper & Forest; the four proposals stay in the prototype as history ([DESIGN](../DESIGN.md#visual-system)) |
| [F02 brand/design refresh, as Kuwenta](../../specs/brand-design-refresh/spec.md) | Applied locally: name, original SVG logos/favicon, light/dark direction, directory/profile/shell refinements, current documentation | Historical observation for the fingerprinted F02 artifact: 78 Edge prototype checks, 34 contrast pairs, syntax/SVG/docs checks passed; screenshots agent-reviewed ([evidence](evidence/2026-11-11-kuwenta-refresh.md)). Not automatically transferred to F03 | User delegated the design choice; owner walkthrough and product/release acceptance pending. No application behavior is established |
| [F03 client services/progress/documents](../../specs/client-services/spec.md) | Docs and local prototype updated: private lists, request/acceptance, per-client services, checklist/progress/history and generated sample file lifecycle | 142 Edge checks passed, including rerun of prior flows and 34 contrast pairs; syntax, documentation, identifier/table and source-hash checks pass; responsive screenshots agent-reviewed ([evidence](evidence/2026-11-11-client-services.md)) | Owner confirmed private-save/request/accept model; remaining D24–D28 policies and overall prototype/release acceptance pending. Real uploads/storage and server behavior unimplemented |

## Evidence register

| Evidence record | Date | Source / artifact | Environment | Attribution | Result / limits |
|---|---|---|---|---|---|
| [Documentation setup checks](evidence/2026-11-10-documentation-setup.md) | 2026-11-10 | `b9b50f3` plus uncommitted documentation changes | Windows 11 Pro 10.0.26300; uv-managed CPython 3.14.4 | Agent-observed | Documentation checks passed for the MAUI plan of that day. Host inventory recorded. Nothing about app behavior is established |
| [Web platform change checks](evidence/2026-11-11-web-platform-change.md) | 2026-11-11 | `b9b50f3` plus uncommitted documentation and prototype changes | Same PC; headless Microsoft Edge for the prototype | Agent-observed | Documentation checks pass after the platform change; prototype walk-through passes. No application exists, so nothing about app behavior is established |
| [Kuwenta brand and prototype checks](evidence/2026-11-11-kuwenta-refresh.md) | 2026-11-11 | `668a9a9` plus F02 changes; source SHA-256 values retained with report | Windows 10.0.26300 x64, Node 24.16.0, headless Edge 157.0.4341.42 | Agent-observed | 78 prototype checks, 34 semantic contrast pairs, syntax/SVG/docs checks pass. Screen readers, other browsers and application acceptance remain pending |
| [Client services prototype checks](evidence/2026-11-11-client-services.md) | 2026-11-11 | Preserved F02 working tree plus F03; exact HTML/CSS/module/checker/asset hashes in report | Windows 10.0.26300 x64, Node 24.16.0, Edge 157.0.4341.42 | Agent-observed | 142 checks and static checks pass; screenshots reviewed; generated fixtures only. TP19–TP22 production boundaries untested |
| [Tax Buddy rename checks](evidence/2026-11-12-tax-buddy-rename.md) | 2026-11-12 | `1fccef3` plus the uncommitted rename; source SHA-256 values retained with report | Windows 10.0.26300 x64, Node 22.12.0, headless Edge 157.0.4341.46 | Agent-observed | 142 prototype checks, 34 contrast pairs, syntax/SVG/docs checks pass on the renamed sources. Name availability, screen readers, other browsers and application acceptance remain pending |
| [Palette options prototype checks](evidence/2026-11-12-palette-options.md) | 2026-11-12 | `1fccef3` plus the uncommitted rename and palette options; source SHA-256 values retained with report | Windows 10.0.26300 x64, Node 22.12.0, headless Edge 157.0.4341.46 | Agent-observed | 178 prototype checks pass; each of four options passes 17 contrast pairs in light and dark. No palette chosen; other pages not reviewed per option |
| [Prototype scope and stack-fit review](evidence/2026-11-12-prototype-scope-review.md) | 2026-11-12 | `1fccef3` plus the uncommitted rename and palette options; reviewed source SHA-256 values in the record | Not applicable: read-through of documents and prototype source, nothing executed | Agent-observed | Two open conflicts recorded (C03 directory interaction, C04 drafts on connection loss) and a list of costly or prototype-only items. Recommendations only; no decision, scope change or browser check. Some stack statements are from the agent's knowledge and unverified |
| [F01 browse accountants](evidence/2026-11-13-f01-browse-accountants.md) | 2026-11-13 | `5ceee37` plus uncommitted F01 source; 62 files with combined SHA-256 `eae0d6ae…` | WSL2 Ubuntu 24.04.1, .NET SDK 11.0.100; Supabase development project; headless Edge 157 on Windows | Agent-observed | Release build with 0 warnings; 62 tests pass with 0 skipped; 32 emulated browser views with 0 layout problems after one fix. Two defects found by the tests were fixed. Browser matrix, screen reader and acceptance pending |
| [Supabase development database connection](evidence/2026-11-13-supabase-dev-connection.md) | 2026-11-13 | No application source; a throwaway check program (Npgsql 10.0.3, .NET SDK 11.0.100) | WSL2 Ubuntu 24.04.1, no IPv6; Supabase free project in `ap-southeast-1` | User created the project and stored the secret; agent-observed check and host correction | Connects through the session pooler with encryption required; PostgreSQL 17.11; empty database. Data API setting and EF Core behaviour not checked |
| [C03 directory and provisional D04/D05](evidence/2026-11-12-c03-directory.md) | 2026-11-12 | `5ceee37` plus this uncommitted change; source SHA-256 values retained with report (WSL checkout, LF line endings) | WSL2 Ubuntu 24.04.1 on Windows 10.0.26300; Windows Node 22.12.0 through WSL interop; headless Edge 157.0.4341.46 | Agent-observed | 186 prototype checks pass after one failing run was fixed (320 px phone overflow). Decisions are the user's; no acceptance. Behaviour without JavaScript is an application check |

Use the [evidence template](evidence/TEMPLATE.md). A template/procedure is not a completed execution record. A previous pass does not automatically apply to changed source or another platform.

## Remaining gates and decisions

| Gate / decision | Owner | Missing evidence or dependency | Next action |
|---|---|---|---|
| G1 .NET 11 SDK on the development PC | lumabanls | Not installed on Windows (checked 2026-11-11). Installed 2026-11-12 in WSL2 Ubuntu 24.04.1 on the same PC: SDK 11.0.100 with runtimes 11.0.0, user-level in `~/.dotnet` beside SDK 10.0.404, through Microsoft's `dotnet-install.sh`. A throwaway Blazor Web App targeting `net11.0` built with 0 warnings and 0 errors. The development certificate in WSL is not trusted yet. Closed for WSL 2026-11-13: [SETUP](../SETUP.md#reproducibility-record) records the WSL toolchain, `global.json` pins 11.0.100, and F01 builds and tests with it | Optional: install on Windows too. Trust the development certificate before using the HTTPS profile |
| G2 Android build host | — | Closed 2026-11-11: not needed for a web application | — |
| G3 Android device for native checks | — | Closed 2026-11-11: replaced by the browser matrix in TEST-PLAN | — |
| G4 Windows sign-in (C01, PRD D02) | — | Closed 2026-11-11 by [ADR 006](../adr/006-web-sign-in.md) | — |
| G5 F01 blocking decisions (PRD D04, D05) | lumabanls | Adopted provisionally 2026-11-12 by the user's instruction: the prototype's proposals ([PRD](../PRD.md#open-decisions)). Page size and field length limits remain open; F01 uses working values (page size 6, limits in `ProfileLimits`) | Decide page size and field limits before R05 profile editing, and expect a migration if fields change |
| G6 Database and hosting (PRD D11) | lumabanls | Development database ready 2026-11-13: the user created a Supabase project in Singapore (`ap-southeast-1`, PostgreSQL 17.11), and the agent connected to it from WSL through the session pooler ([evidence](evidence/2026-11-13-supabase-dev-connection.md)). F01 applied its migration and synthetic seed there; Supabase's automatic RLS covers its tables. Still open: the user's screenshot showed the Data API toggle off with Save still active, and saving was not confirmed; no test-site project yet. Host decided as Render, and the user is considering Oracle Cloud instead; no host service exists ([ADR 007](../adr/007-supabase-brevo-container-host.md)) | Confirm the Data API setting. Delete the earlier Sydney project if it still exists. Create the test-site project and the host service before the first deployment |
| G7 PRD approval | lumabanls | PRD is Draft | Review the PRD and its open decisions; record approval with a date |
| G8 Review current changes | lumabanls | The 2026-11-12 rename, palette options and review record were merged to `main` in `5ceee37` (pull request #1). The C03 resolution, the D04/D05 changes, the ADR 010 architecture update, the Supabase setup records and F01 are uncommitted against it; the user will commit after F01 | Review the current diff and commit |
| G9 Design prototype walkthrough | lumabanls | The visual direction is applied under the delegated design brief and the name Tax Buddy by the user's instruction ([DESIGN](../DESIGN.md#visual-system)); 2026-11-13: the user chose the Paper & Forest palette and instructed starting F01. A recorded walkthrough of the whole prototype, unresolved D03, D06 and D18 behaviour, and production accessibility are still pending | Walk through the F01 pages in the running app (SETUP) and the rest of the [prototype](../../prototypes/README.md); record acceptance |
| G10 Devices for browser checks (PRD D17) | lumabanls | Browsers decided, and Chrome, Edge, Firefox and Brave are installed on the PC. No Mac, iPhone or Android phone is recorded for Safari and phone checks | Record the devices in the TEST-PLAN matrix |
| G11 Email service (PRD D18) | lumabanls | Service decided: Brevo. No account, sender address or sender domain yet | Needed before R08 work, not before F01 |
| G12 Report handling (PRD D22) | lumabanls | Reports go to the owner by email and there is no admin page. No inbox, response time or removal steps are defined | Decide before anyone beyond people you know uses the site |
| G13 Legal text (PRD D10) | lumabanls | The privacy and terms pages hold an outline only | Have the text written or reviewed before real users sign in |
| G14 Private service workflow and file policies | lumabanls | R19–R23 categories authorized; private-save/request/accept confirmed; remaining lifecycle, completion/sign-off, file types/quotas, deletion/retention and private storage/scanning D24–D28 open | Review the [F03 proposal](../../specs/client-services/spec.md) and resolve policies before implementing real service/file intake |
| G15 Prototype and contract conflicts (C03, C04) | lumabanls | C03 resolved 2026-11-12: the prototype directory now follows the SDD's static contract ([evidence](evidence/2026-11-12-c03-directory.md)). C04 is open: the PRD keeps drafts through connection loss while the SDD restores only chat text ([conflict table](../README.md#history-and-unresolved-conflicts), [review record](evidence/2026-11-12-prototype-scope-review.md)) | Decide C04 before the first interactive form; then update the SDD, PRD or prototype to match. The review record's other open decisions are whether real file intake (R23) stays in release 1 and whether the checklist uses one status control |

## Recent delivery changes

2026-11-10:
- Repository created from maui-starter-kit `8069179`, and its documents filled for a MAUI app on Android and Windows.
- A clickable design prototype was added as a proposal for review.

2026-11-11:
- The owner changed the platform to a .NET web application ([ADR 005](../adr/005-web-platform-blazor.md), [ADR 006](../adr/006-web-sign-in.md)).
- PRD, SDD, SETUP, DESIGN, DEVELOPMENT, RELEASE, TEST-PLAN, REFERENCES, the F01 spec and the prototype were reworked for the web. `docs/API.md` was removed. ADRs 001–004 were marked superseded.
- R08 changed from push notifications to email.
- A design pass on the prototype added three visual directions and user-experience changes, all as proposals ([DESIGN-DIRECTIONS](../../prototypes/DESIGN-DIRECTIONS.md)).
- Stack decisions: Supabase for the database, Brevo for email, and Chrome, Edge, Firefox, Brave and Safari as supported browsers. Vercel was ruled out as the host because it cannot keep the Blazor app's live connections open ([ADR 007](../adr/007-supabase-brevo-container-host.md)).
- The owner considered Next.js, chose to stay with C#, and approved the technology stack table in the [SDD](../SDD.md#technology-stack), including Tailwind CSS, xUnit and Render.
- A second design pass studied reference websites and added a Modern look and a website layout to the prototype, as proposals.
- The owner set a new product direction: a portfolio site with client ratings and no approval of accountants ([ADR 008](../adr/008-portfolio-ratings-no-approval.md)). Requirements R12–R18 were added, and the prototype gained portfolio highlights, ratings and reviews, report and block, legal and information pages, and emails for clients.

The earlier web-platform documentation work established no application verification ([evidence](evidence/2026-11-11-web-platform-change.md)). The later Kuwenta refresh applied the delegated brand/design choice and verified only the local prototype ([evidence](evidence/2026-11-11-kuwenta-refresh.md)); application verification and acceptance remain unchanged.

The subsequent F03 expansion added PRD R19–R23, ADR 009, private workspace contracts and clickable client/service/progress/document screens. The owner confirmed private saving followed by service request and accountant acceptance. Remaining lifecycle/edit/file policies stay explicit; [new prototype observations](evidence/2026-11-11-client-services.md) establish no production file or server behavior.

2026-11-12:
- The user renamed the app from Kuwenta to Tax Buddy (PRD D01, [F02 BD07](../../specs/brand-design-refresh/spec.md#rename-to-tax-buddy-2026-11-12)). The prototype, its stylesheet and logo files, current documents and planned project names were updated, and the wordmark was redrawn. The prototype checks were rerun on the renamed sources ([evidence](evidence/2026-11-12-tax-buddy-rename.md)). Name, trademark and domain availability were not checked.
- At the user's request, four colour palette proposals were added to the prototype's new Palette control beside the current Paper & Forest palette ([F02 BD08](../../specs/brand-design-refresh/spec.md#palette-options-2026-11-12), [evidence](evidence/2026-11-12-palette-options.md)). No palette was chosen and DESIGN's current palette is unchanged.
- At the user's request, the agent reviewed the prototype against the PRD, SDD and DESIGN for extra or costly features and stack fit, by reading only ([record](evidence/2026-11-12-prototype-scope-review.md)). Two conflicts were opened (C03, C04; gate G15). The prototype, PRD, SDD and DESIGN were not changed, and nothing was decided.
- Later the same day the user instructed resolving C03 as recommended and adopting the prototype's D04 and D05 proposals for now. The prototype directory became a submitted form with sort and page links, and the PRD, SDD, DESIGN and F01 documents were updated. The prototype checks were rerun ([evidence](evidence/2026-11-12-c03-directory.md)). F01's remaining prerequisites are recording the toolchain in SETUP (G1; SDK 11.0.100 was installed in WSL the same day), a Supabase development project (G6) and the prototype walkthrough (G9).

2026-11-13:
- On the user's instruction, with a pasted architecture note, the technical architecture was reviewed and updated ([ADR 010](../adr/010-single-project-per-page-render-modes.md)):
  - business rules and data access move into the one web project;
  - render modes are listed per page, and review submission is Interactive Server;
  - public pages are built to be indexable, while switching indexing on stays PRD D19.
- Two proposed parts fill gaps the note left: signed-in actions on static pages are plain named forms, and the new-chat page stays open to visitors for R06.
- The SDD, TEST-PLAN, SETUP, DEVELOPMENT, REFERENCES, the PRD's D19 row, the ADR indexes and the F01 plan, spec and tasks were updated. Documentation only: nothing was built or run.
- The user created the development Supabase project in Singapore and stored its connection string in user-secrets. The agent's check connected after correcting a placeholder host name that had come from its own instructions ([evidence](evidence/2026-11-13-supabase-dev-connection.md)). SETUP now records the region, version, storage location, format and troubleshooting. Gate G6's development part is done.
- The user approved ADR 010's two proposals, chose the Paper & Forest palette and instructed starting F01. F01 was implemented:
  - one web project with Domain, Application and Data folders, and two test projects;
  - EF Core 10 with the Npgsql provider, one migration and a synthetic development seed;
  - static directory and profile pages styled with Tailwind on DESIGN's tokens.
- Results: Release build with 0 warnings; 62 tests pass; emulated Edge checks pass after one layout fix ([evidence](evidence/2026-11-13-f01-browse-accountants.md)). The tests found two defects in error statuses, both fixed. SETUP, SDD, DESIGN, ADR 010, the PRD's D05 row and the F01 documents were updated. Nothing was committed.
- Three polish follow-ups were recorded for later, at the user's request ([F01 tasks](../../specs/browse-accountants/tasks.md#polish-follow-ups)): P1 feedback when following links inside the site, P2 shorter database timeouts, and P3 naming the environment in the start-up error. Not started.
