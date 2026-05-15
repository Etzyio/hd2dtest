using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    public partial class QuestLineManager : Node
    {
        private static QuestLineManager _instance;
        public static QuestLineManager Instance => _instance;

        private List<QuestLineData> _allQuestLines = [];
        private Dictionary<string, QuestLineNodeState> _nodeStates = [];

        [Signal]
        public delegate void QuestLineProgressedEventHandler(string questLineId, string nodeId);

        [Signal]
        public delegate void QuestLineCompletedEventHandler(string questLineId);

        public enum QuestLineNodeState
        {
            Locked,
            Available,
            Completed
        }

        public override void _Ready()
        {
            _instance = this;
            LoadQuestLines();
        }

        public void LoadQuestLines()
        {
            try
            {
                if (hd2dtest.Scripts.Managers.ResourcesManager.Instance != null)
                {
                    _allQuestLines = hd2dtest.Scripts.Managers.ResourcesManager.GetAllQuestLines();
                }
            }
            catch (System.Exception e)
            {
                Log.Error($"Error loading quest lines: {e.Message}");
            }
        }

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
                Log.Error($"Error loading quest line save data: {e.Message}");
            }
        }

        public void SaveData()
        {
            try
            {
                Log.Debug("QuestLineManager.SaveData called - data will be saved by SaveManager");
            }
            catch (System.Exception e)
            {
                Log.Error($"Error saving quest line data: {e.Message}");
            }
        }

        public Dictionary<string, int> GetAllNodeStates()
        {
            var nodeStates = new Dictionary<string, int>();
            foreach (var kvp in _nodeStates)
            {
                nodeStates[kvp.Key] = (int)kvp.Value;
            }
            return nodeStates;
        }

        public QuestLineData GetQuestLine(string questLineId)
        {
            return _allQuestLines.Find(ql => ql.Id == questLineId);
        }

        public QuestLineNodeState GetNodeState(string nodeId)
        {
            if (_nodeStates.TryGetValue(nodeId, out QuestLineNodeState state))
            {
                return state;
            }
            return QuestLineNodeState.Locked;
        }

        public void SetNodeState(string nodeId, QuestLineNodeState state)
        {
            _nodeStates[nodeId] = state;
        }

        public void UnlockNode(string nodeId)
        {
            _nodeStates[nodeId] = QuestLineNodeState.Available;
        }

        public void CompleteNode(string nodeId)
        {
            _nodeStates[nodeId] = QuestLineNodeState.Completed;

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

        private void UnlockNextNodes(QuestLineData questLine, QuestLineNode completedNode)
        {
            if (completedNode.NextNodes != null)
            {
                foreach (var nextNodeId in completedNode.NextNodes)
                {
                    var nextNode = questLine.Nodes.Find(n => n.Id == nextNodeId);
                    if (nextNode != null && GetNodeState(nextNodeId) == QuestLineNodeState.Locked)
                    {
                        if (CheckNodeRequirements(nextNode))
                        {
                            UnlockNode(nextNodeId);
                        }
                    }
                }
            }
        }

        private bool CheckNodeRequirements(QuestLineNode node)
        {
            if (node.Requirements == null)
                return true;

            if (node.Requirements.Level > 0 && !CheckLevelRequirement(node.Requirements.Level))
                return false;

            if (node.Requirements.KillCount > 0 && !CheckKillCountRequirement(node.Requirements.KillCount))
                return false;

            if (node.Requirements.SkillCount > 0 && !CheckSkillCountRequirement(node.Requirements.SkillCount))
                return false;

            if (node.Requirements.CompletedQuests > 0 && !CheckCompletedQuestsRequirement(node.Requirements.CompletedQuests))
                return false;

            if (node.Requirements.Gold > 0 && !CheckGoldRequirement(node.Requirements.Gold))
                return false;

            if (!string.IsNullOrEmpty(node.Requirements.CompletedQuest) && !CheckCompletedQuestRequirement(node.Requirements.CompletedQuest))
                return false;

            return true;
        }

        private bool CheckLevelRequirement(int level)
        {
            return true;
        }

        private bool CheckKillCountRequirement(int count)
        {
            return true;
        }

        private bool CheckSkillCountRequirement(int count)
        {
            return true;
        }

        private bool CheckCompletedQuestsRequirement(int count)
        {
            return true;
        }

        private bool CheckGoldRequirement(int amount)
        {
            return true;
        }

        private bool CheckCompletedQuestRequirement(string questId)
        {
            return QuestManager.Instance.GetQuestStatus(questId) == QuestManager.QuestStatus.Completed;
        }

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

        public void OnQuestCompleted(string questId)
        {
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

    public class QuestLineData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<QuestLineNode> Nodes { get; set; } = new List<QuestLineNode>();
    }

    public class QuestLineNode
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public List<string> QuestIds { get; set; } = new List<string>();
        public List<string> NextNodes { get; set; } = new List<string>();
        public NodeRequirements Requirements { get; set; }
        public bool Branching { get; set; }
    }

    public class NodeRequirements
    {
        public int Level { get; set; }
        public int KillCount { get; set; }
        public int SkillCount { get; set; }
        public int CompletedQuests { get; set; }
        public int Gold { get; set; }
        public string CompletedQuest { get; set; }
    }
}