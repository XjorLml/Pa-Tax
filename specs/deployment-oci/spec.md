# F16: Deployment in Docker on an OCI Compute VM

Governing decisions: [ADR 012](../../docs/adr/012-oci-compute-docker-host.md) (2026-11-24), PRD [D11](../../docs/PRD.md#open-decisions), the [SDD](../../docs/SDD.md#errors-diagnostics-and-security) keys, proxy and health rules, and [RELEASE](../../docs/RELEASE.md#deployment). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

Status: files and app changes written and committed 2026-11-24. The image hasn't been built and nothing is deployed: Docker isn't installed on the development PC, and the VM doesn't exist yet.

## Intent and scope

The user changed the release target from Render to Oracle Cloud Infrastructure via Docker, and chose a Compute VM running Docker Compose, the Singapore region, and a free sslip.io name for the test site.

In scope:
- the image ([Dockerfile](../../Dockerfile)), with styles, publish, migration bundle and a non-root runtime;
- [deploy/oci](../../deploy/oci/compose.yaml): Compose with Caddy for HTTPS, a migrations service, and settings templates;
- in the app: kept data-protection keys, trusted forwarded headers, and a `/health` address with the build's commit;
- the release steps.

Not in scope:
- rate limits on sign-in and directory search (SDD; a separate change);
- how the test site is limited to invited testers (D20);
- backups of the VM's volumes (D11);
- a domain (D01).

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| DP01 / RELEASE | `GET /health`, with the database unreachable | 200, `{"status":"ok","version":"<commit>"}` (`dev` when not set), not cached | Web test |
| DP02 / SDD, ADR 006 | Behind Caddy: a request arrives over HTTP with `X-Forwarded-Proto: https`, and Google sign-in starts | Google is given `https://<site>/signin-google`; without the container's setting, `http://` | Web test |
| DP03 / SDD keys | `DataProtection:KeysPath` is set | The key ring is written there, so it outlives the container | Web test |
| DP04 / RELEASE | The published files run with the container's settings | `/health`, `/signin` and the styles load; the key lands in the kept folder | Local run, no Docker |
| DP05 / RELEASE step 3 | The migration bundle runs against an up-to-date database | "No migrations were applied" | Local run against the development database |
| DP06 / ADR 012 | `docker compose up -d --build` on the VM | Caddy gets a certificate for the sslip.io name; the site and live chat work over HTTPS; `/health` names the commit | On the VM: **not run** |
| DP07 / TP07 | The smoke test on the deployed site | One public page, Google sign-in, one message | On the VM: **not run** |

## State, dependencies, and unresolved decisions

- The VM, its open ports, Docker, and `deploy/oci/.env` and `.env.web` (from the `*.example` templates) are the owner's to create.
- Google's OAuth client needs the site's `/signin-google` address.
- The test site's Supabase project needs both buckets.
- Keys are unencrypted at rest in the volume on the private VM. Encrypting them is a later step.

## Evidence limits

[Evidence](../../docs/delivery/evidence/2026-11-24-oci-docker.md). The Dockerfile and Compose file are unexercised until Docker runs them.
