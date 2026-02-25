using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree
{
    [GlobalClass]
    public partial class BehaviorTreeRunner : Node
    {
        private BTNode _root;
        public Dictionary<string, object> Blackboard => _blackboard;
        private Dictionary<string, object> _blackboard = new Dictionary<string, object>();
        
        [Export] public bool IsActive { get; set; } = true;
        [Export] public float TickInterval { get; set; } = 0.1f; // 10 times per second

        private float _tickTimer = 0f;

        public void SetRoot(BTNode root)
        {
            _root = root;
        }

        public void SetBlackboardValue(string key, object value)
        {
            _blackboard[key] = value;
        }

        public object GetBlackboardValue(string key)
        {
            return _blackboard.ContainsKey(key) ? _blackboard[key] : null;
        }

        public override void _Process(double delta)
        {
            if (!IsActive || _root == null) return;

            _tickTimer += (float)delta;
            if (_tickTimer >= TickInterval)
            {
                _tickTimer = 0f;
                _root.Tick((float)delta, _blackboard); // Note: delta passed might be frame delta, but Tick logic might expect accumulated time or just frame delta. 
                // Usually BT ticks are instant logic updates.
            }
        }
    }
}
