// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Game.Abstractions.Input;

namespace GFramework.Game.Input;

/// <summary>
///     提供可由宿主侧更新的默认输入设备跟踪器。
/// </summary>
public sealed class InputDeviceTracker : IInputDeviceTracker
{
    /// <summary>
    ///     初始化输入设备跟踪器。
    /// </summary>
    public InputDeviceTracker()
    {
        CurrentDevice = new InputDeviceContext(InputDeviceKind.Unknown);
    }

    /// <inheritdoc />
    public InputDeviceContext CurrentDevice { get; private set; }

    /// <summary>
    ///     使用新的宿主设备上下文覆盖当前状态。
    /// </summary>
    /// <param name="context">新的设备上下文。</param>
    public void Update(InputDeviceContext context)
    {
        CurrentDevice = context ?? throw new ArgumentNullException(nameof(context));
    }
}
