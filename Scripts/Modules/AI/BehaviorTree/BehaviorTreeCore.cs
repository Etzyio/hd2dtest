using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree
{
    // Node Status
    public enum NodeStatus
    {
        Success,
        Failure,
        Running
    }

    // Base BT Node
    public abstract class BTNode
    {
        public abstract NodeStatus Tick(float delta, Dictionary<string, object> blackboard);
    }

    // Composite: Selector (OR) - Fails only if all children fail
    public class Selector : BTNode
    {
        private List<BTNode> _children = new List<BTNode>();

        public void AddChild(BTNode node) => _children.Add(node);

        public override NodeStatus Tick(float delta, Dictionary<string, object> blackboard)
        {
            foreach (var child in _children)
            {
                var status = child.Tick(delta, blackboard);
                if (status != NodeStatus.Failure)
                {
                    return status;
                }
            }
            return NodeStatus.Failure;
        }
    }

    // Composite: Sequence (AND) - Succeeds only if all children succeed
    public class Sequence : BTNode
    {
        private List<BTNode> _children = new List<BTNode>();

        public void AddChild(BTNode node) => _children.Add(node);

        public override NodeStatus Tick(float delta, Dictionary<string, object> blackboard)
        {
            foreach (var child in _children)
            {
                var status = child.Tick(delta, blackboard);
                if (status != NodeStatus.Success)
                {
                    return status;
                }
            }
            return NodeStatus.Success;
        }
    }

    // Decorator: Inverter - Flips Success/Failure
    public class Inverter : BTNode
    {
        private BTNode _child;

        public Inverter(BTNode child)
        {
            _child = child;
        }

        public override NodeStatus Tick(float delta, Dictionary<string, object> blackboard)
        {
            var status = _child.Tick(delta, blackboard);
            if (status == NodeStatus.Success) return NodeStatus.Failure;
            if (status == NodeStatus.Failure) return NodeStatus.Success;
            return NodeStatus.Running;
        }
    }

    // Leaf: Action - Executes specific logic
    public abstract class ActionNode : BTNode
    {
        // Implement Tick in subclasses
    }
}
