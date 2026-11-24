# F12: Account deletion

Governing requirements: [PRD](../../docs/PRD.md) R09, BR06, R11; decisions D07 (decided 2026-11-21) and D27 (partly). Technical contracts: [SDD](../../docs/SDD.md) (sign-in, sessions and authorization; data and persistence; render modes). UI: [DESIGN](../../docs/DESIGN.md#screen-and-component-states) row "Account deletion (R09)", and the prototype's `deleteHTML`, the "Delete account" item in `meHTML`, and `threadParts` for a deleted accountant. Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A signed-in person deletes their own account on the site, after confirming. The user asked on 2026-11-23 to proceed with the suggested order, account deletion first. The Privacy Policy already promises deletion, so far by email.

**Existing behaviour:**
- There's no deletion on the site.
- Links from other tables to `users` refuse deletion "until R09".
- The legal text tells people to email the owner.

**Behaviour, following D07, D27 (partly) and the privacy default of 2026-11-21:**
- **Removed:** the account's name, email, picture, sign-in links and claims. The account row stays as a nameless "Deleted user" with a deletion time, because chats keep pointing at it.
- **An accountant:** the profile goes, with its services, experience, highlights, reviews and replies.
- **A client:** their reviews go, and ratings recalculate because they're computed.
- **Chats:** each one stays with the other person, shown as from "Deleted user", who can no longer reply. The deleting side's block is set, so the existing block rule refuses sends both ways. A conversation whose other person was already deleted is removed with its messages.
- **Picture:** the object in storage is deleted after the database change.
- **Sessions:** the person is signed out. Other sessions end at the next security check, now every minute instead of every 30.
- **Coming back:** signing in again with the same Google account starts a new registration.

**Exclusions:**
- Notification email settings: R08 wasn't built when this was written, so the page didn't mention emails. Since 2026-11-24 (R08 built in F14) the list says "Your profile picture and email notifications for this account.", as in the prototype.
- Service work and documents (F03, D27).
- Undoing a deletion.
- An admin page; the RELEASE moderation steps still cover closing accounts by hand.
- What happens to reports a deleted person sent: they stay, linked to the nameless account, and are kept for 12 months as the Privacy Policy says.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| AD01 / R09 | A signed-in person opens Account | A "Delete account" item at the end, styled as dangerous, opens `/account/delete` | Web test |
| AD02 / R09, BR06 | On `/account/delete` | "Delete your account?" lists what is removed for their role and says chats stay as from "Deleted user". Cancel returns to Account; Continue goes to "Are you sure?" with "Delete my account" | Web test |
| AD03 / R09, BR06 | They press "Delete my account" | The account is nameless with a deletion time; sign-in links are gone; they're signed out and see "Your account was deleted." Signing in again with the same Google account goes to registration | Web test |
| AD04 / D07 | An accountant deletes their account | Profile, services, experience, highlights, reviews and replies are gone; the old profile link says "no longer available"; the directory no longer lists them | Web test |
| AD05 / D07 | A client with a chat and a review deletes their account | The review is gone and the rating recalculated. The accountant still sees the conversation, named "Deleted user", with "This client deleted their account. You can still read this conversation, but you can't reply." Sending is refused | Web and service tests |
| AD06 / D07 | The accountant in a chat deletes their account | The client sees "Deleted user" and "This accountant deleted their account…"; sending is refused | Web test |
| AD07 / privacy default | The second person in a conversation deletes their account | The conversation and its messages are removed | Web test |
| AD08 / ADR 011 | The person had a picture | The storage object is deleted. If storage fails afterwards, deletion still succeeds and a warning without IDs is logged | Web test with the in-memory store |
| AD09 / R09 failure | The database fails during deletion | Nothing changes; still signed in; "We couldn't delete your account, so nothing was removed…" | Web test with an unreachable database |
| AD10 / SDD | A visitor opens `/account/delete`; a post comes without the form's token | Visitor: asked to sign in. Post: refused, nothing changes | Web test |
| AD11 / R16 | Someone who hasn't agreed to the current terms opens `/account/delete` | Not redirected to agree first: they can leave without accepting new terms | Web test |

## State, dependencies, and unresolved decisions

- **Migration** `AddAccountDeletion`: a nullable `users.deleted_at`. It's additive, so older code still runs.
- **One transaction** covers the profile, reviews, conversations, sign-in links and the user row. The picture object is deleted afterwards, because storage is outside the database.
- **Security check interval** changes from 30 minutes to 1 minute. This also speeds up the RELEASE "close an account" step and the claim refresh for re-agreement.
- **Render mode:** a static form page, not Interactive Server as the SDD had planned. The other account forms became static too (C04), and it works without script.
- **Legal text:** the "until account deletion is available, email us" sentences change, so the version moves to `2026-11-23`, and signed-in people agree again.

## Evidence limits

No result is recorded here; see [STATUS](../../docs/delivery/STATUS.md).
