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
- Upload and remove from the Account page; a "Change picture" link from the profile editor.
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
