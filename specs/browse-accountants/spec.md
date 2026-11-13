# F01: Browse accountants and view a profile

**Feature specification, implemented 2026-11-13; not yet approved or accepted** (drafted 2026-11-11; reworked for the web platform). Results: [evidence](../../docs/delivery/evidence/2026-11-13-f01-browse-accountants.md). Governing requirements: [PRD](../../docs/PRD.md#requirements) R01, R02, R03, R10, R11; rules BR01, BR02. Technical/UI contracts: [SDD](../../docs/SDD.md), [DESIGN](../../docs/DESIGN.md#screen-and-component-states). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A visitor or signed-in user can see which accountants are listed, narrow the list, and read one accountant's full profile, without signing in. This is the first end-to-end slice: browser → web application → database.

- **Existing behavior:** none. The repository holds documentation and a prototype only (baseline `b9b50f3`).
- **Proposed behavior:**
  - a paged directory page showing name, a short career summary and services;
  - search and filter as decided in D05, submitted as a plain form and kept in the page address, in name A–Z order with a unique tie-breaker;
  - a profile page with career summary, experience, contact details and services.
- **Exclusions:**
  - the "Message" action's sign-in and chat flow (R04, R06), which is shown but leads to a "coming soon" page in this slice;
  - accountant profile editing (R05);
  - email notifications;
  - prices;
  - ratings, reviews and portfolio highlights (R12–R14), which get their own feature. Cards and profiles leave room for them.

  Profiles for development and tests are synthetic seed data.
- **Authorization/decision evidence:** owner chose this as the first feature on 2026-11-10. The spec itself is not yet approved.

Product rules are not copied here; the PRD IDs above govern.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| FT01 / R01 | Given listed accountants exist, when a visitor opens the site | The directory shows each listed accountant's name, career summary and services, in a stable order, without a sign-in prompt | Web.Tests (page content), browser |
| FT02 / R01, R02 | Given no listed accountants, versus given accountants exist but none match the search | "No accountants yet" and "No matches" are different messages; clearing the search restores the list | Web.Tests + browser |
| FT03 / R01, BR02 | Given more accountants than one page, including equal sort keys, when the user pages to the end | Every listed accountant appears exactly once, in order | TaxBuddy.Tests (paging query on the real database) |
| FT04 / R03, R11 | When the user opens a profile | Career summary, experience, contact details and services are shown; nothing about clients appears on any public page | Web.Tests (page content), browser |
| FT05 / R03, BR01 | Given a profile address for an accountant who was deleted or whose profile is incomplete, or a malformed ID | "This profile is no longer available", with a way back to the directory, and HTTP 404; no error loop | Web.Tests + browser |
| FT06 / R01, R10 | Given the database is unavailable when the directory or a profile is requested | An error page with a retry, and an error status; never an empty directory | Web.Tests (controlled failure), browser (TP08) |
| FT07 / R01, R02 | When the user searches, opens a profile, presses Back, reloads, or opens a copied address in a new tab | The same search, filter and page are shown each time | Browser (TP01, TP05) |

## State, dependencies, and unresolved decisions

- **States:**
  - Directory: ready, empty, no match, or error.
  - Profile: ready, not available, or error.
  - Both pages are static server-rendered, so there is no in-page loading state beyond the browser's own.
- **Dependencies:**
  - the .NET 11 SDK and the solution scaffold ([SETUP](../../docs/SETUP.md#first-checkout));
  - a Supabase project for development and tests ([ADR 007](../../docs/adr/007-supabase-brevo-container-host.md)).
- **Decisions, provisional since 2026-11-12:** the user instructed adopting the prototype's proposals for D04 (profile fields) and D05 (search, filter and order) for now ([PRD](../../docs/PRD.md#open-decisions)).
  - F01 builds the name A–Z order only. "Highest rated" arrives with ratings (R13).
  - Page size is still open. The implementation uses 6, matching the prototype, as a working value.
  - If the fields change later, expect a migration.
- **Conflict C03:** resolved 2026-11-12. The directory follows the static contract, and the prototype was changed to match ([docs index](../../docs/README.md#history-and-unresolved-conflicts)).
- **Not blocking:** sign-in, hosting, email, and search-engine indexing (public pages ask not to be indexed until D19 is decided).

## Evidence limits

No execution or acceptance is established by this specification. Track implementation, tests, builds, browser checks and acceptance separately in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md). Link dated records when available.
