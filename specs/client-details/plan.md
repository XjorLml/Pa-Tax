# F17 plan: Client details

[Spec](spec.md) · [tasks](tasks.md) · [PRD R25](../../docs/PRD.md#requirements) · [SDD](../../docs/SDD.md#client-details)

## Approach

1. **Domain:** `Domain/ClientDetails.cs`:
   - the limits and the business types;
   - `ClientDetailsRules.Validate`, which returns a message per field;
   - `ClientDetailsRules.Clean`, which trims and turns blanks into null.
2. **Data:**
   - `TaxBuddyUser` gains `City`, `BusinessName`, `BusinessType` and `Industry`; the phone uses Identity's `PhoneNumber`.
   - Migration `AddClientDetails` adds four nullable columns and limits `phone_number` to 20 characters.
   - Account deletion clears all five (BR06).
3. **Account:** `Account/ClientDetailsService.cs`:
   - load the signed-in client's details;
   - save them after the checks;
   - renew the session, so a new name reaches the header claim;
   - report a database failure without storing anything.
4. **Page:** `AccountPage.razor`, client only (prototype `clientDetailsCardHTML`):
   - the "Your details" card;
   - with `?edit=details`, a named form `client-details` posted to the same page;
   - a valid save redirects to `/account?details=saved#details`.
5. **Accountants:**
   - `WorkStore` reads the details into `ClientServicesView` and, for the accountant only, `EngagementView`;
   - `Components/Work/AboutClient.razor` draws the card on `ClientServicesPage` and in the request panel of `ServicePage`.
6. **Styles:** the prototype's `.opt-cd-*` and `.opt-about-client` rules, ported to `Styles/app.css` as `.details-*` and `.about-client` through the tokens.
7. **Legal:**
   - the privacy policy (draft source and `PrivacyPage.razor`) names the details;
   - `LegalText.CurrentVersion` becomes `2026-11-25`;
   - the user names this text approved for the site, as before (STATUS G13).
8. **Tests:** unit tests for the rules, and web tests for CD01–CD04, CD06 and CD07.

## Dependencies and risks

- **Migration:** apply it to the development database before running the site (`dotnet ef database update`). The tests create their own schemas.
- **Agreeing again:** the version change makes every signed-in person agree again on their next visit (F11). That's intended, because the policy changed.
- **Reviews:** they show the first name and last initial from the account's name (D21). A changed name changes how the client's reviews are signed.
