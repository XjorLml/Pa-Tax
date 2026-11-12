# Client services implementation plan

Status: documentation and prototype only. Scenarios: [spec](spec.md); tracking: [tasks](tasks.md).

## Modules

| Path / authority | Work | Scenarios |
|---|---|---|
| PRD, ADR 009 | Add stable requirements/rules; reconcile text-only chat and new service files; retain open policies | CS01–CS08 |
| SDD, RELEASE | Private metadata/byte boundaries, per-operation authorization, proposed routes, storage decision and release dependencies | CS03–CS08 |
| DESIGN | Reuse Tax Buddy tokens; private lists, checklist/progress, timeline, upload/retry controls | CS01–CS09 |
| `prototypes/service-workspace.js` | Isolated synthetic data, role-aware views/actions, per-actor drafts, generated files, failure simulation | CS01–CS08 |
| `prototypes/index.html`, `prototypes/taxbuddy.css` | Integrate navigation, profile action, phone/desktop routes, notes, controls and responsive styles | CS01–CS09 |
| Prototype checker, TEST-PLAN, STATUS, evidence | Meaningful role/failure/layout regressions and artifact identity | CS09–CS10 |

## Approach

Document the scope and proposals first. Add one buildless module using the existing renderer/helpers, without frameworks or dependencies. Seed two accountant relationships for the client persona and multiple clients for the accountant. Request/accept makes a demonstrable path from the directory into private work. Generate safe synthetic download bytes; never read or upload a user's real file. Simulate progress saves and upload failures with review controls. Keep draft state separated by actor/service and recheck permission when asynchronous uploads finish.

Future application implementation stays one Blazor Web App. Proposed entities/services/interfaces are contracts, not created .NET projects. No schema migration runs now. Production needs migrations, a private byte-store implementation, policy decisions and database/server tests before real uploads. Existing portfolio/review/chat eligibility and notification scope remain unchanged.

## Verification

Run syntax checks on each script, the expanded dependency-free Edge checker with an isolated profile, and documentation readiness checks. Exercise all CS scenarios including repeat saves, unrelated IDs, offline/failure retry and role switch during upload; inspect desktop/mobile/light/dark screenshots. Record actual results and source hashes in a new evidence record. Preserve old F02 reports and untracked work. No application build or production acceptance claim.
