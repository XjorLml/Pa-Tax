# Drafting notes: terms of use and privacy policy

**Status:** working notes for the owner, not an authority. On 2026-11-21 an AI model (Claude) ran the [drafting prompt](terms-privacy-drafting-prompt.md) and produced the [Terms of Use](terms-of-use.md) and the [Privacy Policy](privacy-policy.md). Later that day the owner approved both as version `2026-11-21` without a lawyer's review, because there is no lawyer, and the site shows them (F11). Nothing here is legal advice. The laws cited below were checked only against the secondary sources listed in section 6. The issues in section 2 are the owner's risk until a lawyer looks at them; a review before the public launch is still advised.

## Decisions made on 2026-11-21

The user answered 16 questions in this session, choosing the recommended option except where noted. They are applied to the drafts, recorded in the [PRD](../PRD.md#open-decisions), and the product changes are in the prototype:

1. Licence numbers on profiles: discouraged (Option A), with a hint in the profile editor.
2. Minimum age: 18.
3. D19: search engines may index public pages from the public launch.
4. D07: chats stay with the other person, shown as from "Deleted user". A deleted client's reviews are removed; a deleted accountant's profile, reviews and replies go with it.
5. D22: every report is reviewed within 10 working days. The user chose this over the recommended 5 days, with urgent reports in 2.
6. Registration box: "I agree to the Terms of use and have read the Privacy policy."
7. Chat: the warning is in both documents, and a hint sits above the message box.
8. Operator: a sole proprietorship registered with the DTI.
9. Fees: free now; any future fee is announced in advance and charged only with agreement.
10. DPO: the owner.
11. D14: an account unused for 2 years is deleted after a warning email. The user chose this over the recommended 3 years.
12. D09: no licence checks; the "doesn't check licences" note on profiles stays and now points to the PRC.
13. Changes: users agree to a materially changed version at their next sign-in.
14. Notice: 30 days before a material change.
15. Testers: accounts carry over to the public site. The user chose this over the recommended "ask each tester".
16. Build: the prototype now, and the site in a follow-up.

Later the same day the user gave the operator details and four more answers:

17. **Operator:** Lemnuel Lumaban, based in Manila, Metro Manila, with the contact email lumaban.lemnuel.1@gmail.com. This replaces answer 8: there is no DTI registration.
    - The user named the business "xjor.inc". The documents don't use it: a trading name other than the owner's own needs DTI registration (Business Name Law, Act No. 3883 as amended), and "Inc." suggests a corporation registered with the SEC.
    - Add the business name once it is registered, as "Lemnuel Lumaban, doing business as …".
18. **D27, partly:** documents someone uploaded to a service are deleted with their account.
19. **D22 removal steps:** "create simple steps". They are in [RELEASE](../RELEASE.md#acting-on-a-report-d22).
20. **No lawyer:** the owner approved the text as version `2026-11-21` without a lawyer's review.
21. **Build the site follow-up:** F11, [specs/legal-pages](../../specs/legal-pages/spec.md).

On 2026-11-23 account deletion arrived on the site ([F12](../../specs/account-deletion/spec.md)). Both documents moved to version `2026-11-23`: "email us until deletion is available" became deletion from the Account page, and the Privacy Policy now says what remains of a deleted account (a nameless record that the other person's chats point to).

To publish, the agent filled the remaining minor placeholders with the defaults recommended in section 1. The owner can change any of them:
- liability cap PHP 5,000;
- 30 days to resolve a complaint before going to court, and the courts of Manila;
- privacy requests answered within 15 working days, free of charge, with identity checked by writing from the account's email;
- reports kept for 12 months after handling;
- a conversation is deleted once both people have deleted their accounts;
- logs and backups described as kept by the providers for a limited time;
- the security cookie lasts until the browser closes;
- NPC registration stated as "not currently registered";
- no automated decisions, no marketing emails and no sale of data, all matching the site as built.

## 1. Placeholders and open decisions

"T" means the Terms of Use and "P" the Privacy Policy, followed by the section number.

| # | Placeholder | Where | Recommendation |
|---|---|---|---|
| 1 | Operator's legal name, entity type, DTI/SEC registration, address | T1, T24; P1, P15 | Entity decided 2026-11-21: a DTI sole proprietorship. The owner's name, business name, registration number and address are still needed before any real user signs in |
| 2 | Domain (D01) | T1; P1 | Choose it together with the name decision. The name has a high conflict risk (D01 survey) |
| 3 | Contact email for support, legal matters and takedowns | T2, T4, T12, T20, T22, T24; P12 | One monitored address. A separate takedown address is optional |
| 4 | DPO name, email and address; NPC registration | P1, P11, P15 | Decided 2026-11-21: the owner is the DPO. A privacy email address is still needed. Check registration thresholds (issue 2.16) |
| 5 | Version label and last-updated date | Both | Use `YYYY-MM-DD` and set `LegalText.CurrentVersion` to the same value |
| 6 | Free of charge for both roles | T5 | Decided 2026-11-21: free now; any future fee announced in advance and charged only with agreement |
| 7 | Minimum age | T3; P12 | Decided 2026-11-21: 18 |
| 8 | Testers' data at public launch | T2 | Decided 2026-11-21: accounts carry over. Tell testers when they join |
| 9 | D09: licence checks before launch | T5 | Decided 2026-11-21: no checks; the profile note stays and points to the PRC |
| 10 | D19: search-engine indexing | T6; P4 | Decided 2026-11-21: yes, from the public launch |
| 11 | D22: report response time | T12 | Decided 2026-11-21: 10 working days for every report. Removal steps still open |
| 12 | Takedown route for people without an account | T12 | An email address now. An in-site form later (section 3) |
| 13 | Licence numbers on profiles (Option A or B) | T7.8; P2 | Decided 2026-11-21: Option A, discouraged (issue 2.3) |
| 14 | Warning about sensitive information in chat | T8.2; P13 | Decided 2026-11-21: in both documents, with a hint above the message box (issue 2.10) |
| 15 | D07/D27: other participant's copy after account deletion | T20; P7 | D07 decided 2026-11-21 (see the list above). Still open: D27 for service work, and what happens when both people in a conversation have deleted their accounts (recommended: delete it) |
| 16 | D14: retention periods; inactive accounts | P7 | Inactive accounts decided 2026-11-21: deleted after 2 years, following a warning email. Service documents follow D27 |
| 17 | D11: backups | P7 | State how long backups are kept once the database plan is chosen |
| 18 | Report retention | P7 | Keep for the time needed to handle the report and any follow-up, then a fixed period |
| 19 | Log retention; what the hosting provider logs, including IP addresses | P2, P7 | Check Render's request-log contents and retention |
| 20 | Live Supabase region; Brevo and Google processing locations | P5, P6 | Confirm when the test-site project is created |
| 21 | Processor agreements | P5, P6 | Accept each provider's data processing terms and record them |
| 22 | Confirmations: no automated decisions, no marketing email, no sale of data | P3 | Confirm, or remove the sentence |
| 23 | Security cookie duration and content | P8 | Check the antiforgery cookie in the browser |
| 24 | Rights requests: identity check, response time, fees | P11 | For example, reply from the account's email address, respond within 15 working days, free of charge. Confirm against NPC rules |
| 25 | Notice of changes: how, and how many days ahead | T21; P14 | Decided 2026-11-21: 30 days, on the Site, plus email once R08 exists |
| 26 | Re-acceptance of a new version | T21; P14 | Decided 2026-11-21: at the next sign-in. Build it before the first material change (section 3) |
| 27 | Liability cap | T18 | A fixed peso amount. Counsel to set it |
| 28 | Complaint period before going to court; venue | T22 | 30 days. Venue in the operator's city |
| 29 | Microsoft sign-in, emails, deletion, saved list, service work | Optional blocks | Switch each on when its feature launches. Service documents need D26–D28 and issue 2.10 first. 2026-11-23: version `2026-11-23.2` switched on the saved list and service work with documents (F03), filled in D26/D28 as decided, and described the rest of the service work after deletion as built. Microsoft stays off: the owner archived Microsoft sign-in the same day (F15). Awaiting the owner's approval; the privacy impact assessment (item 9 below) is still not decided |

## 2. Issues for counsel

**2.1 Internet Transactions Act (RA 11967).** The Act covers online merchants selling goods or services through e-marketplaces or digital platforms. Platforms are subsidiarily liable, and in some conditions solidarily liable, for example when they fail to act on notice. The DTI reports that the Act has been fully in effect since 20 June 2025. Tax Buddy shows no prices and takes no payments or bookings, and engagements are agreed between the parties, though they may first make contact in chat. *Question:* is Tax Buddy an e-marketplace or digital platform that "facilitates" transactions under the Act and its IRR? If it is, the Act's duties on merchant information, complaints and takedowns apply. *Draft:* T5 (not a party), T12 (takedown route).

**2.2 Unverified accountants and RA 9298.** The Philippine Accountancy Act restricts the CPA title, and the practice of accountancy, to registered CPAs. Secondary sources cite Section 26 and the IRR; verify the section and its penalty. The Site displays credentials exactly as accountants type them. *Risks:* misleading claims shown to consumers, and the platform's diligence duties if 2.1 applies. *Draft:* T5, T7.1–7.3, T8.1 (check with the PRC). *Recommendations:* decide D09; consider a visible "credentials not verified by Tax Buddy" note on profiles (a DESIGN change); handle reports of false credentials first.

**2.3 Licence details are sensitive personal information.** DPA Section 3(l)(3) includes information issued by government agencies, such as "licenses or its denials, suspension or revocation". Under Section 13, such information may be processed only on limited grounds, such as specific prior consent. The Site's current consent covers public contact details only. *Draft:* Option A or B in T7.8 and P2. *Recommendation:* Option A, plus a hint in the profile editor. If Option B is chosen, add a separate, specific consent in the editor.

**2.4 Consent and the single agreement checkbox.** Registration has one box: "I agree to the Terms of use and the Privacy policy." Commentary on NPC Circular 2023-04 (Guidelines on Consent) says consent that isn't needed to provide a service must not be bundled with it or made a condition of it. Sources disagree on the effective date (29 November 2023 or 27 May 2024), so verify both points in the official text. The Circular is read together with NPC Advisory 2023-01 on deceptive design patterns. *Draft:* P3 relies on contract and legitimate interest for processing needed to run the Site, and on consent only for public contact details (and licence details under Option B). *Recommendations:*
- Reword the box as agreement to the Terms plus acknowledgment of the Privacy Policy: "I agree to the Terms of use and have read the Privacy policy" (approved 2026-11-21).
- Keep specific consents where they are given, in the profile editor.
- Document legitimate-interest assessments under the NPC's guidelines on legitimate interest; confirm the circular number.

**2.5 Liability for user content; notice and takedown.** RA 8792 Section 30 protects a service provider that "merely provides access", on conditions: no actual knowledge, no direct financial benefit, and no direct infringement. It preserves contract obligations, regulatory regimes and court injunctions. Tax Buddy hosts and displays content, which may go beyond "access". *Draft:* T12 (no prior check, no duty to monitor, acts on reports, route for people without an account). *Recommendation:* define the takedown procedure under D22 and keep records of actions.

**2.6 Cyber libel in reviews and replies.** RA 10175 Section 4(c)(4) applies. In *Disini v. Secretary of Justice* (G.R. No. 203335, 2014), the Supreme Court held the original author liable and struck down "aiding or abetting" as applied to cyber libel (verify). Reviews of named professionals are prone to defamation claims, and replies risk breaching client confidentiality. *Draft:* T9, T10 standards, T12. Reported content stays visible until the owner acts (BR12). Counsel should confirm that this is acceptable for clearly defamatory content.

**2.7 Impersonation and false credentials.** Computer-related identity theft (RA 10175 Section 4(b)(3)) and the CPA-title rule (2.2) both apply. Only signed-in users can report through the Site, so a professional being impersonated may have no account. *Draft:* T7.4, T9, T12 (email route).

**2.8 Scraping.** Public contact details invite harvesting. *Draft:* T9 prohibits it. *Recommendation:* consider technical limits. Counsel to advise on the DPA status of publicly displayed contact details.

**2.9 Reading reported conversations.** The owner reads a reported conversation only to handle the report ([RELEASE](../RELEASE.md#reading-user-reports-r15-d22)). The basis is legitimate interest, plus establishing or defending legal claims. *Draft:* T11, T12; P3, P13. *Recommendation:* a short internal procedure covering who may read, what is recorded, and how long notes are kept.

**2.10 Sensitive information in chat and service documents.** Clients are likely to send TINs, tax returns and financial records. Tax returns are sensitive personal information (DPA Section 3(l)), and TINs arguably are too. Chat is plain text and isn't designed for documents. Service documents (F03, PLANNED) will hold them. *Recommendations:* the chat warning (placeholder 14); a privacy impact assessment and the NPC security requirements before F03 accepts real files; a Section 13 basis for service documents.

**2.11 Transfers outside the Philippines.** Data is held in Singapore (Render; Supabase's development project), and Google and Brevo may process it elsewhere. DPA Section 21 keeps Tax Buddy accountable. *Recommendation:* outsourcing or processing agreements with Supabase, Render and Brevo. Google acts as an independent controller for sign-in.

**2.12 Minimum age.** 18 is recommended: contracts made by minors are voidable, and accountants offer professional services. If a lower age is chosen, review the NPC's guidance on transparency for children.

**2.13 Visitors and the Terms.** Visitors never tick a box, so enforcing the Terms against them (for example the scraping ban) depends on notice. The footer links to the Terms on every page (R16). Counsel to advise whether that is enough.

**2.14 Limitation of liability.** Under the Civil Code, a waiver of liability for future fraud is void (Article 1171), and courts may moderate liability for negligence (Article 1172); verify both. Contracts of adhesion are read against the drafter, and the Consumer Act limits waivers. *Draft:* T17–T18 carve out fraud, bad faith, gross negligence and wilful misconduct.

**2.15 Venue.** Under the Rules of Court, a venue clause is exclusive only if it uses restrictive words; verify. *Draft:* T22 says "exclusive jurisdiction" and keeps complaints to the DTI and NPC open.

**2.16 DPO designation and NPC registration.** Check the current registration thresholds under NPC Circular 2022-04 (verify). They are understood to cover controllers with at least 250 employees, those processing the sensitive personal information of at least 1,000 people, and high-risk processing. Licence details (2.3) and service documents (2.10) could bring Tax Buddy within them.

**2.17 Changes and re-acceptance.** The Site records the version each user agreed to (`draft-2026-11-16` today), but it doesn't yet ask users to agree to a new version. NPC commentary suggests fresh consent may be needed when terms change. *Draft:* T21 and P14 leave this as a decision.

## 3. Product changes these drafts suggest

State as of 2026-11-21:

1. Registration checkbox wording (2.4): **approved**. In the prototype; the site follows.
2. A licence-number hint in the profile editor (2.3): **approved** (Option A, so no extra consent). In the prototype; the site follows.
3. A "credentials not verified" note on profiles (2.2): **already existed** on the profile and in the footer. The profile note now points to the PRC in the prototype; the site follows.
4. A warning about sensitive information near the chat box (2.10): **approved**. In the prototype; the site follows.
5. Asking users to agree again when the version changes (2.17): **approved**. Still to build in the site before the first material change.
6. Deleting accounts after 2 years without use (D14): **approved**. Needs R08 email and a cleanup job.
7. A way for people without an account to report content (2.7): not decided; an email route for now.
8. Account deletion (R09) before public launch, so users can exercise their rights themselves: not decided; D07 is now settled for it.
9. A privacy impact assessment before F03 accepts real documents (2.10): not decided.

In the prototype, D22's 10 working days also appear on the report page. The legal pages show the long-document layout: the short version, contents, numbered sections with a summary of each draft section, and the cookie table.

## 4. Coverage of the fact sheet

| Fact-sheet item | Terms | Privacy |
|---|---|---|
| Operator details | 1, 24 | 1, 15 |
| Social-media-style public site, open to all | Short version, 3, 5, 6 | Short version, 4 |
| Self-published, unverified credentials | 5, 7.1–7.3, 8.1, 17 | 2 (Credentials) |
| Public content and copies | 6 | 4 |
| No feeds, posts, follows, likes or comments | 5 (what the Site offers) | — |
| No prices, payments or bookings; not a party; no advice | 5 | 2 (no payment details) |
| Testing stage (D20) | 2 | 4 (not indexed while testing) |
| Google sign-in; Microsoft planned; no passwords | 4, 15 | 2, 5, 9 |
| One role, agreement time and version | 4, 21 | 2, 14 |
| 14-day session | — | 8 |
| Minimum age | 3 | 12 |
| Name, email and Google identifier | 4 | 2, 4 |
| Profile picture and its visibility | 6, 13 | 2, 4, 9 |
| Accountant profile and contact-details consent | 6, 7.5 | 2, 3, 4, 11 |
| Portfolio highlights and client permission | 6, 7.6 | 2 |
| Chat: text only, 2,000 characters, one per pair, participants only, draft in the tab | 11 | 2, 4, 8 |
| Reviews and replies rules | 10 | 2, 4 |
| Reports: once per item, 10 a day, reporter not named | 12 | 2, 4 |
| Blocks and their effect on reviews | 10, 12 | 2, 4 |
| Saved list, service work, emails (planned) | 20 (optional) | 2, 3, 4, 7 (optional) |
| Cookies, session storage, no tracking, local fonts | — | 8 |
| Logs; hosting provider's logs | — | 2, 7 |
| No automated decisions or marketing (to confirm) | — | 3 |
| Providers and regions; transfers | 15 | 5, 6 |
| Moderation, reading reported conversations, D22 | 11, 12 | 3, 13 |
| Route for people without an account | 12 | — |
| Account deletion (planned); D07, D14, D11 | 20 | 7 |
| Search engines (D19) | 6 | 4 |

## 5. Assumptions

- The operator is a business registered in the Philippines, and its users are mainly in the Philippines. Foreign privacy laws are not addressed.
- "When the law requires it" disclosures (T11; P5) reflect general legal obligations, not a recorded practice.
- "We limit how often messages and reports can be sent" (P9) rests on the 10-reports-a-day rule (BR12) and the chat limit of 20 messages a minute built in F07 ([STATUS](../delivery/STATUS.md)).
- Session storage being cleared when the tab closes is standard browser behaviour.
- Until R09 is built, account deletion is handled by email request.
- Links between the drafts point to the files in this folder. On the Site they become `/terms` and `/privacy`.
- "The PRC's online licence verification" refers to the PRC's public verification service. Add its address when publishing.

## 6. Sources consulted on 2026-11-21

These are secondary sources; counsel should verify each point against the primary text.

- RA 11967: [LawPhil text](https://lawphil.net/statutes/repacts/ra2023/ra_11967_2023.html), [DTI news](https://dti.gov.ph/news/president-marcos-signs-internet-transactions-act-ita-law-strengthen-e-commerce-ph), [Forvis Mazars summary](https://www.forvismazars.com/ph/en/insights/corporate-secretarial-alerts/internet-transactions-act-of-2023), [SunStar on full enforcement](https://www.sunstar.com.ph/davao/dti-begins-full-enforcement-of-e-commerce-law-liability-rules-2)
- NPC Circular 2023-04: [Baker McKenzie via Global Compliance News](https://www.globalcompliancenews.com/2023/11/18/https-insightplus-bakermckenzie-com-bm-data-technology-philippines-national-privacy-commission-issues-comprehensive-guidelines-on-consent_11162023/), [Digital Policy Alert](https://digitalpolicyalert.org/event/15573-adopted-national-privacy-commission-guidelines-on-consent-circular-no-2023-04), [ACCRALAW commentary](https://accralaw.com/?p=16522), [NPC draft for consultation](https://privacy.gov.ph/wp-content/uploads/2023/05/DRAFT-Circular-Guidelines-on-Consent-For-Public-Consultation.pdf)
- RA 9298: [LawPhil text](https://lawphil.net/statutes/repacts/ra2004/ra_9298_2004.html), [PRC IRR](https://www.prc.gov.ph/uploaded/documents/IRR_cpa.pdf), [digest](https://www.digest.ph/laws/philippine-accountancy-act-of-2004)
- RA 8792 Section 30: [Lawyerly text](https://lawyerly.ph/laws/view/lf30), [Stanford WILMAP](https://wilmap.stanford.edu/node/29600)
- DPA Sections 3(l), 12, 13, 16–18 and 21, the Civil Code articles, the Rules of Court and *Disini*: from the drafter's knowledge, not re-checked on this date.
