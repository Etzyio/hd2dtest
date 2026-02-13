using System;
using System.Numerics;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// Vector2扩展方法类，为System.Numerics.Vector2提供类似Godot.Vector2的便捷方法
    /// </summary>
    /// <remarks>
    /// 提供向量距离计算和归一化等常用方法，使System.Numerics.Vector2使用起来更像Godot.Vector2
    /// </remarks>
    public static class Vector2Extensions
    {
        /// <summary>
        /// 计算两个向量之间的欧几里得距离
        /// </summary>
        /// <param name="a">第一个向量</param>
        /// <param name="b">第二个向量</param>
        /// <returns>两个向量之间的距离值</returns>
        /// <remarks>
        /// 距离计算公式：√[(x2-x1)² + (y2-y1)²]
        /// 与Vector2.Distance方法功能相同，提供更直观的方法名
        /// </remarks>
        public static float DistanceTo(this Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        /// <summary>
        /// 返回归一化的向量（单位向量）
        /// </summary>
        /// <param name="vector">要归一化的向量</param>
        /// <returns>归一化后的单位向量</returns>
        /// <remarks>
        /// 归一化向量的长度为1，方向与原向量相同
        /// 与Vector2.Normalize方法功能相同，提供更直观的方法名
        /// </remarks>
        public static Vector2 Normalized(this Vector2 vector)
        {
            return Vector2.Normalize(vector);
        }
    }
}