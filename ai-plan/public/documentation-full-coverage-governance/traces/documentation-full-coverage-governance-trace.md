# Documentation Full Coverage Governance Trace

## 2026-04-25

### 当前恢复点：RP-038

- 用户明确要求从“低效的单次批次”切到“循环跑到接近阈值”，并允许通过 subagent 避免主线程上下文过长；因此本轮把批处理目标从 PR `#290` 的单点收口扩展为“覆盖整个项目功能的 reader-facing 文档补齐”。
- 先在主线程确认 critical path 仍是“选定低风险文档切片并控制 branch-size stop condition”，再委派 3 个 explorer 做只读巡检：
  - source-generator support modules / 文档失真点
  - CQRS 文档覆盖缺口
  - repo-root / tooling / meta-package surface
- 接受的 explorer 结论：
  - `CQRS` 当前不需要扩独立栏目；最小有用修复是补 `docs/zh-CN/core/cqrs.md` 对 `RequestBase`、stream command/query 与协程入口的说明。
  - source-generators 当前最有价值的是修正文档失真，并补清楚 `GFramework.SourceGenerators.Common` 与 `*.SourceGenerators.Abstractions` 的共享支撑层语义。
  - repo-root / tooling 当前最缺的是 meta-package / install surface、VS Code config tool adoption path，以及 repo-visible support module README。
- 由此收敛出 5 组连续低风险批次：
  - meta-package / 安装入口
  - config tool adoption
  - source-generators 真实契约修正
  - CQRS `Request` / stream 覆盖补齐
  - generator support module README

### 当前决策（RP-038）

- 不把 `CQRS` 从 `Core` 导航中抽成新栏目；本轮优先修正 reader-facing 覆盖缺口，而不是引入新的站点结构。
- 对 repo-visible support modules，不扩成新的 docs 栏目，而是在各目录本地补 `README.md` 说明“为什么存在、跟谁一起走、什么时候需要读这里”。
- 对 config tool，不新建顶级 `tooling/` 栏目，而是挂到 `Game` 下，保持它与 `config-system` 的采用路径一致。
- stop condition 仍按 `origin/main` 与 `50` changed files 追踪；本轮提交前工作树已触达 `18` 个文件，仍明显低于阈值。

### 当前验证（RP-038）

- 文档栏目校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/source-generators`
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/game`
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/core/cqrs.md`
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/index.md`
  - 结果：通过；触达页 frontmatter、链接与代码块校验通过。
- README / 链接校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh README.md tools/gframework-config-tool/README.md GFramework.SourceGenerators.Common/README.md GFramework.Core.SourceGenerators.Abstractions/README.md GFramework.Godot.SourceGenerators.Abstractions/README.md`
  - 结果：通过；根 README、config tool README 与新增 support README 的链接目标有效。
- 站点构建：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；站点可构建，仅保留既有大 chunk warning。
- 元包编译：
  - `dotnet build GFramework.csproj -c Release`
  - 结果：通过；输出 `357` 条既有 analyzer warnings，无新增错误。

### 当前恢复点：RP-037

- 通过 `$gframework-batch-boot 50` 重新进入后，先按技能要求读取 `AGENTS.md`、`.ai/environment/tools.ai.yaml`、`ai-plan/public/README.md`、active topic tracking / trace，并确认当前 worktree 仍映射到 `documentation-full-coverage-governance`。
- 使用显式 `git --git-dir=<repo>/.git/worktrees/GFramework-update-documentation --work-tree=<worktree-root>` 绑定确认 baseline 采用 `origin/main` `79934f7`（`2026-04-25 16:15:55 +08:00`）；branch diff vs baseline 当前为 `0` files，工作树仅有本批次改动。
- 全量运行 `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN` 后确认 reader-facing 文档仅剩 `docs/zh-CN/contributing.md:631` 这一条既有代码块语言警告，适合作为单文件低风险批次收口。
- 将 `docs/zh-CN/contributing.md` 的 Mermaid 示例从“真实嵌套 triple-backtick”改写为“外层 fenced block + 内层转义围栏文本”，避免当前 `validate-code-blocks.sh` 的简单 `^```` 状态机把内层 closing fence 误判成缺语言标记的新 opening fence。

