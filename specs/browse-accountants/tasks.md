# Browse accountants tasks

**Unexecuted.** Scope/scenarios: [spec](spec.md). Sequence/dependencies: [plan](plan.md). Record actual evidence links before completing verification tasks.

## Definition

- [x] Link governing requirements and authorization; record baseline and exclusions (spec, 2026-11-10; reworked for the web 2026-11-11).
- [ ] Review the directory and profile pages in the [design prototype](../../prototypes/README.md) (STATUS gate G9).
- [ ] Resolve blocking decisions D04 (profile fields) and D05 (search, filters, order), or agree on provisional seed fields.
- [ ] Identify source/test dependencies and meaningful checks: the plan lists them; confirm after the SDK install and the database choice.

## Implementation

- [ ] Domain model, data model with the first migration, seed data, and the directory use case.
- [ ] Directory and profile pages, with "not available" and error pages.
- [ ] Failure, not-available and address-keeps-state behavior (FT05–FT07).
- [ ] Update SDD (as built) and SETUP (database, ports, commands); add ADRs if a significant choice is made.

## Automated and build verification

- [ ] Core.Tests and Web.Tests for FT01–FT06 pass; link the result with revision and environment.
- [ ] The web project builds on this PC; record SDK version and outcome.
- [ ] Documentation links, traceability and diff are checked.

## Browser verification

- [ ] TP01, TP05 (FT07) and TP08 pass in the browsers of the agreed matrix, with synthetic data.
- [ ] TP06 accessibility and layout checks pass for the directory and profile pages.
- [ ] Restore changed settings (network, zoom, theme) and record any remaining changes.

## Acceptance

- [ ] Required gates are satisfied or exceptions explicitly decided.
- [ ] Acceptance decision is recorded with its owner (lumabanls) and scope.
- [ ] [STATUS](../../docs/delivery/STATUS.md) links the evidence and remaining limitations.

Blocked/not-applicable tasks:
- Implementation and verification are blocked until the .NET 11 SDK is installed and the solution is scaffolded ([SETUP](../../docs/SETUP.md#first-checkout)).
- Phone-browser checks are blocked until devices are recorded (STATUS gate G10).
- Owner: lumabanls.

Implementation completion alone does not complete the later sections. A checkbox must not imply a test or browser result that has not occurred.
