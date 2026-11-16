# F05: Accountant profile editing

Amended 2026-11-16 by [F02 BD10](../brand-design-refresh/spec.md#application-aligned-to-the-prototype-2026-11-16-bd10): the editor's layout, wording and fields now follow the prototype's `editorParts`. Services are rows with a dropdown, the contact email starts empty, there's a listing checklist, and the messages and the title "Create your profile" come from the prototype. The behaviour in the scenarios below is unchanged; quoted texts are as first built.

Governing requirements: [PRD](../../docs/PRD.md) R05, with BR01 (listing), D04 (fields and limits) and R11 (contact consent). Contracts: [SDD](../../docs/SDD.md) render modes, navigation and errors, and [DESIGN](../../docs/DESIGN.md) states. Builds on F01 (profiles, directory) and F04 (accounts). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

An accountant who has registered (F04) creates and edits their own profile. Once the required fields are complete, it appears in the directory straight away, with no approval step. Nobody else can change it.

**Existing behaviour** (F04, uncommitted, on `feature/f02-f04-theme-switch-google-sign-in`): an accountant account has no profile. The directory shows only the synthetic seed profiles, which belong to no account.

**Authorization and decisions** (all on 2026-11-16):
- The user instructed "implement R05, accountant profile editing (it would be F05)".
- C04 for this page: a **static form**, not an interactive page. The editor is a form posted to its own page; a failed or invalid save shows it again with everything typed. No live connection, so nothing is lost when one drops. The SDD render-mode row changes accordingly.
- D04 limits: **keep F01's working values** (name 100, title 100, city 80, summary 1000, email 254, phone 30, website 200, service description 200, experience role and company 100 characters each), plus at most **10 experience entries**.

**In scope:**
- `/account/profile` for the signed-in accountant: name, title, city, summary, email, phone, website, consent to show contact details, services from the fixed list (each with a short description), and experience entries (role, company, start month, end month or "current").
- Saving an unfinished profile, which stays unlisted, with a clear list of what's still missing.
- The listing status, and a link to the public profile once listed.
- Field-level validation, with all input kept.
- Protection against a stale save from another tab.
- "My profile" in the header for accountants. An accountant registering from the home page lands on the editor.

**Exclusions:** portfolio highlights (R12), account deletion (R09), profile photos, the order of services (they follow the catalog order), an "edit" link on the public profile page, and a preview before saving.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| PE01 / R05, BR01 | A newly registered accountant opens My profile, fills in every required field and saves | Saved and listed: "Profile saved", "Listed in the directory", a link to the public profile. The profile appears in the directory and at its address, with the services, experience and (by consent) contact details | Web.Tests, browser |
| PE02 / R05, BR01 | They save with required fields missing (for example no services, or no consent) | Saved but not listed. The page names exactly what's missing; the directory and the profile address don't show it | Web.Tests |
| PE03 / D04 | Too-long text, an invalid email, website or month, an end before the start, a half-filled experience row, more than 10 entries | Nothing is saved; an error summary plus an error beside each field; everything typed is kept | Rule tests, Web.Tests |
| PE04 / R05 | The accountant edits a saved profile: changes text, removes a service, adds and removes experience rows | The form opens with the saved values. Add and remove only redraw the form, with nothing saved until Save; after Save the public profile shows the new values | Web.Tests |
| PE05 / SDD errors | The same profile is saved from two tabs, the older tab last | The older tab's save is refused: "This profile was changed in another tab or window". Its edits stay in the form for review; saving again replaces the other version | Web.Tests |
| PE06 / R05, BR15 | A visitor, a client, or an accountant posting someone else's data | Visitor: sign-in, then back to the editor. Client: "Only accountant accounts have a profile", with no form. The profile edited is always the signed-in account's own; no ID in the address or form is trusted. A missing antiforgery token gets 400 | Web.Tests |
| PE07 / SDD errors | The database is unavailable when the page loads or saves | "We couldn't load / save your profile" with a retry; on save, the input is kept; nothing partial is stored | Web.Tests (unreachable database) |
| PE08 / DESIGN | Desktop and phone, light and dark, keyboard | Labelled fields, visible focus, errors announced, no horizontal overflow; Enter in a text field saves rather than removing a row | Browser check |

## State, dependencies, and unresolved decisions

**Data:** the profile gains its owner account (unique; deleted with the account, which BR06 requires once R09 exists) and a version number for stale-save detection. The seed profiles keep no owner. This is an additive migration.

**Recorded deviation from DESIGN and the SDD:** there's no confirmation when leaving with unsaved changes, and Save isn't disabled while it runs, because both need script. The version check refuses a repeated submit (409). Recorded in DESIGN and the SDD navigation table on 2026-11-16.

**States:** no profile yet; saved but not listed (missing fields named); listed. Saving never auto-lists anything else, and unlisting happens simply by removing a required field or the consent.

**Still open, not blocking:** D23 (portfolio highlights, R12); D07 and R09 (deletion); D19 (indexing). Before real users: the legal text (G13). The other interactive-page cases of C04 (chat, reviews, F03) remain open.

## Evidence limits

Web tests use the F04 test sign-in. A real accountant editing in the browser is covered by the owner's walkthrough (G9). Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
