# ADR 001: Release 1 targets Android and Windows only

**Superseded on 2026-11-11 by [ADR 005](005-web-platform-blazor.md).** The text below is kept as the record of the original decision. Original status: Accepted. Decision date: 2026-11-10. Decision owner/evidence: lumabanls, answer in the AccApp setup session of 2026-11-10 ("Android, Windows").

## Context

.NET MAUI supports Android, iOS, Mac Catalyst and Windows ([REFERENCES](../REFERENCES.md#toolchain-and-platforms)). iOS and Mac Catalyst builds and signing need a Mac host. The only development host recorded in [SETUP](../SETUP.md#reproducibility-record) is a Windows 11 PC, which can build Android and Windows. Governing scope: [PRD release scope](../PRD.md#release-scope).

## Decision

Release 1 builds and ships `net11.0-android` and the Windows target only. iOS and Mac Catalyst are explicit exclusions. Project files must not list Apple TFMs, so restores on this host don't need Apple workloads.

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Android + Windows | Both buildable and testable on the existing PC; covers phone and desktop use | Chosen by the owner. Windows adds a sign-in conflict (C01) and a separate push mechanism |
| Android only | Smallest scope; WebAuthenticator works on Android | Rejected: the owner wants Windows |
| All four MAUI platforms | Widest reach | Rejected for release 1: no Mac host, Apple signing and review costs |

## Consequences and verification

- Every feature needs Android and Windows build and native checks ([TEST-PLAN](../delivery/TEST-PLAN.md), as it stood on 2026-11-10).
- Platform-specific adapters exist only under `Platforms/Android` and `Platforms/Windows` ([SDD](../SDD.md#platform-capability-matrix)).
- Windows sign-in is blocked until [ADR 004](004-external-sign-in.md) resolves C01.
- Adding iOS later needs a Mac host, Apple adapters and a new ADR that supersedes this one.

## Provenance and supersession

Original decision, recorded the same day it was made. Supersedes nothing. Superseded by [ADR 005](005-web-platform-blazor.md) on 2026-11-11.
