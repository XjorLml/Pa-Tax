# Accountant profile editing tasks

Scope and scenarios: [spec](spec.md). Sequence and dependencies: [plan](plan.md). Evidence: [2026-11-16 record](../../docs/delivery/evidence/2026-11-16-f05-profile-editing.md).

## Definition

- [x] Link R05, BR01, D04, R11; record the authorization, the baseline and the exclusions (spec, 2026-11-16).
- [x] Resolve the blocking decisions with the owner: a static form for this page (C04), and F01's limits adopted as D04 plus a cap of 10 experience entries.
- [x] Define PE01–PE08 and how each is checked.

## Implementation

- [x] Domain: owner account, version, `CreateFor`, `Edit`, `ReplaceServices`, `ReplaceExperience`; the cap also enforced in `AddExperience`.
- [x] Application: `ProfileForm`, `ProfileValidator`, `ProfileEditorService`, `IProfileStore`.
- [x] Data: `ProfileStore` with an optimistic version check; migration `AddProfileOwner`, applied to the development database on 2026-11-16.
- [x] Page `/account/profile`; "My profile" in the navigation for accountants; registration as an accountant from `/` lands on the editor. The redirect uses the account's actual role, which also covers the two-tab case.
- [x] Fixed a defect found by the browser check: a post with no experience rows bound the list as null and crashed the page. Regression test added.
- [x] Update PRD D04 and R05, the SDD render modes and navigation, the C04 entry, DESIGN, STATUS and the docs index.

## Automated and build verification

- [x] 44 new tests (32 rule, 12 web); the whole suite has 148 passed, 0 failed, 0 skipped on the development database; Release build with 0 warnings.
- [x] Documentation links and traceability checked.

## Browser verification

- [x] Headless Edge 157 against the real app (`dotnet run`), signed in through a temporary test host that shares the session: 42 of 42 checks at 320–1440 px in both themes; screenshots reviewed. The synthetic accounts it created were deleted afterwards.
- [ ] With a real Google accountant account: create, save and edit a profile (owner walkthrough).
- [ ] Other browsers, phones, a screen reader and zoom (TP06). Firefox and desktop Safari show the month fields as text boxes; the "YYYY-MM" hint and the server check cover that, but it is unverified.

## Preview beside the form, 2026-11-25 (PE09)

- [x] Prototype: the user's choice of design B ([F02 BD16](../brand-design-refresh/spec.md#my-profile-preview-beside-the-form-2026-11-25-bd16)).
- [x] Site, at the user's "implement to site":
  - `DirectoryService.GetCardAsync` and `IDirectoryStore.FindCardAsync`, reading one listed card the way a directory page reads its cards;
  - the editor's side panel and disclosure, a `Class` on `DeskTitle`, and the styles in `app.css`.
- [x] Tests: a rule test for the card read; a web test for PE09 (before listing, after a save, after a refused save, and the same card as in the directory). The whole suite and the Release build: see the [evidence](../../docs/delivery/evidence/2026-11-25-f05-editor-preview.md).
- [x] Rendered pages from the test host viewed in Edge: 1280 px light and dark, 820 px, and phones at 390 and 320 px.
- [ ] **Found, not fixed (before this change):** at 320 px, a saved experience row's End month field is 27 px wider than the screen.
- [ ] **Owner:** try it on the running site with a real Google sign-in.

## Acceptance

- [ ] Owner walkthrough (G9), including the header: with "My profile" it wraps to two rows even at 1440 px.
- [ ] [STATUS](../../docs/delivery/STATUS.md) links the evidence and the limits.
