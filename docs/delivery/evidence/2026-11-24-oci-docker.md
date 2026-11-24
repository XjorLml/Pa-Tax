# F16 deployment in Docker on an OCI VM: execution evidence

## Identity and attribution

- Date/time and timezone: 2026-11-24, about 11:00–12:30 Asia/Manila (UTC+8).
- Observer: agent-observed (Claude Code). The user said "im changing the deployment to releasing to oic via dockcer". Asked which OCI service, region and address, they chose a Compute VM with Docker Compose, Singapore, and a free sslip.io name.
- Decision and scenarios: [ADR 012](../../adr/012-oci-compute-docker-host.md); PRD D11; [F16](../../../specs/deployment-oci/spec.md) DP01–DP07; RELEASE.
- Source revision: `ff4f0df` on branch `feature/f02-f04-theme-switch-google-sign-in`, plus the uncommitted Google mark change and this one.
- Artifact (SHA-256, first 12 characters):

  | File | Hash |
  |---|---|
  | `Dockerfile` (new) | `cf94d689de97` |
  | `.dockerignore` (new) | `2e6611224571` |
  | `deploy/oci/compose.yaml` (new) | `bb770a297e8f` |
  | `deploy/oci/Caddyfile` (new) | `638808f682f5` |
  | `deploy/oci/env.example` (new) | `89368cf6c5dd` |
  | `deploy/oci/env.web.example` (new) | `2572bce40294` |
  | `src/TaxBuddy.Web/Services/DeploymentSetup.cs` (new) | `abcc997065a1` |
  | `src/TaxBuddy.Web/Program.cs` | `fbb696013680` |
  | `src/TaxBuddy.Web/Components/Pages/PrivacyPage.razor` (hosting provider) | `f3d939763a06` |
  | `docs/legal/privacy-policy.md` (hosting provider) | `c199951bf6d2` |
  | `tests/TaxBuddy.Web.Tests/DeploymentTests.cs` (new) | `b93f5c231688` |
  | `tests/TaxBuddy.Web.Tests/Support/TaxBuddyAppFactory.cs` | `a61992386a74` |

- Environment: WSL2 with .NET SDK 11.0.100 and dotnet-ef 10.0.15. Docker is installed neither in WSL nor on Windows (checked: no `docker` command, no Docker Desktop).

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| `dotnet build TaxBuddy.slnx -c Release` | 0 warnings | 0 Warning(s), 0 Error(s) | Pass |
| `dotnet test --filter DeploymentTests` (DP01–DP03, no database) | 3 pass | 3 passed. Behind the proxy setting, Google got `https://localhost/signin-google`, and without it `http://` | Pass |
| `ASPNETCORE_ENVIRONMENT=Production SkipTailwind=true dotnet ef migrations bundle … --output …/efbundle` (as the image's build stage does) | A bundle without a connection string or Tailwind | "Done. Migrations Bundle: …" (47 MB) | Pass |
| The bundle with `--connection` from an environment variable, against the development database (DP05) | Nothing to apply | "No migrations were applied. The database is already up to date." The connection string wasn't printed | Pass |
| `SkipTailwind=true dotnet publish … -c Release`, then the published files run with the container's settings on port 8088 (Production, forwarded headers on, a keys folder, `Site__Version=localtest`, a dummy Google key, an unreachable database) (DP04) | Health, page and styles load; the key lands in the folder | `/health` gave `{"status":"ok","version":"localtest"}`; `/signin` 200; the stylesheet 200, 52,240 bytes; one `key-….xml` in the folder | Pass |
| The published app's log | Only expected warnings | Database connection failures and email-outbox rounds failing (the database was unreachable on purpose); "No XML encryptor configured" (keys unencrypted at rest, as ADR 012 records); "Failed to determine the https port for redirect" (the container speaks HTTP only; Caddy redirects to HTTPS) | As expected |
| `dotnet test TaxBuddy.slnx -c Release -m:1` | All pass | TaxBuddy.Tests 188 passed; TaxBuddy.Web.Tests 164 passed; 0 failed, exit code 0 (352 in all) | Pass |

Found while checking, not changed: the form-protection (antiforgery) cookie isn't marked Secure. That is ASP.NET Core's default, and was so before this change; session cookies are Secure outside development. Marking it Secure is a small hardening step for later.

## Restoration and limits

- Cleanup: the published app on port 8088 was stopped (the port no longer answers), and the scratch publish output, bundle and keys folder were deleted. The user's own site on port 5234 wasn't touched.
- Not run:
  - building the image or running Compose and Caddy (no Docker here), so the Dockerfile, `.dockerignore`, Compose file and Caddyfile are unexercised (DP06);
  - the Arm (arm64) build;
  - the deployment and smoke test (DP07; no VM yet).
- The privacy text's hosting provider is now Oracle Cloud Infrastructure, Singapore, in pending legal version `2026-11-23.2`; it still needs the owner's approval.
