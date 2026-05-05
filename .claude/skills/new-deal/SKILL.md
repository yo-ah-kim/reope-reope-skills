---
name: new-deal
description: Capture new deal opportunities by scanning recent calendar meetings and emails for external contacts not yet in the Development pipeline. Proposes deal records for review before creating them in HubSpot.
---

# /new-deal — Create New Deals from Calendar & Email Activity

You are helping the CEO of Reope AS capture new deal opportunities by scanning recent calendar meetings and emails, then creating properly structured deals in HubSpot's Development pipeline.

## CRITICAL: USE MCP CONNECTORS ONLY

You MUST use these MCP tools for all data access. NEVER use web browsing, Chrome, or any browser-based tool.

- **Google Calendar:** `gcal_list_events`, `gcal_get_event`
- **Gmail:** `gmail_search_messages`, `gmail_read_message`, `gmail_read_thread`
- **HubSpot:** `search_crm_objects`, `get_crm_objects`, `manage_crm_objects`, `search_properties`, `hubspot-search-objects`, `hubspot-list-associations`, `hubspot-batch-read-objects`
- **Google Drive:** `google_drive_search`, `google_drive_fetch`

If any of these tools fail, report the error — do NOT fall back to web browsing.

## STEP 0: READ CONTEXT

Read these files before starting:
1. `~/.claude/Agent context/crm-schema.md` — Pipeline stages, deal properties, CRM schema
2. `~/.claude/Agent context/guardrails.md` — Safety rules (especially: confirm before any CRM write)

## STEP 1: ESTABLISH TIME WINDOW

Note the current date and time, then ask:

```
How many days back should I scan your calendar and emails?
(Default: 14 days)
```

Wait for Joachim's answer. Use the default if he just says "go" or similar.

## STEP 2: SCAN CALENDAR FOR EXTERNAL MEETINGS

Use `gcal_list_events` to pull all events in the time window.
- Set `timeZone` to `Europe/Oslo`
- Set `condenseEventDetails` to `false` to get attendees

Filter for **external meetings only** — meetings with at least one attendee whose email is NOT `@reope.com`.

For each external meeting, extract:
- Meeting title
- Date and time
- External attendees (name, email, domain)
- Meeting description (if any)
- Whether it has attachments (transcripts, docs)

## STEP 3: CROSS-REFERENCE WITH EXISTING DEALS

For each external meeting, check if a deal already exists:

1. **Search by company domain** — extract the domain from external attendee emails (e.g., `multiconsult.no` from `stian@multiconsult.no`), then search HubSpot companies by domain
2. **Search by contact email** — search HubSpot contacts by attendee email
3. **Check associated deals** — if a company or contact is found, check if they have active open deals using `hubspot-list-associations`

Classify each meeting:
- **Already has deal** — skip (note it for reference)
- **Has company/contact but no open deal** — potential new deal from existing relationship
- **No company or contact in CRM** — brand new opportunity

## STEP 4: GATHER CONTEXT FOR NEW DEAL CANDIDATES

For each meeting that could be a new deal:

**Email context:**
- Search Gmail for threads with the external attendee(s) within the time window
- Summarize the conversation topic in 2-3 lines (do NOT reproduce email content)
- Note: what was discussed, any project/scope mentions, next steps mentioned

**Meeting context:**
- Read the calendar event description/notes if available
- Check for Google Meet transcripts (search Gmail for `"transcript"` + attendee name/company)
- Check Google Drive for related documents (`google_drive_search` with company name)

**Company research:**
- If company not in HubSpot, note the domain for later creation
- Extract what you can about the company from the email/meeting context

## STEP 5: PROPOSE NEW DEALS

Present each candidate deal for review:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
NEW DEAL CANDIDATES — [N] found from last [X] days
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

[For reference: [N] meetings already had active deals — skipped]

── Candidate 1 ──────────────────────────────

Source meeting: [Meeting title] ([date])
Attendees: [names and emails]

Proposed deal:
| Field            | Value                                         |
|------------------|-----------------------------------------------|
| Deal name        | [Company] — [short project description]        |
| Pipeline         | Development                                    |
| Stage            | Meeting booked                                 |
| Amount           | [estimate if discussed, otherwise blank]        |
| Close date       | [3 months from today, or mentioned date]        |
| Owner            | Joachim Viktil                                  |
| Description      | [3-5 sentences based on email/meeting context]  |

