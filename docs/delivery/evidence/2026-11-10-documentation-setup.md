# Documentation setup execution evidence

Procedure definitions: [TEST-PLAN documentation-only checks](../TEST-PLAN.md#documentation-only-checks). Current state: [STATUS](../STATUS.md).

## Identity and attribution

- Date/time and timezone: 2026-11-10, about 18:30–19:40 +08:00.
- Observer: agent-observed (Claude Code session run for lumabanls). Outputs below were read directly from the commands' output.
- Requirements/scenarios/procedures: documentation-only checks; no PRD requirement is verified by this record.
- Source revision and working-tree changes: baseline `b9b50f3` ("Create from maui-starter-kit 8069179"), plus uncommitted customization of every authority document, four new ADRs, `specs/browse-accountants/`, and deletion of `docs/starter-kit/`.
- Artifact: documentation only; no application artifact exists.
- Environment:
  - Windows 11 Pro 10.0.26300;
  - Git Bash and Windows PowerShell 5.1;
  - uv-managed CPython 3.14.4, run as `python.exe -B` with `PYTHONDONTWRITEBYTECODE=1`, so the repository was not written to.
- Preconditions/fixtures: none; the checker reads Markdown only.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Template repo before copying: `check_docs.py` | PASS with draft placeholders | `PASS: 23 Markdown files; 155 local links; 0 errors.` 389 placeholders; exit 0 | Pass |
| Template repo before copying: `check_docs.py --ready` | FAIL (template not customized) | `FAIL: 23 Markdown files; 155 local links; 308 errors.` 81 kept in reusable templates; exit 1 | Pass (expected failure) |
| Template repo: `python -m unittest discover -s tools/tests -v` | 10 tests OK | `Ran 10 tests` … `OK`; exit 0 | Pass |
| New repo right after the baseline commit: `uv run --no-project python -B tools/check_docs.py` | PASS | `PASS: 23 Markdown files; 155 local links; 0 errors.`; exit 0 | Pass |
| New repo after customization: `check_docs.py` | PASS | `PASS: 28 Markdown files; 254 local links; 0 errors.` 81 placeholders, all in reusable templates; exit 0 | Pass |
| New repo after customization: `check_docs.py --ready` | PASS | `PASS: 28 Markdown files; 254 local links; 0 errors.` Readiness enforced; 81 kept in reusable templates; exit 0 | Pass |
| New repo: `python -m unittest discover -s tools/tests` | 10 tests OK | `Ran 10 tests` … `OK` | Pass |
| `grep` for `{{` outside `TEMPLATE.md` and `feature-template` | No matches | No matches | Pass |
| `grep` for reference-app terms "PHP" and "Manila" | Only owner-chosen policy text | Every match is the owner-chosen PHP currency or Asia/Manila timezone policy | Pass |

Failures and subsequent retries: one intermediate run, before this record existed, failed only on the two STATUS links to this file. Both rows above are from after it was created.

### Host inventory (observed 2026-11-10)

| Item | Observation |
|---|---|
| .NET SDKs (`dotnet --list-sdks`) | 8.0.307, 8.0.427, 9.0.123, 9.0.319, 9.0.320, 10.0.400, 10.0.403; `dotnet --version` = 10.0.403 |
| Workloads (`dotnet workload list`) | android 36.1.118/10.0.100; ios and maccatalyst 26.5.10315/10.0.100; maui-windows 10.0.110/10.0.100; source "VS 18.11.12328.437" |
| `dotnet workload --version` | `10.0.400-manifests.899c35d8` (manifests mode) |
| Visual Studio (`vswhere`) | Community 2026 18.11.3; Community 2022 17.14.44 |
| Python | `python` resolves to the Microsoft Store alias ("Python was not found"); `py` not found; uv-managed CPython 3.14.4 and 3.14.0 installed |
| Android SDK | `C:\Program Files (x86)\Android\android-sdk`: platforms android-35 and android-36; build-tools 35.0.0 and 36.0.0; platform-tools (adb 1.0.41); cmdline-tools; `emulator` folder contains only `.installer`; `system-images` empty; no AVDs in the user profile |
| Environment variables | `ANDROID_SDK_ROOT` set to `%LOCALAPPDATA%\Android\Sdk`, which does not exist; `ANDROID_HOME` and `JAVA_HOME` unset |
| JDK | None found on PATH, in `C:\Program Files\{Microsoft,Java,Eclipse Adoptium,Android}`, or in the JavaSoft and Microsoft JDK registry keys |
| Other | git 2.56.0.windows.1; GitHub CLI not installed; `dotnet new list maui` shows the MAUI templates |

## Restoration and limits

No device settings or fixtures were changed. The template repository at `D:\LEM\Projects\accapp\maui-starter-kit` was read and checked, but not modified in this step.

Not exercised: any build, test project, emulator, device or app behavior; external URLs (40 reported as not executed by the checker); visual Markdown rendering. Historical evidence reused: none. Acceptance decision: pending. This record establishes only that the documentation's local links and anchors resolve and that no placeholders remain outside reusable templates.
