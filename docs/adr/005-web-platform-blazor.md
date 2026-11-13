# ADR 005: AccApp is a Blazor Web App on .NET 11, web only, with email notifications

Status: Accepted (web platform, Blazor Web App, web only, .NET 11, email notifications); Proposed (render-mode split, project layout, email job design). Decision date: 2026-11-11. Decision owner/evidence: lumabanls, message of 2026-11-11 ("im changing the platform to .net web application") and answers in that session ("Blazor Web App", "No, web only", ".NET 11", "Email").

## Context

AccApp was planned on 2026-11-10 as a .NET MAUI app for Android and Windows with its own ASP.NET Core API ([ADR 001](001-platform-scope-android-windows.md), [ADR 002](002-own-api-online-only.md), [ADR 003](003-dotnet-11-toolchain.md)). No code existed when the owner changed the platform to a .NET web application. Requirements R01–R11 in the [PRD](../PRD.md#requirements) stay, except that push notifications (R08) have no direct equivalent on every browser.

Facts checked on 2026-11-11 ([REFERENCES](../REFERENCES.md#revalidation-log)):
- .NET 11.0.0 was released on 2026-11-10 as an STS release supported to 2028-11-14.
- .NET 10 LTS ends support on the same date.

## Decision

1. AccApp is a website, built as one ASP.NET Core Blazor Web App. There are no native apps.
2. Web only: there is no separate API project, no shared Contracts project and no OpenAPI document. Pages call application services in the same process.
3. The site targets .NET 11.
4. Accountants are told about new client messages by email (R08). Browser push is excluded from release 1.
5. Proposed: public pages use static server rendering and signed-in pages use Interactive Server, with no WebAssembly client project. The layout is two production projects and two test projects ([SDD](../SDD.md#architecture-and-dependencies)).
6. Proposed: emails are sent from database-backed jobs by a background worker ([SDD](../SDD.md#live-chat-and-email-notifications)).

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Blazor Web App (chosen) | One language and one project to deploy; fast public pages; live chat without hand-written JavaScript | Interactive pages hold a server connection each, and restarts drop them |
| MVC or Razor Pages | Simple request/response model | Chat needs separate real-time client code. Not chosen by the owner |
| JavaScript front end plus ASP.NET Core API | Front end and API scale and deploy separately | Two languages and two projects for a solo developer. Not chosen by the owner |
| Next.js on Vercel with Supabase as the back end | The owner already knows Next.js, and it suits Supabase and Vercel without a hosting workaround | Reconsidered on 2026-11-11 after the Vercel conflict ([ADR 007](007-supabase-brevo-container-host.md)). The owner chose to stay with C# |
| Keep a separate API for a future native app | Ready for a mobile app | Extra versioning and contract work now, for an app that is not planned. Rejected by the owner ("web only") |
| .NET 10 LTS | Already installed on the development PC | Same end-of-support date as .NET 11. The owner chose .NET 11 |
| Browser push notifications | Alerts without email | On iPhones it works only for sites added to the home screen; needs a service worker. Email chosen by the owner |
| Interactive WebAssembly or Auto | Less server load per user | A second project and an API surface, which decision 2 removes |

## Consequences and verification

- The MAUI and Android tooling in the earlier SETUP is no longer needed. The .NET 11 SDK must be installed ([SETUP](../SETUP.md#first-checkout)).
- `docs/API.md` was removed. Its sign-in, error and security rules moved to the [SDD](../SDD.md#sign-in-sessions-and-authorization).
- The Windows sign-in conflict C01 no longer applies ([ADR 006](006-web-sign-in.md)).
- New open decisions: supported browsers (D17), email service and frequency (D18), search-engine indexing (D19), limiting the test site to testers (D20).
- Deployments drop open chat connections; TP15 checks what users see.
- Running more than one server instance later needs session affinity and a shared chat notifier.
- The proposed parts are accepted when the first feature is built and its tests and browser checks pass.

## Provenance and supersession

Original decision, recorded the day it was made. Supersedes [ADR 001](001-platform-scope-android-windows.md), [ADR 002](002-own-api-online-only.md) and [ADR 003](003-dotnet-11-toolchain.md).

2026-11-13: decision 5 (the proposed render-mode split and two-project layout) is superseded by [ADR 010](010-single-project-per-page-render-modes.md). The other decisions stand. The text above is kept as it was recorded.
