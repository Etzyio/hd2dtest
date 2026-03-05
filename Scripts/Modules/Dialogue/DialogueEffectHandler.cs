using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// 处理对话事件/效果的执行
    /// </summary>
    public class DialogueEffectHandler
    {
        /// <summary>
        /// 运行效果的节点上下文（例如 DialogueUI）
        /// </summary>
        private Node _context;

        /// <summary>
        /// 初始化对话效果处理器的新实例
        /// </summary>
        /// <param name="context">运行效果的节点上下文，通常为 DialogueUI 节点</param>
        public DialogueEffectHandler(Node context)
        {
            _context = context;
        }

        /// <summary>
        /// 处理对话事件，根据事件类型调用相应的处理方法
        /// </summary>
        /// <param name="evt">要处理的对话事件</param>
        /// <remarks>
        /// 支持的事件类型包括：
        /// - PlaySound：播放声音效果
        /// - ScreenShake：屏幕震动效果
        /// - AddQuest：添加任务
        /// - GiveItem：给予物品
        /// </remarks>
        public void HandleEvent(DialogueEvent evt)
        {
            Log.Info($"Handling Dialogue Event: {evt.Type}");

            switch (evt.Type)
            {
                case "PlaySound":
                    HandlePlaySound(evt.Parameters);
                    break;
                case "ScreenShake":
                    HandleScreenShake(evt.Parameters);
                    break;
                case "AddQuest":
                    HandleAddQuest(evt.Parameters);
                    break;
                case "GiveItem":
                    HandleGiveItem(evt.Parameters);
                    break;
                default:
                    Log.Warning($"Unknown event type: {evt.Type}");
                    break;
            }
        }

        /// <summary>
        /// 处理播放声音事件
        /// </summary>
        /// <param name="parameters">事件参数字典，应包含"sound_id"键</param>
        /// <remarks>
        /// 该方法从参数字典中获取声音 ID，并尝试通过上下文节点的 AudioStreamPlayer 播放声音。
        /// 在真实游戏中，应集成 AudioManager 来统一管理声音播放。
        /// </remarks>
        private void HandlePlaySound(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("sound_id", out string soundId))
            {
                // 在真实游戏中，这会调用 AudioManager.PlaySound(soundId)
                Log.Info($"Playing Sound: {soundId}");

                // 如果上下文有 AudioStreamPlayer，尝试播放
                if (_context.HasNode("AudioPlayer"))
                {
                    var player = _context.GetNode<AudioStreamPlayer>("AudioPlayer");
                    // 在此处加载声音逻辑
                }
            }
        }

        /// <summary>
        /// 处理屏幕震动事件
        /// </summary>
        /// <param name="parameters">事件参数字典（当前未使用）</param>
        /// <remarks>
        /// 该方法使用 Tween 实现屏幕震动效果，震动 5 次，每次偏移 -5 到 5 像素。
        /// 仅当上下文节点为 Control 类型时生效。
        /// </remarks>
        private void HandleScreenShake(Dictionary<string, string> parameters)
        {
            // 震动逻辑
            if (_context is Control control)
            {
                var tween = control.CreateTween();
                Vector2 originalPos = control.Position;
                for (int i = 0; i < 5; i++)
                {
                    tween.TweenProperty(control, "position", originalPos + new Vector2(GD.RandRange(-5, 5), GD.RandRange(-5, 5)), 0.05f);
                    tween.TweenProperty(control, "position", originalPos, 0.05f);
                }
            }
        }

        /// <summary>
        /// 处理添加任务事件
        /// </summary>
        /// <param name="parameters">事件参数字典，应包含"quest_id"键</param>
        /// <remarks>
        /// 该方法从参数字典中获取任务 ID，并记录日志。
        /// 在真实游戏中，应调用 QuestManager.AddQuest(questId) 来添加任务。
        /// </remarks>
        private void HandleAddQuest(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("quest_id", out string questId))
            {
                Log.Info($"Adding Quest: {questId}");
                // QuestManager.AddQuest(questId); // 添加任务
            }
        }

        /// <summary>
        /// 处理给予物品事件
        /// </summary>
        /// <param name="parameters">事件参数字典，应包含"item_id"键</param>
        /// <remarks>
        /// 该方法从参数字典中获取物品 ID，并记录日志。
        /// 在真实游戏中，应调用 InventoryManager.AddItem(itemId) 来添加物品。
        /// </remarks>
        private void HandleGiveItem(Dictionary<string, string> parameters)
        {
            if (parameters.TryGetValue("item_id", out string itemId))
            {
                Log.Info($"Giving Item: {itemId}");
                // InventoryManager.AddItem(itemId); // 添加物品
            }
        }
    }
}
