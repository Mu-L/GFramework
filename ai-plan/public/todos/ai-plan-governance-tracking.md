# AI-Plan 治理跟踪

## 目标

收口 `ai-plan/` 的目录语义与提交边界，避免不同 worktree 的恢复文件持续膨胀并污染仓库历史。

- 为 `ai-plan/` 建立明确的目录分层
- 区分“可提交共享状态”与“工作树私有状态”
- 明确禁止写入敏感数据、绝对路径和机器本地信息
- 让 `AGENTS.md` 与 boot skill 使用同一套目录语义

## 当前恢复点

- 恢复点编号：`AI-PLAN-GOV-RP-002`
- 当前阶段：`Phase 1`
- 当前焦点：
  - 已将共享恢复文档迁移到 `ai-plan/public/todos/` 与 `ai-plan/public/traces/`
  - 已保留 `ai-plan/private/` 作为工作树私有空间，并通过 `.gitignore` 保持未跟踪
  - 已新增 `ai-plan/README.md`，明确目录语义、命名方式和敏感信息限制
  - 已同步更新 `AGENTS.md` 与 `gframework-boot`，让启动流程和 tracking 规则使用新的目录语义
  - 已将目录根名从 `local-plan/` 正式收口到 `ai-plan/`，避免“本地计划”和“可共享 AI 恢复文档”语义混淆

## 已完成

- `.gitignore` 现只允许 `ai-plan/README.md` 与 `ai-plan/public/**/*.md` 被纳入版本控制
- `AGENTS.md` 已补充：
  - `public/**` 与 `private/` 的职责划分
  - 禁止写入敏感数据、绝对路径、主机与账号信息
  - 复杂任务应更新 `ai-plan/public/**`，而不是把 worktree 私有状态直接丢进 Git
- `.codex/skills/gframework-boot/SKILL.md` 与其 `references/startup-artifacts.md` 已切换到：
  - 优先读取 `ai-plan/public/**`
  - 按需读取 `ai-plan/private/<branch-or-worktree>/` 作为私有上下文
- 既有共享 tracking / trace 文件已迁移到 `ai-plan/public/` 下

## 验证

- `find ai-plan -maxdepth 3 -type f | sort`
  - 结果：通过
  - 备注：当前只剩 `ai-plan/README.md` 与 `ai-plan/public/**` 进入仓库可见范围
- `rg -n "ai-plan/public/|ai-plan/private/" AGENTS.md .codex/skills/gframework-boot/SKILL.md .codex/skills/gframework-boot/references/startup-artifacts.md ai-plan/README.md .gitignore`
  - 结果：通过
  - 备注：新目录语义已统一到仓库规则与 boot skill
- `dotnet build GFramework.Core.Abstractions/GFramework.Core.Abstractions.csproj -c Release`
  - 结果：通过
  - 备注：本轮规则与文档调整未引入构建问题

## 下一步

1. 若后续需要 worktree 级恢复文件，可在 `ai-plan/private/<branch-or-worktree>/` 下建立私有目录，但仍遵守“不写敏感数据、不写绝对路径”的约束
2. 若未来再新增 skill 或仓库规则引用 `ai-plan/`，统一按 `public/**` 与 `private/` 的语义扩展，不再恢复平铺结构
