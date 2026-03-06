using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree.Actions
{
    public class MoveToTarget : ActionNode
    {
        private Node3D _agent;
        private string _targetKey;
        private float _speed;
        private float _stopDistance;

        public MoveToTarget(Node3D agent, string targetKey = "Target", float speed = 5.0f, float stopDistance = 1.0f)
        {
            _agent = agent;
            _targetKey = targetKey;
            _speed = speed;
            _stopDistance = stopDistance;
        }

        public override NodeStatus Tick(float delta, Dictionary<string, object> blackboard)
        {
            if (!blackboard.ContainsKey(_targetKey)) return NodeStatus.Failure;

            Node3D target = blackboard[_targetKey] as Node3D;
            if (target == null) return NodeStatus.Failure;

            float dist = _agent.GlobalPosition.DistanceTo(target.GlobalPosition);
            if (dist <= _stopDistance)
            {
                return NodeStatus.Success; // Reached target
            }

            // Move towards target
            // Assuming _agent handles movement (e.g. CharacterBody3D)
            // Or simple translation for now
            Vector3 direction = (target.GlobalPosition - _agent.GlobalPosition).Normalized();
            // If agent is CharacterBody3D
            if (_agent is CharacterBody3D body)
            {
                body.Velocity = direction * _speed;
                body.MoveAndSlide();
            }
            else
            {
                _agent.GlobalPosition += direction * _speed * delta;
            }

            return NodeStatus.Running;
        }
    }
}
