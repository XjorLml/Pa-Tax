# F13: Information pages

Governing requirement: [PRD](../../docs/PRD.md) R17 (and R16's "reachable from every page"). UI: [DESIGN](../../docs/DESIGN.md#screen-and-component-states) row "Information pages", and the prototype's `infoPage` ('about', 'help', 'for-accountants'), `infoLinksHTML` and `desktopShell`. Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

The site explains itself: About, Help and contact, and For accountants, reachable from every page without signing in. The user asked on 2026-11-23 to proceed with the suggested order, these pages after account deletion.

**Existing behaviour:** none of the three pages; the footer links only Privacy and Terms.

**Behaviour:**
- **Pages:** `/about`, `/help` and `/for-accountants` as static pages in the information-page panel.
- **Links:** About, Help and For accountants join the footer and the Account page's links. Visitors see "For accountants" in the site bar from 900 px, as in the prototype.

**Content:**
- **Prototype text,** except where the site differs. Help answers only questions about features that exist: messaging, reviews, report and block, checking a licence, and deleting an account. Sign-in is Google only.
- **About** names the operator and contact email (`LegalText`).
- **For accountants** offers "Create your profile" (sign in, then the editor) to visitors, "Go to my profile" to accountants, and a one-role note to signed-in clients.

**Exclusions:** the "emails off" page (R08) and Help answers about service work (F03) and emails (R08).

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| IP01 / R17 | A visitor opens each page | 200, its heading and content; no overflow at 320 and 390 px | Web test, Edge |
| IP02 / R17 | About and Help | The operator and a mailto link to the contact email; Help has no Microsoft answers. Since 2026-11-23 (F03, F14 built) it has the prototype's service, documents and email questions | Web test |
| IP03 / R17 | For accountants as a visitor, an accountant and a client | "Create your profile" through sign-in; "Go to my profile"; the one-role note | Web tests |
| IP04 / R16, R17 | Any page | The footer links About, Help, For accountants, Privacy and Terms; visitors see "For accountants" in the site bar | Web test, Edge |

## Evidence limits

Results are in [STATUS](../../docs/delivery/STATUS.md) and its evidence records.
