using GFramework.Core.Abstractions.Events;
using GFramework.Game.Abstractions.Config;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace GFramework.Game.Config;

/// <summary>
///     基于文件目录的 YAML 配置加载器。
///     该实现用于 Runtime MVP 的文本配置接入阶段，通过显式注册表定义描述要加载的配置域，
///     再在一次加载流程中统一解析并写入配置注册表。
/// </summary>
public sealed class YamlConfigLoader : IConfigLoader
{
    private const string RootPathCannotBeNullOrWhiteSpaceMessage = "Root path cannot be null or whitespace.";
    private const string TableNameCannotBeNullOrWhiteSpaceMessage = "Table name cannot be null or whitespace.";
    private const string RelativePathCannotBeNullOrWhiteSpaceMessage = "Relative path cannot be null or whitespace.";

    private const string SchemaRelativePathCannotBeNullOrWhiteSpaceMessage =
        "Schema relative path cannot be null or whitespace.";

    private readonly IDeserializer _deserializer;
    private readonly List<IYamlTableRegistration> _registrations = new();
    private readonly string _rootPath;

    /// <summary>
    ///     使用指定配置根目录创建 YAML 配置加载器。
    /// </summary>
    /// <param name="rootPath">配置根目录。</param>
    /// <exception cref="ArgumentException">当 <paramref name="rootPath" /> 为空时抛出。</exception>
    public YamlConfigLoader(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
        {
            throw new ArgumentException(RootPathCannotBeNullOrWhiteSpaceMessage, nameof(rootPath));
        }

        _rootPath = rootPath;
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <summary>
    ///     获取配置根目录。
    /// </summary>
    public string RootPath => _rootPath;

    /// <summary>
    ///     获取当前已注册的配置表定义数量。
    /// </summary>
    public int RegistrationCount => _registrations.Count;

    /// <inheritdoc />
    public async Task LoadAsync(IConfigRegistry registry, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(registry);

        var loadedTables = new List<(string name, IConfigTable table)>(_registrations.Count);

        foreach (var registration in _registrations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            loadedTables.Add(await registration.LoadAsync(_rootPath, _deserializer, cancellationToken));
        }

        // 仅当本轮所有配置表都成功加载后才写入注册表，避免暴露部分成功的中间状态。
        foreach (var (name, table) in loadedTables)
        {
            RegistrationDispatcher.Register(registry, name, table);
        }
    }

    /// <summary>
    ///     启用开发期热重载。
    ///     该能力会监听已注册配置表对应的配置目录和 schema 文件，并在检测到文件变更后按表粒度重新加载。
    ///     重载失败时会保留注册表中的旧表，避免开发期错误配置直接破坏当前运行时状态。
    /// </summary>
    /// <param name="registry">要被热重载更新的配置注册表。</param>
    /// <param name="onTableReloaded">单个配置表重载成功后的可选回调。</param>
    /// <param name="onTableReloadFailed">单个配置表重载失败后的可选回调。</param>
    /// <param name="debounceDelay">防抖延迟；为空时默认使用 200 毫秒。</param>
    /// <returns>用于停止热重载监听的注销句柄。</returns>
    /// <exception cref="ArgumentNullException">当 <paramref name="registry" /> 为空时抛出。</exception>
    public IUnRegister EnableHotReload(
        IConfigRegistry registry,
        Action<string>? onTableReloaded = null,
        Action<string, Exception>? onTableReloadFailed = null,
        TimeSpan? debounceDelay = null)
    {
        ArgumentNullException.ThrowIfNull(registry);

        return new HotReloadSession(
            _rootPath,
            _deserializer,
            registry,
            _registrations,
            onTableReloaded,
            onTableReloadFailed,
            debounceDelay ?? TimeSpan.FromMilliseconds(200));
    }

    /// <summary>
    ///     注册一个 YAML 配置表定义。
    ///     主键提取逻辑由调用方显式提供，以避免在 Runtime MVP 阶段引入额外特性或约定推断。
    /// </summary>
    /// <typeparam name="TKey">配置主键类型。</typeparam>
    /// <typeparam name="TValue">配置值类型。</typeparam>
    /// <param name="tableName">配置表名称。</param>
    /// <param name="relativePath">相对配置根目录的子目录。</param>
    /// <param name="keySelector">配置项主键提取器。</param>
    /// <param name="comparer">可选主键比较器。</param>
    /// <returns>当前加载器实例，以便链式注册。</returns>
    public YamlConfigLoader RegisterTable<TKey, TValue>(
        string tableName,
        string relativePath,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return RegisterTableCore(tableName, relativePath, null, keySelector, comparer);
    }

    /// <summary>
    ///     注册一个带 schema 校验的 YAML 配置表定义。
    ///     该重载会在 YAML 反序列化之前使用指定 schema 拒绝未知字段、缺失必填字段和基础类型错误，
    ///     以避免错误配置以默认值形式悄悄进入运行时。
    /// </summary>
    /// <typeparam name="TKey">配置主键类型。</typeparam>
    /// <typeparam name="TValue">配置值类型。</typeparam>
    /// <param name="tableName">配置表名称。</param>
    /// <param name="relativePath">相对配置根目录的子目录。</param>
    /// <param name="schemaRelativePath">相对配置根目录的 schema 文件路径。</param>
    /// <param name="keySelector">配置项主键提取器。</param>
    /// <param name="comparer">可选主键比较器。</param>
    /// <returns>当前加载器实例，以便链式注册。</returns>
    public YamlConfigLoader RegisterTable<TKey, TValue>(
        string tableName,
        string relativePath,
        string schemaRelativePath,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey>? comparer = null)
        where TKey : notnull
    {
        return RegisterTableCore(tableName, relativePath, schemaRelativePath, keySelector, comparer);
    }

    private YamlConfigLoader RegisterTableCore<TKey, TValue>(
        string tableName,
        string relativePath,
        string? schemaRelativePath,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey>? comparer)
        where TKey : notnull
    {
        if (string.IsNullOrWhiteSpace(tableName))
        {
            throw new ArgumentException(TableNameCannotBeNullOrWhiteSpaceMessage, nameof(tableName));
        }

        if (string.IsNullOrWhiteSpace(relativePath))
        {
            throw new ArgumentException(RelativePathCannotBeNullOrWhiteSpaceMessage, nameof(relativePath));
        }

        ArgumentNullException.ThrowIfNull(keySelector);

        if (schemaRelativePath != null && string.IsNullOrWhiteSpace(schemaRelativePath))
        {
            throw new ArgumentException(
                SchemaRelativePathCannotBeNullOrWhiteSpaceMessage,
                nameof(schemaRelativePath));
        }

        _registrations.Add(
            new YamlTableRegistration<TKey, TValue>(
                tableName,
                relativePath,
                schemaRelativePath,
                keySelector,
                comparer));
        return this;
    }

    /// <summary>
    ///     负责在非泛型配置表与泛型注册表方法之间做分派。
    ///     该静态助手将运行时反射局部封装在加载器内部，避免向外暴露弱类型注册 API。
    /// </summary>
    private static class RegistrationDispatcher
    {
        /// <summary>
        ///     将强类型配置表写入注册表。
        /// </summary>
        /// <param name="registry">目标配置注册表。</param>
        /// <param name="name">配置表名称。</param>
        /// <param name="table">已加载的配置表实例。</param>
        /// <exception cref="InvalidOperationException">当传入表未实现强类型配置表契约时抛出。</exception>
        public static void Register(IConfigRegistry registry, string name, IConfigTable table)
        {
            var tableInterface = table.GetType()
                .GetInterfaces()
                .FirstOrDefault(static type =>
                    type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IConfigTable<,>));

            if (tableInterface == null)
            {
                throw new InvalidOperationException(
                    $"Loaded config table '{name}' does not implement '{typeof(IConfigTable<,>).Name}'.");
            }

            var genericArguments = tableInterface.GetGenericArguments();
            var method = typeof(IConfigRegistry)
                .GetMethod(nameof(IConfigRegistry.RegisterTable))!
                .MakeGenericMethod(genericArguments[0], genericArguments[1]);

            method.Invoke(registry, new object[] { name, table });
        }
    }

