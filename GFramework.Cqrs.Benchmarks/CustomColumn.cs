// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;
using System;

namespace GFramework.Cqrs.Benchmarks;

/// <summary>
///     为 CQRS benchmark 结果补充可读的场景标签列。
/// </summary>
/// <param name="columnName">列名。</param>
/// <param name="getValue">从 benchmark case 提取列值的委托。</param>
public sealed class CustomColumn(string columnName, Func<Summary, BenchmarkCase, string> getValue) : IColumn
{
    /// <inheritdoc />
    public string Id => $"{nameof(CustomColumn)}.{ColumnName}";

    /// <inheritdoc />
    public string ColumnName { get; } = columnName;

    /// <inheritdoc />
    public bool AlwaysShow => true;

    /// <inheritdoc />
    public ColumnCategory Category => ColumnCategory.Params;

    /// <inheritdoc />
    public int PriorityInCategory => 0;

    /// <inheritdoc />
    public bool IsNumeric => false;

    /// <inheritdoc />
    public UnitType UnitType => UnitType.Dimensionless;

    /// <inheritdoc />
    public string Legend => $"Custom '{ColumnName}' tag column";

    /// <inheritdoc />
    public bool IsDefault(Summary summary, BenchmarkCase benchmarkCase)
    {
        return false;
    }

    /// <inheritdoc />
    public bool IsAvailable(Summary summary)
    {
        return true;
    }

    /// <inheritdoc />
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase)
    {
        return getValue(summary, benchmarkCase);
    }

    /// <inheritdoc />
    public string GetValue(Summary summary, BenchmarkCase benchmarkCase, SummaryStyle style)
    {
        return GetValue(summary, benchmarkCase);
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return ColumnName;
    }
}
