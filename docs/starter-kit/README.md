# Starter kit guide

**Setup help; delete this folder when setup is done.** This repository is a .NET MAUI starter kit template, or a repository created from it. It holds documentation templates, a proposed source layout and a local documentation checker. It contains no generated or build-tested application, and nothing in it is evidence that a project works. The kit was extracted on 2026-11-09 from the author's earlier MAUI project; carry over no identifiers, branding, signing identity, test counts or approvals from that project.

## Start here

1. Create your repository from this template, then clone it.
2. From the repository root, run the checks below. They need Python 3.10+ and no packages; on Windows, `py -3` may work where `python` does not.
3. Customize in the order below, or give the [setup prompt](PROMPT.md) to a coding assistant. Start with documentation only while product or toolchain decisions are open.
4. When setup is done, delete this folder, remove the links the checker then reports, and run the readiness check.

```powershell
python tools/check_docs.py
python tools/check_docs.py --ready
```

- **First command:** passes on an untouched copy, with placeholders allowed.
- **Second command:** fails until every `{{...}}` field outside the reusable templates is resolved. The ADR, evidence and feature templates keep their placeholders for future copies, and their links are still checked.
- **Errors:** each shows the file, line and next action; the [tools guide](../../tools/README.md) explains exit codes and limits.
- **Limits:** the checker does not build an app, use the network, change files or touch devices.

## Customize a new project

| Step | Decide or fill | Ready to continue when |
|---|---|---|
| 1. Product | [PRD](../PRD.md): users, first workflows, platforms, server, data/privacy, locale and distribution | Approved scope and unresolved owners are explicit |
| 2. Technical shape | [STRUCTURE](STRUCTURE.md) and [SDD](../SDD.md) | A small layout and actual platform boundaries fit the approved workflows |
| 3. Server, if any | [API](../API.md) and the [client-server layout](STRUCTURE.md#client-server-baseline) | Environments, compatibility, sign-in, error, retry and offline choices are explicit, or API.md is deleted with its links |
| 4. Development environment | [SETUP](../SETUP.md) | Compatible versions and hosts are selected from rechecked official guidance; command placeholders are resolved |
| 5. Interface and work | [DESIGN](../DESIGN.md) and the [feature template](../../specs/feature-template/spec.md) | The first feature has observable scenarios, a plan, tasks and explicit verification boundaries |
| 6. Evidence and delivery | [STATUS](../delivery/STATUS.md), [TEST-PLAN](../delivery/TEST-PLAN.md), [RELEASE](../RELEASE.md) | Unrun checks stay pending; actual results name their source, artifact and observer |

Create solution, project and workflow files only after the toolchain decisions, and only within the chosen scope. For each feature, copy `specs/feature-template/` to `specs/<feature>/`, and keep the template for the next one.

## What to keep

| Material | Purpose | After setup |
|---|---|---|
| [README](../../README.md), [AGENTS](../../AGENTS.md) | Entry point and coding-agent workflow | Customize |
| [Documentation index](../README.md) | Authority map and task routes | Keep; remove its link to this guide |
| [PRD](../PRD.md), [SDD](../SDD.md), [DESIGN](../DESIGN.md) | Behavior, technical contracts, UI standards | Customize |
| [API](../API.md) | Client–server rules | Customize if the app calls a server; otherwise delete it and the links the checker reports |
| [SETUP](../SETUP.md), [DEVELOPMENT](../DEVELOPMENT.md), [RELEASE](../RELEASE.md) | Setup, coding practices, distribution | Keep the applicable parts |
| [REFERENCES](../REFERENCES.md) | Official guidance with check dates | Recheck when adopting or upgrading |
| [TEST-PLAN](../delivery/TEST-PLAN.md), [STATUS](../delivery/STATUS.md), [evidence template](../delivery/evidence/TEMPLATE.md) | Procedures, current state, observed results | Start with no results |
| [ADR index](../adr/README.md), [ADR template](../adr/TEMPLATE.md) | Decision rationale | Start with no decisions |
| [Feature template](../../specs/feature-template/spec.md) | Scenario, plan and task structure | Keep as the source for copies |
| [Tools](../../tools/README.md) | Documentation checker and its tests | Keep |
| This folder: guide, [STRUCTURE](STRUCTURE.md), [PROMPT](PROMPT.md) | Setup help | Delete after recording the chosen layout in SDD |

Also included: `.gitignore` (the standard `dotnet new gitignore` output plus mobile signing files) and `.gitattributes` (consistent line endings).

## Adoption checklist

- [ ] Product decisions and exclusions are explicit; unresolved choices have owners.
- [ ] Only intended platforms are targeted; required build hosts and test devices are available.
- [ ] SDK, workload set, platform tools and dependencies are recorded and reproducible.
- [ ] If the app calls an API: API.md decisions are explicit, the OpenAPI document is committed, and local addresses are recorded in SETUP.
- [ ] Placeholder and example scenario IDs are adapted; none are marked accepted.
- [ ] This folder is deleted; the normal check and `--ready` both pass.
- [ ] A first vertical slice has meaningful automated coverage and an actual platform build.
- [ ] Required native checks and signing/distribution acceptance are tracked separately.

The suggested baseline is two production projects and two test projects, plus the API projects if there is a server. A one-screen app may start smaller; the responsibility boundaries still apply. Persistence, a server API, reminders, authentication, sync, analytics and a currency are product choices, not kit requirements.

## Improving the template

Make kit improvements in the template repository, not in projects created from it, so later projects inherit them. After changing the checker, run `python -m unittest discover -s tools/tests -v`.
