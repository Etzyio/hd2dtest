/*
 * File: DetectTarget.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 检测目标行为节点，用于行为树中检测可见或可听的目标。
 * 通过感官系统查询可见和可听目标，并将第一个可见目标存入黑板。
 * 
 * Key Features:
 * - 使用感官系统查询可见目标
 * - 将检测到的目标存入黑板供其他节点使用
 * - 支持自定义黑板键名
 */

using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Modules.AI;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree.Actions
{
    /// <summary>
    /// 检测目标行为节点
    /// </summary>
    /// <remarks>
    /// 通过感官系统检测可见目标，将第一个可见目标存入黑板
    /// </remarks>
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
