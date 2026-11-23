# F11: Legal text and privacy prompts

Governing requirements: [PRD](../../docs/PRD.md) R16, R05, R06, R15, with decisions D09, D10, D19 and D22. Technical contracts: [SDD](../../docs/SDD.md) (sign-in, sessions and authorization; render modes). UI: [DESIGN](../../docs/DESIGN.md#screen-and-component-states) rows "Information pages" and "Legal and privacy prompts", and the prototype screens `legalPage`, `agreeHTML`, `signinHTML`, `composerHTML`, `editorParts`, `profileBodyHTML` and the report page. Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

The site's Terms of use and Privacy policy become the owner-approved text, and the product changes decided on 2026-11-21 reach the site. A person who agreed to an older version agrees again before continuing.

**Existing behaviour** (`1565e69`):
- `/terms` and `/privacy` show a short draft outline with a "not legal text yet" note.
- Registration asks "I agree to the Terms of use and the Privacy policy" and stores `draft-2026-11-16`.
- Nobody is asked to agree again.

**Proposed behaviour**, authorized by the user on 2026-11-21 ("Proceed with the site follow up"; the decisions are listed in the [drafting notes](../../docs/legal/drafting-notes.md#decisions-made-on-2026-11-21)):
- **Legal pages:** `/terms` and `/privacy` show the text of [terms-of-use.md](../../docs/legal/terms-of-use.md) and [privacy-policy.md](../../docs/legal/privacy-policy.md), version `2026-11-21`, without the `[OPTIONAL]` blocks for unbuilt features. They use the prototype's long-document layout.
- **Registration:** the box reads "I agree to the Terms of use and have read the Privacy policy."
- **Agreeing again:** a signed-in person whose agreed version isn't current is sent to `/account/agree` before any other page.
- **Other prompts:** the chat hint, the editor's title hint, the PRC pointer on profiles, and "within 10 working days" on the report page.

**Exclusions:**
- Account deletion (R09) and the "Deleted user" label, which depends on it.
- Deleting accounts after 2 years without use (D14), which needs R08 email.
- Microsoft sign-in, notification emails and service documents. Their `[OPTIONAL]` blocks stay off the site.
- A report form for people without an account (email only).
- About and Help pages (R17).

There is no lawyer's review; the owner approved the text (D10).

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| LP01 / R16, D10 | Anyone opens `/terms` or `/privacy` | The full text, "Version 2026-11-21", a "The short version" box, a contents list and numbered sections. No draft note. Footer links are unchanged | Web test |
| LP02 / R16 | The contents list | Every link targets an `id` on the same page | Web test |
| LP03 / R16 | `/privacy` | The cookie table lists the sign-in, first sign-in, security and theme cookies | Web test |
| LP04 / R16, D10 | First sign-in, registration | The box reads "I agree to the Terms of use and have read the Privacy policy." Without the tick, "Tick the box to agree to the Terms of use." appears and nothing is created; with it, the current version is stored | Web test |
| LP05 / R16 | A signed-in person whose stored version isn't current opens any page | Redirected to `/account/agree?returnUrl=…`. `/terms`, `/privacy`, `/account/agree`, sign-in pages, static files and posts are not redirected | Web test |
| LP06 / R16 | On `/account/agree` they tick the box and press Continue | The current version and time are stored, the session is renewed, and they return to the page; later pages aren't redirected | Web test |
| LP07 / R16 | On `/account/agree` they press Continue without the tick | The error is shown, nothing is stored, and the redirect still applies. Sign out works from the page | Web test |
| LP08 / R16 | A new registration | Stores the current version and never sees `/account/agree` | Web test |
| LP09 / D10 | The chat box (thread and new chat) | A hint above the box, "Don't share TINs, tax returns or ID numbers in chat.", referenced by the text box's `aria-describedby` | Web test |
| LP10 / D10, R05 | The profile editor | The professional title has a hint against licence and ID numbers, referenced by `aria-describedby` | Web test |
| LP11 / D09 | A public profile | The note ends "…and check a CPA's licence with the PRC." | Web test |
| LP12 / D22 | The report page | "…who review them by hand within 10 working days." | Web test |
| LP13 / D22 | An account closed by the [RELEASE steps](../../docs/RELEASE.md#acting-on-a-report-d22) (`lockout_end` in the future) signs in with Google again | They land on the sign-in page with the failure message, not signed in | Web test |
| LP14 / D10 | The version constant and the legal documents | `LegalText.CurrentVersion` equals the "Version:" line of both documents | Unit test |

## State, dependencies, and unresolved decisions

- **Session claim.** The agreed version travels in the session cookie as a claim set at sign-in. Sessions issued before F11 have no such claim, so they count as not current and are asked once. Identity's security-stamp check renews claims from the database every minute (30 minutes until F12, 2026-11-23).
- **No schema change:** `users.agreed_terms_version` and `agreed_to_terms_at` exist since F04.
- **Earlier and later than "next sign-in".** The decision said "at the next sign-in". The site asks at the next page a signed-in person opens, which reaches people with long sessions too. Recorded in DESIGN.
- **Text source.** The Razor pages carry the text by hand from `docs/legal/`; LP14 ties the versions together. A change to the text needs both files and a new version.
- **Still open:** D27 for the rest of the service work, D14 for logs, reports and backups beyond the defaults stated in the policy, and the DTI-registered business name.

## Evidence limits

No result is recorded here. Execution evidence goes in [STATUS](../../docs/delivery/STATUS.md) and its linked evidence records.
