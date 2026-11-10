# Reusable setup prompt

Give the text below to a coding assistant in a repository created from this template. Replace the inputs you know; unresolved decisions must remain explicit.

```text
Set up a practical .NET MAUI project. This repository was created from a
documentation starter kit; its guide is docs/starter-kit/README.md.

Project name: {{ProjectName}}
Product purpose and users: {{PurposeAndUsers}}
First release workflows: {{InitialWorkflows}}
Target platforms: {{PlatformsOrUndecided}}
Available development/build hosts: {{Hosts}}
Storage/offline/network needs: {{DataNeedsOrUndecided}}
Server: {{NoServerOwnAspNetCoreApiOrThirdPartyApi}}
Languages, currency, date/timezone policy: {{LocalePolicyOrUndecided}}
Distribution channel: {{DistributionOrUndecided}}
Requested scope: {{DocumentationOnlyOrScaffoldAndFirstSlice}}

Follow AGENTS.md. Check git status and preserve existing changes. Read the
starter kit guide, its structure proposal and the templates.

Customize the documentation index, PRD, SDD, DESIGN, SETUP, DEVELOPMENT,
RELEASE, delivery status/test plan/evidence template, ADR index, root README
and AGENTS.md. Copy specs/feature-template/ for the first feature and keep the
template. With a server, also customize docs/API.md and follow the structure's
client-server baseline; without one, delete docs/API.md and the links that the
checker then reports. Keep relative links valid. Record unknowns and ask
concise questions for decisions that block implementation; continue work that
does not depend on those answers.

Use a small architecture first. Separate platform-independent rules from MAUI
and native adapters. Explain any extra project/layer using a concrete need.
Declare supported platforms explicitly. Recheck current official MAUI support,
SDK/workload compatibility, native tools, and publishing guidance; cite sources
and record exact chosen versions. Do not reuse another app's identifiers,
business rules, branding, credentials, historical evidence, or approval status.

Define observable scenarios for one vertical slice, including failure/retry,
state retention, persistence if relevant, and required native behavior. Make
implementation, automated verification, platform builds, device verification,
and acceptance independently visible. Do not mark unrun checks as passed.

If documentation-only scope was selected, validate links, consistency, and the
diff with `python tools/check_docs.py`. Resolve placeholders honestly or remove
inapplicable examples; a structural pass is not product approval. Do not
generate an application merely to validate the documentation.
If scaffolding/implementation was selected, create the selected solution
structure, pin the toolchain, implement the authorized slice, and run meaningful
checks on available hosts. Record unavailable platform checks as pending.
Prepare distribution instructions without publishing unless authorized.

When setup is done, record the chosen layout in SDD, delete docs/starter-kit/,
remove the links the checker reports, and run `python tools/check_docs.py --ready`.

Finish with created files, architecture choices and tradeoffs, validation
actually performed, remaining decisions/acceptance, and next development steps.
```
