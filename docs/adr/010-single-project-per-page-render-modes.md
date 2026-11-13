# ADR 010: One web project, with render modes chosen per page

Status: Accepted. Decision date: 2026-11-13. Decision owner/evidence: the user's instruction of 2026-11-13, "Review and check if possible for change then update the technical architecture", with a pasted architecture note. Decisions 5 and 6 were the agent's way of meeting that note together with existing requirements; the user approved them later the same day ("1. is approved").

## Context

[ADR 005](005-web-platform-blazor.md) made Tax Buddy one Blazor Web App and proposed (decision 5) a render-mode split and a layout of two production projects: a `TaxBuddy.Core` class library for domain rules, use cases and data access, and `TaxBuddy.Web` for pages. No source exists yet.

The user's note asks for:
- static rendering for the public directory, profile display and review display, because "SEO is critical for a directory";
- Interactive Server for messaging, the shortlist (PRD R20, "my accountants"), service requests and shared checklists (R19–R22), and review submission (R13);
- no separate API project, with business logic and data access "in the same Blazor Web App project";
- no app-wide Interactive WebAssembly;
- all private features behind sign-in;
- no payments, no bookings, and no file storage beyond documents attached to service work (R23).

Checked against the current documents on 2026-11-13. Everything except the project layout already matched the [SDD](../SDD.md#render-modes). The note left three points open:
- The note doesn't say how signed-in actions on the static profile page work: adding an accountant to "my accountants" (R20), reporting (R15) and the accountant's reply to a review (R14).
- R06 lets a visitor type a message before signing in. That conflicts with "all private features behind sign-in" if the new-chat page is interactive.
- "SEO is critical" points towards allowing search indexing, which is still open as PRD D19. That decision has privacy effects (D10).

## Decision

1. **One production project.** Business rules (Domain), use cases (Application) and data access (Data) are folders inside `TaxBuddy.Web`, not a separate class library. The folder rules in the [SDD](../SDD.md#architecture-and-dependencies) still apply and are enforced in review. The test projects become `TaxBuddy.Tests` (rules and data) and `TaxBuddy.Web.Tests` (integration).
2. **Render mode per page or component**, as in the SDD table. Public pages, including review display, use static server rendering. Private features and review submission use Interactive Server. No app-wide interactive mode, and no WebAssembly or Auto.
3. **Public pages are built to be indexed:** complete content in the first response, with a title, description and canonical address. Switching indexing on stays PRD D19, and the invite-only test site always asks not to be indexed.
4. **Scope limits:** no payments, no bookings, and no file storage other than private service documents.
5. **Signed-in actions on static pages are named forms posted to the same page.** This covers saving an accountant, reporting and replying to a review. They are validated on the server, protected by antiforgery, and followed by a redirect back to the page. They are shown only to signed-in users who may act, so a visitor never opens a server connection.
6. **The new-chat page is the one interactive page open to visitors.** It holds no private data, and sending requires sign-in. The draft survives sign-in in the tab's session storage, as the SDD already planned. Until chat (R06) is built, the page is a static "coming soon" page (F01).

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| One web project (chosen) | One project to build, run and deploy; simplest for a solo developer; matches the user's note | The compiler no longer stops page code from reaching data code directly; review enforces the folder rules instead |
| `TaxBuddy.Core` class library plus `TaxBuddy.Web` (ADR 005 proposal) | Rules and data access cannot reference web types | A second project to maintain with no second host to share it. Kept as a later step if the folder rules are broken in review or a second host is approved |
| App-wide Interactive Server | Every page can react without reloading | Each visitor holds a server connection (about 250 KB each, per [REFERENCES](../REFERENCES.md#implementation-and-hosting)), and public pages would need a script to become usable. Rejected |
| Interactive WebAssembly or Auto | Less server load per user | A client project and an API surface. Public pages would no longer be complete in the first response. Rejected in the note and in ADR 005 |
| Interactive "islands" for actions on static pages | Save and report without reloading | Opens a server connection on public pages for every signed-in viewer. Plain forms do the same job without one |
| Sign-in before the new-chat page | Every interactive page is private | Breaks R06 and the prototype flow, in which a visitor's typed text is kept through sign-in |
| Static review form instead of Interactive Server | A failed save keeps the posted text without a server connection, which also avoids conflict C04 for reviews | The note asks for Interactive Server. Kept as an option if C04 is resolved by narrowing what is retained |

## Consequences and verification

- SDD, SETUP, DEVELOPMENT, TEST-PLAN and the [F01 plan](../../specs/browse-accountants/plan.md) use the single-project paths and the new test project names.
- Review checks that Domain and Application use no Razor, HTTP or EF Core types, and that components never use the database context.
- TP01 checks that a public page's content is in the first response. TP12 checks the indexing setting on the deployed site.
- Review drafts stay inside open conflict C04, because review submission is interactive.
- F01 (2026-11-13) built the single project and the static public pages. `LayerRulesTests` in `TaxBuddy.Tests` checks the folder rules on every test run ([evidence](../delivery/evidence/2026-11-13-f01-browse-accountants.md)). Decisions 5 and 6 are first exercised by the features that add a static-page action and chat.

## Provenance and supersession

Original decision, recorded the day it was made. It replaces ADR 005 decision 5 (the proposed project layout and render-mode split); ADR 005's other decisions stand. Static form handling rests on the [Blazor forms overview](https://learn.microsoft.com/en-us/aspnet/core/blazor/forms/?view=aspnetcore-10.0), which was found by a web search on 2026-11-13 for ASP.NET Core 10.0. Check it against the 11.0 version at implementation.
