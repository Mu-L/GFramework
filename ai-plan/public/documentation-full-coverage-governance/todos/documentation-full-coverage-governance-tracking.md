# Documentation Full Coverage Governance 跟踪

## 目标

建立一个长期 active topic，持续治理 `GFramework` 的 README、`docs/zh-CN`、站点导航、XML 文档和 API
参考链路，避免阶段性刷新完成后再次回漂。

- 用源码、测试、`*.csproj` 和必要的 `ai-libs/` 证据校正文档
- 以模块族为单位闭环 README、landing page、专题页、教程入口和 API 参考链路
- 明确哪些目录是可直接消费模块，哪些只是内部支撑模块
- 把 XML 文档缺口纳入治理范围，而不是只刷新 Markdown

## 当前恢复点

- 恢复点编号：`DOCUMENTATION-FULL-COVERAGE-GOV-RP-037`
- 当前阶段：`Phase 5 - Governance Maintenance`
- 当前焦点：
  - 继续以最新 `origin/main`（`79934f7`，`2026-04-25 16:15:55 +08:00`）作为 baseline，当前批处理 stop condition 仍是 branch diff vs baseline 接近 `50` changed files
  - 当前批次只处理 `docs/zh-CN/contributing.md` 中最后 1 条既有代码块语言警告，避免重新扩张到大范围栏目巡检
  - 工作树当前仅有 1 个未提交文件、`4` added / `4` deleted lines；分支级 branch diff vs `origin/main` 仍为 `0` files，距离阈值有充足空间
  - 公开文档的 reader-facing 治理已基本收口，下一轮应优先确认 PR review 是否还有最新 latest-head 残留，而不是重新启动同模板批量修整

## 当前状态摘要

- `Core`、`Ecs.Arch`、`Cqrs`、`Game`、`Godot` 五个模块族当前都已有 README / landing / topic / API 参考层级的已验证入口。
- `2026-04-25` 已重新抓取 PR `#290` 并确认：latest reviewed commit 为 `54b8e5770af9ab3c8a86a396ffa4794fe4bb5181`，open thread 聚焦在 `docs/.vitepress/config.mts` 的侧栏重复 / 标签不一致，以及 `GFramework.Core`、`GFramework.Ecs.Arch`、`GFramework.Game` README 的 reader-facing 表格残留治理字段。
- `2026-04-25` `docs/.vitepress/config.mts` 已保留 `source-generators` 栏目自有子页导航，但不再让 `api-reference` 侧栏重复跳回 `core`、`game`、`godot`、`ecs` 等独立栏目入口。
- `2026-04-25` `GFramework.Core/README.md`、`GFramework.Ecs.Arch/README.md`、`GFramework.Game/README.md` 当前把 XML 阅读表统一收敛为“代表类型 + 阅读重点”，不再暴露日期、覆盖计数或 `已覆盖` 这类治理式字段。
- `2026-04-25` `docs/zh-CN/contributing.md` 中最后一个嵌套 fenced 示例已改写为转义围栏文本，现有 `validate-code-blocks.sh` 不再报告第 `631` 行警告。
- `2026-04-25` 全量 `docs/zh-CN` 验证已无剩余代码块语言警告；前一轮触达的 `tutorials`、`best-practices`、`troubleshooting`、`godot/resource` 等栏目结果保持有效。
- `Game` persistence docs surface 当前以 `docs/zh-CN/game/data.md`、`storage.md`、`serialization.md`、`setting.md`
  作为最小巡检集合；若后续 README、runtime public API 或 `PersistenceTests` 变动，应优先复核这一组页面。
- `Godot` runtime 与 generator 入口当前以 `GFramework.Godot/README.md`、
  `GFramework.Godot.SourceGenerators/README.md`、`docs/zh-CN/godot/index.md`、
  `docs/zh-CN/source-generators/index.md`、`docs/zh-CN/tutorials/godot-integration.md` 维持统一 owner / adoption path。
