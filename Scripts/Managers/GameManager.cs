using Godot;
using hd2dtest.Scripts.Core;
using hd2dtest.Scripts.Modules;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Managers
{
    /// <summary>
    /// 游戏管理器，负责管理游戏状态和流程
    /// </summary>
    /// <remarks>
    /// 该类继承自 Godot.Node，作为自动加载的单例使用，负责管理游戏的状态转换、玩家管理和 UI 显示。
    /// 它提供了游戏的初始化、开始、暂停、恢复和结束等功能，并处理不同游戏状态下的输入。
    /// </remarks>
    public partial class GameManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        private static GameManager _instance;

        /// <summary>
        /// 游戏管理器的单例实例
        /// </summary>
        /// <value>游戏管理器的单例实例</value>
        public static GameManager Instance => _instance;

        /// <summary>
        /// 小队管理对象
        /// </summary>
        public Teammates Teammates { get; private set; } = new();

        /// <summary>
        /// 道具列表
        /// </summary>
        public List<Item> ItemList { get; private set; } = new List<Item>();

        /// <summary>
        /// 节点准备就绪时调用的方法
        /// </summary>
        /// <remarks>
        /// 该方法在节点进入场景树时自动调用，用于设置单例实例和初始化游戏设置。
        /// </remarks>
        public override void _Ready()
        {
            // 设置单例实例
            _instance = this;

            InitializeGame();
        }

        /// <summary>
        /// 每帧处理方法
        /// </summary>
        /// <param name="delta">帧间隔时间（秒）</param>
        /// <remarks>
        /// 该方法每帧被调用，根据当前游戏状态处理对应的输入逻辑。
        /// </remarks>
        public override void _UnhandledInput(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                // 检查当前场景是否不是Start场景
                if (Main.Instance.NowScene != null && Main.Instance.NowScene.Name != "Start")
                {
                    // 仅当弹窗未显示时打开弹窗
                    // 弹窗显示时的关闭逻辑由PopupMenu处理
                    if (!Main.Instance.PopupStatus)
                    {
                        Main.Instance.OpenPopup();
                        GetViewport().SetInputAsHandled();
                    }
                    else
                    {
                        Main.Instance.ClosePopup();
                    }
                }
            }
        }

        /// <summary>
        /// 初始化游戏
        /// </summary>
        /// <remarks>
        /// 该方法用于初始化游戏设置，包括鼠标模式设置。
        /// </remarks>
        private void InitializeGame()
        {
            // 初始化游戏设置 - 鼠标始终可见
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }

        /// <summary>
        /// 添加道具到列表
        /// </summary>
        /// <param name="item">要添加的道具</param>
        public void AddItem(Item item)
        {
            if (item != null && !ItemList.Contains(item))
            {
                ItemList.Add(item);
            }
        }

        /// <summary>
        /// 从列表移除道具
        /// </summary>
        /// <param name="item">要移除的道具</param>
        public void RemoveItem(Item item)
        {
            if (item != null && ItemList.Contains(item))
            {
                ItemList.Remove(item);
            }
        }

        /// <summary>
        /// 根据 ID 获取道具
        /// </summary>
        /// <param name="itemId">道具 ID</param>
        /// <returns>如果找到则返回道具，否则返回 null</returns>
        public Item GetItemById(string itemId)
        {
            return ItemList.Find(item => item.Id == itemId);
        }

        /// <summary>
        /// 获取所有道具
        /// </summary>
        /// <returns>道具列表</returns>
        public List<Item> GetAllItems()
        {
            return [.. ItemList];
        }

        /// <summary>
        /// 清空道具列表
        /// </summary>
        public void ClearItems()
        {
            ItemList.Clear();
        }

    }
}
