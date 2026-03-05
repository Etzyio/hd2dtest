using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree
{
    public partial class BehaviorTreeDebugger : Node
    {
        public BehaviorTreeRunner Runner { get; set; }
        public Label DebugLabel { get; set; }

        public override void _Process(double delta)
        {
            if (Runner == null || DebugLabel == null) return;

            string debugText = "Blackboard:\n";
            if (Runner.Blackboard != null)
            {
                foreach (var kvp in Runner.Blackboard)
                {
                    debugText += $"{kvp.Key}: {kvp.Value}\n";
                }
            }
            DebugLabel.Text = debugText;
        }
    }
}
