using System.Globalization;
using System.Text;

namespace GFramework.Godot.Text;

/// <summary>
///     提供语义化的富文本标签构建辅助方法。
///     该工具层用于减少业务代码直接手写原始 BBCode 字符串的重复工作。
/// </summary>
public static class RichTextMarkup
{
    /// <summary>
    ///     使用指定标签包裹文本。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <param name="tag">标签名。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    public static string Color(string text, string tag)
    {
        return Wrap(text, tag);
    }

    /// <summary>
    ///     使用 `green` 标签包裹文本。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    public static string Green(string text)
    {
        return Wrap(text, "green");
    }

    /// <summary>
    ///     使用 `red` 标签包裹文本。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    public static string Red(string text)
    {
        return Wrap(text, "red");
    }

    /// <summary>
    ///     使用 `gold` 标签包裹文本。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    public static string Gold(string text)
    {
        return Wrap(text, "gold");
    }

    /// <summary>
    ///     使用 `blue` 标签包裹文本。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    public static string Blue(string text)
    {
        return Wrap(text, "blue");
    }

    /// <summary>
    ///     使用指定效果标签包裹文本，并可附带参数环境。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <param name="tag">标签名。</param>
    /// <param name="env">可选的标签参数集合。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    public static string Effect(string text, string tag, IReadOnlyDictionary<string, object?>? env = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        var builder = new StringBuilder();
        builder.Append('[');
        builder.Append(tag);

        if (env is not null)
        {
            foreach (var pair in env)
            {
                if (string.IsNullOrWhiteSpace(pair.Key) || pair.Value is null)
                {
                    continue;
                }

                builder.Append(' ');
                builder.Append(pair.Key);
                builder.Append('=');
                builder.Append(FormatValue(pair.Value));
            }
        }

        builder.Append(']');
        builder.Append(text ?? string.Empty);
        builder.Append("[/");
        builder.Append(tag);
        builder.Append(']');
        return builder.ToString();
    }

    /// <summary>
    ///     使用指定标签包裹文本。
    /// </summary>
    /// <param name="text">原始文本。</param>
    /// <param name="tag">标签名。</param>
    /// <returns>包裹后的 BBCode 文本。</returns>
    private static string Wrap(string text, string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        return $"[{tag}]{text ?? string.Empty}[/{tag}]";
    }

    /// <summary>
    ///     将标签参数值格式化为稳定的 BBCode 字符串表示。
    /// </summary>
    /// <param name="value">待格式化的值。</param>
    /// <returns>适用于 BBCode 参数的字符串。</returns>
    private static string FormatValue(object value)
    {
        return value switch
        {
            string text => text,
            IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty
        };
    }
}
