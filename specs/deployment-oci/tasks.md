# Deployment on OCI tasks

Scope: [spec](spec.md). Approach: [plan](plan.md).

- [x] Record the decision (ADR 012) and update PRD D11, SDD, RELEASE, SETUP and the privacy text's hosting provider.
- [x] Kept keys and `/health` in the app; the Dockerfile, Compose file, Caddyfile and settings templates.
- [x] DP01–DP03 web tests; DP04 and DP05 run locally ([evidence](../../docs/delivery/evidence/2026-11-24-oci-docker.md)).
- [ ] The owner creates the VM in Singapore, opens ports 80 and 443, installs Docker, and fills in `deploy/oci/.env` and `.env.web` (RELEASE).
- [ ] Add the site's `/signin-google` address to the Google OAuth client.
- [ ] DP06: first `docker compose up -d --build` on the VM; DP07: the smoke test.
- [ ] Later: rate limits (SDD), the invited-testers limit (D20), volume and database backups (D11), encrypted keys.
