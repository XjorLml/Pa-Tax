# ADR 004: Sign in with Google or Microsoft through the API

**Superseded on 2026-11-11 by [ADR 006](006-web-sign-in.md).** The text below is kept as the record of the original decision. Original status: Accepted (providers and server-mediated flow); Proposed (Windows mechanism, open as conflict C01). Decision date: 2026-11-10. Decision owner/evidence: lumabanls, setup-session answer of 2026-11-10 ("Google / Microsoft sign-in").

## Context

[PRD](../PRD.md#requirements) R04 requires registration before chatting; R06 and R08 depend on it. MAUI's WebAuthenticator starts browser-based sign-in and recommends a web backend as the middle layer, so that provider secrets stay off the device. On 2026-11-10 its documentation said "WebAuthenticator isn't working on Windows" ([REFERENCES](../REFERENCES.md#revalidation-log)). Windows is a release target ([ADR 001](001-platform-scope-android-windows.md)).

## Decision

1. Users sign in with a Google or Microsoft account. There are no AccApp passwords.
2. The API is the middle layer. The app opens the API's sign-in endpoint in a browser. The API completes the provider flow, creates or finds the user, and returns AccApp's own access and refresh tokens to a registered app callback (API.md, removed on 2026-11-11).
3. Android uses WebAuthenticator.
4. On Windows, the mechanism is open (PRD D02). Until it is resolved, the Windows build supports browsing only (R01–R03), and the sign-in entry point explains that it is not yet available.

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Google/Microsoft via the API (chosen) | No passwords to store; secrets stay on the server; one token format for the app | Two provider registrations; Windows mechanism unresolved |
| Email and password in the API | Same behavior on both platforms | Rejected by the owner; password storage and reset flows |
| Provider SDKs directly in the app | Native experience | Each provider's mobile and desktop SDK differs; still needs server-side token validation. Kept as a candidate for Windows only |

Windows candidates to evaluate against current official guidance before choosing (none verified yet):
- a system-browser flow that returns to the app through a protocol activation or a loopback redirect;
- a provider SDK that supports Windows desktop;
- a sign-in page hosted by the API inside an embedded web view.

Record the evaluation and choice as a superseding ADR, and update SDD and API.

## Consequences and verification

- Api.Tests cover token issue, refresh and revocation, and the rule that a user cannot reach another user's data (TP10).
- Android native check: sign-in, cancel and restart keep or clear the session as specified.
- Windows: blocked until D02. STATUS tracks this.
- Production and development provider registrations, with their redirect URIs, belong in [SETUP](../SETUP.md) and [RELEASE](../RELEASE.md).

## Provenance and supersession

Original decision. Supersedes nothing. Superseded by [ADR 006](006-web-sign-in.md) on 2026-11-11.