### 当前决策（RP-037）

- 当前批处理目标收敛为“消除 `contributing.md` 中最后一个剩余代码块语言 warning”，不再继续扩展到别的栏目页。
- 继续沿用 `origin/main` 作为 branch-size stop condition 基线，主指标仍是 `50` changed files；本批次只新增 1 个工作树文件，远未逼近阈值。
- 对这类“文档中展示 Markdown 代码块”的示例，优先选择仓库现有校验脚本可稳定识别的转义文本写法，而不是依赖嵌套 fenced block 的解析细节。

### 当前验证（RP-037）

- 文档单文件校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/contributing.md`
  - 结果：通过；`docs/zh-CN/contributing.md` 不再报告第 `631` 行代码块语言警告。
- 文档全量校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN`
  - 结果：通过；当前 `docs/zh-CN` 的 frontmatter、链接与代码块校验全部通过。
- 站点构建：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；站点仍可构建，仅保留既有大 chunk warning。

### 当前恢复点：RP-036

- 本轮从 `$gframework-pr-review` 重新进入，目标不再是扩批，而是核对 PR `#290` latest-head review 仍未关闭的 reader-facing 文档问题。
- 使用 `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/gframework-current-pr-review.json` 抓取后确认：PR `#290` 最新 reviewed commit 为 `54b8e5770af9ab3c8a86a396ffa4794fe4bb5181`，CodeRabbit 与 Greptile 各有 `1` 条 open thread，失败检查为 `0`，测试汇总仍为 `2156 passed`。
- 本轮把远端 review 与本地工作树逐项比对后，只接受仍然成立的 5 个 reader-facing 问题：`source-generators` 侧栏 3 个标签与目标标题不一致、`api-reference` 侧栏重复暴露跨栏目入口、`Core` / `Ecs.Arch` / `Game` README 仍保留 XML 覆盖基线字段。
- 当前未提交批次限定在 `docs/.vitepress/config.mts`、3 个模块 README，以及 active tracking / trace；没有继续扩展到其他未被 review 指向的文档文件。

### 当前决策（RP-036）

- 对 PR review 的处理改成“只修当前 latest-head review 仍成立的问题”，不再延续前一轮的批量普查节奏。
- `api-reference` 侧栏不再承载跨栏目目录跳转；跨模块导航继续保留在 `docs/zh-CN/api-reference/index.md` 的正文里，避免侧栏在跳出栏目后发生上下文切换。
- `source-generators` 侧栏项统一与目标文档的 H1 / frontmatter `title` 对齐，避免同一页面在导航、标题与搜索索引里出现多套命名。
- 模块 README 的 XML 阅读表只保留读者有用的“代表类型 / 阅读重点”，把覆盖计数、日期和 `已覆盖` 之类治理痕迹全部留在 `ai-plan/**`。

### 当前验证（RP-036）

- PR review 抓取：
  - `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/gframework-current-pr-review.json`
  - 结果：通过；PR `#290` 处于 `OPEN`，latest head review 还有 `2` 条 open thread，测试汇总为 `2156 passed`。

- README / 链接校验：
  - `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Core/README.md GFramework.Ecs.Arch/README.md GFramework.Game/README.md`
  - 结果：通过；本轮 3 个 README 调整后链接目标仍然有效。
- 站点构建：
  - `bun run build`（工作目录：`docs/`）
  - 结果：通过；`docs/.vitepress/config.mts` 的侧栏调整后站点仍可构建，仅保留既有大 chunk warning。

### 归档指针

- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`
- `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-023-to-rp-025-2026-04-24.md`

### 下一步

1. 完成 `bun run build` 与 README 链接校验后，提交当前 PR `#290` review 收口批次。
2. 提交后再次运行 `$gframework-pr-review`，确认 CodeRabbit / Greptile 的 open thread 是否已关闭。
3. 若仍有 review 残留，再按 latest-head review 精确收口，不恢复到前一轮的广覆盖批处理模式。
