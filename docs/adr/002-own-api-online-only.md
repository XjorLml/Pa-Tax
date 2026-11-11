# ADR 002: Own ASP.NET Core API, shared Contracts project, online-only client

**Superseded on 2026-11-11 by [ADR 005](005-web-platform-blazor.md).** The text below is kept as the record of the original decision. Original status: Accepted (server and offline level); Proposed (shared Contracts project). Decision date: 2026-11-10. Decision owner/evidence: lumabanls, setup-session answers of 2026-11-10 ("Own ASP.NET Core API", "Online only").

## Context

The directory, chat and notifications in [PRD](../PRD.md#requirements) R01–R11 need shared server data, per-user authorization and push dispatch. The kit's client-server baseline offers three ways to share wire types between app and API ([SDD](../SDD.md#architecture-and-dependencies)). Offline writes would need queueing and conflict rules for chat.

## Decision

- AccApp has its own ASP.NET Core API (`src/AccApp.Api`), which is the only store of record.
- The app is online only in release 1. It shows an offline state, keeps typed text, and queues nothing (R10).
- Request and response types live in a plain .NET `AccApp.Contracts` project referenced by Core and Api (Proposed until the first endpoint is built).
- The committed OpenAPI document stays the published contract, and its diff is reviewed on every API change (API.md, removed on 2026-11-11).

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Own API (chosen) | Full control of authorization, chat and push; C# on both sides | Hosting, operations and security are the project's responsibility (D11) |
| Third-party backend service | Less server code | Rejected by the owner |
| Shared Contracts project (proposed) | One definition; in-repo mismatches fail to compile | Can hide a breaking change; mitigated by the OpenAPI diff review |
| Client generated from OpenAPI | Client follows the published contract | Extra generator tooling and generated code; reconsider if Contracts drifts |
| Read cache or offline writes | Usable on weak connections | Rejected for release 1: chat conflicts, queue durability and more testing |

## Consequences and verification

- API rules, error mapping and retry safety in API.md (removed on 2026-11-11) are mandatory.
- Api.Tests (WebApplicationFactory) and Core client tests with a fake handler are required ([TEST-PLAN](../delivery/TEST-PLAN.md), as it stood on 2026-11-10).
- No device database, so no device migrations in release 1.
- Revisit the offline level if testers report unusable behavior on poor connections.

## Provenance and supersession

Original decision. Supersedes nothing. Superseded by [ADR 005](005-web-platform-blazor.md) on 2026-11-11.
