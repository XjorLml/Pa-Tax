# Prototype: My profile with the preview beside the form (F02 BD16): execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-25, about 19:00–20:00 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user said: "in the My profile implement Option B: Preview beside the form in the prototype".
- Requirement and scenario: [F02 BD16](../../../specs/brand-design-refresh/spec.md#my-profile-preview-beside-the-form-2026-11-25-bd16); R05, BR01 and R01 (the directory card). The option was proposed on the [options page](../../../prototypes/README.md#design-options-for-the-accountants-flow) that morning ([evidence](2026-11-25-prototype-accountant-options.md)).
- Source revision: uncommitted work on `06e9be6`, branch `feature/f02-f04-theme-switch-google-sign-in`, after the F17 work of the same day ([evidence](2026-11-25-f17-client-details.md)).
- Artifact (SHA-256, first 12 characters), as checked:

  | File | Hash |
  |---|---|
  | `prototypes/index.html` | `e0e820abe4c8` |
  | `prototypes/options.html` | `6d7767ae01f1` |
  | `prototypes/taxbuddy.css` | `a6acf11f5d0a` |
  | `tools/check_prototype.mjs` | `da4fa7caa126` |
  | Click-check script, in the session's scratch folder, outside the repository | `153077322f7a` |

- Environment: Windows Edge 158.0.4370.33, headless, from WSL2; Windows Node 22.12.0 for the prototype checker and the click checks.

## What changed

- **The prototype's default:** `S.opt.editor` is `'b'`. Without any `opt`, My profile shows:
  - on wide desktop pages, the directory card as last saved beside the form;
  - on phones and on desktop pages narrower than 860 px, a closed disclosure above the form.
- **The old form is still viewable:** `opt=editor-current` shows the one long form the site has. The address reader now accepts `current` for any page.
- **Class names:**
  - the chosen design has semantic names: `.editor-wide`, `.editor-grid`, `.editor-preview`, `.editor-preview-h` and `.editor-preview-mini`, in place of `opt-ed-*`;
  - option A and client details C now use the shared layout classes. Nothing changed in how they look.
- **Wording:**
  - the not-listed text names both places the card will appear;
  - the panel is labelled by its heading;
  - the guide panel's R05 note describes the preview.
- **`options.html`:**
  - B is "Current (chosen 2026-11-25)";
  - "Before · One long form (the site today)" frames `opt=editor-current`;
  - A and C say "not chosen", and so do the details page's not-chosen options.
- **The site:** unchanged. DESIGN and F05 record that the prototype is ahead of it.

## Execution

| Procedure | Expected result | Actual observation | Outcome |
|---|---|---|---|
| `node tools/check_prototype.mjs` on a copy of `prototypes/` in the Windows temp folder | All pass | `PASS: 224 prototype checks` (212 before, 12 new); exit 0 | Pass |
| The new checks:<br>• a listed accountant on a 1440 px desktop page;<br>• the city changed, then Save;<br>• the page at 700 px in dark;<br>• a new accountant on a 390 px phone;<br>• `opt=editor-current` | • The card is right of the form, and the disclosure is hidden.<br>• The card keeps the old city while typing, and shows the new one after Save, with the saved line.<br>• At 700 px, a closed disclosure above the grid, and the side panel hidden.<br>• On the phone, the disclosure opens to "Not listed yet" with no card; its summary is at least 44 px tall.<br>• `opt=editor-current` shows no preview.<br>No horizontal overflow on any of the three pages | As expected | Pass |
| Click checks for the options page and the options (scratch script, updated) | All pass | 43 of 43, 0 script errors. They include the old checks for options A and C, and these new ones:<br>• the default editor shows the card;<br>• `opt=editor-current` shows no preview;<br>• on the options page, Current is the chosen preview with no `opt`, Before frames `opt=editor-current`, A says "not chosen", and Compare has four frames.<br>One old check was out of date: it looked for "Needs a scope decision (C05)", which the F17 work had replaced with "PRD R25 since 2026-11-25". It now looks for the new text | Pass |
| Screenshots in headless Edge, viewed:<br>• desktop 1280 px, light and dark;<br>• a new accountant;<br>• 700 px;<br>• a 390 px phone;<br>• options A and client details C, at 1280 and 700 px;<br>• the options page's Before | Matches the option as proposed; A and C unchanged | As expected. The card's panel is 300 px wide beside a 1000 px page. Client details C at 700 px still puts its preview under the form, as before | Pass |

## Restoration and limits

- **Cleanup:**
  - the Windows temp copy and Edge's profile were removed;
  - the screenshots stay in the session's scratch folder, outside the repository.
- **Not run:**
  - a page-by-page comparison with the previous uncommitted prototype. The change is limited to `editorParts`, client details C's layout classes, the options page and the address reader, and the other 212 checks still pass;
  - real phones, screen readers, keyboard-only use, or contrast calculations (the panel reuses existing tokens);
  - anything on the site, which doesn't have this design.
