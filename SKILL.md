---
name: strategic-board
description: Run a 4-advisor deliberation on a strategic question. Each advisor takes an initial position with a vote, reads the others, then rebuts and casts a final vote. Output is a markdown report. Use when Joachim asks for "the board," "advisory board," or to "stress-test" a decision.
---

# Strategic Board

Orchestrate a simulated advisory board. Two rounds: positions then rebuttals. The goal isn't a verdict, it's surfacing angles Joachim wouldn't see alone.

## The question

$ARGUMENTS

If `$ARGUMENTS` is empty, ask Joachim for the question before doing anything else. Don't guess.

## Step 1 — Load context

Read `context-sources.md`. It lists the Drive URLs that anchor every board run (financials, client list, strategy doc, etc.).

For this specific question, decide which sources are actually relevant. A pricing question doesn't need the recruitment policy. A team question doesn't need the latest P&L. Fetch only what matters.

If a fetch fails, note it in the final report under "context gaps" and continue. Don't abort.

## Step 2 — Load advisors

Read `advisors.md`. Four personas, each with a lens and a bias. Use these verbatim, don't paraphrase.

## Step 3 — Round 1, initial positions

For each advisor, in parallel:

1. State their position in 80–120 words. Direct, no preamble.
2. Cast a vote: For / Against / Conditional. If conditional, state the condition.
3. Name the one number or fact from the loaded context that most shaped their position.

## Step 4 — Cross-read

Show each advisor the other three positions. Each then writes a 40–80 word rebuttal targeting whichever position they disagree with most. If they agree with everyone, they pick the position they'd push hardest and say why it's still incomplete.

## Step 5 — Round 2, final votes

Each advisor casts a final vote. If they changed from Round 1, name what changed their mind in one sentence. If they didn't change, say so explicitly.

## Step 6 — Synthesis

Write the synthesis section last. Cover, in this order:

1. **Vote table.** Round 1 and final, side by side.
2. **Who moved.** Names and the trigger.
3. **The sharpest disagreement.** Not the loudest, the one with the most at stake.
4. **The single most useful insight.** One sentence. The thing Joachim should write down.
5. **Likely call.** What the board would recommend, with the one condition or risk to watch.

Keep synthesis tight. The board's job is to surface, not to decide.

## Output

Save the full report as `board-reports/YYYY-MM-DD-slug.md` where `slug` is a 2–4 word kebab-case summary of the question.

Then in the chat reply, present only the synthesis section. Joachim reads the file when he wants the full deliberation.

## Don't

- Don't generate an interactive HTML dashboard. The markdown report is what gets read.
- Don't invent context. If `context-sources.md` doesn't cover something the question needs, flag it as a gap.
- Don't soften disagreement. The whole point is sharp positions.
- Don't add a 5th advisor or drop one without Joachim asking.
