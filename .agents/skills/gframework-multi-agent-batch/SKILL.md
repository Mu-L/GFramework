---
name: gframework-multi-agent-batch
description: Repository-specific multi-agent orchestration workflow for the GFramework repo. Use when the main agent should keep coordinating multiple parallel subagents, maintain ai-plan recovery artifacts, review subagent results, and continue bounded multi-agent waves until reviewability, context budget, or branch-diff limits say to stop.
---

# GFramework Multi-Agent Batch

## Overview

Use this skill when `gframework-boot` has already established repository context, and the task now benefits from the
main agent acting as the persistent coordinator for multiple parallel subagents.

Treat `AGENTS.md` as the source of truth. This skill expands the repository's multi-agent coordination rules; it does
not replace them.

This skill is for orchestration-heavy work, not for every task that merely happens to use one subagent. Prefer it when
the main agent must keep splitting bounded write slices, monitoring progress, updating `ai-plan`, validating accepted
results, and deciding whether another delegation wave is still safe.

## Use When

Adopt this workflow only when all of the following are true:

1. The task is complex enough that multiple parallel slices materially shorten the work.
2. The candidate write sets can be kept disjoint.
3. The main agent still needs to own review, validation, integration, and `ai-plan` updates.
4. Another wave is still likely to fit the branch-diff, context-budget, and reviewability budget.

Prefer `gframework-batch-boot` instead when the task is mainly repetitive bulk progress with a single obvious slice
pattern and little need for continuous multi-worker orchestration.

## Startup Workflow

1. Execute the normal `gframework-boot` startup sequence first:
   - read `AGENTS.md`
   - read `.ai/environment/tools.ai.yaml`
   - read `ai-plan/public/README.md`
   - read the mapped active topic `todos/` and `traces/`
2. Confirm that the active topic and current branch still match the work you are about to delegate.
3. Define the current wave in one sentence:
   - benchmark-host alignment
   - runtime hotspot reduction
   - documentation synchronization
   - other bounded multi-slice work
4. Identify the critical path and keep it local.
5. Split only the non-blocking work into disjoint ownership slices.
6. Estimate whether one more delegation wave is still safe:
   - include current branch diff vs baseline
   - loaded `ai-plan` context
   - expected validation output
   - expected integration overhead

## Worker Design Rules

For each `worker` subagent, specify:

- the concrete objective
- the exact owned files or subsystem
- files or areas the worker must not touch
- required validation commands
- expected output format
- a reminder that other agents may be editing the repo

Prefer `explorer` subagents when the result is read-only ranking, tracing, or candidate discovery.

Do not launch two workers whose write sets overlap unless the overlap is trivial and the main agent has already decided
how to serialize or reconcile that overlap.

## Main-Agent Loop

While workers run, the main agent should only do non-overlapping work:

- inspect the next candidate slices
- recompute branch-diff and context-budget posture
- review finished worker output
- queue follow-up validation
- keep `ai-plan/public/**` current when accepted scope or next steps change

After each completed worker task:

1. Review the reported ownership, validation, and changed files.
2. Confirm the worker stayed inside its boundary.
3. Run or rerun the required validation locally if the slice is accepted.
4. Record accepted delegated scope, validation milestones, and the next recovery point in the active `ai-plan` files.
5. Reassess whether another wave is still reviewable and safe.

## Stop Conditions

Stop the current multi-agent wave when any of the following becomes true:

- the next wave would likely push the main agent near or beyond a safe context budget
- the remaining work no longer splits into clean disjoint ownership slices
- branch diff vs baseline is approaching the current reviewability budget
- integrating another worker would degrade clarity more than it would save time
- validation failures show that the next step belongs on the critical path and should stay local

If a branch-size threshold is also in play, treat it as a coarse repository-scope signal, not the sole decision rule.

## Task Tracking

When this workflow is active, the main agent must keep the active `ai-plan` topic current with:

- delegated scope that has been accepted
- validation results
- current branch-diff posture if it affects stop decisions
- the next recommended resume step

The main agent should keep active entries concise enough that `boot` can still recover the current wave quickly.

## Example Triggers

- `Use $gframework-multi-agent-batch to coordinate non-conflicting subagents for this complex CQRS task.`
- `Keep delegating bounded parallel slices, update ai-plan, and verify each worker result before continuing.`
- `Run a multi-agent wave where the main agent owns review, validation, and integration.`
