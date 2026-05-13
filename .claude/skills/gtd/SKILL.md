---
name: gtd
description: Friday GTD weekly review. Pulls last week's calendar, finds meeting follow-ups and action items, cross-references HubSpot deals, snapshots next week, and produces an action list.
---

# /gtd — Weekly GTD Review

You are running a Getting Things Done weekly review for the CEO of Reope AS. This review does three things:
1. **Look back** at this week — what happened vs what was planned, follow-ups sent, action items from meetings
2. **Look forward** at next week — snapshot upcoming meetings so next Friday we can compare
3. **Produce an action list** — concrete next actions coming out of the review

## CRITICAL: USE MCP CONNECTORS ONLY

You MUST use these MCP tools for all data access. NEVER use web browsing, Chrome, or any browser-based tool.

- **Google Calendar:** `list_events`, `get_event`, `list_calendars`
- **Gmail:** `search_threads`, `get_thread`, `create_draft`
- **Slack:** `slack_search_channels`, `slack_search_public`, `slack_read_channel`, `slack_read_thread`, `slack_read_user_profile`
- **HubSpot:** `search_crm_objects`, `get_crm_objects`, `manage_crm_objects` (use these to find deals, contacts, and their associations)

If any of these tools fail, report the error — do NOT fall back to web browsing.

## STEP 0: READ CONTEXT

Fetch these from Google Drive using `read_file_content`:
1. `crm-schema.md` — Drive ID `16BvMN7_HA5tKr88zEsFYa57DhsTAai8L` — Pipeline stage IDs (open/won/lost for Development and Toolbox), deal property names, portal ID, Joachim's `hubspot_owner_id`, stage talking points
2. `guardrails.md` — Drive ID `145j9a9woeUFISskCmnT4xfWFus3E2WG2` — Safety rules (never send email, only draft; match language to contact)

These files are the source of truth for HubSpot IDs and rules. Do not hardcode IDs inline in this skill.

## STEP 1: DETERMINE THE REVIEW WEEK

Calculate the current ISO week number and date range (Monday through Friday).
- **This week:** The Monday-to-Friday that includes today (or just ended, if running on Friday/weekend)
- **Next week:** The following Monday-to-Friday

Also check if a **previous snapshot** exists:
- Read `~/Assistant/gtd/snapshots/` directory
- Look for a file matching the current week: `week-YYYY-WNN.md`
- If it exists, this was last week's forward-looking plan — use it for comparison

## STEP 2: PULL THIS WEEK'S CALENDAR

Use `list_events` to get all events from Monday through Friday of this week.
- Use `timeMin` = Monday 00:00:00 and `timeMax` = Friday 23:59:59
- Set `timeZone` to `Europe/Oslo`
- Include attendees and attachments in the response

Categorize each event:
- **External meeting:** Has attendees with non-@reope.com email addresses
- **Internal meeting:** All attendees are @reope.com (or solo)
- **Blocked time:** No attendees, personal blocks, working location events
- **Cancelled:** Status is cancelled — note these separately

Count:
- Total meetings this week
- External meetings
- Internal meetings
- Hours in meetings

## STEP 3: CHECK FOR MEETING TRANSCRIPTS & NOTES

For each meeting this week (external and internal with >2 attendees):

1. **Check calendar event** for attachments using `get_event` — transcripts from Google Meet often appear as attachments or links in the event description
2. **Search Gmail** using `search_threads` for `"transcript" OR "meeting notes" OR "action items"` within 2 days after the meeting date, filtered to threads involving meeting attendees
3. If transcripts or notes found: scan for action items (lines starting with "- [ ]", "TODO", "Action:", or similar patterns). Summarize any found actions.

If no transcripts found for a meeting, just note it: "[No transcript found]"

## STEP 4: CHECK FOLLOW-UP EMAILS FOR EXTERNAL MEETINGS

For each **external meeting** that happened this week:

