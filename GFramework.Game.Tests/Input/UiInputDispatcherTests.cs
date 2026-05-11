// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using GFramework.Game.Abstractions.Input;
using GFramework.Game.Input;

namespace GFramework.Game.Tests.Input;

/// <summary>
///     验证逻辑动作到 UI 路由分发的默认桥接行为。
/// </summary>
[TestFixture]
public sealed class UiInputDispatcherTests
{
    /// <summary>
    ///     验证 `ui_cancel` 会被映射为 `UiInputAction.Cancel` 并继续分发给路由器。
    /// </summary>
    [Test]
    public void TryDispatch_WhenActionCanMapToUiAction_ForwardsToRouter()
    {
        var router = new Mock<IUiRouter>();
        router.Setup(mock => mock.TryDispatchUiAction(UiInputAction.Cancel)).Returns(true);

        var dispatcher = new UiInputDispatcher(new UiInputActionMap(), router.Object);

        var dispatched = dispatcher.TryDispatch("ui_cancel");

        Assert.Multiple(() =>
        {
            Assert.That(dispatched, Is.True);
            router.Verify(mock => mock.TryDispatchUiAction(UiInputAction.Cancel), Times.Once);
        });
    }

    /// <summary>
    ///     验证未映射的逻辑动作不会触发 UI 路由。
    /// </summary>
    [Test]
    public void TryDispatch_WhenActionIsUnknown_ReturnsFalseWithoutRouting()
    {
        var router = new Mock<IUiRouter>();
        var dispatcher = new UiInputDispatcher(new UiInputActionMap(), router.Object);

        var dispatched = dispatcher.TryDispatch("inventory_toggle");

        Assert.Multiple(() =>
        {
            Assert.That(dispatched, Is.False);
            router.Verify(mock => mock.TryDispatchUiAction(It.IsAny<UiInputAction>()), Times.Never);
        });
    }
}
