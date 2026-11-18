# Logo follow-up (F02 BD12): outlines, illustration, build and survey

## Identity and attribution

- **Date/time and timezone:** 2026-11-19, +08:00, shortly after midnight.
- **Observer:** agent-observed (Claude Code).
- **User's instruction:** "do this all", quoting three items the agent had left open after the logo change: the directory's old illustration; the wordmark's strokes, the originality and trademark check and name clearance; and the Release build needing the Tailwind step skipped.
- **Scope:** [BD12](../../../specs/brand-design-refresh/spec.md#logo-replaced-2026-11-18-bd12) follow-up; [PRD D01](../../PRD.md#open-decisions).
- **Source revision:** branch `feature/f02-f04-theme-switch-google-sign-in` at `55e0f12`, with F10, the BD11 alignment and the logo work uncommitted. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `prototypes/assets/taxbuddy-logo-light.svg` | `b1a113702624` |
  | `prototypes/assets/taxbuddy-logo-dark.svg` | `c40846e96d9c` |
  | `prototypes/index.html` | `7d272624ef24` |
  | `prototypes/taxbuddy.css` | `1a209c3dc66b` |
  | `prototypes/logo-directions/generate.py` | `dad9ce88e2a2` |
  | `src/TaxBuddy.Web/Components/Pages/DirectoryPage.razor` | `9af395e6f4ff` |
  | `src/TaxBuddy.Web/TaxBuddy.Web.csproj` | `8115db21d44d` |

  The icon and favicon files are unchanged from the [2026-11-18 record](2026-11-18-f02-logo.md). The site's two logo files are byte-for-byte copies of the prototype's.

- **Environment:** WSL2 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; the Supabase development database, synthetic data only. The survey used the agent's web search and `curl` to `rdap.org`; only the name, generic terms and domain names were sent.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Outline the wordmark in `generate.py`; render the new logo above the stroked original at 4× | The same letter shapes, with no `stroke` in the file | The two rows matched by eye, with no gaps or filled counters. `grep -c stroke` on the light logo: 0 | Pass |
| Convert with `picosvg` (a true merge of the shapes) | One contour per letter | Not run: the agent's permission check refused running a downloaded tool. The outlines are therefore overlapping shapes | Not run |
| `node tools/check_prototype.mjs "<Edge>"` | All checks pass | `PASS: 186 prototype checks` | Pass |
| `dotnet build TaxBuddy.slnx -c Release`, without `-p:SkipTailwind=true` | 0 warnings, 0 errors, and the Tailwind step runs | 0 and 0. The detailed log shows `BuildTailwindCss` printing `tailwindcss v4.3.3` and `Done`. The generated `wwwroot/app.css` is byte-identical to the file from 2026-11-18 23:25 | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build -m:1` | No failures, nothing skipped | `TaxBuddy.Tests`: 154 passed. `TaxBuddy.Web.Tests`: 104 passed. 0 failed, 0 skipped. This is the rerun owed from the 2026-11-18 record | Pass |
| Site run from the Release build on port 5244; directory screenshots at 1440 px, light and dark ([folder](2026-11-19-f02-logo-outlines-hero/)) | The illustration shows the open ledger with the logo's bubble; the logo is unchanged in appearance | As expected in both themes | Pass |
| Prototype screenshots from the checker: desktop light, and Plum & Blush dark | The same illustration; the inline logo and illustration take the palette's colours | As expected in the Plum & Blush dark capture. The desktop light capture was copied to the folder but not opened | Pass for the one reviewed |

## Survey of similar marks and domains

**Not legal or trademark clearance.** It adds to the [2026-11-16 survey](2026-11-16-name-availability.md) and decides nothing.

### Marks like the new logo

Seven web searches for a letter T in a speech bubble, and for the two existing TaxBuddy logos. No image search and no trademark register search was possible.

| Finding | Source | Bearing |
|---|---|---|
| A US trademark record described as a white, stylized lowercase "t" centred in a round orange speech bubble, serial 86574563. Its owner, goods and status were not retrieved, and the page was not opened | [Justia listing](https://trademarks.justia.com/865/74/t-86574563.html), as summarized by the search tool | The closest idea found. It differs in colour, letter case and, as described, bubble shape. Whether it is live or covers related services is unknown |
| Talkspace's earlier logo was defined by a speech bubble; its 2024 identity leads with a capital T and has no bubble | [Talkspace](https://www.talkspace.com/blog/talkspace-redesign/) | A different sector (therapy), and the T and the bubble were never one mark there, as far as the sources say |
| Google Chat and Apple Messages use bubbles without a letter | [9to5Google](https://9to5google.com/2023/11/05/new-google-chat-logo/) | The square-cornered bubble is a common shape in messaging; none found with a T |
| No tax or accounting brand turned up with a T in a speech bubble | The searches above | An absence in search results, not proof |
| The logos of TaxBuddy PH and TaxBuddy India | Searches returned no image or description | **Not compared.** This is the main gap: both use the same name in the same field |

### Domain records

Read on 2026-11-19 through `https://rdap.org/domain/<name>`.

| Domain | Observation |
|---|---|
| taxbuddy.app | Registered 2024-10-26, expires 2029-10-26 |
| taxbuddy.ai | Registered 2023-05-24, expires 2027-05-24 |
| taxbuddy.net | Registered 2010-02-22, expires 2027-02-22 |
| taxbuddy.org | Registered 2026-07-05, expires 2027-07-05 |
| taxbuddy.io, taxbuddy.co | No record found (HTTP 404). Probably unregistered; not confirmed with a registrar |
| taxbuddy.ph, taxbuddy.com.ph | No record found, which means little here: the lookup service may not cover `.ph`. Both names resolve to the same address, so they may be registered, parked or answered by a registry wildcard. Unconfirmed |

taxbuddy.com and taxbuddyph.com are in use, as the 2026-11-16 survey recorded.

### Assessment

- Nothing found suggests the new symbol copies a mark in tax or accounting.
- The name remains the larger risk. The 2026-11-16 survey rated it high, and this survey does not change that.
- Not searched: IPOPHL, the WIPO Global Brand Database, IP India and EUIPO, for the name or for the figurative mark.

## Restoration and limits

The temporary site instance on port 5244 was stopped and the port checked as closed. No database rows were written by this work.

Not exercised: real devices, screen readers and zoom; the phone layouts, which this work does not change; and a build on Windows or on a Linux machine with its own Node, so only the WSL fallback path of the Tailwind step was run.

Acceptance decision: pending the owner's walkthrough (G9). D01 stays with the owner.
