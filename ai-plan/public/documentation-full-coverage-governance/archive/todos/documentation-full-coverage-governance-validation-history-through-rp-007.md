# Documentation Full Coverage Governance Validation History Through RP-007

以下内容从 active tracking 中迁出，用于保留 `DOCUMENTATION-FULL-COVERAGE-GOV-RP-001` 到
`DOCUMENTATION-FULL-COVERAGE-GOV-RP-007` 的详细验证历史。默认 `boot` 只需要读取 active tracking 中的最新摘要；
若需要追溯早期验证命令与结果，再回到本归档文件。

## 详细验证历史

- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/abstractions/index.md`
- 结果：通过
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/abstractions/ecs-arch-abstractions.md`
- 结果：通过
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/api-reference/index.md`
- 结果：通过
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/core/index.md`
- 结果：通过
- 备注：`2026-04-22` 在补充 Core XML inventory 后重新验证
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/abstractions/core-abstractions.md`
- 结果：通过
- 备注：`2026-04-22` 在补充 Core.Abstractions XML inventory 后重新验证
- `cd docs && bun run build`
- 结果：通过
- 备注：`2026-04-22` 重新构建通过；仅保留 VitePress 大 chunk warning，无构建失败
- `dotnet build GFramework.Ecs.Arch.Abstractions/GFramework.Ecs.Arch.Abstractions.csproj -c Release -p:RestoreFallbackFolders=`
- 结果：通过
- 备注：`0 Warning(s) / 0 Error(s)`
- `DOTNET_CLI_HOME=/tmp/dotnet-home dotnet build GFramework.Core.Abstractions/GFramework.Core.Abstractions.csproj -c Release -p:RestoreFallbackFolders=`
- 结果：通过
- 备注：`0 Warning(s) / 0 Error(s)`
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/ecs/index.md`
- 结果：通过
- 备注：`2026-04-22` 在重写 ECS landing 后重新验证
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/ecs/arch.md`
- 结果：通过
- 备注：`2026-04-22` 在重写 Arch ECS 专题页后重新验证
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/abstractions/ecs-arch-abstractions.md`
- 结果：通过
- 备注：`2026-04-22` 在补充抽象页 XML inventory 后重新验证
- `cd docs && bun run build`
- 结果：通过
- 备注：`2026-04-22` 在 Ecs 波次重写后重新构建通过；仅保留 VitePress 大 chunk warning，无构建失败
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/core/cqrs.md`
- 结果：通过
- 备注：`2026-04-22` 在重写 `Cqrs` family landing 后重新验证
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/source-generators/cqrs-handler-registry-generator.md`
- 结果：通过
- 备注：`2026-04-22` 在新增 `Cqrs.SourceGenerators` 专题页后验证通过
- `python3` 轻量 XML inventory 扫描
- 结果：通过
- 备注：`2026-04-22` 确认 `GFramework.Cqrs` 的 `Internal/` 为 `14/14`、`GFramework.Cqrs.SourceGenerators/Cqrs/` 为 `3/3`、`GFramework.Cqrs.Abstractions/Cqrs/` 为 `20/20`
- `DOTNET_CLI_HOME=/tmp/dotnet-home dotnet build GFramework.Cqrs.SourceGenerators/GFramework.Cqrs.SourceGenerators.csproj -c Release -p:RestoreFallbackFolders=`
- 结果：通过
- 备注：保留既有 `NU1900` 与 `MA0051` warnings；无新增编译错误
- `DOTNET_CLI_HOME=/tmp/dotnet-home dotnet build GFramework.Cqrs/GFramework.Cqrs.csproj -c Release`
- 结果：失败
- 备注：当前环境会命中失效的 Windows fallback package folder，并在多目标 inner build 阶段触发 `MSB4276` / `MSB4018`；失败原因已记录为环境阻塞，不属于本轮文档改动回归
- `cd docs && bun run build`
- 结果：通过
- 备注：`2026-04-22` 在 `Cqrs` 波次文档刷新后重新构建通过；仅保留 VitePress 大 chunk warning，无构建失败
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/abstractions/game-abstractions.md`
- 结果：通过
- 备注：`2026-04-23` 在重写 `Game.Abstractions` 页面后验证通过
- `bash .agents/skills/gframework-doc-refresh/scripts/validate-all.sh docs/zh-CN/game/index.md`
- 结果：通过
- 备注：`2026-04-23` 在补充 frontmatter 与 XML inventory 后重新验证
- `python3` 轻量 XML inventory 扫描
- 结果：通过
- 备注：`2026-04-23` 确认 `GFramework.Game` 为 `56/56`、`GFramework.Game.Abstractions` 为 `80/80`、`GFramework.Game.SourceGenerators` 为 `2/2`
- `cd docs && bun run build`
- 结果：通过
- 备注：`2026-04-23` 在 `Game` 波次文档刷新后重新构建通过；仅保留 VitePress 大 chunk warning，无构建失败
- `cd docs && bun run build`
- 结果：通过
- 备注：`2026-04-23` 在更新 `AGENTS.md` 的 WSL Git 优先级后重新构建通过；仅保留 VitePress 大 chunk warning，无构建失败
- `cd docs && bun run build`
- 结果：通过
- 备注：`2026-04-23` 在推进 `DOCUMENTATION-FULL-COVERAGE-GOV-RP-007`、回写 `Game` family 巡检结论后重新构建通过；仅保留 VitePress 大 chunk warning，无构建失败
