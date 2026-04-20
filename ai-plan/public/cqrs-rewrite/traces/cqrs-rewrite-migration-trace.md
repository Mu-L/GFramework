# CQRS 重写迁移追踪

## 2026-04-20

### 阶段：generated registry 激活反射收敛（CQRS-REWRITE-RP-046）

- 已在 `CqrsHandlerRegistrar` 中将 generated registry 的无参构造激活改为类型级缓存工厂
- 默认路径优先使用一次性动态方法直接创建 registry，避免后续每次命中缓存仍走 `ConstructorInfo.Invoke`
- 若运行环境不允许动态方法，则保留原有反射激活回退，确保 generated registry 路径不因运行时限制失效
- 已补充“私有无参构造 generated registry 仍可激活”的回归测试，覆盖现有生成器产物兼容性
- 定向验证已通过：
  - `dotnet test GFramework.Cqrs.Tests/GFramework.Cqrs.Tests.csproj -c Release --no-restore -p:RestoreFallbackFolders= -m:1 -nodeReuse:false`
  - `63/63` passed
  - 当前沙箱限制 MSBuild named pipe，因此验证在提权环境下执行

### Archive Context

- 历史跟踪归档：
  - `ai-plan/public/cqrs-rewrite/archive/todos/cqrs-rewrite-history-through-rp043.md`
- 历史 trace 归档：
  - `ai-plan/public/cqrs-rewrite/archive/traces/cqrs-rewrite-history-through-rp043.md`

### 当前下一步

1. 回到 `Phase 8` 主线，优先选一个明确的 dispatch / invoker 反射缩减点继续推进
2. 若继续文档主线，优先补齐 `docs/zh-CN/api-reference` 与教程入口页中仍过时的 CQRS API / 命名空间表述
3. 若后续 review thread 或 PR 状态再次变化，再重新执行 `$gframework-pr-review` 复核远端信号
