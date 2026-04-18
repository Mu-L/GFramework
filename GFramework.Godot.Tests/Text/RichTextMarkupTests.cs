using GFramework.Godot.Text;

namespace GFramework.Godot.Tests.Text;

/// <summary>
///     <see cref="RichTextMarkup" /> 的测试。
/// </summary>
[TestFixture]
public sealed class RichTextMarkupTests
{
    /// <summary>
    ///     验证颜色快捷方法会输出预期标签。
    /// </summary>
    [Test]
    public void Green_Should_Wrap_Text_With_Green_Tag()
    {
        var result = RichTextMarkup.Green("Ready");

        Assert.That(result, Is.EqualTo("[green]Ready[/green]"));
    }

    /// <summary>
    ///     验证效果方法会按稳定顺序拼接环境参数。
    /// </summary>
    [Test]
    public void Effect_Should_Append_Environment_Parameters()
    {
        var env = new Dictionary<string, object?>
        {
            ["speed"] = 4,
            ["tick"] = 0.1f
        };

        var result = RichTextMarkup.Effect("Hello", "fade_in", env);

        Assert.That(result, Is.EqualTo("[fade_in speed=4 tick=0.1]Hello[/fade_in]"));
    }
}
