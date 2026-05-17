/*
 * File: BattleRewards.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 *
 * Purpose:
 * 战斗结算数据，包含战斗结束后玩家获得的奖励信息。
 *
 * Key Features:
 * - 总金币和总经验值
 * - 掉落物品列表
 * - 升级角色列表
 * - 胜负标记
 */

using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules.Battle
{
    /// <summary>
    /// 战斗结算奖励数据
    /// </summary>
    public class BattleRewards
    {
        /// <summary>获得的金币总数</summary>
        public int TotalGold { get; set; }

        /// <summary>获得的经验值总数</summary>
        public int TotalExperience { get; set; }

        /// <summary>获得的JP (Job Points) 总数</summary>
        public int TotalJP { get; set; }

        /// <summary>掉落的物品名称列表</summary>
        public List<string> DroppedItems { get; set; } = new();

        /// <summary>升级的角色名称列表</summary>
        public List<string> LevelUps { get; set; } = new();

        /// <summary>是否胜利</summary>
        public bool Victory { get; set; }
    }
}
