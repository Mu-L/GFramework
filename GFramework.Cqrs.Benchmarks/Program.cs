// Copyright (c) 2025-2026 GeWuYou
// SPDX-License-Identifier: Apache-2.0

using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

namespace GFramework.Cqrs.Benchmarks;

/// <summary>
///     提供 GFramework.CQRS benchmark 的统一命令行入口。
/// </summary>
internal static class Program
{
    /// <summary>
    ///     运行当前程序集中的全部 benchmark。
    /// </summary>
    /// <param name="args">透传给 BenchmarkDotNet 的命令行参数。</param>
    private static void Main(string[] args)
    {
        ConsoleLogger.Default.WriteLine("Running GFramework.Cqrs benchmarks");
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
