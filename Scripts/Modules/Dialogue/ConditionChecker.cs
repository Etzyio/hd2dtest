/*
 * File: ConditionChecker.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 条件检查器，负责检查对话选项的条件是否满足。
 * 支持物品检查和任务状态检查等条件类型。
 * 
 * Key Features:
 * - HasItem：检查玩家是否拥有指定物品
 * - QuestState：检查任务状态是否为指定值
 * - 支持自定义条件格式："条件类型:键:值"
 */

using System;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Modules.Dialogue
{
    /// <summary>
    /// 处理对话选项的条件检查
    /// </summary>
    public class ConditionChecker
    {
        /// <summary>
        /// 检查条件字符串是否满足
        /// </summary>
        /// <param name="conditionString">条件字符串，格式为"条件类型：键：值"</param>
        /// <returns>如果条件满足返回 true，否则返回 false</returns>
        /// <remarks>
        /// 支持的条件类型包括：
        /// - HasItem：检查是否拥有指定物品
        /// - QuestState：检查任务状态是否为指定值
        /// 
        /// 示例：
        /// - "HasItem:Key" - 检查是否有钥匙
        /// - "QuestState:Quest001:Completed" - 检查任务 Quest001 是否已完成
        /// 
        /// 注意：当前为占位实现，始终返回 true
        /// </remarks>
        public bool CheckCondition(string conditionString)
        {
            if (string.IsNullOrEmpty(conditionString)) return true;

            string[] parts = conditionString.Split(':');
            if (parts.Length < 2)
            {
                Log.Warning($"Invalid condition format: {conditionString}");
                return false;
            }

            string type = parts[0];
            string key = parts[1];
            string value = parts.Length > 2 ? parts[2] : "";

            switch (type)
            {
                case "HasItem":
                    // return InventoryManager.HasItem(key); // 检查是否有物品
                    Log.Info($"Checking condition HasItem: {key}");
                    return true; // 占位实现
                case "QuestState":
                    // return QuestManager.GetQuestState(key) == value; // 检查任务状态
                    Log.Info($"Checking condition QuestState: {key} == {value}");
                    return true; // 占位实现
                default:
                    Log.Warning($"Unknown condition type: {type}");
                    return false;
            }
        }
    }
}
