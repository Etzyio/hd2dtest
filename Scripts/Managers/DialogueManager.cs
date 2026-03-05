using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules.Dialogue;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Managers
{
    public partial class DialogueManager : Node
    {
        private static DialogueManager _instance;
        public static DialogueManager Instance => _instance;

        // Current state
        private DialogueGraph _currentGraph;
        private DialogueNode _currentNode;
        private Stack<string> _history = new Stack<string>();
        private ConditionChecker _conditionChecker;

        // Events for UI to subscribe to
        public event Action<DialogueNode> OnNodeChanged;
        public event Action<DialogueEvent> OnEventTriggered;
        public event Action OnDialogueEnded;

        public override void _Ready()
        {
            _instance = this;
            _conditionChecker = new ConditionChecker();
            Log.Info("DialogueManager Loaded");
        }

        public void StartDialogue(string graphPath)
        {
            _currentGraph = LoadGraph(graphPath);
            if (_currentGraph == null)
            {
                Log.Error($"Failed to start dialogue: {graphPath}");
                return;
            }

            _history.Clear();
            SetNode(_currentGraph.StartNodeId);
        }

        public void Advance()
        {
            if (_currentNode == null) return;

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

        public void SelectOption(int optionIndex)
        {
            if (_currentNode == null || _currentNode.Options == null || optionIndex < 0 || optionIndex >= _currentNode.Options.Count)
            {
                Log.Error("Invalid option selection.");
                return;
            }

            var option = _currentNode.Options[optionIndex];

            if (_conditionChecker != null && !string.IsNullOrEmpty(option.Condition))
            {
                if (!_conditionChecker.CheckCondition(option.Condition))
                {
                    Log.Info($"Condition not met: {option.Condition}");
                    return;
                }
            }

            SetNode(option.TargetNodeId);
        }

        public void Rollback()
        {
            if (_history.Count > 1)
            {
                _history.Pop(); // Remove current
                string previousId = _history.Peek(); // Get previous

                if (_currentGraph.Nodes.ContainsKey(previousId))
                {
                    _currentNode = _currentGraph.Nodes[previousId];
                    OnNodeChanged?.Invoke(_currentNode);
                }
            }
            else
            {
                Log.Info("Cannot rollback further.");
            }
        }

        private void SetNode(string nodeId)
        {
            if (_currentGraph.Nodes.TryGetValue(nodeId, out var node))
            {
                _currentNode = node;
                _history.Push(nodeId);
                OnNodeChanged?.Invoke(_currentNode);

                // Trigger events
                foreach (var evt in _currentNode.Events)
                {
                    HandleEvent(evt);
                }
            }
            else
            {
                Log.Error($"Node not found: {nodeId}");
                EndDialogue();
            }
        }

        private void HandleEvent(DialogueEvent evt)
        {
            Log.Info($"Triggering Event: {evt.Type}");
            OnEventTriggered?.Invoke(evt);
            // Todo: Implement other event logic (e.g. PlaySound, QuestUpdate)
        }

        private void EndDialogue()
        {
            _currentNode = null;
            _currentGraph = null;
            _history.Clear();
            OnDialogueEnded?.Invoke();
        }

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
