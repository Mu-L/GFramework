namespace GFramework.Core.Tests.Query;

/// <summary>
///     表示 <see cref="AsyncQueryExecutorTests" /> 使用的复杂测试查询结果。
/// </summary>
public sealed class TestAsyncQueryResult
{
    /// <summary>
    ///     获取或设置主结果值。
    /// </summary>
    public int Value { get; init; }

    /// <summary>
    ///     获取或设置派生的双重结果值。
    /// </summary>
    public int DoubleValue { get; init; }
}
