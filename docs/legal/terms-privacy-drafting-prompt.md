# Drafting prompt: terms of use and privacy policy

**Purpose:** this is a ready-to-use prompt for drafting the text of `/terms` and `/privacy` ([PRD R16](../PRD.md#requirements), [D10](../PRD.md#open-decisions), [STATUS G13](../delivery/STATUS.md#remaining-gates-and-decisions)). It is a working aid, not an authority. Its facts come from the documents listed below, and the text it produces still needs review by a licensed Philippine lawyer before real users sign in.

The prompt was created on 2026-11-21 at the user's request. Its fact sheet reflects that date and was compiled from:

- the [PRD](../PRD.md): R01–R24, BR01–BR19 and D01–D28;
- the [SDD](../SDD.md): data and persistence, sign-in and sessions, diagnostics;
- [ADR 007](../adr/007-supabase-brevo-container-host.md), [ADR 010](../adr/010-single-project-per-page-render-modes.md) and [ADR 011](../adr/011-profile-pictures-supabase-storage.md);
- [SETUP](../SETUP.md), for the Supabase region;
- [RELEASE](../RELEASE.md#reading-user-reports-r15-d22), for report handling;
- the F04–F10 specs;
- the draft pages `src/TaxBuddy.Web/Components/Pages/TermsPage.razor` and `PrivacyPage.razor`;
- the user's description of 2026-11-21: the site is like social media, where accountants post their professional credentials publicly, and it is open to all.

**Ran 2026-11-21:** the prompt produced the [Terms of Use](terms-of-use.md), the [Privacy Policy](privacy-policy.md) and the [drafting notes](drafting-notes.md). The same day the user decided 16 of the open items (the [decision list](drafting-notes.md#decisions-made-on-2026-11-21)), so the fact sheet below is out of date on those points. Update it before running the prompt again.

## How to use it

1. Copy the prompt below. Fill in the operator details in your copy, not in this file, unless they are public business details.
2. Paste it into a capable AI model, or give it to the drafting lawyer.
3. Resolve the placeholders and open decisions that come back, and record each decision in the PRD.
4. Have a licensed Philippine lawyer review the result.
5. Replace the draft pages, set `LegalText.CurrentVersion` (in `src/TaxBuddy.Web/Domain/AccountRole.cs`) to the new version label, and update G13.

## Keeping it current

Update the fact sheet when any of these change:

- a decision it lists: D01, D07, D09, D11, D14, D18, D19, D20, D22 or D26–D28;
- a feature that collects or shows personal data: R08 email, R09 account deletion, R19–R23 service workspaces, or Microsoft sign-in;
- a service provider or its region;
- a cookie.

## The prompt

```markdown
# Task: draft the Terms of Use and Privacy Policy for "Tax Buddy"

You are an experienced Philippine technology and data-privacy lawyer. Draft two documents for the website Tax Buddy:

1. **Terms of Use**, shown at /terms
2. **Privacy Policy**, shown at /privacy. This is the privacy notice the Data Privacy Act of 2012 requires.

Tax Buddy works like a professional social-media site. Accountants post their professional credentials on public profiles, and the site is open to everyone. Draft both documents with that in mind.

Every user agrees to both documents with one checkbox when they first sign in. A licensed Philippine lawyer will review your draft before any real user sees it. Write it as a finished, professional draft, but never fill a gap with an invented fact.

## Ground rules

1. **Use only the facts below.** Don't describe any feature, practice, security measure, certification, provider or promise that isn't in the fact sheet.
2. **Mark every gap.** Use `[FILL: what is needed]` for missing information. Use `[DECISION Dxx: question — Option A / Option B (recommended: …)]` for an open owner decision. Write the sentences around each placeholder in full, so the owner can swap in an answer without rewriting anything.
3. **Keep built and planned features apart.** Features marked PLANNED go in clearly labelled optional blocks that start with `[OPTIONAL — include when <feature> launches]`.
4. **Philippine law governs.** Consider at least:
   - RA 10173 (Data Privacy Act of 2012), its IRR, and the current National Privacy Commission (NPC) issuances on consent, breach management, and DPO designation and registration. Note that licences issued by government agencies count as sensitive personal information;
   - RA 8792 (E-Commerce Act), including its rules on a service provider's liability for content posted by others;
   - RA 11967 (Internet Transactions Act of 2023), including whether Tax Buddy counts as an online marketplace or e-marketplace under it;
   - RA 7394 (Consumer Act);
   - RA 10175 (Cybercrime Prevention Act), as it relates to user reviews, public posts and impersonation;
   - the Civil Code on contracts and on the limits of liability waivers;
   - RA 9298 (Philippine Accountancy Act of 2004), as it relates to the accountants listed on the site and to who may call themselves a CPA.

   Cite a law only where it supports a clause, and check that the citation is current. Don't draft for GDPR, CCPA or other foreign laws. If you think one applies, raise it in your notes.
5. **Write in plain English.** Use short sentences and the reader as "you". Define the operator once as "Tax Buddy" ("we" is fine after that). Define the site's name once so it can be replaced, because the name isn't final (D01).
6. **Don't overstate.** Say nothing like "fully secure" or "guaranteed", and make no compliance-certification claims. Don't let any limitation of liability exclude what Philippine law forbids excluding, such as fraud, bad faith or gross negligence.
7. **Version each document.** The site records which version each user agreed to. Start each document with `Version: [FILL: YYYY-MM-DD]` and `Last updated: [FILL]`. Explain how changes are announced and when users must agree again.

## Fact sheet

### Operator (not yet recorded; use placeholders)
- Legal name, entity type and DTI/SEC registration: [FILL]
- Business address: [FILL]
- Website domain: [FILL] (still open, D01)
- Contact email for support, privacy and legal matters: [FILL]
- Data Protection Officer's name and contact, and NPC registration status: [FILL]
- Venue for disputes: [FILL]
- Don't claim any registered trademarks. The trademark clearance of the name "Tax Buddy" is open.

### What the site is
- A website (no app) that works like a professional social-media site for accountants: a public directory of profiles where accountants post their professional credentials and portfolios. It is aimed at the Philippines, is in English only, and shows times in Philippine time.
- **Open to all.** Anyone can view public pages without an account. Anyone with a Google account can register as a client or an accountant, with no approval step. For now the site is limited to invited testers (see Stage).
- **Self-published credentials.** Accountants describe their own credentials in free text they write themselves: their title (for example "Certified Public Accountant"), career summary, work history, services and portfolio highlights. There is no dedicated licence-number field, but nothing stops an accountant from typing a PRC licence number, a BIR accreditation number or similar details into these fields.
- **Public means public.** Profiles, highlights, reviews and replies are visible to anyone, including people without an account and, if indexing is switched on, search engines (see Search engines). Anyone can copy or screenshot public content, so removing it from the site can't recall copies.
- **Not a full social network.** There are no feeds, posts, follows, likes or comments; release 1 excludes them. Users interact through private one-to-one chat and through public reviews and replies.
- **Visitors** (not signed in) browse the directory and read accountant profiles, portfolio highlights and reviews.
- **Clients** (signed in) can also message accountants, review accountants who have replied to them, report content and block people.
- **Accountants** (signed in) publish and edit their own profile and portfolio, reply to client messages, reply publicly to reviews, report content and block people.
- **No verification.** Tax Buddy does not approve accountants or verify their licences, PRC/BOA registration, BIR accreditation, qualifications or identity. Profiles are listed as soon as their required fields are complete, and credentials are shown exactly as the accountant wrote them. Ratings, reviews, reports and blocking are the only safeguards. Whether to add licence checks before a public launch is open (D09).
- **No money.** The site has no prices, payments or bookings. Whether the site is free for both roles: [FILL: confirm]. Work, fees and deadlines are agreed directly between client and accountant. Tax Buddy is not a party to those agreements and gives no tax, accounting or legal advice.
- **Stage.** The site is in internal testing, at a test address limited to invited testers (D20). A public launch is a later decision. Draft an optional testing clause.

### Accounts
- Users sign in with Google only. Microsoft sign-in is PLANNED. There are no Tax Buddy passwords, and Tax Buddy never sees the Google password.
- At first sign-in the person chooses one role, client or accountant, once per account. They must tick "I agree to the Terms of use and the Privacy policy" to finish registering. The site stores the time of agreement and the version agreed to.
- The sign-in session lasts 14 days, renewed on use, or until sign-out.
- The minimum age has not been decided. Write it as `[DECISION: minimum age — recommend 18]` and give your reasoning.

### Personal data, by category
| Data | Source | Who can see it | Status |
|---|---|---|---|
| Name, email address and Google account identifier | Google, at sign-in | Tax Buddy. The name is shown to chat partners, and a reviewer appears as first name and last initial ("Liza F."). The email is used for notification emails (PLANNED) | Built |
| Role, plus the time and version of agreement | User | Tax Buddy | Built |
| Profile picture (optional): JPEG, PNG or WebP up to 5 MB, saved as a 400×400 image with location and camera data removed, in private cloud storage reached only through the site. Users can change or remove it; without one, initials are shown | User | An accountant's picture is public. A client's picture is visible to that client and to the accountants they have a conversation with | Built |
| Accountant profile: name, title, city, career summary, experience (role, company, start and end month), services from a fixed list (Bookkeeping, Tax filing, Payroll, Financial statements, Audit, Business registration, Tax advisory) each with a short description, and email, phone and website. To be listed, the accountant must consent to show their contact details publicly | Accountant | Public | Built |
| Portfolio highlights (up to 6): title, type of client, what was done, result. Client names may appear only with the client's permission | Accountant | Public | Built |
| Chat messages: plain text up to 2,000 characters, with sender, time and unread marks. One conversation per client and accountant; no attachments. Unsent text stays in that browser tab's session storage | Users | The two participants. Tax Buddy may read a reported conversation (see Moderation) | Built |
| Reviews: 1–5 stars plus optional text up to 1,000 characters. Allowed only after the accountant has sent at least one message; one per client per accountant. The client can edit or delete it; the accountant cannot edit, hide or delete it. The average rating and the review count are shown | Client | Public | Built |
| Review replies: one per review, up to 600 characters, editable but not deletable | Accountant | Public | Built |
| Reports: reporter, the item reported (profile, review or conversation), reason, optional details and time. One per person per item, at most 10 a day. The reported person isn't told who reported them | Reporter | Tax Buddy only | Built |
| Blocks: the time each side blocked the other in a conversation. While either side has blocked the other, neither can send messages. A client blocked by an accountant can't write or edit a review of them, but can delete their own. The blocked person isn't told why, and the blocker can undo the block | User | The participants (through the block's effects) and Tax Buddy | Built |
| Saved-accountants list | Client | That client only | PLANNED |
| Service workspaces: a service request (which shares the client's name and the requested service with that accountant), checklists, task statuses, shared notes with author and time, and private documents attached to a service. File types, limits, storage provider and retention are undecided (D26–D28) | Both | The two participants only | PLANNED |
| Notification emails about new messages. The email never contains the message text, and the user can turn these emails off from the email or their account. The sending frequency is undecided (D18) | Tax Buddy | The recipient | PLANNED |

### Cookies, browser storage and logs
- **Cookies:**
  - a sign-in cookie: HTTPS-only, not readable by scripts, lasts 14 days and is renewed on use;
  - a short-lived cookie that holds a first sign-in for a few minutes, until registration finishes;
  - a security (antiforgery) cookie that protects forms;
  - a theme cookie that remembers light or dark mode for one year, holds no personal data, and isn't set when the user picks "System".
- **Nothing else tracks users.** There are no advertising, analytics or tracking cookies, and no analytics or telemetry service. Fonts are served from the site itself.
- **Application logs** keep only event IDs, error categories, durations and correlation IDs. They never keep message text, email addresses, contact details or tokens. The hosting provider may keep its own request logs, including IP addresses: [FILL: confirm what it keeps and for how long].
- There is no automated decision-making or profiling, and no marketing email is planned: [FILL: confirm both].

### Service providers and cross-border transfers
- **Google:** sign-in.
- **Microsoft:** sign-in (PLANNED).
- **Supabase:** the database, plus private storage for profile pictures. The development project is in the Singapore region. Region of the live project: [FILL].
- **Render:** hosting, in a container in the Singapore region.
- **Brevo:** notification emails (PLANNED). Where Brevo processes data: [FILL].
- Personal data is therefore stored and processed outside the Philippines. Cover cross-border transfers and Tax Buddy's continuing accountability for its processors. Status of processor agreements with each provider: [FILL].

### Moderation
- There is no admin page. Reports are stored in the database, and the operator reads and handles them by hand.
- Reported content stays visible until the operator acts. The operator may remove content or close accounts that break the Terms.
- To handle a report about a conversation, the operator may read that conversation's messages, and only for that purpose.
- How quickly reports are handled, and the exact steps for removal, are open (D22).
- Only signed-in users can report through the site. People without an account have no way to report on the site, for example someone whose name or credentials are being misused. Their route for takedown requests: [FILL: contact route].

### Deletion and retention
- **Account deletion (PLANNED).** Users will delete their account on the site, after a confirmation step. Deletion removes the account, the accountant profile, the profile picture, the sign-in links and the notification settings. The profile leaves the directory, emails stop and the user is signed out.
- **Open decisions.** Give each one a [DECISION] placeholder with recommended options:
  - D07/D27: what happens to the other participant's copy of chats, reviews and service work after an account is deleted;
  - D14: how long chats, service work, documents and inactive accounts are kept;
  - D11: backups.
- **Available today.** Users can remove their profile picture and edit their own content, and clients can delete their own reviews.

### Search engines
- Public profile pages are built so search engines can index them. Whether indexing is switched on is open (D19); draft text that covers both outcomes.
- The invite-only test site asks search engines not to index it.

## What to produce

### 1. Terms of Use
At least these sections:
- acceptance and eligibility
- the testing stage (optional block)
- accounts and roles
- what Tax Buddy is and is not (not a party to engagements, no verification, no professional advice)
- public content: what is public, who can see it, and that copies can't be recalled
- accountants' obligations: accurate and current credentials and profiles; no impersonation, and no professional title they don't hold; holding any licence, registration or accreditation their services require; professional and confidentiality duties; consent for client names in highlights; their own responsibility as controllers of client data they receive
- clients' obligations, including a reminder to check an accountant's credentials with the official sources themselves (for example, the PRC's licence verification)
- acceptable use, including no scraping or bulk copying of profiles
- notice and takedown, including a route for people without an account
- reviews and replies, with the rules above
- reports, blocking and moderation
- user content and the licence granted to display it
- Tax Buddy's intellectual property
- third-party sign-in
- disclaimers and limitation of liability
- indemnity
- suspension and termination
- account deletion (optional block)
- changes to the Terms and re-acceptance
- governing law and disputes
- contact

### 2. Privacy Policy
At least these sections:
- who we are, and the DPO contact
- what we collect and where it comes from
- why we use it, with the lawful basis for each purpose under the DPA
- what is public and what is private, including what "public" means (anyone, possibly search engines, and copies)
- credentials and other details accountants choose to publish, including licence details as sensitive personal information
- who we share it with
- transfers outside the Philippines
- cookies and browser storage
- how long we keep data
- security, in general terms only
- breach notification
- your rights as a data subject (to be informed, to access, to object, to erasure or blocking, to damages, to data portability, to rectification, and to complain to the NPC), and how to exercise them, with a response time of [FILL]
- children
- changes
- contact

### 3. A short summary at the top of each document
Begin each document with a 4–6 bullet "The short version" summary.

### 4. Drafter's notes, kept separate from the documents
- **Placeholders:** every one, with the question, the options and your recommendation.
- **Issues for counsel**, at least:
  - whether Tax Buddy is an e-marketplace under RA 11967;
  - the risk of listing unverified accountants under RA 9298;
  - accountants publishing licence numbers or other licence details, which are sensitive personal information under the DPA. Cover the lawful basis, whether the existing consent to show contact details covers it, and whether the site should discourage licence numbers in free text;
  - Tax Buddy's liability as a platform for what users post (credentials, reviews, replies), and a notice-and-takedown process;
  - impersonation and false credentials, including use of the CPA title by someone who isn't registered;
  - scraping and reuse of public profiles;
  - cyber libel risk in reviews, and the duty to act on reports;
  - the operator reading reported conversations;
  - clients sharing TINs, tax returns or financial records in chat or service documents, since tax returns are sensitive personal information under the DPA, and whether the site should warn users;
  - whether one checkbox for both documents is valid consent under current NPC guidance. Say which processing should rest on consent, contract or legitimate interest, and recommend any change to the registration screen.
- **Coverage table:** each fact-sheet item and the section(s) that cover it, so nothing is missed.
- **Assumptions** you made.

**Format:** Markdown. One H1 per document and H2 for sections, with short paragraphs and bullet lists. The pages are read on phones, so keep each document as short as completeness allows.
```
