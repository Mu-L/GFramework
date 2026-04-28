using System.IO;
using GFramework.Core.Abstractions.Resource;

namespace GFramework.Core.Tests.Resource;

/// <summary>
///     为 ResourceManager 测试提供可控数据源的资源加载器。
/// </summary>
public class TestResourceLoader : IResourceLoader<TestResource>
{
    private readonly Dictionary<string, string> _resourceData = new(StringComparer.Ordinal);

    /// <inheritdoc />
    public TestResource Load(string path)
    {
        if (_resourceData.TryGetValue(path, out var content))
        {
            return new TestResource { Content = content };
        }

        throw new FileNotFoundException($"Resource not found: {path}");
    }

    /// <inheritdoc />
    public async Task<TestResource> LoadAsync(string path)
    {
        await Task.Delay(10).ConfigureAwait(false); // 模拟异步加载
        return Load(path);
    }

    /// <inheritdoc />
    public void Unload(TestResource resource)
    {
        resource.IsDisposed = true;
    }

    /// <inheritdoc />
    public bool CanLoad(string path)
    {
        return _resourceData.ContainsKey(path);
    }

    /// <summary>
    ///     向测试加载器注册一条可返回的资源数据。
    /// </summary>
    /// <param name="path">资源路径。</param>
    /// <param name="content">资源内容。</param>
    public void AddTestData(string path, string content)
    {
        _resourceData[path] = content;
    }
}
