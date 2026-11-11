# ADR 006: Sign in with Google or Microsoft through ASP.NET Core external authentication

Status: Accepted, including ASP.NET Core Identity as the user store, which the owner approved on 2026-11-11 with the stack table in the [SDD](../SDD.md#technology-stack). Decision date: 2026-11-11. Decision owner/evidence: lumabanls. The providers were chosen on 2026-11-10 ("Google / Microsoft sign-in"); the web platform was chosen on 2026-11-11 ([ADR 005](005-web-platform-blazor.md)).

## Context

[PRD](../PRD.md#requirements) R04 requires registration before chatting. [ADR 004](004-external-sign-in.md) chose Google and Microsoft sign-in for the MAUI app and left Windows desktop sign-in unresolved (conflict C01). On the web, signing in by redirecting the browser to the provider is the standard, documented flow ([REFERENCES](../REFERENCES.md#implementation-and-hosting)), so the Windows problem does not arise.

## Decision

1. Users sign in with a Google or Microsoft account. There are no AccApp passwords.
2. The site uses ASP.NET Core's external authentication handlers. The browser goes to the provider and returns to the site's callback address, and the site then issues its own session cookie ([SDD](../SDD.md#sign-in-sessions-and-authorization)).
3. Provider client secrets stay on the server.
4. Proposed: ASP.NET Core Identity stores users and their external sign-in links, with password sign-in turned off.
5. On first sign-in the user chooses the client or accountant role (PRD D03 decides whether an account can hold both).

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| External handlers plus Identity (chosen; Identity proposed) | Standard flow; ready-made user store and sign-in links; supports account deletion | Identity brings tables and features the site doesn't use |
| External handlers with a hand-written user table | Smallest data model | Security-sensitive code to write and maintain |
| Email and password | No dependence on Google or Microsoft | Rejected by the owner on 2026-11-10; password storage and reset flows |

## Consequences and verification

- Each provider needs an app registration with redirect addresses for development and for each deployed environment ([SETUP](../SETUP.md#local-development), [RELEASE](../RELEASE.md#deployment)).
- Integration tests use a test sign-in scheme. The real providers are checked on a deployed site (TP07).
- Tests cover that a signed-in user cannot reach another user's conversation (TP10).
- Conflict C01 and PRD decision D02 are closed.

## Provenance and supersession

Original decision. Supersedes [ADR 004](004-external-sign-in.md).
