# F17: Client details

Governing requirement: [PRD R25](../../docs/PRD.md#requirements), with R11, R24 and D21. Technical/UI contracts: [SDD](../../docs/SDD.md#client-details), [DESIGN](../../docs/DESIGN.md) (Account row). Approach: [plan](plan.md). Tracking: [tasks](tasks.md).

## Intent and scope

A client keeps a few facts about themselves, so the accountants they contact know who they're helping without asking in chat.

- **Existing behavior:** a client account has the name and email from Google and an optional picture (R04, R24). Account shows the name and "Client · Signed in with Google"; nothing can be edited except the picture.
- **Authorized change** (the owner, 2026-11-25): the prototype's design B, chosen that day ([F02 BD15](../brand-design-refresh/spec.md#client-details-on-account-2026-11-25-bd15)), then "implement to site".
  - **Fields:** a name (starting from the Google name), phone and city; a business name, type and industry. All but the name are optional.
  - **Where:** a "Your details" card on Account. Edit details opens the same card as a form, at `/account?edit=details#details`.
  - **Who sees them:** the accountants the client contacts. Accountants see them on their page for that client and in a service request. Never public (R11).
- **Exclusions:**
  - The email stays the Google address, shown but not editable.
  - No TIN or other ID numbers; the form says so.
  - No details for accountants, whose public profile is F05.
  - No details in chat; the prototype's chat panels are separate, unadopted options.
- **Authorization evidence:** the user's messages of 2026-11-25. The prior conflict [C05](../../docs/README.md#history-and-unresolved-conflicts) is resolved by this requirement.

## Acceptance scenarios

| ID / requirement | Given / when | Observable result | Verification boundary |
|---|---|---|---|
| CD01 / R25 | A signed-in client opens Account | A "Your details" card under the name card, with phone, city and business ("Not added" when empty), the sharing note and Edit details. An accountant and a visitor have no such card | Web test; Edge on the running site |
| CD02 / R25 | The client opens Edit details (`/account?edit=details`) | The form in the card: About you (name, phone, city, and the email shown, not editable) and Your business (name, type from the list, industry). It says who can see the details and not to add a TIN or ID numbers. Save details and Cancel | Web test |
| CD03 / R25 | The client saves valid details | Stored. The browser goes to `/account?details=saved#details`, where the card shows them with "Saved. The accountants you contact see these details." A new name shows in the header at once (the session is renewed) | Web test (database and session) |
| CD04 / R25, BR18 | The client saves an empty name, a phone with letters, a field that's too long, or a type not on the list | Not stored. The form shows again with everything typed and a message by each problem field | Unit and web tests |
| CD05 / R25, BR18 | Saving fails in the database | Not stored. The form shows again with everything typed and "We couldn't save your details. Please try again." | Code path only; a database failure isn't simulated in the tests |
| CD06 / R25, R11 | An accountant opens their page for a client with services, or the request page of a service awaiting acceptance | An "About {first name}" card with the client's phone, city and business, when the client has added any. Nothing on any public page or for other accountants | Web test |
| CD07 / R09, BR06 | The client deletes their account | Phone, city and business details are removed with the name and email | Web test |
| CD08 / R16 | A client who agreed to an older version of the terms visits after this change | They're asked to agree again, because the privacy policy now names these details | Existing F11 tests with the new version |

## State, dependencies, and unresolved decisions

- **Stored on the account row:** `users.phone_number` (Identity's column, now limited to 20 characters), `city`, `business_name`, `business_type` and `industry`, all nullable. A blank field is stored as null.
- **Checks:**
  - name: 1–100 characters, as Google names are kept today;
  - phone: 7–20 characters of digits, spaces, `+`, `-` and parentheses, with at least 7 digits;
  - city and industry: up to 80 characters; business name: up to 120;
  - type: one of the listed values.
- **Visibility:** an accountant sees the details only on pages they can already open for that client: a client page (a service exists) and a service page (they're its accountant). There's no new address.
- **Legal:** the privacy policy gains these details (what's collected, purposes, who sees them, deletion). The version becomes `2026-11-25`, so signed-in people agree again (F11). Like `2026-11-23.2`, the text awaits the owner's approval (STATUS G13).
- **Open:** none for this scope. Showing the details in chat is a separate, unadopted prototype option.

## Evidence limits

Execution is recorded in [tasks](tasks.md), [STATUS](../../docs/delivery/STATUS.md) and the dated evidence record.
