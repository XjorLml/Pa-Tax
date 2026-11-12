# Browse accountants implementation plan

**Proposed (2026-11-11; reworked for the web platform).** [Spec](spec.md) owns feature scenarios; [tasks](tasks.md) owns execution tracking. Nothing here is implemented.

## Affected modules and contracts

| Module / source path | Planned change | Governing contract / scenario |
|---|---|---|
| `src/TaxBuddy.Core/Domain/` | Accountant profile model, typed IDs, the "listed" rule | BR01; FT01, FT05 |
| `src/TaxBuddy.Core/Application/` | Directory use case: list with search, filter and paging; get one listed profile | [SDD data contracts](../../docs/SDD.md#data-and-persistence-contracts); BR02; FT01–FT05 |
| `src/TaxBuddy.Core/Data/` | Database context, first migration, directory queries, a synthetic seed command for development | [SDD data contracts](../../docs/SDD.md#data-and-persistence-contracts) |
| `src/TaxBuddy.Web/Components/Pages/` | Directory page (`/`) and profile page (`/accountants/{id}`), both static server-rendered; "not available" and error pages | [SDD navigation](../../docs/SDD.md#navigation-and-screen-state); [DESIGN states](../../docs/DESIGN.md#screen-and-component-states) |
| `src/TaxBuddy.Web/Program.cs` | Register the context factory and directory use case; error handling; no-index header for public pages | [SDD errors and security](../../docs/SDD.md#errors-diagnostics-and-security) |
| `tests/TaxBuddy.Core.Tests/`, `tests/TaxBuddy.Web.Tests/` | Scenario tests listed under Verification | [TEST-PLAN](../../docs/delivery/TEST-PLAN.md) |

## Dependencies and approach

Prerequisites, in order:
1. .NET 11 SDK installed and recorded in SETUP.
2. Solution scaffold with the SDD layout.
3. D04 and D05 decided, or explicitly provisional fields agreed for seed data.
4. A Supabase development project exists, and its connection string is in user-secrets.

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

Search and filter are a plain GET form, so they work without JavaScript and put their state in the address.

Persisted data: this feature creates the first schema, so there is no old-schema fixture yet. From the next schema change on, TP04 applies.

## Verification

1. Core.Tests:
   - listed-only filtering (BR01);
   - stable paging with equal sort keys (FT03).
2. Web.Tests:
   - directory content, empty versus no match (FT01, FT02);
   - profile content with no client data (FT04);
   - 404 for deleted, incomplete or malformed IDs (FT05);
   - an error page and status when the database fails (FT06).
3. Build the web project; record versions and outcome.
4. Browser checks with synthetic seed data: TP01, TP05 (FT07), TP06 and TP08, in the browsers of the agreed matrix.
5. Update STATUS with evidence records.

Use [TEST-PLAN](../../docs/delivery/TEST-PLAN.md). Expected results belong in the spec/procedures; actual outcomes belong in evidence records. Do not write planned results as completed work.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation / owner |
|---|---|---|
| D04 and D05 undecided | Profile fields and search change after implementation | Decide before the data model; or mark seed fields provisional and expect a migration. Owner lumabanls |
| Public contact details (R11) before the privacy review (D10) | Personal data published without reviewed wording | Synthetic data only until D10; profile creation (R05) must include a consent statement |
| Search engines indexing test data or real profiles before D19 | Unwanted public exposure | Public pages ask not to be indexed until D19; the test site is limited to testers (D20) |

Significant rationale: [ADR 005](../../docs/adr/005-web-platform-blazor.md). Documentation updates when implemented: SDD (as built), SETUP (database and commands), STATUS.
