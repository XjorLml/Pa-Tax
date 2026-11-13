# Development standards

**Adopted baseline (2026-11-11) for a Blazor Web App.** It replaces the MAUI baseline inherited from the starter kit. No source exists yet, so none of these practices has been exercised in this repository. [SDD](SDD.md) owns actual project contracts. Each exception should have a reason and an owner.

## Essential practices

| Area | Standard and purpose | Review / verification |
|---|---|---|
| Scope and architecture | Link work to stable requirement/scenario IDs. Keep rules in the Domain and Application folders, away from pages; those folders use no Razor, HTTP or EF Core types ([SDD](SDD.md#architecture-and-dependencies)) | Review changed source against PRD/SDD; record contradictions. `LayerRulesTests` checks folder dependencies on every test run; review covers method bodies |
| Components | Pages render state and forward user actions to Application services. No database access or business rules in `.razor` files | Tests cover invalid input, state changes and saved effects through the services |
| Render modes | Public pages stay static. Add interactivity per page, only where the [SDD render-mode table](SDD.md#render-modes) lists it; never app-wide. Signed-in actions on public pages are named forms, not interactive components. Interactive pages prerender by default, so start-up code must be safe to run twice | Check each new page's mode in review; test that loading a page does not repeat a write |
| Authorization | Enforce every rule in Application services on the server. Hiding a link or button is not a check | Integration tests call each protected operation as the wrong user and as a visitor |
| Forms | Antiforgery on every form. Validate on the server; client checks are for convenience only | Integration tests post invalid and forged input |
| DI and lifetimes | Constructor dependencies, registered in `Program.cs`. A scoped service on an interactive page lives as long as the connection, so it holds no per-request state | Review singleton and scoped dependencies; document lifetimes in SDD |
| Database access | One short-lived context per operation, from the context factory. Parameterized queries, explicit ordering | Data tests against the real provider, including equal-sort-key cases |
| Async and events | Return and await tasks; no blocking waits. Disable a button while its operation runs so repeated clicks can't run it twice | Controlled failure, repeated click and late-result tests |
| Lifecycle | Components cancel work and unsubscribe when disposed. State kept only in server memory can vanish on reconnect; durable state goes to the database, the address, or browser storage | Lost-connection and reload checks (TP03, TP15) |
| Navigation | Validate route and query values. Return addresses after sign-in must be local | Tests with malformed IDs and external return addresses |
| Schema changes | EF Core migrations in source control; never edit an applied migration | Upgrade tests from the previous schema (TP04) |
| Error recovery | Separate loading, empty, error and "not available" states; keep the user's input | Controlled read, write and start-up failures |
| Diagnostics | Log event IDs, categories, duration and correlation IDs. Never log message text, email addresses, contact details or tokens | Review each logged field; test failure paths |
| Security | HTTPS and HSTS, secure cookies, rate limits, capped input sizes, secrets outside the repository | Inspect production configuration (TP12) |
| UI and locale | Follow DESIGN tokens, semantic HTML, labels and focus order. Format times through the one Asia/Manila helper | Browser accessibility and layout checks (TP06) |
| Verification | Combine unit tests, data tests on the real provider, integration tests and browser checks | Evidence includes revision, environment, date, attribution, result |
| Documentation | Update the owning contract and link it from specs; preserve attributed history | Link/anchor checks and final diff review |

The official guidance behind the render-mode, context-factory, authorization and repeated-event rows is in [REFERENCES](REFERENCES.md#implementation-and-hosting).

## CI baseline

No CI exists yet, and there is no remote repository. When one is added:

1. On proposed merges and pushes to maintained branches, run both test projects and build the web project with SETUP's pinned SDK.
2. Check nullable and compiler warnings; document justified exceptions rather than hiding all warnings.
3. Run the documentation checker for documentation changes.
4. Keep deployment jobs separate from ordinary contributions, with minimal permissions and protected secrets.

Record an actual successful remote run before claiming CI works. Choose runner and action versions at setup time and maintain them.

## Optional additions and triggers

| Addition | Add when | Tradeoff |
|---|---|---|
| Browser automation tests | Repeated regressions in browser-only behavior | Test infrastructure and selectors need ownership |
| Component test library | Interactive components gain logic worth testing in isolation | Another dependency to keep current |
| Telemetry and error reporting | There is a defined support need and privacy policy | Useful diagnosis; data retention and operations costs |
| Separate API project | A native app or third-party client is approved | Versioning and compatibility contracts become mandatory |
| Caching and paging changes | Measured data scale or latency exceeds an approved target | Complexity and invalidation; measure first |

Introduce interfaces for meaningful substitution (email sending, the clock) or for tests. Avoid creating an interface for every class. Keep human-readable labels separate from enum and ID logic.

## Change completion

Review policy (Proposed, solo developer lumabanls). Before merging to `main`:
- review the full diff yourself;
- run the documentation checker for documentation changes;
- run the relevant test projects and builds for code changes, as defined in [TEST-PLAN](delivery/TEST-PLAN.md).

Branches and pull requests are optional while there is no remote. Revisit this policy when a second contributor or CI is added.

A completed change has its authorized behavior implemented, appropriate observed verification, updated contracts/evidence, and explicit remaining acceptance. Documentation-only changes require documentation checks, not application test runs.
