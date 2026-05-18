/*
 * File: ResourcesManager.DataTypes.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * ResourcesManager 的嵌套类型定义，包括 JSON 转换器、枚举、DTO 包装类。
 * 拆分自 ResourcesManager.cs 以减小主文件体积。
 */

using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Quest;

namespace hd2dtest.Scripts.Managers
{
    public partial class ResourcesManager : Node
    {
        /// <summary>
        /// 自定义任务目标转换器（用于JSON反序列化）
        /// </summary>
        public class QuestObjectiveConverter : JsonConverter<QuestObjective>
        {
            public override QuestObjective Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                    throw new JsonException("Expected StartObject token");

                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeElement))
                    throw new JsonException("Missing 'type' property in QuestObjective");

                var objectiveType = typeElement.GetString();
                var rawText = root.GetRawText();

                QuestObjective objective = objectiveType switch
                {
                    "Talk" => JsonSerializer.Deserialize<TalkObjective>(rawText, options),
                    "Kill" => JsonSerializer.Deserialize<KillObjective>(rawText, options),
                    "Collect" => JsonSerializer.Deserialize<CollectObjective>(rawText, options),
                    "ReachLocation" => JsonSerializer.Deserialize<ReachLocationObjective>(rawText, options),
                    "Escort" => JsonSerializer.Deserialize<EscortObjective>(rawText, options),
                    "Rescue" => JsonSerializer.Deserialize<RescueObjective>(rawText, options),
                    "Defend" => JsonSerializer.Deserialize<DefendObjective>(rawText, options),
                    "Protect" => JsonSerializer.Deserialize<ProtectObjective>(rawText, options),
                    "Prepare" => JsonSerializer.Deserialize<PrepareObjective>(rawText, options),
                    "Attend" => JsonSerializer.Deserialize<AttendObjective>(rawText, options),
                    "Discover" => JsonSerializer.Deserialize<DiscoverObjective>(rawText, options),
                    _ => JsonSerializer.Deserialize<TalkObjective>(rawText, options)
                };

