/*
 * File: BehaviorTreeRunner.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 行为树运行器，负责执行行为树的主循环。
 * 管理黑板数据、控制执行频率、执行根节点的Tick方法。
 * 
 * Key Features:
 * - 黑板管理：提供黑板数据的设置和获取方法
 * - 执行控制：支持激活/禁用行为树
 * - 频率控制：可配置的Tick间隔
 * - 自动更新：在Process中按间隔执行行为树
 */

using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree
{
    /// <summary>
    /// 行为树运行器，负责执行行为树的主循环
    /// </summary>
    [GlobalClass]
    public partial class BehaviorTreeRunner : Node
    {
        private BTNode _root;
        public Dictionary<string, object> Blackboard => _blackboard;
        private Dictionary<string, object> _blackboard = new Dictionary<string, object>();

        /// <summary>
        /// 是否激活
        /// </summary>
        /// <value>true 表示行为树正在运行，false 表示暂停</value>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Tick间隔（秒）
        /// </summary>
        /// <value>行为树执行的时间间隔，默认为0.1秒（10次/秒）</value>
        public float TickInterval { get; set; } = 0.1f;

        private float _tickTimer = 0f;

        /// <summary>
        /// 设置行为树根节点
        /// </summary>
        /// <param name="root">行为树的根节点</param>
        public void SetRoot(BTNode root)
        {
            _root = root;
        }

        /// <summary>
        /// 设置黑板值
        /// </summary>
        /// <param name="key">键名</param>
        /// <param name="value">值</param>
        public void SetBlackboardValue(string key, object value)
        {
            _blackboard[key] = value;
        }

        /// <summary>
        /// 获取黑板值
        /// </summary>
        /// <param name="key">键名</param>
        /// <returns>对应的值，如果不存在则返回null</returns>
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
                _root.Tick((float)delta, _blackboard);
            }
        }
    }
}
