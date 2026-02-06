using Godot;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    /// <summary>
    /// 剧情线管理器
    /// 负责管理剧情线的加载、进度跟踪和状态更新
    /// </summary>
    public partial class QuestLineManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static QuestLineManager _instance;
        
        /// <summary>
        /// 获取单例实例
        /// </summary>
        public static QuestLineManager Instance => _instance;

        /// <summary>
        /// 所有剧情线数据
        /// </summary>
        private List<QuestLineData> _allQuestLines = [];
        
        /// <summary>
        /// 剧情线节点状态字典
        /// 键：节点ID，值：节点状态
        /// </summary>
        private Dictionary<string, QuestLineNodeState> _nodeStates = [];

        /// <summary>
        /// 剧情线进度更新信号
        /// </summary>
        /// <param name="questLineId">剧情线ID</param>
        /// <param name="nodeId">节点ID</param>
        [Signal]
        public delegate void QuestLineProgressedEventHandler(string questLineId, string nodeId);

        /// <summary>
        /// 剧情线完成信号
        /// </summary>
        /// <param name="questLineId">剧情线ID</param>
        [Signal]
        public delegate void QuestLineCompletedEventHandler(string questLineId);

        /// <summary>
        /// 剧情线节点状态枚举
        /// </summary>
        public enum QuestLineNodeState
        {
            Locked,    // 锁定
            Available, // 可用
            Completed  // 已完成
        }

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化单例实例，加载剧情线数据
        /// </summary>
        public override void _Ready()
        {
            _instance = this;
            LoadQuestLines();
            // 存档数据会在SaveManager.LoadGame时自动加载，不需要在这里调用LoadSaveData()
        }

        /// <summary>
        /// 加载剧情线数据
        /// 从ResourcesManager加载所有剧情线数据
        /// </summary>
        public void LoadQuestLines()
        {
            try
            {
                // 使用ResourcesManager加载所有剧情线数据
                if (hd2dtest.Scripts.Managers.ResourcesManager.Instance != null)
                {
                    _allQuestLines = hd2dtest.Scripts.Managers.ResourcesManager.GetAllQuestLines();
                }
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Error loading quest lines: {e.Message}");
            }
        }

        /// <summary>
        /// 加载存档数据
        /// 剧情线数据会在SaveManager.LoadGame时自动加载
        /// </summary>
        /// <param name="nodeStates">节点状态字典，键为节点ID，值为状态编码</param>
        public void LoadSaveData(Dictionary<string, int> nodeStates)
        {
            try
            {
                if (nodeStates != null)
                {
                    foreach (var kvp in nodeStates)
                    {
                        var nodeId = kvp.Key;
                        var state = (QuestLineNodeState)kvp.Value;
                        _nodeStates[nodeId] = state;
                    }
                    Log.Info($"Loaded {nodeStates.Count} quest line node states from save data");
                }
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Error loading quest line save data: {e.Message}");
            }
        }

        /// <summary>
        /// 保存数据
        /// 剧情线数据会在SaveManager.SaveGame时自动保存
        /// </summary>
        public void SaveData()
        {
            try
            {
                // 剧情线数据会在SaveManager.SaveGame时自动保存
                // 这里不需要做任何操作，SaveManager会调用SaveQuestData来保存节点状态
                Log.Debug("QuestLineManager.SaveData called - data will be saved by SaveManager");
            }
            catch (System.Exception e)
            {
                GD.PrintErr($"Error saving quest line data: {e.Message}");
            }
        }

        /// <summary>
        /// 获取所有节点状态（用于保存）
        /// </summary>
        /// <returns>节点状态字典，键为节点ID，值为状态编码</returns>
        public Dictionary<string, int> GetAllNodeStates()
        {
            var nodeStates = new Dictionary<string, int>();
            foreach (var kvp in _nodeStates)
            {
                nodeStates[kvp.Key] = (int)kvp.Value;
            }
            return nodeStates;
        }

        /// <summary>
        /// 获取剧情线数据
        /// </summary>
        /// <param name="questLineId">剧情线ID</param>
        /// <returns>剧情线数据</returns>
        public QuestLineData GetQuestLine(string questLineId)
        {
            return _allQuestLines.Find(ql => ql.Id == questLineId);
        }

        /// <summary>
        /// 获取节点状态
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <returns>节点状态</returns>
        public QuestLineNodeState GetNodeState(string nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out QuestLineNodeState state))
            {
                return state;
            }
            return QuestLineNodeState.Locked;
        }

        /// <summary>
        /// 设置节点状态
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        /// <param name="state">节点状态</param>
        public void SetNodeState(string nodeId, QuestLineNodeState state)
        {
            _nodeStates[nodeId] = state;
        }

        /// <summary>
        /// 解锁节点
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        public void UnlockNode(string nodeId)
        {
            _nodeStates[nodeId] = QuestLineNodeState.Available;
            // 不再调用SaveData()，因为SaveManager会自动保存
        }

        /// <summary>
        /// 完成节点
        /// </summary>
        /// <param name="nodeId">节点ID</param>
        public void CompleteNode(string nodeId)
        {
            _nodeStates[nodeId] = QuestLineNodeState.Completed;
            // 不再调用SaveData()，因为SaveManager会自动保存

            // 检查是否解锁下一个节点
            foreach (var questLine in _allQuestLines)
            {
                foreach (var node in questLine.Nodes)
                {
                    if (node.Id == nodeId)
                    {
                        EmitSignal(SignalName.QuestLineProgressed, questLine.Id, nodeId);
                        UnlockNextNodes(questLine, node);
                        CheckQuestLineCompletion(questLine);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// 解锁下一个节点
        /// </summary>
        /// <param name="questLine">剧情线数据</param>
        /// <param name="completedNode">已完成的节点</param>
        private void UnlockNextNodes(QuestLineData questLine, QuestLineNode completedNode)
        {
            if (completedNode.NextNodes != null)
            {
                foreach (var nextNodeId in completedNode.NextNodes)
                {
                    var nextNode = questLine.Nodes.Find(n => n.Id == nextNodeId);
                    if (nextNode != null && GetNodeState(nextNodeId) == QuestLineNodeState.Locked)
                    {
                        UnlockNode(nextNodeId);
                    }
                }
            }
        }

        /// <summary>
        /// 检查剧情线是否完成
        /// </summary>
        /// <param name="questLine">剧情线数据</param>
        private void CheckQuestLineCompletion(QuestLineData questLine)
        {
            bool allNodesCompleted = true;
            foreach (var node in questLine.Nodes)
            {
                if (GetNodeState(node.Id) != QuestLineNodeState.Completed)
                {
                    allNodesCompleted = false;
                    break;
                }
            }

            if (allNodesCompleted)
            {
                EmitSignal(SignalName.QuestLineCompleted, questLine.Id);
            }
        }

        /// <summary>
        /// 获取可用节点列表
        /// </summary>
        /// <param name="questLineId">剧情线ID</param>
        /// <returns>可用节点列表</returns>
        public List<QuestLineNode> GetAvailableNodes(string questLineId)
        {
            List<QuestLineNode> availableNodes = [];
            var questLine = GetQuestLine(questLineId);
            if (questLine != null)
            {
                foreach (var node in questLine.Nodes)
                {
                    if (GetNodeState(node.Id) == QuestLineNodeState.Available)
                    {
                        availableNodes.Add(node);
                    }
                }
            }
            return availableNodes;
        }

        /// <summary>
        /// 获取已完成节点列表
        /// </summary>
        /// <param name="questLineId">剧情线ID</param>
        /// <returns>已完成节点列表</returns>
        public List<QuestLineNode> GetCompletedNodes(string questLineId)
        {
            List<QuestLineNode> completedNodes = [];
            var questLine = GetQuestLine(questLineId);
            if (questLine != null)
            {
                foreach (var node in questLine.Nodes)
                {
                    if (GetNodeState(node.Id) == QuestLineNodeState.Completed)
                    {
                        completedNodes.Add(node);
                    }
                }
            }
            return completedNodes;
        }

        /// <summary>
        /// 任务完成时的回调
        /// 检查是否有相关的剧情节点需要更新
        /// </summary>
        /// <param name="questId">任务ID</param>
        public void OnQuestCompleted(string questId)
        {
            // 当任务完成时，检查是否有相关的剧情节点需要更新
            var quest = QuestManager.Instance.GetQuest(questId);
            if (quest != null && !string.IsNullOrEmpty(quest.QuestLineId))
            {
                var questLine = GetQuestLine(quest.QuestLineId);
                if (questLine != null)
                {
                    foreach (var node in questLine.Nodes)
                    {
                        if (node.QuestIds != null && node.QuestIds.Contains(questId))
                        {
                            CompleteNode(node.Id);
                            break;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// 剧情线数据类
    /// </summary>
    public class QuestLineData
    {
        /// <summary>
        /// 剧情线ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 剧情线名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 剧情线描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 剧情线节点列表
        /// </summary>
        public List<QuestLineNode> Nodes { get; set; }
    }

    /// <summary>
    /// 剧情线节点类
    /// </summary>
    public class QuestLineNode
    {
        /// <summary>
        /// 节点ID
        /// </summary>
        public string Id { get; set; }
        
        /// <summary>
        /// 节点名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 节点描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 关联的任务ID列表
        /// </summary>
        public List<string> QuestIds { get; set; }
        
        /// <summary>
        /// 下一个节点ID列表
        /// </summary>
        public List<string> NextNodes { get; set; }
        
        /// <summary>
        /// 节点要求
        /// </summary>
        public List<string> Requirements { get; set; }
        
        /// <summary>
        /// 是否为分支点
        /// </summary>
        public bool IsBranchPoint { get; set; }
    }
}
