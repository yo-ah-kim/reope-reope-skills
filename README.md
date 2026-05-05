# reope-skills

Source of truth for Reope's Claude skills. One directory per skill, each with a `SKILL.md`. Markdown only, no scripts, no secrets.

## What lives here

- `board/` — 4-advisor strategic deliberation (`/board <question>`)
- (more skills to follow: deal-triage, new-deal, content, weekly-review)

## What does NOT live here

Sensitive context (financials, team CVs, client revenue, time stats) stays out of git, even private git. The `board` skill reads from `~/.claude/Board context/`, a local folder you populate per-machine. Set this up once, then the skill works.

The Board context folder should contain:

- `00-REOPE-CONTEXT-START-HERE.md` — Index and company snapshot
- `01-strategy.md` — REIMAGINE 2024-2027 strategy summary
- `02-financials.md` — P&L summaries with KPIs
- `03-team.md` — Team profiles and CVs
- `04-time-stats.md` — Utilization and project allocation
- `05-positioning.md` — Positioning draft
- `SemplicitaPro.otf` (optional) — Brand font for HTML output

These files are pre-digested from Drive sources so the board command runs instantly. Re-digest quarterly or after major strategic updates.

## How to use

### Locally in Claude Code

Clone this repo, then symlink (or copy) the skill folders into your Claude skills directory:

```bash
# Linux/Mac
git clone git@github.com:yo-ah-kim/reope-reope-skills.git ~/code/reope-skills
ln -s ~/code/reope-skills/board ~/.claude/skills/board
```

```powershell
# Windows
git clone git@github.com:yo-ah-kim/reope-reope-skills.git C:\code\reope-skills
New-Item -ItemType SymbolicLink -Path "$env:USERPROFILE\.claude\skills\board" -Target "C:\code\reope-skills\board"
```

Verify by running `claude` and typing `/`. You should see `board` in the list.

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
