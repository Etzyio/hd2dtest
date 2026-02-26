using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules.AI; // For SensorySystem

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree.Actions
{
    public class DetectTarget : ActionNode
    {
        private SensorySystem _sensor;
        private string _targetKey;

        public DetectTarget(SensorySystem sensor, string targetKey = "Target")
        {
            _sensor = sensor;
            _targetKey = targetKey;
        }

        public override NodeStatus Tick(float delta, Dictionary<string, object> blackboard)
        {
            if (_sensor == null) return NodeStatus.Failure;

            if (_sensor.VisibleTargets.Count > 0)
            {
                // Simple: Pick first target
                blackboard[_targetKey] = _sensor.VisibleTargets[0];
                return NodeStatus.Success;
            }
            
            if (_sensor.AudibleTargets.Count > 0)
            {
                // Can hear something, investigate
                // blackboard["InvestigationPoint"] = _sensor.AudibleTargets[0].GlobalPosition;
                // But for target detection, maybe we need line of sight
            }

            return NodeStatus.Failure;
        }
    }
}
