// Copyright (c) 2026 GeWuYou
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using GFramework.Core.Abstractions.Coroutine;
using GFramework.Core.Abstractions.Cqrs;
using GFramework.Core.Abstractions.Rule;

namespace GFramework.Core.Coroutine.Extensions;

/// <summary>
/// 提供 CQRS 命令与协程集成的扩展方法。
/// 历史命名保留了 Mediator 前缀，但当前实现直接走 <see cref="IContextAware.GetContext" /> 返回的
/// <see cref="GFramework.Core.Abstractions.Architectures.IArchitectureContext" /> CQRS 入口，不再依赖外部 Mediator 服务。
/// </summary>
public static class MediatorCoroutineExtensions
{
    /// <summary>
    /// 以协程方式发送无返回值 CQRS 命令并处理可能的异常。
    /// </summary>
    /// <typeparam name="TCommand">命令的类型。</typeparam>
    /// <param name="contextAware">上下文感知对象，用于获取服务</param>
    /// <param name="command">要发送的命令对象</param>
    /// <param name="onError">发生异常时的回调处理函数</param>
    /// <returns>协程枚举器，用于协程执行</returns>
    public static IEnumerator<IYieldInstruction> SendCommandCoroutine<TCommand>(
        this IContextAware contextAware,
        TCommand command,
        Action<Exception>? onError = null)
        where TCommand : IRequest<Unit>
    {
        ArgumentNullException.ThrowIfNull(contextAware);
        ArgumentNullException.ThrowIfNull(command);

        var task = contextAware.GetContext().SendAsync(command).AsTask();

        yield return task.AsCoroutineInstruction();

        if (!task.IsFaulted) yield break;
        if (onError != null)
            onError.Invoke(task.Exception!);
        else
            throw task.Exception!.InnerException ?? task.Exception;
    }
}
