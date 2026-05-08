// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     用于验证双 stream pipeline 行为执行顺序的最小流式请求。
/// </summary>
internal sealed record DispatcherStreamPipelineOrderRequest : IStreamRequest<int>;
