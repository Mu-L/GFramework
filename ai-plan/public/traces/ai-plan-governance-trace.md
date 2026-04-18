# AI-Plan 治理追踪

## 2026-04-19

### 阶段：目录语义收口（RP-002）

- 建立 `AI-PLAN-GOV-RP-002` 恢复点
- 用户指出当前 `ai-plan/` 存在三个治理问题：
  - 缺少更细的目录分层，容易随着 worktree 增长持续膨胀
  - 缺少“不得写入敏感数据、真实路径、机器信息”的明确约束
  - 目录语义没有区分共享恢复信息与 worktree 私有状态
- 已据此完成以下收口：
  - 将既有共享 tracking / trace 文件迁移到 `ai-plan/public/todos/` 与 `ai-plan/public/traces/`
  - 新增 `ai-plan/private/` 作为工作树私有恢复空间，并通过 `.gitignore` 保持未跟踪
  - 新增 `ai-plan/README.md` 作为目录语义与内容规范的单点说明
  - 在 `AGENTS.md` 中补齐 public/private 职责边界，以及敏感信息与绝对路径禁写规则
  - 在 `gframework-boot` 中同步新的读取顺序：优先 public，按需读取当前 worktree 私有目录

### 验证

- `find ai-plan -maxdepth 3 -type f | sort`
  - 结果：通过
- `rg -n "ai-plan/public/|ai-plan/private/" AGENTS.md .codex/skills/gframework-boot/SKILL.md .codex/skills/gframework-boot/references/startup-artifacts.md ai-plan/README.md .gitignore`
  - 结果：通过
- `dotnet build GFramework.Core.Abstractions/GFramework.Core.Abstractions.csproj -c Release`
  - 结果：通过

### 下一步

1. 后续若出现新的 worktree 私有恢复需求，直接在 `ai-plan/private/<branch-or-worktree>/` 下创建，不再向共享目录追加本地临时状态
2. 若将来需要进一步限制格式，可再为 `public/**` 与 `private/` 各自补一个模板文件，但本轮先把目录语义和安全边界固定下来
