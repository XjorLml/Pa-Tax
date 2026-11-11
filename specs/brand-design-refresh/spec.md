# F02: Kuwenta brand and prototype refresh

Status: authorized prototype work, 2026-11-11. The user asked to execute the preceding design brief: review documentation and prototype, improve the design within the existing stack, choose a replacement name, create reusable logo assets, and verify the result. This delegates the design choice; it does not approve product release or unresolved behavior.

Governing requirements: [PRD](../../docs/PRD.md) R01–R18, especially R01–R03, R06, R10–R14, R16–R17; D01 (name, domain remains open). Contracts: [DESIGN](../../docs/DESIGN.md), [SDD stack](../../docs/SDD.md#technology-stack). Implementation: [plan](plan.md); tracking: [tasks](tasks.md).

## Intent and scope

Baseline: commit `668a9a9e122787f2c86f6f6b4a63c398cc78d282`; clean working tree. The existing HTML/JavaScript prototype contains four visual proposals, directory, profiles, messaging, account, and information flows. No Blazor projects exist.

Apply Kuwenta as the current identity, with an original ledger/conversation logo, a coherent light/dark visual system, clearer directory hierarchy and responsive layouts. Keep the existing simulation controls and alternative looks. Work in this checkout; no publication, runtime scaffold, dependency change, migration, new API, or new product behavior. Historical evidence and ADRs retain the old name and their provenance. Other PRD decisions remain open.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| BD01 / D01 | Open the prototype and current docs | Kuwenta replaces the working identity; header, sign-in, help and simulated emails agree; domain remains an example | Source review and browser |
| BD02 / R01–R03, R12–R13 | Browse and open a profile | Search leads the directory; comparable cards show services, experience and ratings; profile contact action remains clear | Desktop/phone browser |
| BD03 / DESIGN | Switch theme and use a narrow viewport | Readable light/dark themes, visible focus, no horizontal clipping; reduced motion is respected | Browser layout and token contrast checks |
| BD04 / R04–R10, R14–R18 | Exercise existing sign-in, chat failure/retry, editor and information flows | Existing prototype interactions still work; typed text survives the simulated failures | Browser simulation, not server acceptance |
| BD05 / D01 | Open logo variants and favicon | Original SVG assets render locally without fonts or network downloads; usage and provenance documented | XML/source and browser |
| BD06 / documentation | Run documentation checks and inspect diff | Links resolve, scope remains traceable, historical observations are preserved | Documentation checker and review |

## Evidence limits

Prototype verification cannot establish server authorization, real provider sign-in, delivery, persistence, production accessibility or release acceptance. D15's product accessibility targets remain open; tested widths and contrast are implementation observations. Results belong in [STATUS](../../docs/delivery/STATUS.md).
