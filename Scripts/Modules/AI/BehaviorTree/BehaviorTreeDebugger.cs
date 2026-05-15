/*
 * File: BehaviorTreeDebugger.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 行为树调试器，用于显示行为树运行时的黑板数据。
 * 在调试界面实时显示黑板中的所有键值对。
 * 
 * Key Features:
 * - 实时监控：在Process中更新显示黑板内容
 * - 格式化输出：将黑板数据格式化为可读文本
 * - 依赖注入：需要设置运行器和调试标签
 */

using Godot;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.AI.BehaviorTree
{
    /// <summary>
    /// 行为树调试器，用于显示行为树运行时的黑板数据
    /// </summary>
    public partial class BehaviorTreeDebugger : Node
    {
        /// <summary>
        /// 行为树运行器引用
        /// </summary>
        public BehaviorTreeRunner Runner { get; set; }

        /// <summary>
        /// 调试标签引用
        /// </summary>
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
