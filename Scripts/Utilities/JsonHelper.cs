using System;
using System.Text.Json;
using Godot;

namespace hd2dtest.Scripts.Utilities
{
    public static class JsonHelper
    {
        private static readonly JsonSerializerOptions DefaultOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true
        };

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
