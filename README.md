# Tax Buddy

Tax Buddy (formerly Kuwenta, originally AccApp) is a website built with ASP.NET Core Blazor on .NET 11. People looking for an accountant browse registered accountants' profiles, register, and chat with the one they choose; accountants manage their profile and get an email when a client messages them. Private service workspaces organize client lists, checklists, shared progress and service documents.

Start with the [documentation index](docs/README.md). Follow [SETUP](docs/SETUP.md) for prerequisites and recorded commands, and [AGENTS](AGENTS.md) for the coding-agent workflow.

## Current state

Documentation and a design prototype only, as of 2026-11-11. Scope and open decisions are in the [PRD](docs/PRD.md). The first application feature is [browse-accountants](specs/browse-accountants/spec.md); the [client-services expansion](specs/client-services/spec.md) adds private lists, checklists, progress and service documents to the prototype, and the clickable [prototype](prototypes/README.md) shows the proposed pages. The Tax Buddy branding (renamed 2026-11-12) and original [logo assets](prototypes/assets/README.md) follow the [UI standards](docs/DESIGN.md). No projects exist yet: scaffolding waits for the .NET 11 SDK to be installed ([SETUP](docs/SETUP.md#first-checkout)) and for the prototype review.

See [delivery status](docs/delivery/STATUS.md) for implementation, verification, and acceptance. No application has been built or accepted. Prototype checks are recorded separately in the delivery evidence.

## Check the documentation

With Python 3.10+ (on this project's PC, through uv):

```powershell
uv run --no-project python tools/check_docs.py --ready
```

[Tools help](tools/README.md) covers errors, exits and limits.

## Contributing and deployment

Use [development standards](docs/DEVELOPMENT.md) and the [test plan](docs/delivery/TEST-PLAN.md). The deployment procedure belongs in [RELEASE](docs/RELEASE.md). License: not decided yet ([PRD D16](docs/PRD.md#open-decisions)). Asset licenses must be recorded separately where required.