1. Identify the external attendees (non-@reope.com emails)
2. Search Gmail using `search_threads` with:
   - Query: `to:{external-email}` or `to:{external-domain}`
   - Filter to threads sent AFTER the meeting date
3. Classify:
   - **Follow-up sent** — Found an outbound email to external attendees after the meeting
   - **No follow-up yet** — No outbound email found after the meeting
   - **Same-day meeting** — Meeting was today, too early to expect follow-up

## STEP 5: CROSS-REFERENCE EXTERNAL MEETINGS WITH HUBSPOT DEALS

For each **external meeting** this week, check if the attendees or company are linked to an active HubSpot deal:

1. **Search for contacts** using `search_crm_objects` with `objectType="contacts"` — search by the external attendee's email address
2. **Get associated deals** using the HubSpot MCP (search/get crm objects with associations) — find deals associated with the matched contact or their company
3. **Check deal freshness** — for each associated open deal, compare `hs_lastmodifieddate` against the meeting date

Classify:
- **Deal updated after meeting** — CRM is current, no action needed
- **Deal NOT updated since meeting** — the meeting happened but the deal record is stale. Flag this.
- **No deal found** — external meeting with no associated deal. Could be a new opportunity or non-deal meeting. Note it but don't flag as an issue.

Only check open deals — use the open-stage IDs from `crm-schema.md`. Focus on the Development pipeline — Toolbox leads are typically handled differently.

## STEP 6: CHECK SLACK FOR ACTION ITEMS & FOLLOW-UPS

Scan Slack for relevant activity this week:

1. **Find channels** using `slack_search_channels` for key channels (general, sales, product, deals, etc.)
2. **Get recent history** using `slack_read_channel` for the most active/relevant channels, filtered to this week
3. **Look for action items directed at Joachim:**
   - Messages mentioning Joachim or @-mentioning him
   - Messages with keywords: "can you", "please", "action", "TODO", "follow up", "next step", "deadline"
   - Threads where Joachim was involved but may not have replied
4. **Cross-reference with meetings:** Check if any Slack conversations reference meetings from this week (people sometimes share notes or follow-ups in Slack instead of email)

Classify Slack items:
- **Needs response** — Joachim was asked something and hasn't replied
- **FYI** — Relevant updates, no action needed
- **Action item** — Explicit task assigned or committed to

Keep this lightweight — focus on items that need action, skip general chatter.

## STEP 7: COMPARE AGAINST LAST WEEK'S SNAPSHOT (if exists)

If a snapshot file exists for this week (`week-YYYY-WNN.md`):

Read it and compare the planned meetings against what actually happened:
- **Happened as planned** — meeting occurred at roughly the same time
- **Rescheduled** — meeting moved to different time/day
- **Cancelled** — meeting was in the plan but didn't happen
- **Added** — meeting happened this week but wasn't in last week's snapshot

This gives a "plan vs reality" view of how the week unfolded.

## STEP 8: SNAPSHOT NEXT WEEK

Use `list_events` to pull next week's calendar (next Monday through Friday).
- Request a condensed view (summary, time, attendees count)
- Set `timeZone` to `Europe/Oslo`

Save the snapshot to `~/Assistant/gtd/snapshots/week-YYYY-WNN.md` where YYYY-WNN is NEXT week's ISO year and week number.

Format the snapshot file:
```markdown
# Week Plan: W[NN] ([Mon date] - [Fri date])
Snapshot created: [today's date and time]

## Planned Meetings
| Day | Time | Meeting | Type | Attendees |
|-----|------|---------|------|-----------|
| Mon | 09:00 | BIG Dev Colab sync | External | 3 |
| Mon | 13:00 | Team standup | Internal | 5 |
| ... | ... | ... | ... | ... |

## Summary
- Total planned meetings: [N]
- External: [N]
- Internal: [N]
- Planned meeting hours: [X]h
```

## STEP 9: PRODUCE THE WEEKLY REVIEW

Output the full review in the terminal:

