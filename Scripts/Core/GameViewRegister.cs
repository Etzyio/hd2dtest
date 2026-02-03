using System.Collections.Generic;
using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Managers;

namespace hd2dtest.Scripts.Core
{
    /// <summary>
    /// 游戏视图注册器，负责场景的注册和获取
    /// </summary>
    /// <remarks>
    /// 该类提供了场景的注册和获取功能，通过ResourcesManager的ViewRegister字典来管理场景名称和路径的映射关系。
    /// 它是游戏视图管理系统的重要组成部分，为场景切换和弹窗管理提供场景加载支持。
    /// </remarks>
    public class GameViewRegister
    {
        /// <summary>
        /// 场景字典，存储场景名称和路径的映射关系
        /// </summary>
        /// <value>场景名称到场景路径的映射字典</value>
        /// <remarks>
        /// 该属性从ResourcesManager的ViewRegister获取场景映射关系，
        /// 如果ResourcesManager实例为null，则返回空数组并记录错误日志。
        /// </remarks>
        private static Dictionary<string, string> Scenes
        {
            get
            {
                if (ResourcesManager.Instance == null)
                {
                    Log.Error("ResourcesManager instance is null!");
                    return [];
                }
                return ResourcesManager.ViewRegister;
            }
        }

        /// <summary>
        /// 根据场景名称获取场景
        /// </summary>
        /// <param name="sceneName">场景名称</param>
        /// <returns>加载的场景对象，失败则返回null</returns>
        /// <remarks>
        /// 该方法根据场景名称从场景字典中获取场景路径，然后使用GD.Load加载场景。
        /// 如果场景名称不存在于字典中或加载失败，会记录错误日志并返回null。
        /// </remarks>
        /// <exception cref="System.Exception">加载场景过程中可能发生的异常</exception>
        public static PackedScene GetScene(string sceneName)
        {
            Log.Info($"load[{sceneName}]");

            if (!Scenes.TryGetValue(sceneName, out string scenePath))
            {
                Log.Error($"Scene '{sceneName}' not found in ViewRegister!");
                return null;
            }

            PackedScene packedScene = GD.Load<PackedScene>(scenePath);

            if (packedScene == null)
            {
                Log.Error($"Failed to load scene: {scenePath}");
                return null;
            }

            return packedScene;
        }
    }
}