    /// <summary>
    ///     定义 YAML 配置表注册项的统一内部契约。
    /// </summary>
    private interface IYamlTableRegistration
    {
        /// <summary>
        ///     获取配置表名称。
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     获取相对配置根目录的子目录。
        /// </summary>
        string RelativePath { get; }

        /// <summary>
        ///     获取相对配置根目录的 schema 文件路径；未启用 schema 校验时返回空。
        /// </summary>
        string? SchemaRelativePath { get; }

        /// <summary>
        ///     从指定根目录加载配置表。
        /// </summary>
        /// <param name="rootPath">配置根目录。</param>
        /// <param name="deserializer">YAML 反序列化器。</param>
        /// <param name="cancellationToken">取消令牌。</param>
        /// <returns>已加载的配置表名称与配置表实例。</returns>
        Task<(string name, IConfigTable table)> LoadAsync(
            string rootPath,
            IDeserializer deserializer,
            CancellationToken cancellationToken);
    }

    /// <summary>
    ///     YAML 配置表注册项。
    /// </summary>
    /// <typeparam name="TKey">配置主键类型。</typeparam>
    /// <typeparam name="TValue">配置项值类型。</typeparam>
    private sealed class YamlTableRegistration<TKey, TValue> : IYamlTableRegistration
        where TKey : notnull
    {
        private readonly IEqualityComparer<TKey>? _comparer;
        private readonly Func<TValue, TKey> _keySelector;

        /// <summary>
        ///     初始化 YAML 配置表注册项。
        /// </summary>
        /// <param name="name">配置表名称。</param>
        /// <param name="relativePath">相对配置根目录的子目录。</param>
        /// <param name="schemaRelativePath">相对配置根目录的 schema 文件路径；未启用 schema 校验时为空。</param>
        /// <param name="keySelector">配置项主键提取器。</param>
        /// <param name="comparer">可选主键比较器。</param>
        public YamlTableRegistration(
            string name,
            string relativePath,
            string? schemaRelativePath,
            Func<TValue, TKey> keySelector,
            IEqualityComparer<TKey>? comparer)
        {
            Name = name;
            RelativePath = relativePath;
            SchemaRelativePath = schemaRelativePath;
            _keySelector = keySelector;
            _comparer = comparer;
        }

        /// <summary>
        ///     获取配置表名称。
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     获取相对配置根目录的子目录。
        /// </summary>
        public string RelativePath { get; }

        /// <summary>
        ///     获取相对配置根目录的 schema 文件路径；未启用 schema 校验时返回空。
        /// </summary>
        public string? SchemaRelativePath { get; }

        /// <inheritdoc />
        public async Task<(string name, IConfigTable table)> LoadAsync(
            string rootPath,
            IDeserializer deserializer,
            CancellationToken cancellationToken)
        {
            var directoryPath = Path.Combine(rootPath, RelativePath);
            if (!Directory.Exists(directoryPath))
            {
                throw new DirectoryNotFoundException(
                    $"Config directory '{directoryPath}' was not found for table '{Name}'.");
            }

            YamlConfigSchema? schema = null;
            if (!string.IsNullOrEmpty(SchemaRelativePath))
            {
                var schemaPath = Path.Combine(rootPath, SchemaRelativePath);
                schema = await YamlConfigSchemaValidator.LoadAsync(schemaPath, cancellationToken);
            }

            var values = new List<TValue>();
            var files = Directory
                .EnumerateFiles(directoryPath, "*.*", SearchOption.TopDirectoryOnly)
                .Where(static path =>
                    path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
                .OrderBy(static path => path, StringComparer.Ordinal)
                .ToArray();

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string yaml;
                try
                {
                    yaml = await File.ReadAllTextAsync(file, cancellationToken);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Failed to read config file '{file}' for table '{Name}'.",
                        exception);
                }

                if (schema != null)
                {
                    // 先按 schema 拒绝结构问题，避免被 IgnoreUnmatchedProperties 或默认值掩盖配置错误。
                    YamlConfigSchemaValidator.Validate(schema, file, yaml);
                }

                try
                {
                    var value = deserializer.Deserialize<TValue>(yaml);

                    if (value == null)
                    {
                        throw new InvalidOperationException("YAML content was deserialized to null.");
                    }

                    values.Add(value);
                }
                catch (Exception exception)
                {
                    throw new InvalidOperationException(
                        $"Failed to deserialize config file '{file}' for table '{Name}' as '{typeof(TValue).Name}'.",
                        exception);
                }
            }

            try
            {
                var table = new InMemoryConfigTable<TKey, TValue>(values, _keySelector, _comparer);
                return (Name, table);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    $"Failed to build config table '{Name}' from directory '{directoryPath}'.",
                    exception);
            }
        }
    }

    /// <summary>
    ///     封装开发期热重载所需的文件监听与按表重载逻辑。
    ///     该会话只影响通过当前加载器注册的表，不尝试接管注册表中的其他来源数据。
    /// </summary>
    private sealed class HotReloadSession : IUnRegister, IDisposable
    {
        private readonly TimeSpan _debounceDelay;
        private readonly IDeserializer _deserializer;
        private readonly object _gate = new();
        private readonly Action<string>? _onTableReloaded;
        private readonly Action<string, Exception>? _onTableReloadFailed;
        private readonly Dictionary<string, IYamlTableRegistration> _registrations = new(StringComparer.Ordinal);
        private readonly IConfigRegistry _registry;
        private readonly Dictionary<string, SemaphoreSlim> _reloadLocks = new(StringComparer.Ordinal);
        private readonly Dictionary<string, CancellationTokenSource> _reloadTokens = new(StringComparer.Ordinal);
        private readonly string _rootPath;
        private readonly List<FileSystemWatcher> _watchers = new();
        private bool _disposed;

        /// <summary>
        ///     初始化一个热重载会话并立即开始监听文件变更。
        /// </summary>
        /// <param name="rootPath">配置根目录。</param>
        /// <param name="deserializer">YAML 反序列化器。</param>
        /// <param name="registry">要更新的配置注册表。</param>
        /// <param name="registrations">已注册的配置表定义。</param>
        /// <param name="onTableReloaded">单表重载成功回调。</param>
        /// <param name="onTableReloadFailed">单表重载失败回调。</param>
        /// <param name="debounceDelay">监听事件防抖延迟。</param>
        public HotReloadSession(
            string rootPath,
            IDeserializer deserializer,
            IConfigRegistry registry,
            IEnumerable<IYamlTableRegistration> registrations,
            Action<string>? onTableReloaded,
            Action<string, Exception>? onTableReloadFailed,
            TimeSpan debounceDelay)
        {
            ArgumentNullException.ThrowIfNull(rootPath);
            ArgumentNullException.ThrowIfNull(deserializer);
            ArgumentNullException.ThrowIfNull(registry);
            ArgumentNullException.ThrowIfNull(registrations);

            _rootPath = rootPath;
            _deserializer = deserializer;
            _registry = registry;
            _onTableReloaded = onTableReloaded;
            _onTableReloadFailed = onTableReloadFailed;
            _debounceDelay = debounceDelay;

            foreach (var registration in registrations)
            {
                _registrations.Add(registration.Name, registration);
                _reloadLocks.Add(registration.Name, new SemaphoreSlim(1, 1));
                CreateWatchersForRegistration(registration);
            }
        }

        /// <summary>
        ///     释放热重载会话持有的文件监听器与等待资源。
        /// </summary>
        public void Dispose()
        {
            List<FileSystemWatcher> watchersToDispose;
            List<CancellationTokenSource> reloadTokensToDispose;
            List<SemaphoreSlim> reloadLocksToDispose;

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                watchersToDispose = _watchers.ToList();
                _watchers.Clear();
                reloadTokensToDispose = _reloadTokens.Values.ToList();
                _reloadTokens.Clear();
                reloadLocksToDispose = _reloadLocks.Values.ToList();
                _reloadLocks.Clear();
            }

            foreach (var reloadToken in reloadTokensToDispose)
            {
                reloadToken.Cancel();
                reloadToken.Dispose();
            }

            foreach (var watcher in watchersToDispose)
            {
                watcher.Dispose();
            }

            foreach (var reloadLock in reloadLocksToDispose)
            {
                reloadLock.Dispose();
            }
        }

        /// <summary>
        ///     停止热重载监听。
        /// </summary>
        public void UnRegister()
        {
            Dispose();
        }

        private void CreateWatchersForRegistration(IYamlTableRegistration registration)
        {
            var configDirectoryPath = Path.Combine(_rootPath, registration.RelativePath);
            AddWatcher(configDirectoryPath, "*.yaml", registration.Name);
            AddWatcher(configDirectoryPath, "*.yml", registration.Name);

            if (string.IsNullOrEmpty(registration.SchemaRelativePath))
            {
                return;
            }

            var schemaFullPath = Path.Combine(_rootPath, registration.SchemaRelativePath);
            var schemaDirectoryPath = Path.GetDirectoryName(schemaFullPath);
            if (string.IsNullOrWhiteSpace(schemaDirectoryPath))
            {
                schemaDirectoryPath = _rootPath;
            }

            AddWatcher(schemaDirectoryPath, Path.GetFileName(schemaFullPath), registration.Name);
        }

        private void AddWatcher(string directoryPath, string filter, string tableName)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            var watcher = new FileSystemWatcher(directoryPath, filter)
            {
                IncludeSubdirectories = false,
                NotifyFilter = NotifyFilters.FileName |
                               NotifyFilters.LastWrite |
                               NotifyFilters.Size |
                               NotifyFilters.CreationTime |
                               NotifyFilters.DirectoryName
            };

            watcher.Changed += (_, _) => ScheduleReload(tableName);
            watcher.Created += (_, _) => ScheduleReload(tableName);
            watcher.Deleted += (_, _) => ScheduleReload(tableName);
            watcher.Renamed += (_, _) => ScheduleReload(tableName);
            watcher.Error += (_, eventArgs) =>
            {
                var exception = eventArgs.GetException() ?? new InvalidOperationException(
                    $"Hot reload watcher for table '{tableName}' encountered an unknown error.");
                InvokeReloadFailed(tableName, exception);
            };

            watcher.EnableRaisingEvents = true;

            lock (_gate)
            {
                if (_disposed)
                {
                    watcher.Dispose();
                    return;
                }

                _watchers.Add(watcher);
            }
        }

        private void ScheduleReload(string tableName)
        {
            CancellationTokenSource reloadTokenSource;

            lock (_gate)
            {
                if (_disposed)
                {
                    return;
                }

                if (_reloadTokens.TryGetValue(tableName, out var previousTokenSource))
                {
                    previousTokenSource.Cancel();
                    previousTokenSource.Dispose();
                }

                reloadTokenSource = new CancellationTokenSource();
                _reloadTokens[tableName] = reloadTokenSource;
            }

            _ = Task.Run(async () =>
            {
                try
                {
                    await Task.Delay(_debounceDelay, reloadTokenSource.Token);
                    await ReloadTableAsync(tableName, reloadTokenSource.Token);
                }
                catch (OperationCanceledException) when (reloadTokenSource.IsCancellationRequested)
                {
                    // 新事件会替换旧任务；取消属于正常防抖行为。
                }
                finally
                {
                    lock (_gate)
                    {
                        if (_reloadTokens.TryGetValue(tableName, out var currentTokenSource) &&
                            ReferenceEquals(currentTokenSource, reloadTokenSource))
                        {
                            _reloadTokens.Remove(tableName);
                        }
                    }

                    reloadTokenSource.Dispose();
                }
            });
        }

        private async Task ReloadTableAsync(string tableName, CancellationToken cancellationToken)
        {
            if (!_registrations.TryGetValue(tableName, out var registration))
            {
                return;
            }

            var reloadLock = _reloadLocks[tableName];
            await reloadLock.WaitAsync(cancellationToken);

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var (name, table) = await registration.LoadAsync(_rootPath, _deserializer, cancellationToken);
                RegistrationDispatcher.Register(_registry, name, table);
                InvokeReloaded(name);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // 防抖替换或会话关闭导致的取消不应视为错误。
            }
            catch (Exception exception)
            {
                InvokeReloadFailed(tableName, exception);
            }
            finally
            {
                reloadLock.Release();
            }
        }

        private void InvokeReloaded(string tableName)
        {
            if (_onTableReloaded == null)
            {
                return;
            }

            try
            {
                _onTableReloaded(tableName);
            }
            catch
            {
                // 诊断回调不应反向破坏热重载流程。
            }
        }

        private void InvokeReloadFailed(string tableName, Exception exception)
        {
            if (_onTableReloadFailed == null)
            {
                return;
            }

            try
            {
                _onTableReloadFailed(tableName, exception);
            }
            catch
            {
                // 诊断回调不应反向破坏热重载流程。
            }
        }
    }
}