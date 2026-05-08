// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using GFramework.Cqrs.Abstractions.Cqrs;

namespace GFramework.Core.Tests.Architectures;

/// <summary>
///     处理 <see cref="ModuleStreamBehaviorRequest" /> 并返回一个固定元素。
/// </summary>
public sealed class ModuleStreamBehaviorRequestHandler : IStreamRequestHandler<ModuleStreamBehaviorRequest, int>
{
    /// <summary>
    ///     返回一个固定元素，供架构 stream pipeline 行为回归断言使用。
    /// </summary>
    /// <param name="request">当前流式请求。</param>
    /// <param name="cancellationToken">取消令牌。</param>
    /// <returns>包含一个固定元素的异步流。</returns>
    public async IAsyncEnumerable<int> Handle(
        ModuleStreamBehaviorRequest request,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        yield return 7;
        await ValueTask.CompletedTask.ConfigureAwait(false);
    }
}
