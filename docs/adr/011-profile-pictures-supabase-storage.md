# ADR 011: Profile pictures in a private Supabase Storage bucket, re-encoded with SkiaSharp

Status: Accepted. Decision date: 2026-11-16. Decision owner/evidence: lumabanls, who on 2026-11-16 chose "Everyone", "Supabase Storage", "Re-encode with SkiaSharp" and "Initials on a colour tone" for the new profile-picture feature ([F06](../../specs/profile-pictures/spec.md)). Amends [ADR 007](007-supabase-brevo-container-host.md) and the [SDD](../SDD.md#technology-stack) line "Not used: Supabase's … storage".

## Context

Users asked to be able to edit their profile picture (PRD R24). Pictures of accountants are public; clients' are personal. Phone photos carry location and camera data. The site already uses Supabase for PostgreSQL only. The owner preferred Supabase Storage over storing the bytes in the database.

## Decision

1. **Storage:** pictures are stored in a **private** Supabase Storage bucket, `profile-pictures`, reached only from the server through Storage's REST API. The project URL and a server-side secret key come from user-secrets in development and the host's secret store in production. The key never reaches a browser.
2. **Serving:** the site serves pictures at `/pictures/{account id}`, applying the access rules itself:
   - an accountant's picture is public;
   - a client's picture goes to that client only;
   - anything else gets 404.
   Each upload has a new object name, so responses are cacheable and a new picture has a new address. A small in-memory cache keeps Storage requests down.
3. **Processing:** every upload is decoded and re-encoded with **SkiaSharp** (MIT licence, .NET Foundation):
   - checked to be a real JPEG, PNG or WebP;
   - limited to 5 MB and 40 megapixels;
   - cropped to a centred square and resized to 400 × 400;
   - saved as a new JPEG, which leaves all metadata behind.
4. **Default avatar:** with no picture, the avatar is the person's initials on one of the prototype's three tones.

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| Private bucket served through the site (chosen) | Access rules stay in the app (R11); no storage key in the browser; works for clients' private pictures | Each picture request passes through the site; mitigated by caching |
| Public bucket, direct links | No load on the site | A client's picture would be public to anyone holding the link, and links leak |
| Pictures as bytes in PostgreSQL | No new service or key | The owner chose Supabase Storage |
| S3-compatible access keys (storage-only) through an AWS SDK | Least privilege: storage only | Adds a large dependency. The server already holds a full database credential, so a server-only secret key adds no new kind of exposure; revisit if keys are separated by role |
| No processing (store as uploaded) | No new package | Location data would be published (R11, D10); large originals; no proof the file is an image |

## Consequences and verification

- SETUP records the new secrets (`Supabase:Url`, `Supabase:SecretKey`) and the bucket. Without them, upload is switched off and avatars use initials.
- SkiaSharp's native library ships with the app (`SkiaSharp.NativeAssets.Linux.NoDependencies` for WSL and the Linux host).
- Tests use an in-memory store; the processor is tested with synthetic images, including one with injected EXIF data.
- Deleting an account (R09) must also delete its picture object.
