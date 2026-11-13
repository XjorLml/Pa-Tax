# Project automation

Add scripts only when there is a repeatable task. Record setup in [SETUP](../docs/SETUP.md), expected verification in [TEST-PLAN](../docs/delivery/TEST-PLAN.md), and observed results in [STATUS](../docs/delivery/STATUS.md).

## Check documentation

Requires Python 3.10+ and no pip packages. From the copied repository root:

```powershell
python tools/check_docs.py
python tools/check_docs.py --ready
```

If `python` is not on PATH (as on this project's development PC on 2026-11-10), run the same commands through uv: `uv run --no-project python tools/check_docs.py --ready`. Any Python 3.10+ works; [SETUP](../docs/SETUP.md#troubleshooting-and-maintenance) records the host details.

The normal check allows unfinished template placeholders. Use `--ready` after customization; it fails on remaining `{{...}}` fields, including examples. Reusable templates are the exception: files named `TEMPLATE.md` and anything in a `feature-template` folder keep their placeholders, and the summary counts them. Their links are still checked, and copies you make from them (new names or folders) are enforced. Resolve decisions honestly or remove inapplicable examples with a reason. A structurally ready document can still have pending product approval, builds or device checks.

PASS means supported local paths and heading/HTML anchors resolve. Exit 0 = pass, 1 = documentation errors, 2 = invalid arguments/configuration. Failures include file/line and corrective action. Use `python tools/check_docs.py --help` for options; paths are relative to `--root`, which defaults to this copied project independently of the working directory. Example subset: `python tools/check_docs.py docs/README.md`.

The checker does not modify files, use the network, generate/build an app or access a device. External links and application routes are reported as not executed. Conventional inline/reference links, percent-encoded/angle-bracket paths, ATX/setext headings, duplicate headings and quoted HTML id/name anchors are supported. Fenced examples, comments and inline code are excluded. Raw HTML hrefs, arbitrary embedded Markdown, visual rendering and runtime behavior require separate review.

| Report | Action |
|---|---|
| Missing target or outside root | Correct the relative path, restore its target or copy the missing documentation. A copied project must not depend on the source repository. |
| Missing anchor | Match the actual heading/id and update inbound links after renaming. |
| Undefined reference | Add the matching link definition or use an inline link. |
| Unresolved placeholder | Customize the field or remove an inapplicable example; the original kit is expected to fail readiness. |
| No Markdown files / missing root | Pass the correct `--root PATH` or existing documentation paths. |
| Unsupported fragment/scheme | Use portable relative links or perform and document a manual check for unsupported targets. |

If the project later has JavaScript routes, add `tools/docs-routes.json` mapping relative HTML paths to exact inspected fragment lists. These declarations are not browser verification; unrelated missing fragments still fail. The default needs no manifest.

Run the reusable failure/recovery tests with:

```powershell
python -m unittest discover -s tools/tests -v
```

## Check the prototype

Requires Node 22+ with the built-in WebSocket client and an installed Chromium-based browser. No npm packages or browser download. From the repository root, pass the browser's absolute executable path:

```powershell
node tools/check_prototype.mjs "<absolute-path-to-browser-executable>"
```

Optionally pass an artifact directory as the second argument; default is ignored `artifacts/brand-refresh`. The helper opens the local prototype with synthetic data using a new isolated browser profile. It checks directory recovery, profile content, draft retention through simulated sign-in, send retry, reviews/replies, block/unblock, unsaved edits, layouts, comparison looks and reduced motion. It writes screenshots and `results.json`, exits 0 on pass or nonzero on failure, closes only its own browser, and removes its temporary profile. If renderer startup is restricted, running the same helper with the executor's approved process permissions may be necessary. A CDP timeout is not a product failure or a passing browser result.

This is a prototype DOM/layout check in the supplied browser, not real device, screen-reader, zoom, OAuth, email, persistence or Blazor verification. Screenshot inspection is a separate observation. Keep artifacts local unless deliberately selected as sanitized evidence. Syntax can be checked by extracting the prototype's inline script and running `node --check` on it.

F03 extends the helper with private client/added-accountant lists, request/accept/decline, task progress, shared notes, role guards, generated sample document validation/retry/removal, and account changes during upload. The download check intercepts the final anchor handoff to inspect a generated synthetic PDF Blob; it does not establish operating-system download behavior. Run `node --check prototypes/service-workspace.js` for the external module. Pass `artifacts/client-services` as the optional output directory to keep the current run separate from F02 artifacts; retained reports include source SHA-256 values. No actual user file or real upload is used.

The helper also covers the palette options (F02 BD08): the default palette, each option in light and dark with its 17 contrast pairs, the phone frame, and the Palette control. It writes `contrast-palettes.json` beside the other reports. Pass `artifacts/palette-options` as the output directory to keep that run separate.

Since 2026-11-12 it also checks the directory's static-page contract (conflict C03): typing alone does not filter, Search and the service buttons submit, sort and page are links, their values appear in the address, and cards carry no per-user tag. Pass `artifacts/c03-directory` to keep that run separate.

From WSL, run the checker with Windows Node and a Windows browser against a copy of `prototypes/` and `tools/` in a Windows folder. Use Windows paths for every argument. The checker opens files by path and reads the browser's debugging port from its profile folder, so mixing Linux and Windows paths does not work.

## Add automation when needed

Add repeatable build/test entry points only after actual projects and verified commands exist. Add browser automation helpers only when their maintenance cost is justified.

Each helper should document its required host/tools, input arguments, selected target/device, preconditions, output/artifact location, exit behavior, cleanup, and known limits. Select devices explicitly; isolate test data and make cleanup scoped and repeatable. Keep logs/screenshots free of personal data and signing material. Preserve original settings before modifying them and verify restoration afterwards.

Use repository-relative paths or documented parameters. Stop on failed prerequisites. Never use a passing script exit as acceptance of scenarios it did not exercise. Do not hardcode machine-specific paths, device names or fixture assumptions.
