/*
 * File: MoveToTarget.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 移动到目标行为节点，用于行为树中控制代理移动到指定目标位置。
 * 支持CharacterBody3D和普通Node3D的移动，可配置移动速度和停止距离。
 * 
 * Key Features:
 * - 从黑板获取目标节点
 * - 支持CharacterBody3D的MoveAndSlide移动
 * - 支持普通Node3D的直接位置更新
 * - 可配置移动速度和停止距离
 */

using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree.Actions
{
    /// <summary>
    /// 移动到目标行为节点
    /// </summary>
    /// <remarks>
    /// 控制代理移动到黑板中指定的目标位置，到达目标时返回成功
    /// </remarks>
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
