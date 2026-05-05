# reope-skills

Source of truth for Reope's Claude skills. One directory per skill, each with a `SKILL.md` plus any supporting files. Markdown only, no scripts, no secrets.

## What lives here

- `strategic-board/` — 4-advisor deliberation on strategic questions
- (more skills to follow: sales, content, weekly-review)

## How to use

### Locally in Claude Code

Clone this repo somewhere stable, then symlink it into your Claude skills directory:

```bash
git clone git@github.com:reope/reope-skills.git ~/code/reope-skills
ln -s ~/code/reope-skills ~/.claude/skills
```

Verify by running `claude` and typing `/`. You should see `strategic-board` in the list.

### In Claude.ai (web and mobile)

Each skill here is the source. Copy the `SKILL.md` content into a Claude.ai Project's instructions, or upload the directory contents as project knowledge. When you update a skill, push to this repo first, then sync to the relevant Claude.ai Project. Don't edit the Claude.ai copy directly, it'll drift.

## Adding a new skill

1. Create a directory: `kebab-case-name/`
2. Add `SKILL.md` with frontmatter (`name`, `description`)
3. Add supporting files (advisors, context lists, templates, schemas) as separate markdown files in the same directory
4. Reference supporting files from `SKILL.md` so the skill stays the entry point
5. Commit with a message describing what the skill does

## Pinned schemas

Some skills depend on IDs that change rarely but break things when they do (HubSpot pipeline IDs, channel IDs, etc.). Keep these in `_schemas/` as a single source. When IDs change, update once, all skills reading from there pick up the change.

## Conventions

- Skills are markdown, not code. No bash, no python, no shell-outs.
- Voice rules and banned words live in your account-level Claude preferences, not in skills. Skills don't repeat them.
- Client-specific context (BIG, KPF, Multiconsult, etc.) belongs in client projects, not in shared skills.
