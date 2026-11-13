# Browse accountants implementation plan

**Implemented 2026-11-13** (planned 2026-11-11; reworked for the web platform). [Spec](spec.md) owns feature scenarios; [tasks](tasks.md) owns execution tracking; results are in the [evidence record](../../docs/delivery/evidence/2026-11-13-f01-browse-accountants.md). The module paths below are as built.

## Affected modules and contracts

| Module / source path | Planned change | Governing contract / scenario |
|---|---|---|
| `src/TaxBuddy.Web/Domain/` | Accountant profile model, typed IDs, the "listed" rule | BR01; FT01, FT05 |
| `src/TaxBuddy.Web/Application/` | Directory use case: list with search, filter and paging; get one listed profile | [SDD data contracts](../../docs/SDD.md#data-and-persistence-contracts); BR02; FT01–FT05 |
| `src/TaxBuddy.Web/Data/` | Database context, first migration, directory queries, a synthetic seed command for development | [SDD data contracts](../../docs/SDD.md#data-and-persistence-contracts) |
| `src/TaxBuddy.Web/Components/Pages/` | Directory page (`/`) and profile page (`/accountants/{id}`), both static server-rendered, each with its own title, description and canonical address; "not available" and error pages | [SDD render modes](../../docs/SDD.md#render-modes); [SDD navigation](../../docs/SDD.md#navigation-and-screen-state); [DESIGN states](../../docs/DESIGN.md#screen-and-component-states) |
| `src/TaxBuddy.Web/Program.cs` | Register the context factory and directory use case; error handling; no-index header for public pages | [SDD errors and security](../../docs/SDD.md#errors-diagnostics-and-security) |
| `tests/TaxBuddy.Tests/`, `tests/TaxBuddy.Web.Tests/` | Scenario tests listed under Verification | [TEST-PLAN](../../docs/delivery/TEST-PLAN.md) |

## Dependencies and approach

Prerequisites, in order:
1. .NET 11 SDK installed and recorded in SETUP.
2. Solution scaffold with the SDD layout.
3. D04 and D05 decided, or explicitly provisional fields agreed for seed data. Done 2026-11-12: adopted provisionally ([PRD](../../docs/PRD.md#open-decisions)).
4. A Supabase development project exists, and its connection string is in user-secrets. Done 2026-11-13 ([SETUP](../../docs/SETUP.md#local-development)).

Sequence:
1. Domain model and the listed rule, with unit tests.
2. Data model, migration and paging query, with tests on the real database.
3. Directory use case.
4. Directory and profile pages, following the reviewed prototype.
5. Integration tests through `WebApplicationFactory`.
6. Build, then browser checks.

New packages (reason recorded when added):
- EF Core with the Npgsql PostgreSQL provider, for Supabase;
- `Microsoft.AspNetCore.Mvc.Testing` for Web.Tests.

Search and filter are a plain GET form, so they work without JavaScript and put their state in the address. Page links carry the current search and filter. This contract resolved conflict C03 on 2026-11-12, and the prototype now behaves the same way.

Persisted data: this feature creates the first schema, so there is no old-schema fixture yet. From the next schema change on, TP04 applies.

## Verification

1. TaxBuddy.Tests:
   - listed-only filtering (BR01);
   - stable paging with equal sort keys (FT03).
2. Web.Tests:
   - directory content, empty versus no match (FT01, FT02);
   - profile content with no client data (FT04), present in the first HTML response with the page's title and canonical address (TP01);
   - 404 for deleted, incomplete or malformed IDs (FT05);
   - an error page and status when the database fails (FT06).
3. Build the web project; record versions and outcome.
4. Browser checks with synthetic seed data: TP01, TP05 (FT07), TP06 and TP08, in the browsers of the agreed matrix.
5. Update STATUS with evidence records.

Use [TEST-PLAN](../../docs/delivery/TEST-PLAN.md). Expected results belong in the spec/procedures; actual outcomes belong in evidence records. Do not write planned results as completed work.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation / owner |
|---|---|---|
| D04 and D05 adopted provisionally (2026-11-12) | Profile fields and search may change after implementation | Treat the fields as provisional and expect a migration if they change; revisit them before R05. Owner lumabanls |
| Public contact details (R11) before the privacy review (D10) | Personal data published without reviewed wording | Synthetic data only until D10; profile creation (R05) must include a consent statement |
| Search engines indexing test data or real profiles before D19 | Unwanted public exposure | Public pages ask not to be indexed until D19; the test site is limited to testers (D20) |

Significant rationale: [ADR 005](../../docs/adr/005-web-platform-blazor.md), [ADR 010](../../docs/adr/010-single-project-per-page-render-modes.md). Documentation updated on 2026-11-13: SDD (as built), SETUP (toolchain, database, commands), DESIGN (tokens and components), STATUS.

As built, beyond the plan above:
- Tailwind CSS 4.3.3 builds the styles from DESIGN's tokens.
- EF Core 10 is used with the Npgsql 10 provider (SETUP explains why).
- Page size is 6, a working value.
- Two service values in the address redirect to the one-value address.
- An architecture test checks the folder rules.
- The not-found page renders "This profile is no longer available" for profile addresses, because this Blazor version sends no body with a page-set 404.
