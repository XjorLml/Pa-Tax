# F06: Profile pictures

Governing requirements: new [PRD](../../docs/PRD.md) R24 (added 2026-11-16), with R11 (privacy), R03 and R05. Decision: [ADR 011](../../docs/adr/011-profile-pictures-supabase-storage.md). Contracts: [SDD](../../docs/SDD.md), [DESIGN](../../docs/DESIGN.md). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

Every signed-in user can add, change and remove their own profile picture. When there is none, the default avatar is their initials on one of the prototype's three colour tones, as today.

**Authorization and decisions** (all on 2026-11-16):
- The user's instruction: "User should able to edit their profile picture and when no picture povided make a default user avatar, plan for changes to the current plan and implement then up date the docs".
- The user chose:
  - everyone can have a picture;
  - files are kept in **Supabase Storage**;
  - uploads are **re-encoded with SkiaSharp**;
  - the default is initials on a colour tone.

This reverses F05's "profile photos" exclusion and the SDD's "Supabase Storage not used".

**Who sees a picture:**
- An accountant's picture is public: header, directory card, public profile, Account.
- A client's picture is shown to that client (header, Account) and, since F07 (2026-11-16), to the accountants they have a conversation with, in the chat list.

**In scope:**
- Upload and remove from the Account page; a "Change picture" link from the profile editor. Since 2026-11-19 both start from a pencil on the avatar ([PP09](#pencil-on-the-picture-2026-11-19-pp09)).
- Server-side processing: a real image only; at most 5 MB uploaded and 40 megapixels decoded; cropped to a centred square; 400 × 400 JPEG; all metadata (location, camera) removed.
- A private bucket, with pictures served by the site under these access rules.
- The avatar everywhere it appears today.
- Running without Storage configured.

**Exclusions:**
- Cropping or rotating in the browser (script).
- Pictures in chat (R06 not built). Superseded 2026-11-16: [F07](../chat/spec.md) shows them in the conversation list.
- Moderation beyond reporting (R15 not built).
- Pictures for the synthetic seed profiles.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| PP01 / R24 | A signed-in user uploads a JPEG, PNG or WebP photo on Account | "Picture updated"; the header and Account show it straight away; an accountant's card and public profile show it too | Web.Tests (storage fake), browser |
| PP02 / R24, R11 | The upload is a phone photo with location (EXIF) data, or is not square | The stored picture is 400 × 400 JPEG with no metadata, cropped to the centre | Rule tests on the processor |
| PP03 / R24 | The file is not an image, is over 5 MB, is too large in pixels, or no file was chosen | A clear message on Account, the current picture is kept, and nothing is stored | Rule tests, Web.Tests |
| PP04 / R24 | The user presses "Remove picture" | The initials avatar returns everywhere; the stored file is deleted | Web.Tests |
| PP05 / R11, R24 | Someone requests a picture address | An accountant's picture: anyone. A client's picture: only that client; anyone else gets 404. Unknown or removed: 404. Responses can be cached; a new picture has a new address | Web.Tests |
| PP06 / SDD | A forged post: no antiforgery token, or signed out | 400, or sign-in first; nothing changes | Web.Tests |
| PP07 / SETUP | Storage is not configured, or is unreachable | Account says picture upload isn't set up; avatars fall back to initials; an unreachable store gives "We couldn't save your picture" and keeps the old one | Web.Tests |
| PP08 / DESIGN | Phone and desktop, light and dark | The picture controls follow the prototype's Account style; the images keep the avatar shapes and sizes; every image is decorative, since the name is always shown beside it | Browser |

## Pencil on the picture, 2026-11-19 (PP09)

The user asked "is the profile pic change implemented in the prototype? improve the design and move it in the profile picture pencil recommend simple design for changing and updating profile picture".

- **Finding:** the prototype had no picture upload; the site's "Profile picture" card was a recorded extension of it.
- **Design:** a pencil badge on the avatar opens a small "Profile picture" panel: one file picker, "Upload picture", the hint, "Remove picture" when there is one, and "Close". Account otherwise shows only the name card and the menu.
- **Prototype:** added first, as the design origin: the pencil and panel on Account, the same pencil on the editor's About you avatar, the picture in the Account card and the site bar, and four checks in the prototype checker. The picture is read in the browser and kept only in the page.
- **Site:** Account and the editor follow it. The pencil is a plain link to `#picture` and the panel shows while it is the page's target, so no script is needed. The forms, addresses, messages and server rules are unchanged.
- **Kept:** the wording of the controls and messages, so PP01 to PP08 hold as written.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| PP09 / R24, DESIGN | A signed-in user opens Account, or an accountant opens the profile editor | The avatar has a pencil labelled "Change profile picture". On Account the picture panel is closed until the pencil is used, and is open after an upload or removal to show the result; the editor's pencil opens Account with the panel showing. With script off it still works | Web.Tests, prototype checker, rendered pages |

## Picture design options, 2026-11-19

**Decided 2026-11-19: the user said "implement option e"** ([PP10](#pick-preview-save-2026-11-19-pp10)). A remains what the site shows without script; B, C and D were not chosen and stay in the prototype for comparison. Before that decision these were proposals. The user asked where the design recommendations were, then said "put the choices in the prototype so i can clearly visualize". The prototype's new "Picture design" control (URL setting `picture`) switches how the pencil behaves on Account:

| Option | After the pencil | Trade-off |
|---|---|---|
| A · Inline panel (current, [PP09](#pencil-on-the-picture-2026-11-19-pp09)) | A panel under the name card: choose, "Upload picture", "Remove picture", "Close" | Works without script. Two steps to upload; no preview |
| B · One-tap upload | The device's file chooser opens; the picture is saved as soon as one is chosen. "Remove picture" is a link under the name | Fastest. Needs script; a wrong choice is saved at once |
| C · Small menu | A menu on the avatar: "Upload new photo", "Remove photo"; the upload saves at once | Familiar. Needs script |
| D · Separate page | A "Profile picture" page at `/account/picture`: a large preview, the file picker, "Save picture", "Remove picture" | Works without script and is roomy on phones. Leaves Account |
| E · Pick, preview, save (the agent's recommendation) | The file chooser opens; the chosen photo shows in the avatar as a preview with "Save" and "Cancel". "Remove picture" is a link under the name | Shows the result before it is stored. Needs script, so the site would keep A as the fallback without script |

All five use the same checks and messages as PP03.

## Pick, preview, save, 2026-11-19 (PP10)

The user said "implement option e".

- **With script:** on Account the pencil opens the device's file chooser. The chosen photo is shown in the avatar as a preview, with "Preview. Not saved yet.", "Save" and "Cancel" under the name. Save posts the page's upload form; Cancel puts the avatar back. With a picture, "Remove picture" is a link under the name.
- **Checks before the preview:** a file that isn't JPEG, PNG or WebP, or is over 5 MB, is refused in the page with the server's wording. The server still checks everything (PP03).
- **Results:** a banner under the name card, for both paths.
- **Without script:** PP09's panel, unchanged: the pencil's link shows it, and the redirect after a post shows it again with the result above.
- **How:** `wwwroot/TaxBuddy.Web.lib.module.js`, loaded by `blazor.web.js`. Save and Remove are ordinary submit buttons tied to the page's two forms, so there is no new address, no background request and no change to the endpoints, data or storage.
- **Prototype:** E is now the default of the "Picture design" control.

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| PP10 / R24, DESIGN | A signed-in user presses the pencil on Account and chooses a photo | The photo is previewed in the avatar and nothing is stored; Save stores it and shows "Picture updated."; Cancel restores the avatar; a wrong type or a file over 5 MB shows the reason and no preview. With script off, the panel of PP09 does the job | Web.Tests for the markup; the script run in Edge against the rendered page; prototype checker |

## State, dependencies, and unresolved decisions

**Data:** the account gets the name of its current picture object (null for none). Each upload stores a new object, `users/{account id}/{random}.jpg`; after the database update the old object is deleted, so a failed upload never leaves the account pointing at nothing.

**Dependencies:**
- Supabase project URL and a server-side secret key in user-secrets.
- A private bucket `profile-pictures`. The owner creates the key; the agent can create the bucket once the key is stored (see plan).
- `SkiaSharp` with Linux native assets.

**Open, not blocking:**
- Whether accountants' pictures should be checked before publication. D22 (report handling) applies once R15 exists.
- Retention of deleted pictures: deleted immediately (Proposed).

## Evidence limits

Web tests use an in-memory store in place of Supabase. A real round trip to Supabase Storage is checked once the owner stores the key. Results belong in [tasks](tasks.md) and [STATUS](../../docs/delivery/STATUS.md).
