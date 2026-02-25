using System;
using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// Represents a single node in a dialogue graph.
    /// </summary>
    public class DialogueNode
    {
        /// <summary>
        /// Unique identifier for the node.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Name of the speaker.
        /// </summary>
        public string SpeakerName { get; set; }

        /// <summary>
        /// The text content of the dialogue.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Path to the audio file to play (optional).
        /// </summary>
        public string AudioPath { get; set; }

        /// <summary>
        /// Path to the background image (optional).
        /// </summary>
        public string BackgroundPath { get; set; }

        /// <summary>
        /// Path to the character portrait (optional).
        /// </summary>
        public string PortraitPath { get; set; }

        /// <summary>
        /// List of options available to the player. If empty, it's a linear progression.
        /// </summary>
        public List<DialogueOption> Options { get; set; } = new List<DialogueOption>();

        /// <summary>
        /// List of events to trigger when this node is entered.
        /// </summary>
        public List<DialogueEvent> Events { get; set; } = new List<DialogueEvent>();

        /// <summary>
        /// The ID of the next node for linear dialogue (if Options is empty).
        /// </summary>
        public string NextNodeId { get; set; }
    }

    /// <summary>
    /// Represents a choice the player can make.
    /// </summary>
    public class DialogueOption
    {
        /// <summary>
        /// The text displayed for the option.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// The ID of the target node if this option is selected.
        /// </summary>
        public string TargetNodeId { get; set; }

        /// <summary>
        /// A condition that must be met for this option to be available (optional).
        /// format: "ConditionType:Parameter" e.g., "HasItem:Key", "QuestState:QuestID:Completed"
        /// </summary>
        public string Condition { get; set; }
    }

    /// <summary>
    /// Represents an event that occurs during dialogue.
    /// </summary>
    public class DialogueEvent
    {
        /// <summary>
        /// The type of event (e.g., "PlaySound", "AddQuest", "ScreenShake").
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// Parameters for the event.
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();
    }

    /// <summary>
    /// Container for a complete dialogue graph.
    /// </summary>
    public class DialogueGraph
    {
        /// <summary>
        /// Unique ID for the dialogue graph.
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The starting node ID.
        /// </summary>
        public string StartNodeId { get; set; }

        /// <summary>
        /// All nodes in the graph, keyed by ID.
        /// </summary>
        public Dictionary<string, DialogueNode> Nodes { get; set; } = new Dictionary<string, DialogueNode>();
    }
}
