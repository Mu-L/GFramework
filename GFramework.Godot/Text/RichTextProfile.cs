namespace GFramework.Godot.Text;

/// <summary>
///     描述一个富文本效果组合配置。
///     该资源是组合式扩展的核心载体，用于声明宿主标签需要安装的效果集合。
/// </summary>
[GlobalClass]
public partial class RichTextProfile : Resource
{
    /// <summary>
    ///     获取或设置当前配置启用的效果条目集合。
    /// </summary>
    [Export]
    public RichTextEffectEntry[] Effects { get; set; } = [];

    /// <summary>
    ///     创建包含全部内置效果的默认配置。
    ///     该方法为第一阶段提供零配置可用的回退组合。
    /// </summary>
    /// <returns>包含全部内置效果键的默认配置。</returns>
    public static RichTextProfile CreateBuiltInDefault()
    {
        var profile = new RichTextProfile();
        profile.Effects =
        [
            new RichTextEffectEntry { Key = "green" },
            new RichTextEffectEntry { Key = "red" },
            new RichTextEffectEntry { Key = "gold" },
            new RichTextEffectEntry { Key = "blue" },
            new RichTextEffectEntry { Key = "fade_in" },
            new RichTextEffectEntry { Key = "sine" },
            new RichTextEffectEntry { Key = "jitter" },
            new RichTextEffectEntry { Key = "fly_in" }
        ];
        return profile;
    }
}
