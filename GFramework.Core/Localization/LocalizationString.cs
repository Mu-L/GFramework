using System.Text.RegularExpressions;
using GFramework.Core.Abstractions.Localization;

namespace GFramework.Core.Localization;

/// <summary>
/// 本地化字符串实现
/// </summary>
public class LocalizationString : ILocalizationString
{
    /// <summary>
    /// 匹配 {variableName} 或 {variableName:formatter:args} 的正则表达式模式
    /// </summary>
    private const string FormatVariablePattern =
        @"\{([a-zA-Z_][a-zA-Z0-9_]*)(?::([a-zA-Z_][a-zA-Z0-9_]*)(?::([^}]+))?)?\}";

    /// <summary>
    /// 预编译的静态正则表达式，用于格式化字符串中的变量替换
    /// </summary>
    private static readonly Regex FormatVariableRegex =
        new(FormatVariablePattern, RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly ILocalizationManager _manager;
    private readonly Dictionary<string, object> _variables;

    /// <summary>
    /// 初始化本地化字符串
    /// </summary>
    /// <param name="manager">本地化管理器</param>
    /// <param name="table">表名</param>
    /// <param name="key">键名</param>
    public LocalizationString(ILocalizationManager manager, string table, string key)
    {
        _manager = manager ?? throw new ArgumentNullException(nameof(manager));
        Table = table ?? throw new ArgumentNullException(nameof(table));
        Key = key ?? throw new ArgumentNullException(nameof(key));
        _variables = new Dictionary<string, object>();
    }

    /// <inheritdoc/>
    public string Table { get; }

    /// <inheritdoc/>
    public string Key { get; }

    /// <inheritdoc/>
    public ILocalizationString WithVariable(string name, object value)
    {
        if (name == null)
        {
            throw new ArgumentNullException(nameof(name));
        }

        _variables[name] = value;
        return this;
    }

    /// <inheritdoc/>
    public ILocalizationString WithVariables(params (string name, object value)[] variables)
    {
        if (variables == null)
        {
            throw new ArgumentNullException(nameof(variables));
        }

        foreach (var (name, value) in variables)
        {
            WithVariable(name, value);
        }

        return this;
    }

    /// <inheritdoc/>
    public string Format()
    {
        var rawText = GetRaw();
        return FormatString(rawText, _variables, _manager);
    }

    /// <inheritdoc/>
    public string GetRaw()
    {
        if (!_manager.TryGetText(Table, Key, out var text))
        {
            return $"[{Table}.{Key}]";
        }

        return text;
    }

    /// <inheritdoc/>
    public bool Exists()
    {
        return _manager.TryGetText(Table, Key, out _);
    }

    /// <summary>
    /// 格式化字符串（支持变量替换和格式化器）
    /// </summary>
    private static string FormatString(
        string template,
        Dictionary<string, object> variables,
        ILocalizationManager manager)
    {
        if (string.IsNullOrEmpty(template))
        {
            return template;
        }

        // 使用预编译的静态正则表达式匹配 {variableName} 或 {variableName:formatter:args}
        return FormatVariableRegex.Replace(template, match => FormatMatch(match, variables, manager));
    }

    private static string FormatMatch(
        Match match,
        Dictionary<string, object> variables,
        ILocalizationManager manager)
    {
        var variableName = match.Groups[1].Value;
        if (!variables.TryGetValue(variableName, out var value))
        {
            return match.Value;
        }

        var formatterName = GetOptionalGroupValue(match, 2);
        if (string.IsNullOrEmpty(formatterName))
        {
            return FormatValue(value, manager);
        }

        return TryFormatValue(match, value, formatterName, manager, out var result)
            ? result
            : FormatValue(value, manager);
    }

    private static bool TryFormatValue(
        Match match,
        object value,
        string formatterName,
        ILocalizationManager manager,
        out string result)
    {
        var formatterArgs = GetOptionalGroupValue(match, 3) ?? string.Empty;
        if (GetFormatter(manager, formatterName) is { } formatter &&
            formatter.TryFormat(formatterArgs, value, manager.CurrentCulture, out result))
        {
            return true;
        }

        result = string.Empty;
        return false;
    }

    private static string FormatValue(object value, ILocalizationManager manager)
    {
        return value switch
        {
            IFormattable formattable => formattable.ToString(null, manager.CurrentCulture),
            _ => value.ToString() ?? string.Empty
        };
    }

    private static string? GetOptionalGroupValue(Match match, int groupIndex)
    {
        return match.Groups[groupIndex].Success ? match.Groups[groupIndex].Value : null;
    }

    /// <summary>
    /// 获取格式化器
    /// </summary>
    private static ILocalizationFormatter? GetFormatter(ILocalizationManager manager, string name)
    {
        return manager.GetFormatter(name);
    }
}