Company: [name] ([domain]) — [In CRM / New — will create]
Contact: [name] ([email]) — [In CRM / New — will create]

── Candidate 2 ──────────────────────────────
[same format]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Actions:
- "create all" — create all proposed deals
- "create 1,3" — create specific candidates
- "edit 2" — modify a candidate before creating
- "skip 4" — remove a candidate
- "skip all" — cancel without creating anything
```

## STEP 6: HANDLE EDITS

If Joachim says "edit [N]":

Show the editable fields:
```
Editing Candidate [N]:

1. Deal name: [current value]
2. Stage: Meeting booked | Working on scope | Proposal sent | Contract sent
3. Amount (NOK): [current or blank]
4. Close date: [current]
5. Description: [current]

What would you like to change? (e.g., "2 working on scope" or "3 250000")
```

Apply changes and re-display the candidate for confirmation.

## STEP 7: CREATE DEALS

For each confirmed deal, in order:

### 7a. Create company (if new)
If the company doesn't exist in HubSpot:
- Create with: `name`, `domain`
- Note the new company ID

### 7b. Create contact (if new)
If the contact doesn't exist in HubSpot:
- Create with: `email`, `firstname`, `lastname` (parse from meeting attendee name)
- Associate with the company
- Note the new contact ID

### 7c. Create the deal
Use `manage_crm_objects` with `confirmationStatus: "CONFIRMED"` (since Joachim already confirmed in Step 5/6):

Properties:
- `dealname` — the proposed deal name
- `pipeline` — `default` (Development pipeline)
- `dealstage` — `2012068` (Meeting booked) or whichever stage was selected
- `amount` — deal value if provided
- `closedate` — the proposed close date (epoch ms)
- `hubspot_owner_id` — `1604195799` (Joachim)
- `description` — the generated description

Associations:
- Associate with the company (by company ID)
- Associate with the contact (by contact ID)

### 7d. Confirm creation
After each deal is created:
```
✅ Deal created: [Deal name]
   ID: [dealId]
   Link: https://app.hubspot.com/contacts/7504484/record/0-3/[dealId]
   Associated: [company name] + [contact name]
```

## STEP 8: WRAP UP

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
DEAL CAPTURE COMPLETE
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

Scanned: [X] days of calendar + email activity
External meetings found: [N]
Already had deals: [N] (skipped)
New deals created: [N]
  - [Deal 1 name] ([amount] NOK)
  - [Deal 2 name] ([amount] NOK)
New companies created: [N]
New contacts created: [N]

Total new pipeline value: [amount] NOK
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## STAGE SELECTION GUIDE

Help Joachim pick the right stage if he's unsure:

| If the conversation is about... | Suggested stage |
|--------------------------------|-----------------|
| Just had an intro meeting, exploring | Meeting booked (`2012068`) |
| Discussing specific project scope, requirements | Working on scope (`appointmentscheduled`) |
| Already sent or discussing a proposal/quote | Proposal sent (`presentationscheduled`) |
| Contract is being reviewed/signed | Contract sent (`114883342`) |

Default to "Meeting booked" if unclear — it's easy to advance later.

## DEAL NAMING CONVENTION

Generate deal names as: `[Company] — [Short project description]`

Examples:
- "Multiconsult — Revit coordination automation"
- "BIG — Custom Rhino-to-Revit workflow"
- "Snohetta — BIM template system"

If the project scope is unclear, use a generic: "[Company] — BIM development project"

## DESCRIPTION GENERATION

Write descriptions based ONLY on information found in emails, meetings, and docs. Include:
1. **What** — what the prospect needs (project type, problem to solve)
2. **Who** — key people involved and their roles
3. **Where** — how far along the conversation is
4. **When** — any timeline or deadline mentioned
5. **Why** — what's driving the need (if known)

Keep it factual and concise (3-8 sentences). Do not fabricate project details.

## RULES
- NEVER create deals without Joachim's explicit confirmation
- NEVER create deals in the Toolbox pipeline — this command is for Development only
- NEVER send emails — this is a CRM-only command
- If no new deal candidates are found, say so and suggest looking further back
- Default close date is 3 months from today unless Joachim specifies otherwise
- Amount can be left blank if not discussed — it's better to add it later than to guess
- When creating companies/contacts, use minimal properties — don't over-fill with assumed data
- Match the deal description language to the client's likely language (Norwegian companies = Norwegian is fine, international = English)
