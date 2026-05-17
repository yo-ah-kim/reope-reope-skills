# reope-skills

Source of truth for Reope's Claude skills. One directory per skill, each with a `SKILL.md`. Markdown only, no scripts, no secrets.

## Skills

- `board/` — 4-advisor strategic deliberation (`/board <question>`)
- `meeting-prep/` — Pre-meeting intelligence brief (`/meeting-prep <company>`)
- `new-deal/` — Capture new deals from calendar and email activity (`/new-deal`)
- `deal-triage/` — Interactive cleanup of stale HubSpot deals (`/deal-triage`)
- `gtd/` — Friday GTD weekly review (`/gtd`)
- `content/` — Blog post drafts from internal team conversations (`/content`)
- `contact-cleanup/` — Score and demote off-ICP HubSpot marketing contacts (`/contact-cleanup`)
- `proposal/` — Customized engagement-model slide for a specific prospect (`/proposal <company>`)

## What does NOT live here

Sensitive context (financials, team CVs, client revenue, time stats, CRM schema) stays out of git, even private git. Skills read from two local folders you populate per-machine. Set these up once, then skills work.

### `~/.claude/Board context/` (used by `/board`)

- `00-REOPE-CONTEXT-START-HERE.md` — Index and company snapshot
- `01-strategy.md` — REIMAGINE 2024-2027 strategy summary
- `02-financials.md` — P&L summaries with KPIs
- `03-team.md` — Team profiles and CVs
- `04-time-stats.md` — Utilization and project allocation
- `05-positioning.md` — Positioning draft
- `SemplicitaPro.otf` (optional) — Brand font for HTML output

These files are pre-digested from Drive sources so the board command runs instantly. Re-digest quarterly or after major strategic updates.

### Google Drive — `Agent context/` folder (used by `/gtd`, `/meeting-prep`, `/new-deal`, `/deal-triage`, `/contact-cleanup`, `/content`, `/proposal`)

Skills fetch these via the Google Drive MCP (`read_file_content`). The folder lives at the root of Joachim's My Drive: https://drive.google.com/drive/folders/105hQapsv9cbi4JqsfwHIDmJGKIzkc27J

| File | Drive ID | Purpose |
|---|---|---|
| `crm-schema.md` | `1EluEMDP0u4Z0ZHywYGcGaqR1nIOA_FhK` | HubSpot portal ID, pipeline stage IDs (Development + Toolbox: open, won, lost), deal properties, stage talking points. **Ground truth for pipeline IDs.** |
| `guardrails.md` | `1aXV4OiIa9Q8olQBhYoFl8Dxap_aiiSTD` | Safety rules every CRM/email-touching skill reads first (AI drafts/human sends, no silent CRM updates, draft-language matching, tone, scope limits). Banned words live in account-level Claude preferences, not here. |

Update the file in Drive when an ID changes — skills will pick it up on the next run. Don't duplicate the IDs into individual SKILL.md files.

### `~/Assistant/` (output, not input)

Skills write to:

- `~/Assistant/content/posts/` — blog post drafts
- `~/Assistant/content/log.md` — content topic history
- `~/Assistant/gtd/snapshots/` — weekly plan snapshots for plan-vs-reality comparison

## How to use

### Locally in Claude Code

Clone this repo, then symlink the whole skills directory (or each skill folder individually):

```bash
# Linux/Mac, symlink everything at once
git clone git@github.com:yo-ah-kim/reope-reope-skills.git ~/code/reope-skills
ln -s ~/code/reope-skills ~/.claude/skills
```

```powershell
# Windows, symlink each skill
git clone git@github.com:yo-ah-kim/reope-reope-skills.git C:\code\reope-skills
foreach ($skill in @('board','meeting-prep','new-deal','deal-triage','gtd','content','contact-cleanup','proposal')) {
  New-Item -ItemType SymbolicLink -Path "$env:USERPROFILE\.claude\skills\$skill" -Target "C:\code\reope-skills\$skill"
}
```

Verify by running `claude` and typing `/`. You should see all skills in the list.

### In Claude.ai (web and mobile)

Each skill here is the source. Paste the `SKILL.md` content into a Claude.ai Project's instructions, and upload the relevant Board context files as project knowledge (excluding anything you don't want stored in Anthropic's cloud). When you update a skill, push to this repo first, then sync to the relevant Claude.ai Project.

## Adding a new skill

1. Create a directory: `kebab-case-name/`
2. Add `SKILL.md` with frontmatter (`name`, `description`)
3. Add supporting files only if they're not sensitive
4. Sensitive context goes in `~/.claude/<context-name>/`, referenced from the skill, never committed
5. Commit with a message describing what the skill does

## Conventions

- Skills are markdown, not code. No bash, no python, no shell-outs.
- Voice rules and banned words live in your account-level Claude preferences, not in skills.
- Sensitive context (finances, CVs, client revenue) stays local in `~/.claude/<folder>/`, never in git.
- Use `~` for paths in skill files so they work on any OS.
