# AI Plan

`ai-plan/` stores AI task recovery artifacts for this repository, but not every file under it has the same sharing rules.

## Directory Semantics

- `public/todos/`
  - Repository-safe recovery documents.
  - Use these for durable task state that another contributor or worktree may need to resume safely.
  - These files may be committed.
- `public/traces/`
  - Repository-safe execution traces that record decisions, validation milestones, and the immediate next step.
  - These files may be committed.
- `private/`
  - Worktree-private recovery space.
  - Use this for temporary notes, local scratch recovery points, or state that only matters in the current worktree.
  - Keep this directory untracked.

## Content Rules

- Never write secrets, tokens, credentials, private keys, hostnames, IP addresses, proprietary URLs, or other sensitive data.
- Never write absolute file-system paths, home-directory paths, or machine usernames.
- Use repository-relative paths, branch names, PR numbers, recovery-point IDs, and stable document identifiers instead.
- Keep committed `public/**` content concise, handoff-safe, and understandable without machine-local context.

## Naming

- Shared recovery documents should describe the task, for example:
  - `public/todos/cqrs-rewrite-migration-tracking.md`
  - `public/traces/cqrs-rewrite-migration-trace.md`
- Worktree-private files should live under a folder named for the current branch or worktree, for example:
  - `private/feat-cqrs-optimization/`
  - `private/gframework-cqrs/`
