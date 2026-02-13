
using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 关卡数据类，定义游戏关卡的基本属性和配置
    /// </summary>
    /// <remarks>
    /// 存储关卡的ID、名称、描述、场景路径、出生位置等信息
    /// 用于关卡管理和加载系统
    /// </remarks>
    public class Level
    {
        /// <summary>
        /// 关卡ID
        /// </summary>
        /// <value>关卡的唯一标识符</value>
        public string Id { get; set; }
        
        /// <summary>
        /// 关卡名称
        /// </summary>
        /// <value>关卡的显示名称</value>
        public string Name { get; set; }
        
        /// <summary>
        /// 关卡描述
        /// </summary>
        /// <value>关卡的详细描述信息</value>
        public string Description { get; set; }
        
        /// <summary>
        /// 场景路径
        /// </summary>
        /// <value>关卡场景文件的路径</value>
        public string ScenePath { get; set; }
        
        /// <summary>
        /// 出生位置
        /// </summary>
        /// <value>关卡出生位置的坐标数组 [x, y, z]</value>
        public float[] SpawnPosition { get; set; }
        
        /// <summary>
        /// 难度等级
        /// </summary>
        /// <value>关卡的难度等级，数值越大难度越高</value>
        public int Difficulty { get; set; }
        
        /// <summary>
        /// 时间限制
        /// </summary>
        /// <value>关卡的时间限制（秒）</value>
        public int TimeLimit { get; set; }
        
        /// <summary>
        /// 可收集物品数量
        /// </summary>
        /// <value>关卡中可收集物品的数量</value>
        public int Collectibles { get; set; }
        
        /// <summary>
        /// 敌人数量
        /// </summary>
        /// <value>关卡中敌人的数量</value>
        public int Enemies { get; set; }
        
        /// <summary>
        /// 是否锁定
        /// </summary>
        /// <value>true 表示关卡已锁定，false 表示关卡未锁定</value>
        public bool IsLocked { get; set; }
        
        /// <summary>
        /// 所需物品列表
        /// </summary>
        /// <value>解锁关卡所需的物品ID列表</value>
        public List<string> RequiredItems { get; set; }

        /// <summary>
        /// 获取Vector3格式的出生位置
        /// </summary>
        /// <returns>关卡出生位置的Vector3坐标</returns>
        /// <remarks>
        /// 如果SpawnPosition为null或长度不足2，则返回默认位置 (100, 0, 0)
        /// 否则返回SpawnPosition指定的位置
        /// </remarks>
        public Vector3 GetSpawnPosition()
        {
            if (SpawnPosition == null || SpawnPosition.Length < 2)
                return new Vector3(100, 0, 0);
            return new Vector3(SpawnPosition[0], SpawnPosition[1], SpawnPosition[2]);
        }
    }
}