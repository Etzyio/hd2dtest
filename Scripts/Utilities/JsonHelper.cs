using System;
using System.Text.Json;
using Godot;

namespace hd2dtest.Scripts.Utilities
{
    /// <summary>
    /// JSON 助手类，提供简化的 JSON 文件读写操作
    /// </summary>
    /// <remarks>
    /// 该类封装了 System.Text.Json 的功能，提供了更便捷的文件读写方法。
    /// 所有方法都包含完整的错误处理和日志记录。
    /// </remarks>
    public static class JsonHelper
    {
        /// <summary>
        /// 默认的 JSON 序列化选项
        /// </summary>
        /// <remarks>
        /// 配置说明：
        /// - PropertyNameCaseInsensitive = true：属性名不区分大小写
        /// - WriteIndented = true：格式化输出（缩进）
        /// </remarks>
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

        /// <summary>
        /// 从文件加载 JSON 数据并反序列化为指定类型
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="filePath">JSON 文件路径</param>
        /// <param name="options">序列化选项（可选，使用默认选项如果为 null）</param>
        /// <returns>反序列化后的对象，失败时返回 default(T)</returns>
        /// <remarks>
        /// 该方法执行以下步骤：
        /// 1. 检查文件是否存在
        /// 2. 读取文件内容
        /// 3. 反序列化 JSON 为指定类型
        /// 4. 记录任何错误到日志
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = JsonHelper.LoadJsonFile&lt;ConfigData&gt;("config.json");
        /// if (config != null)
        /// {
        ///     // 使用配置数据
        /// }
        /// </code>
        /// </example>
        public static T LoadJsonFile<T>(string filePath, JsonSerializerOptions options = null)
        {
            try
            {
                if (!Godot.FileAccess.FileExists(filePath))
                {
                    Log.Error($"File not found: {filePath}");
                    return default;
                }

                using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    Log.Error($"Failed to open file for reading: {filePath}");
                    return default;
                }

                string json = file.GetAsText();
                if (string.IsNullOrWhiteSpace(json))
                {
                    Log.Error($"File is empty or contains only whitespace: {filePath}");
                    return default;
                }

                var result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                if (result == null)
                {
                    Log.Error($"Failed to deserialize JSON from file: {filePath}");
                }

                return result;
            }
            catch (JsonException e)
            {
                Log.Error($"JSON parsing error in file {filePath}: {e.Message}");
                return default;
            }
            catch (Exception e)
            {
                Log.Error($"Error loading JSON file {filePath}: {e.Message}");
                return default;
            }
        }

        /// <summary>
        /// 将对象序列化为 JSON 并保存到文件
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="filePath">保存的文件路径</param>
        /// <param name="data">要序列化的数据对象</param>
        /// <param name="options">序列化选项（可选，使用默认选项如果为 null）</param>
        /// <returns>保存成功返回 true，失败返回 false</returns>
        /// <remarks>
        /// 该方法执行以下步骤：
        /// 1. 序列化对象为 JSON 字符串
        /// 2. 创建或覆盖目标文件
        /// 3. 写入 JSON 数据到文件
        /// 4. 记录任何错误到日志
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = new ConfigData { Volume = 0.8f };
        /// if (JsonHelper.SaveJsonFile("config.json", config))
        /// {
        ///     // 保存成功
        /// }
        /// </code>
        /// </example>
        public static bool SaveJsonFile<T>(string filePath, T data, JsonSerializerOptions options = null)
        {
            try
            {
                if (data == null)
                {
                    Log.Error($"Cannot save null data to file: {filePath}");
                    return false;
                }

                string json = JsonSerializer.Serialize(data, options ?? DefaultOptions);
                if (string.IsNullOrWhiteSpace(json))
                {
                    Log.Error($"Failed to serialize data to JSON for file: {filePath}");
                    return false;
                }

                using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Write);
                if (file == null)
                {
                    Log.Error($"Failed to open file for writing: {filePath}");
                    return false;
                }

                file.StoreString(json);
                Log.Info($"Successfully saved JSON file: {filePath}");
                return true;
            }
            catch (JsonException e)
            {
                Log.Error($"JSON serialization error for file {filePath}: {e.Message}");
                return false;
            }
            catch (Exception e)
            {
                Log.Error($"Error saving JSON file {filePath}: {e.Message}");
                return false;
            }
        }

        /// <summary>
        /// 从文件加载 JSON 数据并反序列化为指定类型，返回是否成功
        /// </summary>
        /// <typeparam name="T">目标类型</typeparam>
        /// <param name="filePath">JSON 文件路径</param>
        /// <param name="options">序列化选项</param>
        /// <param name="success">输出参数，表示加载是否成功</param>
        /// <returns>反序列化后的对象，失败时返回 default(T)</returns>
        /// <remarks>
        /// 该方法与 LoadJsonFile&lt;T&gt;(string, JsonSerializerOptions) 类似，
        /// 但通过 out 参数明确返回操作是否成功。
        /// </remarks>
        /// <example>
        /// <code>
        /// var config = JsonHelper.LoadJsonFile&lt;ConfigData&gt;("config.json", null, out bool success);
        /// if (success &amp;&amp; config != null)
        /// {
        ///     // 使用配置数据
        /// }
        /// </code>
        /// </example>
        public static T LoadJsonFile<T>(string filePath, JsonSerializerOptions options, out bool success)
        {
            success = false;
            try
            {
                if (!Godot.FileAccess.FileExists(filePath))
                {
                    Log.Error($"File not found: {filePath}");
                    return default;
                }

                using var file = Godot.FileAccess.Open(filePath, Godot.FileAccess.ModeFlags.Read);
                if (file == null)
                {
                    Log.Error($"Failed to open file for reading: {filePath}");
                    return default;
                }

                string json = file.GetAsText();
                if (string.IsNullOrWhiteSpace(json))
                {
                    Log.Error($"File is empty or contains only whitespace: {filePath}");
                    return default;
                }

                var result = JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
                if (result == null)
                {
                    Log.Error($"Failed to deserialize JSON from file: {filePath}");
                    return default;
                }

                success = true;
                return result;
            }
            catch (JsonException e)
            {
                Log.Error($"JSON parsing error in file {filePath}: {e.Message}");
                return default;
            }
            catch (Exception e)
            {
                Log.Error($"Error loading JSON file {filePath}: {e.Message}");
                return default;
            }
        }
    }
}
