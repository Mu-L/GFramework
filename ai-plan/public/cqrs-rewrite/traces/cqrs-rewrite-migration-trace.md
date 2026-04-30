# CQRS 重写迁移追踪

## 2026-04-30

### 阶段：PR #307 active 入口收敛（CQRS-REWRITE-RP-076）

- 继续沿用 `$gframework-pr-review` 对 `PR #307` 做 latest-head triage，本轮只处理仍成立的 `ai-plan` 恢复入口问题
- 主线程确认当前远端权威信号：
  - 当前分支对应 `PR #307`，状态为 `OPEN`
  - 远端 `CTRF` 最新汇总为 `2247/2247` passed
  - `MegaLinter` 仅剩 `dotnet-format` 的 `Restore operation failed` 环境噪音
  - 仍未闭环的 review 重点集中在 `cqrs-rewrite` active tracking / trace 仍保留过多历史锚点，而非新的运行时代码缺陷
- 本轮决策：
  - 将 active tracking 收敛为单一恢复入口，只保留 `RP-076`、`PR #307`、活跃风险、最近权威验证与下一推荐步骤
  - 将 active trace 收敛为当前阶段的关键事实与决策，不再在默认恢复入口中保留 `RP-062` 之后的长阶段流水账
  - 新增 `archive/traces/cqrs-rewrite-history-rp062-through-rp076.md` 承接 `RP-062` 至 `RP-076` 的详细 trace 历史，保持旧阶段仍可追溯

### 验证（RP-076）

- `python3 .agents/skills/gframework-pr-review/scripts/fetch_current_pr_review.py --format json --json-output /tmp/current-pr-review.json`
  - 结果：通过
  - 备注：确认 `PR #307` 的当前 review 重点已收敛到 `ai-plan` 文档收尾
- `dotnet build GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release`
  - 结果：通过，`0 warning / 0 error`
- `dotnet test GFramework.SourceGenerators.Tests/GFramework.SourceGenerators.Tests.csproj -c Release --filter "FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Provider_Interface|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Enumerator|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Does_Not_Emit_Stream_Invoker_Provider_Metadata_When_Runtime_Lacks_Stream_Descriptor_Entry_Type|FullyQualifiedName~CqrsHandlerRegistryGeneratorTests.Emits_Stream_Invoker_Provider_Metadata_When_Runtime_Contract_Is_Available"`
  - 结果：通过，`5/5` passed

### 当前下一步（RP-076）

1. 继续按 `PR #307` 的 latest-head review 收尾，优先保持 active tracking 与 active trace 的单一锚点一致
2. 若继续推进代码切片，先复核 request 侧是否仍存在与 stream invoker gate 对称的生成合同遗漏
3. 进入下一批前继续使用最小 Release build 或 targeted test 作为权威验证，避免把环境噪音误判为代码问题
