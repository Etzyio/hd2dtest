/*
 * File: BehaviorTreeCore.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 行为树核心节点定义，包含行为树的基础节点类型。
 * 定义节点状态枚举、基础节点类、组合节点（选择器、序列）、装饰器（反转器）和动作节点基类。
 * 
 * Key Features:
 * - NodeStatus：节点状态枚举（成功、失败、运行中）
 * - BTNode：行为树节点基类
 * - Selector：选择器节点（OR逻辑，任一子节点成功则成功）
 * - Sequence：序列节点（AND逻辑，所有子节点成功则成功）
 * - Inverter：反转器装饰器（反转成功/失败状态）
 * - ActionNode：动作节点基类（叶子节点）
 */

using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree
{
    /// <summary>
    /// 节点状态枚举
    /// </summary>
    public enum NodeStatus
    {
        /// <summary>执行成功</summary>
        Success,
        /// <summary>执行失败</summary>
        Failure,
        /// <summary>正在执行</summary>
        Running
    }

    /// <summary>
    /// 行为树节点基类
    /// </summary>
    public abstract class BTNode
    {
        /// <summary>
        /// 执行节点逻辑
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        /// <param name="blackboard">黑板数据，用于节点间共享数据</param>
        /// <returns>节点执行状态</returns>
        public abstract NodeStatus Tick(float delta, Dictionary<string, object> blackboard);
    }

    /// <summary>
    /// 选择器节点（OR逻辑）
    /// </summary>
    /// <remarks>
    /// 依次执行子节点，任一子节点成功则返回成功，所有子节点失败才返回失败
    /// </remarks>
    public class Selector : BTNode
    {
        private List<BTNode> _children = new List<BTNode>();

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node">要添加的子节点</param>
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

    /// <summary>
    /// 序列节点（AND逻辑）
    /// </summary>
    /// <remarks>
    /// 依次执行子节点，所有子节点成功则返回成功，任一子节点失败则返回失败
    /// </remarks>
    public class Sequence : BTNode
    {
        private List<BTNode> _children = new List<BTNode>();

        /// <summary>
        /// 添加子节点
        /// </summary>
        /// <param name="node">要添加的子节点</param>
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

    /// <summary>
    /// 反转器装饰器
    /// </summary>
    /// <remarks>
    /// 反转子节点的成功/失败状态，运行中状态保持不变
    /// </remarks>
    public class Inverter : BTNode
    {
        private BTNode _child;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="child">要包装的子节点</param>
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

    /// <summary>
    /// 动作节点基类（叶子节点）
    /// </summary>
    /// <remarks>
    /// 执行具体的行为逻辑，需要在子类中实现Tick方法
    /// </remarks>
    public abstract class ActionNode : BTNode
    {
        // 在子类中实现Tick方法
    }
}
