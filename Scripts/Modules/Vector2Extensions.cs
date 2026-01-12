using System;
using System.Numerics;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// Vector2扩展方法，为System.Numerics.Vector2提供Godot.Vector2类似的方法
    /// </summary>
    public static class Vector2Extensions
    {
        /// <summary>
        /// 计算两个向量之间的距离
        /// </summary>
        /// <param name="a">第一个向量</param>
        /// <param name="b">第二个向量</param>
        /// <returns>两个向量之间的距离</returns>
        public static float DistanceTo(this Vector2 a, Vector2 b)
        {
            return Vector2.Distance(a, b);
        }

        /// <summary>
        /// 返回归一化的向量
        /// </summary>
        /// <param name="vector">要归一化的向量</param>
        /// <returns>归一化的向量</returns>
        public static Vector2 Normalized(this Vector2 vector)
        {
            return Vector2.Normalize(vector);
        }
    }
}