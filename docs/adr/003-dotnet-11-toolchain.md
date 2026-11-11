# ADR 003: Build on .NET 11 once it is generally available

**Superseded on 2026-11-11 by [ADR 005](005-web-platform-blazor.md).** The text below is kept as the record of the original decision. Original status: Proposed, never accepted. Decision date: 2026-11-10 (choice made; pin pending). Decision owner/evidence: lumabanls, setup-session answer of 2026-11-10 (".NET 11 once GA is confirmed").

## Context

On 2026-11-10 ([REFERENCES](../REFERENCES.md#revalidation-log)):
- MAUI 10 (latest patch 10.0.120) was the only supported MAUI line, with support ending 2027-05-11.
- .NET 11 was at RC3 (2026-10-25), with GA expected at .NET Conf, 2026-11-10 to 13. It is an STS release supported to 2028-11-14.

This PC has .NET 10 SDKs and MAUI 10 workloads, but no .NET 11 ([SETUP](../SETUP.md#reproducibility-record)).

## Decision

- Use .NET 11 and its MAUI release for all projects.
- Documentation and design work continue now.
- Pin the exact SDK and workload-set versions in `global.json` only after GA appears on the official support pages.
- Do not scaffold on a release candidate.

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| .NET 11 after GA (chosen) | Longest support window for a new app; avoids an early forced upgrade | Short wait; Visual Studio 2026 update and new SDK/workload installs |
| MAUI 10 now | Tools already installed | Rejected: MAUI 10 support ends 2027-05-11, roughly six months away |
| .NET 11 RC3 now | Start immediately | Rejected: prerelease toolchain for a new project; `allowPrerelease` would be needed |

## Consequences and verification

- Accept this ADR when SETUP records the GA SDK version, workload-set version and Visual Studio version, and `dotnet workload --version` shows the pinned set from the repository root.
- MAUI support is shorter than the .NET runtime's. Record the MAUI 11 end-of-support date when it is published, and plan servicing updates monthly.

## Provenance and supersession

Original decision. Supersedes nothing. Superseded by [ADR 005](005-web-platform-blazor.md) on 2026-11-11.
