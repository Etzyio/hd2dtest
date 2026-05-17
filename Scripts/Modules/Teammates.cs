/*
 * File: Teammates.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 小队成员管理类，负责管理游戏中的小队成员列表。
 * 提供添加、移除、设置和获取小队成员的功能。
 * 
 * Key Features:
 * - 小队成员列表管理
 * - 添加成员（避免重复）
 * - 移除成员
 * - 设置成员列表
 * - 获取成员列表（返回副本防止外部修改）
 */

using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 小队成员管理类
    /// </summary>
    /// <remarks>
    /// 负责管理游戏中的小队成员列表，提供添加、移除、设置和获取小队成员的功能
    /// </remarks>
    public class Teammates
    {
        /// <summary>
        /// 小队成员列表
        /// </summary>
        private List<Player> _teammateList = [];

        /// <summary>
        /// 玩家角色
        /// </summary>
        /// <value>当前玩家角色对象</value>
        public Player Player { get; set; }

        /// <summary>
        /// 添加小队成员
        /// </summary>
        /// <param name="teammate">要添加的队员</param>
        /// <remarks>
        /// 将队员添加到小队列表中，如果队员已存在则不重复添加
        /// </remarks>
        public void Add(Player teammate)
        {
            if (!_teammateList.Contains(teammate))
            {
                _teammateList.Add(teammate);
            }
        }

        /// <summary>
        /// 移除小队成员
        /// </summary>
        /// <param name="teammate">要移除的队员</param>
        /// <remarks>
        /// 从小队列表中移除指定的队员
        /// </remarks>
        public void Remove(Player teammate)
        {
            _teammateList.Remove(teammate);
        }

        /// <summary>
        /// 设置小队成员列表
        /// </summary>
        /// <param name="teammates">新的小队成员列表</param>
        /// <remarks>
        /// 用新的队员列表替换当前列表，如果传入null则清空列表
        /// </remarks>
        public void Set(List<Player> teammates)
        {
            _teammateList = teammates ?? [];
        }

        /// <summary>
        /// 获取小队成员列表
        /// </summary>
        /// <returns>小队成员列表的副本</returns>
        /// <remarks>
        /// 返回小队成员列表的副本，防止外部直接修改内部列表
        /// </remarks>
        public List<Player> Get()
        {
            return [.. _teammateList];
        }
    }
}
