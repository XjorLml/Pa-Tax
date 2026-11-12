# Local setup and toolchain

**Authority:** selected versions, build hosts, configuration, and developer commands. [SDD](SDD.md) owns browser and hosting capabilities; [RELEASE](RELEASE.md) owns deployment.

State on 2026-11-11: documentation only. The toolchain is chosen: an ASP.NET Core Blazor Web App on .NET 11 ([ADR 005](adr/005-web-platform-blazor.md)). The .NET 11 SDK is not installed on the development PC yet, so no project, `global.json` or build command exists.

## Reproducibility record

| Item | Selected value | Source / checked date |
|---|---|---|
| .NET SDK | .NET 11. The download page listed SDK 11.0.100 on 2026-11-11; record the exact version from `dotnet --version` after installing. Installed today: 8.0.307, 8.0.427, 9.0.123, 9.0.319, 9.0.320, 10.0.400, 10.0.403 | [REFERENCES](REFERENCES.md#revalidation-log); `dotnet --list-sdks`, 2026-11-11 |
| ASP.NET Core runtime | 11.0.x, installed with the SDK. Installed today: 8.0.x, 9.0.x, 10.0.11, 10.0.14 | `dotnet --list-runtimes`, 2026-11-11 |
| Target framework | `net11.0` for every project | [ADR 005](adr/005-web-platform-blazor.md) |
| Workloads | None needed. The MAUI workloads on this PC are not used by this project | — |
| IDE/extensions | VS Code 1.145.2 with C# Dev Kit 3.40.215 and the C# extension 2.160.4, which the owner asked to use. Visual Studio 2026 18.11.3 and 2022 17.14.44 are installed and not needed | `code --list-extensions` and `vswhere`, 2026-11-11 |
| Browsers for local checks | Supported: Chrome, Edge, Firefox, Brave and Safari (PRD D17). Installed on this PC: Chrome 157.0.8103.39, Edge 157.0.4341.46, Firefox 159.0.1, Brave 157.1.99.56. Safari needs an Apple device. Versions are recorded with each test run | File versions, 2026-11-11 |
| HTTPS development certificate | A valid certificate exists (expires 2027-08-26). Whether it is trusted was not checked | `dotnet dev-certs https --check`, 2026-11-11 |
| Database | Supabase (managed PostgreSQL), through EF Core and the Npgsql provider; package versions are recorded at scaffold. No Supabase project exists yet | [ADR 007](adr/007-supabase-brevo-container-host.md), 2026-11-11 |
| Email | Brevo. No account exists yet. Development sends no real email | [ADR 007](adr/007-supabase-brevo-container-host.md), 2026-11-11 |
| Container tooling | Docker is not installed on this PC. It is needed only if the chosen host cannot build the image itself | Filesystem, 2026-11-11 |
| Package sources / dependencies | nuget.org only (Proposed). Explicit versions for every package; central package management and lock files decided at scaffold | Project files, after scaffold |
| Documentation checker | Python 3.10+. On this PC, `python` and `py` are not on PATH; uv-managed CPython 3.14.4 is used through `uv run --no-project python` | Shell, 2026-11-10 |
| Source control | git 2.56.0.windows.1. GitHub CLI not installed (needed only for GitHub/CI work). No remote configured | Shell, 2026-11-10 |

The Android items recorded on 2026-11-10 (missing JDK, missing emulator, `ANDROID_SDK_ROOT` pointing to a missing folder) no longer matter for this project. They are kept in the [2026-11-10 evidence record](delivery/evidence/2026-11-10-documentation-setup.md).

Prototype refresh tooling observed on 2026-11-11: Node 24.16.0 and Python 3.14.6 are on PATH in the agent execution environment. The new dependency-free browser checker requires Node 22+ and a supplied Chromium executable ([tools guide](../tools/README.md#check-the-prototype)); it does not change the application's toolchain. Earlier inventory above is historical; installed SDKs and other browser versions were not re-inventoried in this task. Each run records the browser's own returned version.

## SDK selection

After installing the SDK, create a root `global.json` ([global.json reference](https://learn.microsoft.com/en-us/dotnet/core/tools/global-json)):
- `sdk.version`: the exact full SDK version, not `11.0.x`.
- `sdk.rollForward`: `disable` (Proposed). This means each SDK update is a deliberate, recorded change. Record a different policy here if one is chosen.
- `sdk.allowPrerelease`: `false`.

.NET 11 is an STS release supported until 2028-11-14 and gets monthly patches. Plan to take them, because a web application is exposed to the internet.

## First checkout

Host preparation still to be done on this PC:

1. Install the .NET 11 SDK with `winget install Microsoft.DotNet.SDK.11` (winget listed version 11.0.100 on 2026-11-11), or from the [download page](https://dotnet.microsoft.com/en-us/download/dotnet/11.0).
2. Run `dotnet --list-sdks` and `dotnet --version`, and record the .NET 11 values in the table above.
3. Trust the development certificate with `dotnet dev-certs https --trust` (it shows a Windows confirmation prompt).
4. Create a Supabase project for development, with synthetic data only. Store its connection string with `dotnet user-secrets`, and record its region and PostgreSQL version here.

Then, after the scaffold step creates the projects:

1. Restore and run the two test projects.
2. Build the web project.
3. Run the site locally, open it in a browser, and record the smoke result separately from release acceptance.

PowerShell commands for after scaffolding. They have not been executed, because the projects do not exist yet:

```powershell
dotnet test tests/TaxBuddy.Core.Tests/TaxBuddy.Core.Tests.csproj -c Release
dotnet test tests/TaxBuddy.Web.Tests/TaxBuddy.Web.Tests.csproj -c Release
dotnet build src/TaxBuddy.Web/TaxBuddy.Web.csproj -c Release
dotnet run --project src/TaxBuddy.Web/TaxBuddy.Web.csproj --launch-profile https
```

## Configuration and dependencies

- Check in `.editorconfig` with nullable reference types enabled and warnings reviewed. Use `Directory.Build.props` only for settings shared by all projects.
- Use explicit package versions. Consider `Directory.Packages.props` when shared versions become repetitive.
- Lock files: decided at scaffold. If adopted, commit them and use locked mode in CI only after generating them ([NuGet dependency locking](https://learn.microsoft.com/en-us/nuget/consume-packages/package-references-in-project-files#locking-dependencies)).
- Secrets come from `dotnet user-secrets` locally and the host's secret store in production; never from tracked files.
- Build output and local data files stay ignored (see `.gitignore`).

## Local development

The values below are filled when `TaxBuddy.Web` is created and verified.

| Item | Selected value |
|---|---|
| Web project and local addresses | `src/TaxBuddy.Web/TaxBuddy.Web.csproj`; `https://localhost:<port>` from its launch settings, recorded at scaffold |
| Google sign-in (development registration) | Redirect address `https://localhost:<port>/signin-google`. Secret keys `Authentication:Google:ClientId` and `Authentication:Google:ClientSecret` in user-secrets |
| Microsoft sign-in (development registration) | Redirect address: the handler's callback path, confirmed at implementation. Secret keys `Authentication:Microsoft:ClientId` and `Authentication:Microsoft:ClientSecret` in user-secrets |
| Development database | A Supabase project used only for development and tests, with synthetic data and a reset-and-seed command. Connection string in user-secrets under `ConnectionStrings:TaxBuddy` (Proposed). Use the session pooler address on port 5432 if this network has no IPv6. Data tests run against it in their own schema (Proposed); a local PostgreSQL can replace it later |
| Email in development | A development sender that writes emails to a local ignored folder or the log. No real email is sent |
| Testing from a phone | Open: reach the PC over the local network with a firewall rule, or use a development tunnel. Sign-in redirect addresses must match the address used |

- **Development secrets:** `dotnet user-secrets` is unencrypted and for development only ([REFERENCES](REFERENCES.md#implementation-and-hosting)).
- **Synthetic data only:** local and test databases never hold real people's details.

PowerShell commands for after scaffolding:

```powershell
dotnet user-secrets list --project src/TaxBuddy.Web/TaxBuddy.Web.csproj
dotnet run --project src/TaxBuddy.Web/TaxBuddy.Web.csproj --launch-profile https
```

## Troubleshooting and maintenance

| Situation | Recovery |
|---|---|
| Documentation path/anchor error | Run `uv run --no-project python tools/check_docs.py` (or `python tools/check_docs.py` where Python is on PATH) from the repository root; use its file/line to correct the link or restore the target. Rerun after renaming a feature |
| `python` opens the Microsoft Store or is not found | Use the uv-managed interpreter: `uv run --no-project python ...`. Any Python 3.10+ works |
| `dotnet` reports that the requested SDK was not found | The SDK in `global.json` is not installed. Install that exact version, or update `global.json` deliberately and record it here |
| Browser warns about the local HTTPS certificate | Run `dotnet dev-certs https --trust`, then restart the browser |
| Sign-in fails locally with a redirect error | The redirect address registered at the provider must match the local address and port exactly |
| A test/build could not execute | Record the concrete failure and next step in STATUS. A successful compile, restored package or another machine's historical pass does not replace that execution |

Project-specific failures and artifact locations are recorded here as they occur; the servicing owner is lumabanls. The [tools guide](../tools/README.md) owns checker commands, exit meanings and parser limits.

After a toolchain change, record exact versions and relevant test/build/browser results in [STATUS](delivery/STATUS.md). Do not inherit earlier results automatically.