- `2026-04-23` 到 `2026-04-24` 的批次细节、验证日志与旧恢复建议已迁入：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`

## 当前风险

- 当前 `Core` / `Core.Abstractions`、`Ecs.Arch`、`Cqrs`、`Game` 的 XML 治理证据仍主要来自类型与入口级阅读，不等于成员级契约全审计；这类治理状态只应保留在 `ai-plan/**`，不应再回流到公开文档。
- `GFramework.Cqrs` 在当前 WSL / dotnet 环境下仍会读取失效的 fallback package folder，并在标准 build 中触发
  `MSB4276` / `MSB4018`；这是已知环境阻塞，不属于本轮文档回归。
- 当前 WSL 会话里 `git.exe` 可解析但不能执行，应继续使用显式 `--git-dir` / `--work-tree` 绑定作为默认 Git 策略。
- PR `#290` 当前 review 线程来自 bot，对 reader-facing 导航和文案一致性的期望比较细；本轮提交后仍需重新抓取 latest-head review，确认是否还有新的 open thread 或旧线程未自动关闭。

## 归档指针

- 详细验证历史（`RP-001` 到 `RP-007`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-validation-history-through-rp-007.md`
- 阶段状态归档（`RP-001` 到 `RP-016`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-through-rp-016.md`
- 阶段状态归档（`RP-023` 到 `RP-025`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/todos/documentation-full-coverage-governance-status-history-rp-023-to-rp-025-2026-04-24.md`
- 时间线归档（`RP-001` 到 `RP-016`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-through-rp-016.md`
- 时间线归档（`RP-023` 到 `RP-025`）：
  `ai-plan/public/documentation-full-coverage-governance/archive/traces/documentation-full-coverage-governance-trace-history-rp-023-to-rp-025-2026-04-24.md`

## 最新验证

- `2026-04-25` `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/gframework-current-pr-review.json`
  - 结果：通过；PR `#290` 处于 `OPEN`，latest head commit `54b8e5770af9ab3c8a86a396ffa4794fe4bb5181` 有 `2` 条 open thread（CodeRabbit `1`、Greptile `1`），测试汇总为 `2156 passed`，无 failed checks。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Core/README.md GFramework.Ecs.Arch/README.md GFramework.Game/README.md`
  - 结果：通过；本轮 3 个模块 README 调整后链接目标仍然有效。
- `2026-04-25` `bun run build`（工作目录：`docs/`）
  - 结果：通过；移除 `api-reference` 侧栏重复项并统一 `source-generators` 标签后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh README.md GFramework.Core/README.md GFramework.Core.Abstractions/README.md GFramework.Game/README.md GFramework.Game.Abstractions/README.md GFramework.Game.SourceGenerators/README.md GFramework.Ecs.Arch/README.md GFramework.Ecs.Arch.Abstractions/README.md`
  - 结果：通过；根 README 与本轮触达的模块 README 链接目标有效。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/tutorials`
  - 结果：通过；本轮新增触达的 10 个教程页与其余教程页 frontmatter、链接、代码块校验均通过。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/best-practices`
  - 结果：通过；`index.md` 与 `architecture-patterns.md` 的代码块标记补齐后栏目验证通过。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/troubleshooting.md`
  - 结果：通过；错误输出与完整错误信息块补齐为 `text` 后页面验证通过。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/contributing.md`
  - 结果：通过；嵌套 fenced 示例已改写为转义围栏文本，`docs/zh-CN/contributing.md` 不再保留代码块语言警告。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN`
  - 结果：通过；当前 `docs/zh-CN` 全量 frontmatter、链接与代码块校验均通过，不再保留既有代码块语言警告。
- `2026-04-25` `bun run build`（工作目录：`docs/`）
  - 结果：通过；`contributing.md` 的 Mermaid 示例改写后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/godot/resource.md`
  - 结果：通过；`Godot` 资源页剩余 bare opening fence 已补齐语言标记。
- `2026-04-25` `bun run build`（工作目录：`docs/`）
  - 结果：通过；本轮导航补齐、README reader-facing 改写与教程 / 排障 / 资源页代码块语言标记更新后站点仍可构建，仅保留既有大 chunk warning。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Core/README.md`
  - 结果：通过；README 链接目标有效。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Core.SourceGenerators/README.md`
  - 结果：通过；README 链接目标有效。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Cqrs.SourceGenerators/README.md`
  - 结果：通过；README 链接目标有效。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Ecs.Arch/README.md`
  - 结果：通过；README 链接目标有效。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-links.sh GFramework.Game.SourceGenerators/README.md`
  - 结果：通过；README 链接目标有效。
- `2026-04-25` `rg -n '\\[[^\\]]*(README\\.md|\\.md|\\.md/|/zh-CN/[^\\]]*)\\]\\([^)]*\\)' GFramework.Core/README.md GFramework.Core.SourceGenerators/README.md GFramework.Cqrs.SourceGenerators/README.md GFramework.Ecs.Arch/README.md GFramework.Game.SourceGenerators/README.md`
  - 结果：无命中；本轮 5 个 README 已无可见路径式 / 文件名式 Markdown 链接标签残留。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/core`
  - 结果：通过；`Core` 栏目本轮触达页面的 frontmatter、链接与代码块校验均通过。
- `2026-04-25` `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/tutorials/basic`
  - 结果：通过；基础教程栏目本轮触达页面的 frontmatter、链接与代码块校验均通过。
- `2026-04-25` `bun run build`（工作目录：`docs/`）
  - 结果：通过；README 标签修正与 `Core` / 基础教程代码块语言标记补齐后站点仍可构建，仅保留既有大 chunk warning。
- `2026-04-24` `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
  - 结果：通过；PR `#284` 处于 `OPEN`，latest head commit `77540c07f0890cc05b10a849722c87b8bed8f561` 有 `3` 条 CodeRabbit 与 `1` 条 Greptile open thread，测试汇总为 `2156 passed`，仅剩 `Title check` 的 inconclusive PR 元数据提示。
- `2026-04-24` `rg -n --pcre2 '\\]\\(/zh-CN/[^)]+(?<!\\.md)\\)' docs/zh-CN/troubleshooting.md`
  - 结果：当前无命中；`/zh-CN/core/architecture` 与 `/zh-CN/faq` 已统一补成显式 `.md` 链接。
- `2026-04-24` `bun run build`（工作目录：`docs/`）
  - 结果：通过；文档标题本地化、站内链接修正与 `ai-plan` 归档瘦身落地后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-24` `python3 - <<'PY' ...`（扫描 `docs/zh-CN/**/*.md` frontmatter 是否缺 `title` / `description`）
  - 结果：通过；当前带 frontmatter 的 `docs/zh-CN` 页面已无 `title` / `description` 缺口。
- `2026-04-24` `bun run build`（工作目录：`docs/`）
  - 结果：通过；首页与基础教程 metadata 补齐后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-24` `python3 - <<'PY' ...`（扫描 `docs/zh-CN/**/*.md` 中以 `./`、`../`、`/zh-CN/` 开头且未带扩展名的 Markdown 链接）
  - 结果：通过；当前 `docs/zh-CN` 站内 Markdown 链接已无缺失扩展名的命中。
