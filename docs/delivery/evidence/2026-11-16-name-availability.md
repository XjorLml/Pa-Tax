# Tax Buddy name availability survey

Preliminary web survey for [PRD D01](../../PRD.md#open-decisions) and the open [F02 rename task](../../../specs/brand-design-refresh/tasks.md). **This is not legal or trademark clearance**, and nothing was decided.

## Identity and attribution

- Date: 2026-11-16 (+08:00).
- Observer: agent-observed. A Claude Code subagent ran web searches and opened pages, at the user's instruction (scope chosen 2026-11-16: "Check name availability"). The main agent did not reopen every source.
- Requirements/scenarios: PRD D01; F02 BD07 open task.
- Source revision: not applicable (web research); repository at `af9d3ea` plus the uncommitted F02 BD09 work.
- Environment: web search and page fetches from the agent's tools. No WHOIS, trademark-database sessions or paid services. Only the name and generic terms were searched; no personal or project details were sent.

## Execution

### Existing uses of the name

| Name / use | Country | Sector | Active? | Source |
|---|---|---|---|---|
| **TaxBuddy PH**, "BIR Tax Filing for Filipino Online Sellers": a ₱199/month tool for Shopee, Lazada and TikTok Shop sellers. No legal entity or trademark claim is shown | Philippines | Tax / fintech | Yes. The page was opened; its footer reads "© 2026 TaxBuddy PH" | https://taxbuddyph.com |
| **TaxBuddy.com**: ITR/GST filing platform from SSBA Innovations, Mumbai | India | Tax / fintech | Yes. The page was opened | https://taxbuddy.com/ |
| "Income Tax Filing by TaxBuddy" (SSBA Innovations), also listed in the Philippine App Store | India, sold in the Philippines | Tax app | Yes | https://apps.apple.com/ph/app/income-tax-filing-by-taxbuddy/id1494133161 |
| "TaxBuddy: Income Tax & Finance" on Google Play (`com.taxbuddy.gst`). Install figures come only from third-party trackers and are unverified | India | Tax app | Yes | https://play.google.com/store/apps/details?id=com.taxbuddy.gst |
| Taxbuddies Inc., a tax preparation firm | US | Tax services | Mark live (below); business not verified | https://www.trustindex.io/reviews/taxbuddies.net |
| TaxBuddies Business & Tax Solutions Corp. | Canada | Accounting | Directory listing only | https://www.leadstal.com/sb/taxbuddies-business-and-tax-solutions-corp |
| MyTaxBuddy (iOS, UK); "Tax Buddy planner" (Google Play, India) | UK, India | Tax apps | Unverified, or apparently dormant | Store pages, as found by search |

No Philippine app named Tax Buddy turned up in the app stores. Nearby Philippine tax apps: TaxPal PH, Taxmate, Taxa PH, TallyPH.

### Trademarks

These are US records, read on USPTO TSDR by the subagent:

| Mark | Serial / registration | Owner | Goods / services | Status |
|---|---|---|---|---|
| TAXBUDDY | SN 88506066 / Reg. 6789980 | Syntegrity LLC | Class 9: loyalty and rewards app software (not tax) | Registered, live (2022-07-12) |
| TAXBUDDIES | SN 90139552 / Reg. 6790424 | Taxbuddies Inc. | Class 35: income tax preparation and advice | Registered, live (2022-07-12) |
| TAXBUDDY | SN 98209696 | SSBA Innovations Limited US, LLC | Class 35 | Dead, abandoned 2024-08-29 |

Not searched directly: IPOPHL (Philippines), IP India, the WIPO Global Brand Database (it needs a CAPTCHA) and EUIPO. Search engines found no Philippine or Indian record, which does not show that none exists.

### Domains

WHOIS was not run, so registration is not confirmed for any of these.

| Domain | Observation |
|---|---|
| taxbuddy.com | In use: TaxBuddy India |
| taxbuddyph.com | In use: TaxBuddy PH |
| taxbuddy.ph, taxbuddy.com.ph | Did not load (TLS error); registration unconfirmed |
| taxbuddy.app | Responds with an empty page; probably registered |
| taxbuddy.io, .co, .ai | Not visited |

## Assessment and limits

The subagent rated the conflict risk **high** for a Philippine tax and accounting site. The main reasons:
- A live Philippine product, TaxBuddy PH, already uses the exact name for BIR tax help.
- India's TaxBuddy owns taxbuddy.com and lists its app in the Philippine stores.
- In the US, TAXBUDDIES is registered for tax services.

The site would compete with both for search results and recognition, and might face a dispute with the Philippine operator.

What remains for the owner:
- Search IPOPHL by hand at https://wipopublish.ipophil.gov.ph/ for TAXBUDDY, TAX BUDDY and TAXBUDDIES in Nice classes 9, 35, 36 and 42. Also search WIPO at https://branddb.wipo.int/ and USPTO at https://tmsearch.uspto.gov/.
- Check domain availability with a registrar.
- Then keep the name or choose another (D01).

Earlier name sketches remain in [brand-options.html](../../../prototypes/brand-options.html). Acceptance decision: none. D01 stays open.