```
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
GTD WEEKLY REVIEW — W[NN] ([date range])
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

── THIS WEEK IN NUMBERS ─────────────────

Meetings: [N] total ([N] external, [N] internal)
Meeting hours: [X]h
Follow-ups sent: [N]/[N] external meetings
Action items found: [N] (meetings) + [N] (Slack)
Slack items needing response: [N]
Deals needing CRM update: [N]

[If snapshot comparison available:]
── PLAN vs REALITY ──────────────────────

Planned meetings: [N]
Actually happened: [N]
Added during week: [N]
Cancelled/moved: [N]

[List notable changes]

── EXTERNAL MEETINGS & FOLLOW-UPS ───────

[For each external meeting:]
✅ [Meeting name] ([day]) — Follow-up sent [date]
⚠️ [Meeting name] ([day]) — No follow-up sent yet
📅 [Meeting name] (today) — Too recent for follow-up

── ACTION ITEMS FROM MEETINGS ───────────

[From transcripts/notes, if found:]
• [Action item] — from [meeting name] ([day])
• [Action item] — from [meeting name] ([day])

[If no transcripts found:]
No meeting transcripts found this week.

── DEAL CRM STATUS ─────────────────────

[For each external meeting with an associated deal:]
⚠️ [Meeting name] ([day]) → [Deal name] ([stage]) — NOT updated since meeting
✅ [Meeting name] ([day]) → [Deal name] ([stage]) — updated [date]
➖ [Meeting name] ([day]) — No associated deal

[If no external meetings this week:]
No external meetings to cross-reference.

── SLACK ACTION ITEMS ──────────────────

[If items found:]
🔴 [Channel] — [Person]: "[message summary]" — Needs response
📌 [Channel] — [Person]: "[action item summary]" — Action needed
ℹ️ [Channel] — "[update summary]" — FYI

[If nothing actionable:]
No pending Slack items this week.

── MISSING FOLLOW-UPS (ACTION NEEDED) ───

[List external meetings with no follow-up, sorted by date:]
1. [Meeting name] ([day]) — [attendees] — needs follow-up
2. [Meeting name] ([day]) — [attendees] — needs follow-up

── NEXT WEEK PREVIEW ────────────────────

[Quick overview of next week's meetings:]
Mon: [N] meetings ([list key ones])
Tue: [N] meetings ([list key ones])
Wed: [N] meetings ([list key ones])
Thu: [N] meetings ([list key ones])
Fri: [N] meetings ([list key ones])

External meetings requiring prep: [N]
[List them — these are candidates for /meeting-prep]

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
```

## STEP 10: OFFER ACTIONS

After the review, offer:

```
What would you like to do?
1. Draft follow-up emails for missed follow-ups
2. Run /meeting-prep for a specific meeting next week
3. Run /deal-triage to clean up stale deals while we're reviewing
4. Save this review to a file
```

If Joachim asks to draft follow-ups:
- For each missing follow-up, use `create_draft` to draft a short email referencing the meeting (NEVER send)
- Keep drafts concise: "Hi [name], thanks for the meeting on [day]. [Reference key topic if known from transcript/calendar description]. [Suggest next step based on context]."
- Apply the language-matching rule from `guardrails.md`

## RULES
- NEVER use web browsing, Chrome tools, or browser-based tools — use ONLY the MCP connectors listed above
- This is primarily a READ-ONLY review — it reads calendar, email, Slack, and snapshots
- Only WRITE operations: saving the next-week snapshot file, and drafting emails if asked
- Never send emails — only create Gmail drafts (see `guardrails.md`)
- Never post Slack messages — only read
- If a meeting has many attendees, focus on the primary external contact
- Keep the review scannable — Joachim should absorb it in 3-5 minutes
- Transcripts may not exist for most meetings — don't treat this as a failure, just note it
- For the plan vs reality comparison, a meeting counts as "same" if it has the same title and happened on the same day (time changes are noted but not flagged as "cancelled")
