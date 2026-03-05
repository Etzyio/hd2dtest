using System;
using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// 表示对话图中的一个节点
    /// </summary>
    public class DialogueNode
    {
        /// <summary>
        /// 节点的唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 说话者的名称
        /// </summary>
        public string SpeakerName { get; set; }

        /// <summary>
        /// 对话的文本内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 要播放的音频文件路径（可选）
        /// </summary>
        public string AudioPath { get; set; }

        /// <summary>
        /// 背景图片路径（可选）
        /// </summary>
        public string BackgroundPath { get; set; }

        /// <summary>
        /// 角色头像路径（可选）
        /// </summary>
        public string PortraitPath { get; set; }

        /// <summary>
        /// 玩家可用的选项列表。如果为空，则为线性对话
        /// </summary>
        public List<DialogueOption> Options { get; set; } = new List<DialogueOption>();

        /// <summary>
        /// 进入此节点时触发的事件列表
        /// </summary>
        public List<DialogueEvent> Events { get; set; } = new List<DialogueEvent>();

        /// <summary>
        /// 线性对话的下一个节点 ID（如果选项为空）
        /// </summary>
        public string NextNodeId { get; set; }
    }

    /// <summary>
    /// 表示玩家可以做出的选择
    /// </summary>
    public class DialogueOption
    {
        /// <summary>
        /// 选项显示的文本
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 如果选择此选项，目标节点的 ID
        /// </summary>
        public string TargetNodeId { get; set; }

        /// <summary>
        /// 此选项可用的条件（可选）
        /// 格式："条件类型：参数" 例如："HasItem:Key", "QuestState:QuestID:Completed"
        /// </summary>
        public string Condition { get; set; }
    }

    /// <summary>
    /// 表示对话期间发生的事件
    /// </summary>
    public class DialogueEvent
    {
        /// <summary>
        /// 事件类型（例如："PlaySound", "AddQuest", "ScreenShake"）
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 事件的参数
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// 完整对话图的容器
    /// </summary>
    public class DialogueGraph
    {
        /// <summary>
        /// 对话图的唯一 ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 起始节点 ID
        /// </summary>
        public string StartNodeId { get; set; }

        /// <summary>
        /// 图中的所有节点，以 ID 为键
        /// </summary>
        public Dictionary<string, DialogueNode> Nodes { get; set; } = new Dictionary<string, DialogueNode>();
    }
}
