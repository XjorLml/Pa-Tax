# Agent workflow

Tax Buddy (formerly Kuwenta, originally AccApp): a website built as one ASP.NET Core Blazor Web App on .NET 11, connecting clients with accountants through a directory, chat and private service workspaces. As of 2026-11-13 the repository holds documentation, a design prototype and the first feature's source (F01: `src/TaxBuddy.Web`, `tests/`). Its scope comes from owner decisions recorded in the PRD and ADRs; a "Proposed" or "Open" marking means not yet approved.

Start with [docs/README.md](docs/README.md), then read only the documents relevant to the task. Inspect applicable source and tests before making implementation claims.

## Authorities

- [PRD](docs/PRD.md): approved behavior, stable requirement IDs, exclusions, acceptance outcomes.
- [SDD](docs/SDD.md): technical contracts, sign-in and authorization rules, and dependency boundaries.
- [DESIGN](docs/DESIGN.md): UI standards and explicitly scoped concepts, including the [prototype](prototypes/README.md).
- [SETUP](docs/SETUP.md): chosen toolchain and reproducible commands.
- [DEVELOPMENT](docs/DEVELOPMENT.md): implementation and review practices.
- [STATUS](docs/delivery/STATUS.md): delivery state and attributed evidence.
- [TEST-PLAN](docs/delivery/TEST-PLAN.md): procedures and acceptance gates.
- [RELEASE](docs/RELEASE.md): publishing and deployment procedure.
- [Feature specs](specs/browse-accountants/spec.md): feature scenarios, plans and tasks; copy [the template](specs/feature-template/spec.md) for new features.
- [ADR index](docs/adr/README.md): recorded decision rationale.

## Workflow

1. Check the working tree and preserve unrelated modifications and untracked work.
2. Identify governing requirement/scenario IDs. Record contradictions between requirements, documentation, and code instead of silently changing scope.
3. For an independently implementable feature, create or update `specs/<feature>/spec.md`, `plan.md`, and `tasks.md`. Reuse established specs. Small fixes may reference existing IDs directly.
4. Define observable acceptance outcomes, affected modules, dependencies, and verification before implementing.
5. Work within the user's authorization. Documentation reviews are part of the task, not automatic additional permission gates. Learner checkpoints: none agreed.
6. Follow DEVELOPMENT and verify at the relevant boundary. A test definition is not a passing result; a build is not browser or product acceptance.
7. Update affected authorities and evidence. Report changes, observed checks, limitations, and remaining acceptance.

## Documentation and evidence rules

- Keep each contract authoritative in one place. Link to it from plans and tests.
- Review PRD for behavior changes, SDD for contract changes, and DESIGN for UI-system changes.
- The site has no separate API. Record a decision before adding one or any other new client.
- Future concepts do not authorize features. Keep proposals and accepted behavior distinct.
- Record actual dates, source/artifact identity, environments, observer attribution, and expected/actual outcomes. Never transfer another project's passes or approvals.
- Use ADRs for significant choices with supported rationale. Do not invent decision dates, identities, or approval.
- Preserve historical records; mark supersession and update links when moving documents.
- For documentation-only work, validate links, anchors, traceability, preservation, and the diff. Do not run application builds merely to validate Markdown.
- Keep credentials, signing keys, personal data, and sensitive logs out of version control. Use isolated test fixtures and record cleanup where browser, device settings or data are changed.
