# Accountant profile editing implementation plan

[Spec](spec.md) owns the scenarios; [tasks](tasks.md) tracks execution. Status: as built 2026-11-16, uncommitted ([evidence](../../docs/delivery/evidence/2026-11-16-f05-profile-editing.md)).

## Affected modules and contracts

| Module / source path | Change | Contract / scenario |
|---|---|---|
| `Domain/AccountantProfile.cs` | Owner account, version, `CreateFor`, `Edit`, `ReplaceServices`, `ReplaceExperience` (at most 10); limits adopted as D04 | R05, D04, PE01–PE05 |
| `Application/Profiles/` (new) | `ProfileForm` (the form's values as text), `ProfileValidator` (field errors, normalisation), `ProfileEditorService` (load for the owner, save, listing status), `IProfileStore` | PE01–PE07 |
| `Data/ProfileStore.cs` (new), `TaxBuddyDbContext.cs`, migration `AddProfileOwner` | Load and save by owner, with an optimistic version check; owner foreign key to `users` (cascade), unique owner index, version as a concurrency token | PE01, PE05, PE07 |
| `Components/Pages/ProfileEditorPage.razor` (new) | Static `/account/profile`, `[Authorize]`; a named form posted to the same page; Save, add row and remove row are submit buttons with a `command` value; redirect after a successful save | SDD render modes, PE01–PE08 |
| `Components/Layout/MainLayout.razor`, `Pages/RegisterPage.razor` | "My profile" in the navigation for accountants; registration as an accountant from `/` lands on the editor | PE01 |
| `Program.cs`, `Account/AccountSetup.cs` | Register the services; authorization for the page | PE06 |
| Tests | Rule tests for the validator and domain; web tests for the flows; shared form helpers moved to `Support` | All |
| 2026-11-25 (F02 BD16): `Application/Directory/DirectoryService.cs`, `DirectoryModels.cs`, `Data/DirectoryStore.cs`, `Components/Shared/DeskTitle.razor`, the editor page, `Styles/app.css` | The card beside the form: one listed card read the way directory pages read theirs (a shared projection); a side panel from 860 px and a disclosure below it, both drawn by the server | PE09 |

## Dependencies and approach

No new packages.

**Owner from the session only:** the page reads the account ID from the session cookie (`NameIdentifier`) and the store loads by that ID. The form carries only the version, so no other ID is ever accepted.

**Validation before the domain:** the validator turns text into checked values and collects one message per field. The domain's own checks stay as a backstop and should never be hit through the form.

**Rules the validator applies:**
- Empty optional fields become null.
- A website without `http://` or `https://` gets `https://` added and must then be a valid web address.
- Email must be a single plain address.
- Phone allows digits, spaces and `+ - ( ) .`.
- An experience row left completely empty is dropped. A partly filled row needs a role, a company and a start month; the end is a month or "current", and not before the start.

**Version check:** the form posts the version it loaded. The store sets that as the original concurrency value, so PostgreSQL refuses the update if another save happened in between. A refused save is reported as a conflict: the page keeps the user's input and takes the newer version number, so a second Save overwrites deliberately.

**Data:** an additive migration. `owner_account_id` (uuid, null for seeds) with a unique index and `fk_accountant_profiles_owner_account_id` → `users(id)` on delete cascade; `version` integer, default 0. Rollback: `dotnet ef database update AddAccounts`.

## Verification

1. Rule tests for the validator: lengths, formats, the month rules, the row rules, the 10-entry cap, normalisation. Domain tests for the replace methods.
2. Web tests on the development database: PE01–PE07, using F04's test sign-in to register an accountant or a client.
3. Release build; the full suite; apply the migration. Browser check: the real app runs as usual, and a temporary, uncommitted test host on another port provides the test sign-in against the development database. A browser shares cookies across ports on `localhost`, and both hosts share the data-protection keys, so the session works in the real, styled app. Synthetic accounts are deleted afterwards.
4. Evidence record; PRD D04 and R05, SDD render modes and C04, DESIGN, STATUS and the docs index.

## Risks and tradeoffs

| Risk / decision | Impact | Mitigation / owner |
|---|---|---|
| Static form: add and remove a row redraws the page | A small jump in scroll position on each add or remove | The page returns to the experience section through an anchor; owner walkthrough |
| Replacing the service and experience rows on each save | Rows are deleted and re-inserted; the unique (profile, service) index has to accept a delete and re-insert in one save | Covered by the edit test on PostgreSQL |
| Seeds have no owner | Seeds can't be edited from the site | Intended: they are synthetic development data |
