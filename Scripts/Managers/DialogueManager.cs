/*
 * File: DialogueManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 对话管理器，作为全局单例负责管理游戏对话系统。
 * 支持对话图加载、节点导航、选项选择、条件检查和事件触发。
 * 
 * Key Features:
 * - 单例模式设计，全局可访问
 * - 对话图JSON文件加载和解析
 * - 节点导航（前进、选择选项、回滚）
 * - 条件检查机制
 * - 事件触发系统
 * - 对话历史记录支持回滚
 * - 完整的异常处理和日志记录
 */

using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules.Dialogue;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 对话管理器，负责管理对话系统
    /// </summary>
    /// <remarks>
    /// 该类继承自 Godot.Node，作为单例使用，负责加载和解析对话图数据。
    /// 支持对话节点的导航、选项选择、条件检查和事件触发等功能。
    /// </remarks>
    public partial class DialogueManager : Node
    {
        /// <summary>
        /// 对话管理器的单例实例
        /// </summary>
        private static DialogueManager _instance;

        /// <summary>
        /// 获取对话管理器的单例实例
        /// </summary>
        public static DialogueManager Instance => _instance;

        /// <summary>
        /// 当前对话图
        /// </summary>
        private DialogueGraph _currentGraph;

        /// <summary>
        /// 当前对话节点
        /// </summary>
        private DialogueNode _currentNode;

        /// <summary>
        /// 对话历史记录（用于回滚）
        /// </summary>
        private Stack<string> _history = new Stack<string>();

        /// <summary>
        /// 条件检查器
        /// </summary>
        private ConditionChecker _conditionChecker;

        /// <summary>
        /// 节点变化事件
        /// </summary>
        /// <param name="node">新的对话节点</param>
        public event Action<DialogueNode> OnNodeChanged;

        /// <summary>
        /// 事件触发事件
        /// </summary>
        /// <param name="evt">触发的对话事件</param>
        public event Action<DialogueEvent> OnEventTriggered;

        /// <summary>
        /// 对话结束事件
        /// </summary>
        public event Action OnDialogueEnded;

        /// <summary>
        /// 节点准备就绪时调用
        /// </summary>
        /// <remarks>
        /// 设置单例实例，初始化条件检查器
        /// </remarks>
        public override void _Ready()
        {
            _instance = this;
            _conditionChecker = new ConditionChecker();
            Log.Info("DialogueManager Loaded");
        }

        public void StartDialogue(string graphPath)
        {
            try
            {
                if (string.IsNullOrEmpty(graphPath))
                {
                    Log.Error("Cannot start dialogue with null or empty path");
                    return;
                }

                _currentGraph = LoadGraph(graphPath);
                if (_currentGraph == null)
                {
                    Log.Error($"Failed to start dialogue: {graphPath}");
                    return;
                }

                _history.Clear();
                
                if (string.IsNullOrEmpty(_currentGraph.StartNodeId))
                {
                    Log.Error($"Dialogue graph {graphPath} has no start node");
                    return;
                }

                SetNode(_currentGraph.StartNodeId);
                Log.Info($"Dialogue started: {graphPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Error starting dialogue {graphPath}: {ex.Message}");
            }
        }

        public void Advance()
        {
            try
            {
                if (_currentNode == null)
                {
                    Log.Warning("Cannot advance: no current dialogue node");
                    return;
                }

                if (_currentNode.Options != null && _currentNode.Options.Count > 0)
                {
                    Log.Info("Cannot advance linearly; options are available.");
                    return;
                }

                if (string.IsNullOrEmpty(_currentNode.NextNodeId))
                {
                    EndDialogue();
                }
                else
                {
                    SetNode(_currentNode.NextNodeId);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error advancing dialogue: {ex.Message}");
            }
        }

        public void SelectOption(int optionIndex)
        {
            try
            {
                if (_currentNode == null)
                {
                    Log.Error("Cannot select option: no current dialogue node");
                    return;
                }

                if (_currentNode.Options == null)
                {
                    Log.Error("Cannot select option: no options available");
                    return;
                }

                if (optionIndex < 0 || optionIndex >= _currentNode.Options.Count)
                {
                    Log.Error($"Invalid option index: {optionIndex}. Valid range: 0-{_currentNode.Options.Count - 1}");
                    return;
                }

                var option = _currentNode.Options[optionIndex];
                if (option == null)
                {
                    Log.Error($"Option at index {optionIndex} is null");
                    return;
                }

                if (_conditionChecker != null && !string.IsNullOrEmpty(option.Condition))
                {
                    if (!_conditionChecker.CheckCondition(option.Condition))
                    {
                        Log.Info($"Condition not met: {option.Condition}");
                        return;
                    }
                }

                if (string.IsNullOrEmpty(option.TargetNodeId))
                {
                    Log.Error($"Option has no target node: {option.Text}");
                    return;
                }

                SetNode(option.TargetNodeId);
            }
            catch (Exception ex)
            {
                Log.Error($"Error selecting option {optionIndex}: {ex.Message}");
            }
        }

        public void Rollback()
        {
            try
            {
                if (_history.Count <= 1)
                {
                    Log.Info("Cannot rollback further: no previous node");
                    return;
                }

                if (_currentGraph == null)
                {
                    Log.Error("Cannot rollback: no current dialogue graph");
                    return;
                }

                _history.Pop();
                string previousId = _history.Peek();

                if (_currentGraph.Nodes.ContainsKey(previousId))
                {
                    _currentNode = _currentGraph.Nodes[previousId];
                    OnNodeChanged?.Invoke(_currentNode);
                }
                else
                {
                    Log.Error($"Cannot rollback: node {previousId} not found in graph");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error rolling back dialogue: {ex.Message}");
            }
        }

        private void SetNode(string nodeId)
        {
            try
            {
                if (string.IsNullOrEmpty(nodeId))
                {
                    Log.Error("Cannot set null or empty node ID");
                    EndDialogue();
                    return;
                }

                if (_currentGraph == null)
                {
                    Log.Error("Cannot set node: no current dialogue graph");
                    return;
                }

                if (_currentGraph.Nodes.TryGetValue(nodeId, out var node))
                {
                    _currentNode = node;
                    _history.Push(nodeId);
                    OnNodeChanged?.Invoke(_currentNode);

                    if (_currentNode.Events != null)
                    {
                        foreach (var evt in _currentNode.Events)
                        {
                            if (evt != null)
                            {
                                HandleEvent(evt);
                            }
                            else
                            {
                                Log.Warning("Null event in node events list");
                            }
                        }
                    }
                }
                else
                {
                    Log.Error($"Node not found: {nodeId}");
                    EndDialogue();
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Error setting node {nodeId}: {ex.Message}");
                EndDialogue();
            }
        }

        /// <summary>
        /// 处理对话事件
        /// </summary>
        /// <param name="evt">要处理的对话事件</param>
        /// <remarks>
        /// 触发事件信号，通知订阅者处理事件
        /// 目前支持基础事件触发，后续可扩展为播放音效、更新任务等
        /// </remarks>
        private void HandleEvent(DialogueEvent evt)
        {
            Log.Info($"Triggering Event: {evt.Type}");
            OnEventTriggered?.Invoke(evt);
        }

        /// <summary>
        /// 结束对话
        /// </summary>
        /// <remarks>
        /// 重置当前对话节点、对话图和历史记录，触发对话结束事件
        /// </remarks>
        private void EndDialogue()
        {
            _currentNode = null;
            _currentGraph = null;
            _history.Clear();
            OnDialogueEnded?.Invoke();
        }

        /// <summary>
        /// 加载对话图
        /// </summary>
        /// <param name="path">对话图文件路径</param>
        /// <returns>解析后的对话图对象，如果加载失败返回null</returns>
        /// <remarks>
        /// 从指定路径读取JSON文件，解析为对话图对象
        /// </remarks>
        private DialogueGraph LoadGraph(string path)
        {
            var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
            if (file == null)
            {
                Log.Error($"File not found: {path}");
                return null;
            }

            string content = file.GetAsText();
            file.Close();

            var json = new Json();
            var error = json.Parse(content);
            if (error != Error.Ok)
            {
                Log.Error($"JSON Parse Error: {json.GetErrorMessage()} in {path} at line {json.GetErrorLine()}");
                return null;
            }

            var data = json.Data.AsGodotDictionary();
            return ParseGraphData(data);
        }

        /// <summary>
        /// 解析对话图数据
        /// </summary>
        /// <param name="data">Godot字典格式的对话图数据</param>
        /// <returns>解析后的对话图对象</returns>
        /// <remarks>
        /// 将JSON解析后的字典数据转换为DialogueGraph对象，包括节点、选项和事件
        /// </remarks>
        private DialogueGraph ParseGraphData(Godot.Collections.Dictionary data)
        {
            var graph = new DialogueGraph();

            if (data.ContainsKey("id")) graph.Id = data["id"].AsString();
            if (data.ContainsKey("start_node_id")) graph.StartNodeId = data["start_node_id"].AsString();

            if (data.ContainsKey("nodes"))
            {
                var nodesDict = data["nodes"].AsGodotDictionary();
                foreach (var key in nodesDict.Keys)
                {
                    var nodeData = nodesDict[key].AsGodotDictionary();
                    var node = new DialogueNode
                    {
                        Id = key.AsString()
                    };
                    if (nodeData.ContainsKey("speaker_name")) node.SpeakerName = nodeData["speaker_name"].AsString();
                    if (nodeData.ContainsKey("text")) node.Text = nodeData["text"].AsString();
                    if (nodeData.ContainsKey("next_node_id")) node.NextNodeId = nodeData["next_node_id"].AsString();
                    if (nodeData.ContainsKey("portrait_path")) node.PortraitPath = nodeData["portrait_path"].AsString();
                    if (nodeData.ContainsKey("background_path")) node.BackgroundPath = nodeData["background_path"].AsString();
                    if (nodeData.ContainsKey("audio_path")) node.AudioPath = nodeData["audio_path"].AsString();

                    if (nodeData.ContainsKey("options"))
                    {
                        var optionsArray = nodeData["options"].AsGodotArray();
                        foreach (var opt in optionsArray)
                        {
                            var optDict = opt.AsGodotDictionary();
                            var option = new DialogueOption();
                            if (optDict.ContainsKey("text")) option.Text = optDict["text"].AsString();
                            if (optDict.ContainsKey("target_node_id")) option.TargetNodeId = optDict["target_node_id"].AsString();
                            if (optDict.ContainsKey("condition")) option.Condition = optDict["condition"].AsString();
                            node.Options.Add(option);
                        }
                    }

                    if (nodeData.ContainsKey("events"))
                    {
                        var eventsArray = nodeData["events"].AsGodotArray();
                        foreach (var evt in eventsArray)
                        {
                            var evtDict = evt.AsGodotDictionary();
                            var dialogueEvent = new DialogueEvent();
                            if (evtDict.ContainsKey("type")) dialogueEvent.Type = evtDict["type"].AsString();
                            if (evtDict.ContainsKey("parameters"))
                            {
                                var paramsDict = evtDict["parameters"].AsGodotDictionary();
                                foreach (var pKey in paramsDict.Keys)
                                {
                                    dialogueEvent.Parameters[pKey.AsString()] = paramsDict[pKey].AsString();
                                }
                            }
                            node.Events.Add(dialogueEvent);
                        }
                    }

                    graph.Nodes[node.Id] = node;
                }
            }

            return graph;
        }
    }
}
