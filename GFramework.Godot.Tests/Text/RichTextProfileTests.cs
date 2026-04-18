using GFramework.Godot.Text;

namespace GFramework.Godot.Tests.Text;

/// <summary>
///     <see cref="RichTextProfile" /> 的测试。
/// </summary>
[TestFixture]
public sealed class RichTextProfileTests
{
    /// <summary>
    ///     验证默认内置配置会暴露完整的第一阶段效果键集合。
    /// </summary>
    [Test]
    public void CreateBuiltInDefault_Should_Contain_The_First_Phase_Effect_Keys()
    {
        var profile = RichTextProfile.CreateBuiltInDefault();

        Assert.That(profile.Effects.Select(static entry => entry.Key), Is.EqualTo(new[]
        {
            "green",
            "red",
            "gold",
            "blue",
            "fade_in",
            "sine",
            "jitter",
            "fly_in"
        }));
    }
}