- `2026-04-24` `bun run build`（工作目录：`docs/`）
  - 结果：通过；`25` 个页面的站内链接补齐为显式 `.md` / `index.md` 后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-24` `bun run build`（工作目录：`docs/`）
  - 结果：通过；模块 README、中文落地页 reader-facing 文档入口对齐，以及 `docs/index.md` metadata 调整后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-24` `python3 - <<'PY' ...`（扫描 `docs/zh-CN/**/*.md` 中纯英文 `title`）
  - 结果：通过；经过三轮标题本地化后，仅剩 `CQRS` 与 `GFramework` 两个品牌/缩写型标题。
- `2026-04-24` `bun run build`（工作目录：`docs/`）
  - 结果：通过；根 `README.md` reader-friendly 链接标签修正与 `docs/zh-CN` 多页标题本地化落地后站点仍可正常构建，仅保留既有大 chunk warning。
- `2026-04-25` `python3 .agents/skills/gframework-doc-refresh/scripts/scan_module_evidence.py Core`
  - 结果：通过；技能仍能正常解析 `Core` 模块证据面，说明新增的 reader-facing 输出约束未破坏模块扫描主流程。
- `2026-04-25` `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --json-output /tmp/current-pr-review.json`
  - 结果：通过；PR `#287` 处于 `OPEN`，latest head commit `8209d7a29f35d969fca6258b9817da9b33a203a3` 仅剩
    `1` 条 Greptile open thread，无 failed checks，测试汇总为 `2156 passed`。
- `2026-04-25` `bun run build`（工作目录：`docs/`）
  - 结果：通过；`docs/zh-CN/api-reference/index.md` 的站内入口标签统一为语义化写法后站点仍可正常构建，仅保留既有大 chunk warning。

## 下一步

1. 提交当前 `contributing.md` 警告收口批次后，重新抓取 `$gframework-pr-review` 确认 PR `#290` 的 latest-head review 是否已清空 open thread。
2. 若 review 已清空，则把下一轮文档治理切回“新问题发现”模式，不再围绕已清零的代码块语言警告做重复扫描。
3. 若后续分支继续调整 `Game` persistence runtime、README 或公共 API，优先复核 `docs/zh-CN/game/data.md`、
   `storage.md`、`serialization.md`、`setting.md` 与 landing page 是否仍保持同一套职责边界。
4. 若后续分支继续调整 `Godot` generator 接法，优先复核 `GFramework.Godot.SourceGenerators/README.md`、
   `docs/zh-CN/tutorials/godot-integration.md` 与相关专题页是否仍保持一致。