                return objective;
            }

            public override void Write(Utf8JsonWriter writer, QuestObjective value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        private class QuestRewardConverter : JsonConverter<QuestReward>
        {
            public override QuestReward Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            {
                if (reader.TokenType != JsonTokenType.StartObject)
                {
                    throw new JsonException("Expected StartObject token");
                }

                using var doc = JsonDocument.ParseValue(ref reader);
                var root = doc.RootElement;

                if (!root.TryGetProperty("type", out var typeElement))
                {
                    throw new JsonException("Missing 'type' property in QuestReward");
                }

                var rewardType = typeElement.GetString();
                var rawText = root.GetRawText();

                return rewardType switch
                {
                    "Experience" => JsonSerializer.Deserialize<ExperienceReward>(rawText, options),
                    "Item" => JsonSerializer.Deserialize<ItemReward>(rawText, options),
                    "Currency" => JsonSerializer.Deserialize<CurrencyReward>(rawText, options),
                    "Equipment" => JsonSerializer.Deserialize<EquipmentReward>(rawText, options),
                    _ => JsonSerializer.Deserialize<ItemReward>(rawText, options)
                };
            }

            public override void Write(Utf8JsonWriter writer, QuestReward value, JsonSerializerOptions options)
            {
                JsonSerializer.Serialize(writer, value, value.GetType(), options);
            }
        }

        /// <summary>
        /// 资源加载优先级
        /// </summary>
        public enum ResourceLoadPriority
        {
            Critical = 0,
            High = 1,
            Medium = 2,
            Low = 3
        }

        /// <summary>
        /// 资源加载状态
        /// </summary>
        public enum ResourceLoadStatus
        {
            NotLoaded,
            Loading,
            Loaded,
            Failed
        }

        /// <summary>
        /// 资源加载信息
        /// </summary>
        public class ResourceLoadInfo
        {
            public string ResourceName { get; set; }
            public ResourceLoadPriority Priority { get; set; }
            public ResourceLoadStatus Status { get; set; }
            public double LoadTimeMs { get; set; }
            public long FileSizeBytes { get; set; }
            public string ErrorMessage { get; set; }
        }

        /// <summary>
        /// ViewRegister项临时类
        /// </summary>
        private class ViewRegisterItem
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("tscn")]
            public string Tscn { get; set; }
        }

        /// <summary>
        /// 任务数据包装类（用于JSON反序列化）
        /// </summary>
        public class QuestDataWrapper
        {
            [JsonPropertyName("quests")]
            public List<QuestData> Quests { get; set; }
        }

        /// <summary>
        /// 任务线数据包装类（用于JSON反序列化）
        /// </summary>
        public class QuestLineWrapper
        {
            [JsonPropertyName("questLines")]
            public List<QuestLineData> QuestLines { get; set; }
        }

        /// <summary>
        /// 地图数据包装类（用于JSON反序列化）
        /// </summary>
        public class MapDataWrapper
        {
            [JsonPropertyName("maps")]
            public List<MapData> Maps { get; set; }
        }

        /// <summary>
        /// NPC 静态数据包装类（用于 JSON 反序列化）
        /// </summary>
        public class NpcStaticDataWrapper
        {
            [JsonPropertyName("npcs")]
            public List<NpcStaticData> Npcs { get; set; }
        }

        /// <summary>
        /// NPC 静态数据
        /// </summary>
        public class NpcStaticData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("type")]
            public NPC.NPCType? Type { get; set; }

            [JsonPropertyName("dialogue")]
            public string Dialogue { get; set; }

            [JsonPropertyName("dialogueGraphId")]
            public string DialogueGraphId { get; set; }

            [JsonPropertyName("isInteractive")]
            public bool? IsInteractive { get; set; }

            [JsonPropertyName("availableInteractions")]
            public List<NPC.InteractionType> AvailableInteractions { get; set; }

            [JsonPropertyName("detectionRadius")]
            public float? DetectionRadius { get; set; }
        }

        /// <summary>
        /// 对话章节数据包装类（用于JSON反序列化）
        /// </summary>
        public class DialogueChapterWrapper
        {
            [JsonPropertyName("chapters")]
            public List<DialogueChapterData> Chapters { get; set; }
        }

        /// <summary>
        /// 对话章节数据
        /// </summary>
        public class DialogueChapterData
        {
            [JsonPropertyName("chapterId")]
            public string ChapterId { get; set; }

            [JsonPropertyName("chapterName")]
            public string ChapterName { get; set; }

            [JsonPropertyName("dialogues")]
            public List<DialogueEntryData> Dialogues { get; set; }
        }

        /// <summary>
        /// 对话条目数据
        /// </summary>
        public class DialogueEntryData
        {
            [JsonPropertyName("id")]
            public string Id { get; set; }

            [JsonPropertyName("questId")]
            public string QuestId { get; set; }

            [JsonPropertyName("npcId")]
            public string NpcId { get; set; }

            [JsonPropertyName("mapId")]
            public string MapId { get; set; }

            [JsonPropertyName("lines")]
            public List<DialogueLineData> Lines { get; set; }

            [JsonPropertyName("conditions")]
            public List<Dictionary<string, object>> Conditions { get; set; }

            [JsonPropertyName("effects")]
            public List<Dictionary<string, object>> Effects { get; set; }
        }

        /// <summary>
        /// 对话行数据
        /// </summary>
        public class DialogueLineData
        {
            [JsonPropertyName("speaker")]
            public string Speaker { get; set; }

            [JsonPropertyName("text")]
            public string Text { get; set; }

            [JsonPropertyName("emotion")]
            public string Emotion { get; set; }

            [JsonPropertyName("animation")]
            public string Animation { get; set; }
        }
    }
}
