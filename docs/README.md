# Kuwenta documentation

This index owns document navigation and authority. Link new feature, decision and evidence records here as they are created. Keep the root README short so it does not become a second status or requirements document.

## Start here

- First checkout: [SETUP](SETUP.md). Start with its reproducibility record and host-preparation steps; no projects exist yet.
- Product scope and open decisions: [PRD](PRD.md), then the first feature, [browse-accountants](../specs/browse-accountants/spec.md).
- Design proposal to review before implementation: the clickable [prototype](../prototypes/README.md).
- Current progress and blockers: [STATUS](delivery/STATUS.md).
- Documentation check or an error to fix: [tools guide](../tools/README.md).

## Document ownership

| Document | Owns | Update when |
|---|---|---|
| [PRD](PRD.md) | Product problem, approved requirements, business rules, platform scope, acceptance outcomes | User-visible behavior or scope changes |
| [SDD](SDD.md) | Architecture, dependency directions, data, sign-in, navigation and lifecycle contracts, browser and hosting capability matrix | A technical contract changes |
| [DESIGN](DESIGN.md) | UI tokens, interactions, accessibility, responsive and localized presentation | UI-system decisions change |
| [SETUP](SETUP.md) | Exact toolchain, environment configuration, developer commands | Toolchain or setup changes |
| [DEVELOPMENT](DEVELOPMENT.md) | Coding, review, and maintenance practices | Team practices change |
| [RELEASE](RELEASE.md) | Publishing, deployment and recovery procedures | Delivery method changes |
| [STATUS](delivery/STATUS.md) | Current implementation and evidence-linked verification/acceptance state | New work or evidence changes the state |
| [TEST-PLAN](delivery/TEST-PLAN.md) | Procedures, environments and gate definitions | Behavior or verification boundaries change |
| [Evidence](delivery/evidence/TEMPLATE.md) | Dated actual results and attribution | A check is executed |
| [Feature specs](../specs/feature-template/spec.md) | Scenario refinement, bounded plans and tasks linked to requirements | A feature is planned or changed |
| [ADRs](adr/README.md) | Significant decisions and rationale | A consequential choice is made or superseded |
| [REFERENCES](REFERENCES.md) | External technical guidance and review dates | Guidance is revalidated |
| [AGENTS](../AGENTS.md) | Agent workflow | Workflow changes |
| [Tools guide](../tools/README.md) | Safe automation entry points, documentation checker and exit meanings | A helper or its prerequisites change |

## Task routes

- Product change: PRD -> feature spec -> SDD/DESIGN -> TEST-PLAN.
- Data or hosting change: SDD -> affected source/tests -> relevant feature plan -> evidence.
- First checkout or toolchain upgrade: SETUP -> REFERENCES -> builds/tests -> STATUS.
- Release: STATUS -> TEST-PLAN -> RELEASE -> deployment evidence.
- Documentation-only task: affected authority -> local path/anchor and consistency checks -> diff.

## Current feature and decision indexes

| Feature | Requirements | Files | State |
|---|---|---|---|
| F01 Browse accountants and view a profile | R01–R03, R10, R11 | [spec](../specs/browse-accountants/spec.md), [plan](../specs/browse-accountants/plan.md), [tasks](../specs/browse-accountants/tasks.md) | Draft; not started |
| F02 Kuwenta brand and prototype refresh | D01; R01–R18 presentation | [spec](../specs/brand-design-refresh/spec.md), [plan](../specs/brand-design-refresh/plan.md), [tasks](../specs/brand-design-refresh/tasks.md) | Local prototype design; application implementation remains not started |
| F03 Client relationships, service progress and private documents | R19–R23, R10–R11 | [spec](../specs/client-services/spec.md), [plan](../specs/client-services/plan.md), [tasks](../specs/client-services/tasks.md) | Authorized expansion; private-save/request/accept confirmed; remaining D24–D28 policies open; application not started |

New features start from the [feature template](../specs/feature-template/spec.md) (with its [plan](../specs/feature-template/plan.md) and [tasks](../specs/feature-template/tasks.md)), copied to `specs/<feature>/`. Decisions: [ADR index](adr/README.md). ADRs 005 to 009 are current; 001–004 are superseded.

## History and unresolved conflicts

- 2026-11-10: this repository was created from the maui-starter-kit template at commit `8069179` (baseline commit `b9b50f3`). The kit's setup guide and structure proposal were removed after the chosen layout was recorded in the SDD; they remain in the template repository.
- 2026-11-11: the owner changed the platform from a .NET MAUI app (Android and Windows, with a separate API) to one .NET web application ([ADR 005](adr/005-web-platform-blazor.md)). The MAUI plan survives in superseded ADRs 001–004 and in the [2026-11-10 evidence record](delivery/evidence/2026-11-10-documentation-setup.md). `docs/API.md` was removed, and its rules moved to the [SDD](SDD.md#sign-in-sessions-and-authorization). The starter kit's document structure is still used; its MAUI-specific guidance was replaced.

| Conflict ID | Conflicting sources | Effect | Owner / resolution |
|---|---|---|---|
| C01 | ADR 001 targeted Windows and ADR 004 chose browser-based Google/Microsoft sign-in, but MAUI's WebAuthenticator was documented as not working on Windows | Sign-in would not have worked in a Windows app | Resolved 2026-11-11: the platform changed to the web, where redirect sign-in is the standard flow ([ADR 006](adr/006-web-sign-in.md)) |
| C02 | The owner chose Vercel for hosting, but the design in ADR 005 needs a host that keeps the app running with long-lived WebSockets. Vercel lists no .NET runtime, and its container option stops idle apps and closes connections at the function time limit ([REFERENCES](REFERENCES.md#hosting-database-and-email-services)) | Live chat pages and the background email sender would not work reliably on Vercel | Resolved 2026-11-11: the owner kept the Blazor design and gave up Vercel ([ADR 007](adr/007-supabase-brevo-container-host.md)). Render was subsequently chosen the same day ([SDD stack](SDD.md#technology-stack)); plan and environment details remain in PRD D11 |

No conflicts are open. Requirements describe intended behavior, source shows implementation, and execution records show verified behavior. Record discrepancies explicitly. Archived records retain provenance and do not override current authorities.
