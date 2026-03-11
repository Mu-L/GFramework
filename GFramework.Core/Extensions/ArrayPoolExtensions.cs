using System.Buffers;

namespace GFramework.Core.Extensions;

/// <summary>
///     ArrayPool 扩展方法，提供更便捷的数组池操作
/// </summary>
public static class ArrayPoolExtensions
{
    /// <summary>
    ///     从数组池中租用数组
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="pool">数组池实例</param>
    /// <param name="minimumLength">最小长度</param>
    /// <returns>租用的数组</returns>
    /// <example>
    /// <code>
    /// var pool = ArrayPool&lt;int&gt;.Shared;
    /// var array = pool.RentArray(100);
    /// try
    /// {
    ///     // 使用数组
    /// }
    /// finally
    /// {
    ///     pool.ReturnArray(array);
    /// }
    /// </code>
    /// </example>
    public static T[] RentArray<T>(this ArrayPool<T> pool, int minimumLength)
    {
        ArgumentNullException.ThrowIfNull(pool);
        return pool.Rent(minimumLength);
    }

    /// <summary>
    ///     将数组归还到数组池
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="pool">数组池实例</param>
    /// <param name="array">要归还的数组</param>
    /// <param name="clearArray">是否清空数组内容</param>
    /// <example>
    /// <code>
    /// var pool = ArrayPool&lt;int&gt;.Shared;
    /// var array = pool.RentArray(100);
    /// try
    /// {
    ///     // 使用数组
    /// }
    /// finally
    /// {
    ///     pool.ReturnArray(array, clearArray: true);
    /// }
    /// </code>
    /// </example>
    public static void ReturnArray<T>(this ArrayPool<T> pool, T[] array, bool clearArray = false)
    {
        ArgumentNullException.ThrowIfNull(pool);
        ArgumentNullException.ThrowIfNull(array);
        pool.Return(array, clearArray);
    }

    /// <summary>
    ///     获取一个作用域数组，使用完后自动归还
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    /// <param name="pool">数组池实例</param>
    /// <param name="minimumLength">最小长度</param>
    /// <param name="clearOnReturn">归还时是否清空数组</param>
    /// <returns>可自动释放的数组包装器</returns>
    /// <example>
    /// <code>
    /// var pool = ArrayPool&lt;int&gt;.Shared;
    /// using var scopedArray = pool.GetScoped(100);
    /// var array = scopedArray.Array;
    /// // 使用数组
    /// // 自动归还
    /// </code>
    /// </example>
    public static ScopedArray<T> GetScoped<T>(this ArrayPool<T> pool, int minimumLength, bool clearOnReturn = false)
    {
        ArgumentNullException.ThrowIfNull(pool);
        return new ScopedArray<T>(pool, minimumLength, clearOnReturn);
    }

    /// <summary>
    ///     可自动释放的数组包装器
    /// </summary>
    /// <typeparam name="T">数组元素类型</typeparam>
    public ref struct ScopedArray<T>
    {
        private readonly ArrayPool<T> _pool;
        private readonly bool _clearOnReturn;
        private T[]? _array;

#pragma warning disable CA1819
        /// <summary>
        ///     获取租用的数组
        /// </summary>
        public T[] Array => _array!;
#pragma warning restore CA1819

        /// <summary>
        ///     获取数组的长度
        /// </summary>
        public int Length => _array!.Length;

        internal ScopedArray(ArrayPool<T> pool, int minimumLength, bool clearOnReturn)
        {
            _pool = pool;
            _clearOnReturn = clearOnReturn;
            _array = pool.Rent(minimumLength);
        }

        /// <summary>
        ///     释放数组并归还到池中
        /// </summary>
        public void Dispose()
        {
            if (_array is null)
                return;

            _pool.Return(_array, _clearOnReturn);
            _array = null;
        }

        /// <summary>
        ///     获取数组的 Span 视图
        /// </summary>
        /// <returns>数组的 Span</returns>
        public Span<T> AsSpan() => _array!;

        /// <summary>
        ///     获取数组指定范围的 Span 视图
        /// </summary>
        /// <param name="start">起始索引</param>
        /// <param name="length">长度</param>
        /// <returns>数组指定范围的 Span</returns>
        public Span<T> AsSpan(int start, int length)
            => _array!.AsSpan(start, length);

        /// <summary>
        ///     获取数组指定索引处的引用
        /// </summary>
        /// <param name="index">要获取引用的索引位置</param>
        /// <returns>指定索引处元素的引用</returns>
        public ref T this[int index] => ref _array![index];
    }
}