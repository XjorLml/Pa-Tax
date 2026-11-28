# Prototype: client details (a proposal outside the PRD): execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-25, about 13:30–14:25 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user said: "on the client side i want users to store and edit thier basic information and their profile information suggest design recommendations and show it the prototype". The user then chose:
  - the fields: basic (name, phone, city) and business (name, type, industry);
  - who sees them: the accountants the client contacts;
  - the designs: A, B and C;
  - where accountants see them: the client page and the service request.
- Requirement and scenario: **none yet.** The [PRD](../../PRD.md) has no client details. Today a client has the Google name and email and an optional picture (R04, R24), and the [privacy policy](../../legal/privacy-policy.md) lists only those. Related: R11 (clients are never public), R24 (the client's picture is shown to the accountants they chat with) and D21 (reviews show the first name and last initial). The [prototype README](../../../prototypes/README.md#client-details) describes the proposal.
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`. It includes the uncommitted accountant options of the same morning ([evidence](2026-11-25-prototype-accountant-options.md)). `06e9be6` is the baseline for the unchanged-default check.
- Artifact (SHA-256, first 12 characters), as checked:

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `cf8d2c055c1e` |
  | `prototypes/options.html` | `b960b7e79099` |
  | `prototypes/service-workspace.js` | `fe0aae857277` |
  | `prototypes/taxbuddy.css` | `88304b837266` |

- Environment: WSL2 with Node 24.16.0 for the syntax checks; Windows Edge 158.0.4370.33, headless; Windows Node 22.12.0 for the click checks and the prototype checker.

## What was built

All of it appears only with `opt=details-a`, `-b` or `-c`. Without that, the prototype shows the current design.

- **The form** (A, B and C):
  - About you: name (required), phone and city (optional), and the email shown but not editable;
  - Your business: name, type (a menu) and industry, all optional;
  - a note not to add a TIN or ID numbers;
  - a line naming the accountants who can see the details.
  - Saving checks the lengths, the phone's characters and the type. A failed save keeps everything typed.
- **The three designs:**
  - A: a "My details" row on Account opens `/account/details`.
  - B: a "Your details" card on Account, edited in place at `/account?edit=details`.
  - C: the page from A with a preview of the card accountants see.
- **The accountant's side:** an "About Maria" card at the top of the accountant's page for a client, and inside the request panel of a service awaiting acceptance.
- **`options.html`:** the client's flow has a fifth step, My details. The notes for each option mark it "Needs a scope decision" and link to the accountant's side.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `node --check` on `service-workspace.js`, and on the inline scripts of `index.html` and `options.html` | No syntax errors | No errors | Pass |
| Screenshots, Edge headless, light theme:<br>• A, the page and Account's new row;<br>• B, the card and the form in place;<br>• C, the page with its preview;<br>• the accountant's page for Maria and Jose's request.<br>Desktop frame 1120 × 860; A, B and C also in the phone frame | Each draws as described | All drew. Fixed: in B's edit card, the sharing note had no space below it, because a card rule cleared its margin | Pass after fix |
| 33 click checks with Windows Node and Edge over the debugging protocol, on a copy of `prototypes/` in the Windows temp folder (script outside the repository): the 21 from the accountant-options record, plus 12 new | All pass | 33 of 33, 0 script errors. **A:** the row opens the page; a new city saves, and then shows on the accountant's page for Maria after switching to the accountant; an empty name is refused and focused; an offline save fails and keeps the typed city. **B:** Edit details opens the form in place, with `edit=details` in the address; a phone with letters is refused; a valid one saves and shows on the card. **C:** the preview shows "About Maria". **Accountant:** Jose's request shows "About Jose". **Options page:** five client steps, and A's notes have the scope line and two accountant links | Pass |
| The default design unchanged: the rendered `#app` of 14 pages with no option chosen, compared with `06e9be6`, times masked. Account for a client (desktop and phone), an accountant and a visitor; the accountant's page for Maria; Jose's request (desktop and phone); and other client and accountant pages | Identical | 14 of 14 identical | Pass |
| `node tools/check_prototype.mjs "<Edge>"`, Windows Node 22.12.0, on the same copy | All pass | `PASS: 202 prototype checks; Edg/158.0.4370.33` | Pass |

## Restoration and limits

- **Cleanup:** the Windows temp copy, the click script's browser profile and Edge's screenshot profile were removed. The screenshots stay in the session's scratch folder, outside the repository.
- **Not in scope yet:** nothing authorizes building this on the site. That needs a PRD requirement, a privacy-policy update for the new personal data (phone, city, business details), and a data and migration plan. The prototype shows the design only.
- **Not run:** real phones, screen readers, a keyboard-only walk-through, contrast calculations for the new styles (they reuse existing token pairs), or the dark theme for these screens.
- **Nothing adopted:** DESIGN, the specs, the PRD and the site are unchanged.

## Follow-up the same day: Edit on Account is the prototype's default

About 14:30–14:55. The user said "implement the edit on account in the prototype": design B.

**What changed:**
- **Default:** B is the prototype's default, with no `opt` needed. A and C stay on the options page, marked "not chosen".
- **Focus:** Edit details, Save details and Cancel move focus to the card's heading.
- **Saved line:** after a save, a line on the card says so, and it is announced. It goes once the client leaves Account.
- **Fresh start:** Edit starts again from what was saved.
- **Notes panel and Jump to:** "Client details" was added to both.
- **Prototype checker:** 10 new checks cover the card.
- **Recorded:** [F02 BD15](../../../specs/brand-design-refresh/spec.md#client-details-on-account-2026-11-25-bd15), conflict [C05](../../README.md#history-and-unresolved-conflicts) (no PRD requirement or privacy notice), and DESIGN's Account row, which says the site leaves the card out.

| File | Hash (SHA-256, first 12) |
|---|---|
| `prototypes/index.html` | `2038c0092b45` |
| `prototypes/options.html` | `7dbfca0671e6` |
| `prototypes/service-workspace.js` | `fe0aae857277` |
| `prototypes/taxbuddy.css` | `015b84a11d99` |
| `tools/check_prototype.mjs` | `5acfdd1a17ed` |

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `node --check` on the three scripts and the checker | No syntax errors | No errors | Pass |
| The 38 click checks on a Windows temp copy: the 33 above, with B now run as the default, and 5 new | All pass | 38 of 38, 0 script errors. **Edit details:** opens the form in place, focus on "Edit your details", `edit=details` in the address. **Cancel:** keeps Pasig City, focus on "Your details". **Edit again:** starts from what was saved. **Phone with letters:** refused and focused. **Valid save:** the card has the new phone, a saved line, and focus on "Your details". **Leaving Account:** clears the line. **Accountant:** the page for Maria shows "About Maria" without an option set | Pass |
| `node tools/check_prototype.mjs "<Edge>"`, Windows Node 22.12.0 | All pass, including the new checks | `PASS: 212 prototype checks; Edg/158.0.4370.33` (10 new: card, form, refused phone, save, layout on desktop and a 390 px phone, touch targets, the accountant's card) | Pass |
| The default pages compared with `06e9be6` (rendered `#app`, times masked) | Only the intended pages change | 4 changed: Account for a client (desktop and phone), the accountant's page for Maria, and Jose's request. 12 unchanged: Account for an accountant and a visitor, other service, workspace, chat, editor, directory and profile pages | Pass |
| Screenshots: the card in the dark theme on desktop, and the form on a phone, dark | Draws correctly | Fixed: the empty status line left blank space at the bottom of the card. It now appears only after a save | Pass after fix |

**Cleanup:** the Windows temp copy, its artifacts and Edge's profiles were removed. **Not run:** real phones, screen readers, true zoom, or contrast calculations. **Not built on the site,** because of C05.
