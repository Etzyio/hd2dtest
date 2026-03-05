using System;

namespace hd2dtest.Scripts.Utilities
{
    /// <summary>
    /// 单例助手类，提供安全的单例模式访问方法
    /// </summary>
    /// <remarks>
    /// 该类提供了一系列静态方法，用于安全地访问和操作单例实例。
    /// 所有方法都包含空值检查和错误处理，确保在单例不可用时的安全性。
    /// </remarks>
    public static class SingletonHelper
    {
        /// <summary>
        /// 获取单例实例，如果实例不可用则返回 null
        /// </summary>
        /// <typeparam name="T">单例类型</typeparam>
        /// <param name="instance">单例实例</param>
        /// <param name="singletonName">单例名称，用于错误日志</param>
        /// <returns>单例实例，如果不可用则返回 null</returns>
        /// <remarks>
        /// 该方法用于安全地获取单例实例。
        /// 如果实例为 null，将记录错误日志并返回 null。
        /// </remarks>
        /// <example>
        /// <code>
        /// var instance = SingletonHelper.GetInstance(myInstance, "MySingleton");
        /// if (instance != null)
        /// {
        ///     // 使用实例
        /// }
        /// </code>
        /// </example>
        public static T GetInstance<T>(T instance, string singletonName) where T : class
        {
            if (instance == null)
            {
                Log.Error($"{singletonName} instance is not available");
                return null;
            }
            return instance;
        }

        /// <summary>
        /// 尝试获取单例实例，返回是否成功
        /// </summary>
        /// <typeparam name="T">单例类型</typeparam>
        /// <param name="instance">单例实例</param>
        /// <param name="result">输出参数，获取到的实例</param>
        /// <param name="singletonName">单例名称，用于错误日志</param>
        /// <returns>如果实例可用返回 true，否则返回 false</returns>
        /// <remarks>
        /// 该方法提供了一种更安全的方式来获取单例实例，避免了 null 检查。
        /// 如果实例为 null，将记录错误日志并返回 false。
        /// </remarks>
        /// <example>
        /// <code>
        /// if (SingletonHelper.TryGetInstance(myInstance, out var result, "MySingleton"))
        /// {
        ///     // 使用 result
        /// }
        /// </code>
        /// </example>
        public static bool TryGetInstance<T>(T instance, out T result, string singletonName) where T : class
        {
            result = instance;
            if (instance == null)
            {
                Log.Error($"{singletonName} instance is not available");
                return false;
            }
            return true;
        }

        /// <summary>
        /// 如果单例实例可用，执行指定的操作
        /// </summary>
        /// <typeparam name="T">单例类型</typeparam>
        /// <param name="instance">单例实例</param>
        /// <param name="singletonName">单例名称，用于错误日志</param>
        /// <param name="action">要执行的操作</param>
        /// <remarks>
        /// 该方法用于安全地执行需要单例实例的操作。
        /// 如果实例为 null 或操作抛出异常，将记录错误日志。
        /// </remarks>
        /// <example>
        /// <code>
        /// SingletonHelper.ExecuteIfAvailable(gameManager, "GameManager", mgr => {
        ///     mgr.LoadScene("Level1");
        /// });
        /// </code>
        /// </example>
        public static void ExecuteIfAvailable<T>(T instance, string singletonName, Action<T> action) where T : class
        {
            if (instance != null)
            {
                try
                {
                    action(instance);
                }
                catch (Exception e)
                {
                    Log.Error($"Error executing action on {singletonName}: {e.Message}");
                }
            }
            else
            {
                Log.Error($"{singletonName} instance is not available");
            }
        }

        /// <summary>
        /// 如果单例实例可用，执行指定的函数并返回结果
        /// </summary>
        /// <typeparam name="T">单例类型</typeparam>
        /// <typeparam name="TResult">返回值类型</typeparam>
        /// <param name="instance">单例实例</param>
        /// <param name="singletonName">单例名称，用于错误日志</param>
        /// <param name="func">要执行的函数</param>
        /// <param name="defaultValue">默认返回值，当实例不可用或发生异常时返回</param>
        /// <returns>函数执行结果，如果实例不可用或发生异常则返回 defaultValue</returns>
        /// <remarks>
        /// 该方法用于安全地执行需要单例实例并返回结果的函数。
        /// 如果实例为 null 或函数抛出异常，将记录错误日志并返回默认值。
        /// </remarks>
        /// <example>
        /// <code>
        /// var score = SingletonHelper.ExecuteIfAvailable(
        ///     gameManager, 
        ///     "GameManager", 
        ///     mgr => mgr.GetPlayerScore(), 
        ///     0
        /// );
        /// </code>
        /// </example>
        public static TResult ExecuteIfAvailable<T, TResult>(T instance, string singletonName, Func<T, TResult> func, TResult defaultValue = default) where T : class
        {
            if (instance != null)
            {
                try
                {
                    return func(instance);
                }
                catch (Exception e)
                {
                    Log.Error($"Error executing function on {singletonName}: {e.Message}");
                    return defaultValue;
                }
            }
            else
            {
                Log.Error($"{singletonName} instance is not available");
                return defaultValue;
            }
        }
    }
}
