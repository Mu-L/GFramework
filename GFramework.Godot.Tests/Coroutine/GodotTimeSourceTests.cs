using System.Collections.Generic;
using GFramework.Godot.Coroutine;
using NUnit.Framework;

namespace GFramework.Godot.Tests.Coroutine;

/// <summary>
///     GodotTimeSource 的单元测试。
/// </summary>
[TestFixture]
public sealed class GodotTimeSourceTests
{
    /// <summary>
    ///     验证增量模式会直接累加传入的 delta。
    /// </summary>
    [Test]
    public void Update_Should_Accumulate_Delta_When_Using_Delta_Mode()
    {
        var values = new Queue<double>([0.1, 0.2]);
        var timeSource = new GodotTimeSource(() => values.Dequeue());

        timeSource.Update();
        Assert.That(timeSource.DeltaTime, Is.EqualTo(0.1).Within(0.0001));
        Assert.That(timeSource.CurrentTime, Is.EqualTo(0.1).Within(0.0001));

        timeSource.Update();
        Assert.That(timeSource.DeltaTime, Is.EqualTo(0.2).Within(0.0001));
        Assert.That(timeSource.CurrentTime, Is.EqualTo(0.3).Within(0.0001));
    }

    /// <summary>
    ///     验证绝对时间模式会根据前后两次采样计算 delta。
    /// </summary>
    [Test]
    public void Update_Should_Calculate_Delta_When_Using_Absolute_Time_Mode()
    {
        var values = new Queue<double>([1.0, 1.25, 2.0]);
        var timeSource = new GodotTimeSource(() => values.Dequeue(), useAbsoluteTime: true);

        timeSource.Update();
        Assert.That(timeSource.DeltaTime, Is.EqualTo(0).Within(0.0001));
        Assert.That(timeSource.CurrentTime, Is.EqualTo(1.0).Within(0.0001));

        timeSource.Update();
        Assert.That(timeSource.DeltaTime, Is.EqualTo(0.25).Within(0.0001));
        Assert.That(timeSource.CurrentTime, Is.EqualTo(1.25).Within(0.0001));

        timeSource.Update();
        Assert.That(timeSource.DeltaTime, Is.EqualTo(0.75).Within(0.0001));
        Assert.That(timeSource.CurrentTime, Is.EqualTo(2.0).Within(0.0001));
    }
}