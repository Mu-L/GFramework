// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Cqrs.Tests.Cqrs;

/// <summary>
///     表示未注册任何 stream pipeline behavior 的最小缓存验证请求。
/// </summary>
internal sealed record DispatcherZeroPipelineStreamRequest : IStreamRequest<int>;
