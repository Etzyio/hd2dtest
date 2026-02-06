using System;

namespace hd2dtest.Scripts.Utilities
{
    public static class SingletonHelper
    {
        public static T GetInstance<T>(T instance, string singletonName) where T : class
        {
            if (instance == null)
            {
                Log.Error($"{singletonName} instance is not available");
                return null;
            }
            return instance;
        }

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
