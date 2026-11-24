# Deployment on OCI plan

Scenarios: [spec](spec.md). Tracking: [tasks](tasks.md).

## Modules

| Path | Change | Scenarios |
|---|---|---|
| `src/TaxBuddy.Web/Services/DeploymentSetup.cs`, `Program.cs` | Kept keys when `DataProtection:KeysPath` is set; `GET /health` with `Site:Version` | DP01, DP03 |
| `Dockerfile`, `.dockerignore` | Styles (Node 22), publish and migration bundle (SDK 11.0.100, `dotnet-ef` from the tool manifest), `migrations` and `runtime` targets on `aspnet:11.0.0`, non-root, port 8080, `ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`, `/keys` | DP02, DP04–DP06 |
| `deploy/oci/compose.yaml`, `Caddyfile`, `env.example`, `env.web.example`; `.gitignore` | Web and Caddy services, the migrations profile, volumes for keys and certificates; the real `.env` files ignored | DP06, DP07 |
| `tests/TaxBuddy.Web.Tests/DeploymentTests.cs`; the factory's `settings` | DP01–DP03 without a database | DP01–DP03 |
| ADR 012, ADR 007, PRD, SDD, RELEASE, SETUP, privacy text, STATUS | The decision and the steps | All |

## Approach

Build on the VM, so the image matches its Arm processor without cross-building. Pin images to the versions the project already pins. Forwarded headers through ASP.NET Core's own container setting rather than code, because only Caddy can reach the app's port. Database changes stay a separate, manual step before each release.

## Verification

The web tests and the full suite; the published files run locally with the container's settings; the migration bundle against the development database. The image build and the deployment wait for Docker on the VM.
