# ADR 012: Host the site in Docker on an Oracle Cloud Infrastructure Compute VM

Status: Accepted. Decision date: 2026-11-24. Decision owner: lumabanls, by the user's instruction "im changing the deployment to releasing to oic via dockcer", read as Oracle Cloud Infrastructure (OCI). The user then chose, from the agent's options the same day: a Compute VM running Docker Compose, the Singapore region, and a free sslip.io name for the test site's address. This replaces the host part of [ADR 007](007-supabase-brevo-container-host.md) (Render); its database (Supabase) and email (Brevo) decisions stand.

## Context

ADR 007 kept the Blazor design of [ADR 005](005-web-platform-blazor.md) and proposed Render as a container host. Nothing was deployed there. The site needs a host that keeps the app running with long-lived WebSockets for live chat, one instance while the chat notifier is in-process (STATUS G18), HTTPS for sign-in cookies, and keys that survive a redeploy ([RELEASE](../RELEASE.md#deployment)).

## Decision

1. The site runs as a Docker container on one OCI Compute VM in the Singapore region (`ap-singapore-1`), beside the Supabase project. An Arm (Ampere A1) shape is assumed, built on the VM itself; the shape and its Always Free eligibility are confirmed when the VM is created.
2. Docker Compose ([deploy/oci](../../deploy/oci/compose.yaml)) runs the app and Caddy. Caddy terminates HTTPS with an automatic certificate, passes WebSockets through, and forwards the scheme. The app's port is not published, so only Caddy reaches it.
3. The image ([Dockerfile](../../Dockerfile)) builds the Tailwind styles, publishes with the SDK that `global.json` pins, and runs as a non-root user on the ASP.NET Core 11.0 runtime. A second target holds the EF migration bundle, run by hand before a new build starts (RELEASE step 3).
4. The app trusts forwarded headers (`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`), keeps data-protection keys in a Docker volume (`DataProtection:KeysPath`), and answers `/health` with the build's git commit.
5. The test site's address is the VM's public IP as an sslip.io name (for example `152-67-1-2.sslip.io`) until a domain is chosen (PRD D01).

## Alternatives and rationale

| Option | Benefits | Costs / reason chosen or rejected |
|---|---|---|
| OCI Compute VM with Docker Compose and Caddy (chosen by the user) | One instance by construction, as live chat needs; WebSockets and HTTPS handled by Caddy; no load balancer to pay for | The owner runs the VM: operating system updates, Docker, disk space and backups of the volumes |
| OCI Container Instances | Oracle runs the container | HTTPS needs an OCI load balancer in front; no data volume for keys without more set-up |
| Render (ADR 007's proposal) | Managed, documented WebSockets | Replaced by the owner's choice; its free service sleeps when idle |

## Consequences and verification

- The privacy policy's provider table names Oracle Cloud Infrastructure for hosting, in Singapore, in pending legal version `2026-11-23.2`.
- Google's OAuth client needs `https://<site address>/signin-google` among its redirect URIs, and `Email:SiteAddress` is the same address.
- Data-protection keys are stored unencrypted in the volume; they stay on the private VM. Encrypting them at rest is a later step.
- The image hasn't been built yet: Docker isn't installed on the development PC. The app's part (health, forwarded scheme, kept keys) is covered by web tests, and the published files ran locally with the container's settings ([evidence](../delivery/evidence/2026-11-24-oci-docker.md)).
- Revisit if the site needs more than one instance (G18), or when a public launch needs a domain (D01) and backups (D11).
