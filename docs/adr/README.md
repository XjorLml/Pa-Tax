# Architecture decision records

**Authority:** decision rationale and provenance. Current technical contracts belong in [SDD](../SDD.md); approved behavior in [PRD](../PRD.md).

## Index

The [template](TEMPLATE.md) is not an accepted ADR.

| ID / title | Status | Decision date | Supersedes / superseded by |
|---|---|---|---|
| [005 Blazor Web App on .NET 11, web only, email notifications](005-web-platform-blazor.md) | Accepted; render modes, layout and email jobs Proposed | 2026-11-11 | Supersedes 001, 002, 003 |
| [006 Google/Microsoft sign-in on the web](006-web-sign-in.md) | Accepted | 2026-11-11 | Supersedes 004 |
| [007 Supabase database, Brevo email, and a host other than Vercel](007-supabase-brevo-container-host.md) | Accepted; Render chosen as the host | 2026-11-11 | None |
| [008 A portfolio site with client ratings and no approval](008-portfolio-ratings-no-approval.md) | Accepted; reviewer naming, report handling and portfolio limits Proposed | 2026-11-11 | None |
| [009 Private service workspaces](009-private-service-workspaces.md) | Accepted scope and private-save/request/accept model; remaining lifecycle, permissions and storage Proposed | 2026-11-11 | None; extends 008; chat stays text only |
| [001 Android and Windows only](001-platform-scope-android-windows.md) | Superseded | 2026-11-10 | Superseded by 005 |
| [002 Own API, shared Contracts, online-only client](002-own-api-online-only.md) | Superseded | 2026-11-10 | Superseded by 005 |
| [003 .NET 11 after GA](003-dotnet-11-toolchain.md) | Superseded (was Proposed) | 2026-11-10 | Superseded by 005 |
| [004 Google/Microsoft sign-in through the API](004-external-sign-in.md) | Superseded | 2026-11-10 | Superseded by 006 |

Superseded records describe the MAUI plan of 2026-11-10. They are kept for history and do not describe the current design.

Decisions expected next:
- Email sender address and email frequency (PRD D18).
- Whether search engines may index public profiles (PRD D19).
- How reports are handled without an admin page (PRD D22).

## When and how

Record consequential choices such as storage strategy, sync/conflict behavior, platform scope constraints, or a meaningful project split. Routine naming choices rarely need an ADR.

Choose the next available `NNN-short-title.md`; check existing and historical references before assigning a number. Link the requirement/spec, alternatives, rationale and decision evidence. Unknown approval/date remains unknown. Mark retrospective reconstruction explicitly and cite its source. When superseding, link both records and update SDD so rationale and current contracts stay consistent.
