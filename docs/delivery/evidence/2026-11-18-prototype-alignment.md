# Prototype alignment check (F02 BD11) execution evidence

## Identity and attribution

- **Date/time and timezone:** 2026-11-18, +08:00; the Edge check ran at about 23:25.
- **Observer:** agent-observed (Claude Code). The comparison was done by three read-only subagents, each reporting differences with file lines and prototype functions. The main agent checked the larger findings against `prototypes/index.html` and `taxbuddy.css`, then made the changes.
- **User's instruction:** "check current implementation against the prototype. follow what's on the prototype then update the documentation".
- **Scope:** every built page: F01, F02 shell, F04–F10. [BD11 scenario](../../../specs/brand-design-refresh/spec.md#prototype-alignment-check-2026-11-18-bd11). The changes and the deviations kept are listed in [DESIGN](../../DESIGN.md#visual-system).
- **Source revision:** branch `feature/f02-f04-theme-switch-google-sign-in` at `55e0f12`, with F10 and this alignment uncommitted. Leading 12 hex digits of SHA-256:

  | File | SHA-256 (prefix) |
  |---|---|
  | `src/TaxBuddy.Web/Styles/app.css` | `3e347906332a` |
  | `src/TaxBuddy.Web/Components/Shared/StateMessage.razor` | `c6c7e5307569` |
  | `src/TaxBuddy.Web/Components/Layout/MainLayout.razor` | `813695dc05a8` |
  | `src/TaxBuddy.Web/Components/Pages/DirectoryPage.razor` | `936eedfa9f78` |
  | `src/TaxBuddy.Web/Components/Pages/AccountantProfilePage.razor` | `a1969d0e2412` |
  | `src/TaxBuddy.Web/Components/Pages/NotFound.razor` | `458eb7c8cbc6` |
  | `src/TaxBuddy.Web/Components/Pages/Error.razor` | `f0ff25f22e8d` |
  | `src/TaxBuddy.Web/Components/Pages/ReviewPage.razor` | `022bbc694590` |
  | `src/TaxBuddy.Web/Components/Pages/SignInPage.razor` | `5e8f68175cc2` |
  | `src/TaxBuddy.Web/Components/Pages/ChatsPage.razor` | `bbb737889231` |
  | `src/TaxBuddy.Web/Components/Chat/ChatPanes.razor` | `114e0aba792e` |

- **Environment:** WSL2 Ubuntu 26.04 with .NET SDK 11.0.100; Tailwind 4.3.3 through Windows Node 24.16.0; headless Edge 157.0.4341.60; the Supabase development database.

## Execution

| Procedure / command | Expected result | Actual observation / exit code | Outcome |
|---|---|---|---|
| Three read-only audits | Differences classified as recorded, waiting on an unbuilt feature, or unrecorded drift | About 40 unrecorded differences, nearly all copy, spacing and state layout. Stale lines in DESIGN and the F02 and F07 specs | Done |
| `dotnet build TaxBuddy.slnx -c Release -p:SkipTailwind=true` | 0 warnings, 0 errors | 0 and 0 | Pass |
| `dotnet test TaxBuddy.slnx -c Release --no-build -m:1` | No failures, nothing skipped | 154 and 104 passed, 0 failed, 0 skipped. Two web tests were updated for the prototype's wording ("Back to directory", "Sign in to send a report") | Pass |
| Edge check ([results](2026-11-18-prototype-alignment/results.json)) | The changed pages follow the prototype | 16 of 16; details below. App log: no exceptions | Pass |
| Screenshot review: directory (1440 visitor; 390 hero), profile (390 visitor), not-found (1440), editor (390), block dialog (390 dark) | The prototype's layout | As expected, apart from one finding: the not-found page's button was underlined, because the panel's link style also applied to buttons. Fixed with `.prose a:not(.btn)` after the run; CSS only | Pass after a fix |

**Edge check, with a temporary sign-in host and the real app on port 5240:**
- **Visitor directory:**
  - At 1440: the eyebrow, and "How it works" with three steps after the results.
  - At 390: the symbol and "Tax Buddy" in the app bar, the How it works toggle, and no panel around the search form.
- **Visitor profile:**
  - At 390: no card, and the bar shows "Message Nina" with its hint.
  - At 1440: "Find an accountant" is current.
- **Not found:**
  - At 1440: a "Page not found" panel holding "We couldn't find that page".
  - At 390 dark: no frame, and the title is for screen readers only.
- **Missing profile:** the prototype's text, with "Back to directory".
- **Privacy at 390:** 24 px section headings, and no icon on the draft note.
- **Visitor at `/chats`, 1440:** the two panes.
- **Client:**
  - The directory shows "Find an accountant" without the visitor parts.
  - The profile shows "Open chat" with no hint.
  - The review page at 390 has one visible title.
  - The block dialog is a centred box.
- **Accountant:**
  - The owner's note on their profile, and no report link.
  - The editor's Save bar is stuck to the bottom at 390.
  - Account says "My profile and portfolio".

## Restoration and limits

- **Development database:** the check's 2 synthetic accounts were deleted afterwards, with their profile, conversation, 2 messages and review. Afterwards: 2 accounts, 0 `example.test`, 10 profiles, 0 conversations, 0 reviews, 0 reports.
- The temporary host, scripts and Edge profile were deleted; none are committed.
- **Not compared pixel by pixel:** the check looks at structure, wording and key sizes.
- **Not run:** other browsers and phones.
- **Not re-run after the fix:** the underline fix came after the Edge run. It is one CSS selector; the screenshot review saw the problem, and the fix wasn't re-shot.
- **Waiting on unbuilt features** (Microsoft sign-in, For accountants, About and Help, email settings, account deletion, the F03 workspace): left out as before, and listed in DESIGN.

Acceptance: pending.
