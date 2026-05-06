// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Benchmarks.Messaging;

/// <summary>
///     为纯 runtime benchmark 提供最小 CQRS 上下文标记，避免把完整架构上下文初始化成本混入 steady-state dispatch。
/// </summary>
internal sealed class BenchmarkContext : ICqrsContext
{
    /// <summary>
    ///     共享的最小 CQRS 上下文实例。
    /// </summary>
    public static BenchmarkContext Instance { get; } = new();
}
