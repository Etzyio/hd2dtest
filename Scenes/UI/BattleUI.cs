using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules;

namespace hd2dtest.Scenes.UI
{
    public partial class BattleUI : CanvasLayer
    {
        public VBoxContainer VBoxContainer;
        public Button AttackButton;
        public Button SkillButton;
        public Button ItemsButton;
        public Button DefentButton;
        public Button PassButton;
        public ScrollContainer SkillList;
        public VBoxContainer SkillListVBox;
        public ScrollContainer ItemList;
        public VBoxContainer ItemListVBox;

        private List<Button> _itemButtons = new List<Button>();

        public override void _Ready()
        {
            VBoxContainer ??= GetNode<VBoxContainer>("VBoxContainer");
            AttackButton ??= GetNode<Button>("VBoxContainer/Attack/Attack");
            SkillButton ??= GetNode<Button>("VBoxContainer/Skill/Skill");
            ItemsButton ??= GetNode<Button>("VBoxContainer/Items/Items");
            DefentButton ??= GetNode<Button>("VBoxContainer/Defent/Defent");
            PassButton ??= GetNode<Button>("VBoxContainer/Pass/Pass");
            SkillList ??= GetNode<ScrollContainer>("SkillList");
            SkillListVBox ??= GetNode<VBoxContainer>("SkillList/VBoxContainer");
            ItemList ??= GetNode<ScrollContainer>("ItemList");
            ItemListVBox ??= GetNode<VBoxContainer>("ItemList/VBoxContainer");

            AttackButton.Pressed += OnAttackPressed;
            SkillButton.Pressed += OnSkillPressed;
            ItemsButton.Pressed += OnItemsPressed;
            DefentButton.Pressed += OnDefentPressed;
            PassButton.Pressed += OnPassPressed;

            // 初始焦点设置
            AttackButton.GrabFocus();

            // 初始化道具列表
            InitializeItemButtons();
        }

        private void OnAttackPressed()
        {
            Log.Info("攻击按钮被按下");
        }

        private void OnSkillPressed()
        {
            Log.Info("技能按钮被按下");
            SkillList.Visible = true;
            ItemList.Visible = false;
        }

        private void OnItemsPressed()
        {
            Log.Info("道具按钮被按下");
            ItemList.Visible = true;
            SkillList.Visible = false;
        }

        private void OnDefentPressed()
        {
            Log.Info("防御按钮被按下");
        }

        private void OnPassPressed()
        {
            Log.Info("跳过按钮被按下");
        }

        private void InitializeItemButtons()
        {
            // 清除现有的子节点
            while (ItemListVBox.GetChildCount() > 0)
            {
                var child = ItemListVBox.GetChild(0);
                ItemListVBox.RemoveChild(child);
                child.QueueFree();
            }

            _itemButtons.Clear();

            // 从 GameDataManager 获取道具列表
            if (GameDataManager.Instance == null)
            {
                Log.Warning("GameDataManager.Instance is null");
                return;
            }

            var items = GameDataManager.Instance.GetAllItems();

            foreach (var item in items)
            {
                var button = new Button
                {
                    Text = item.ItemName,
                    CustomMinimumSize = new Vector2(150, 45)
                };

                // 绑定道具数据到按钮
                button.SetMeta("ItemData", Variant.From(item));

                // 连接点击事件
                button.Pressed += () => OnItemButtonPressed(button);

                ItemListVBox.AddChild(button);
                _itemButtons.Add(button);
            }
        }

        private void OnItemPressed(Item item)
        {
            if (item == null)
            {
                Log.Warning("Item is null");
                return;
            }

            Log.Info($"使用道具：{item.ItemName}");

            // 获取玩家实例
            var player = GetTree().Root.GetNodeOrNull<Player>("Main/Player");
            if (player == null)
            {
                Log.Warning("Player not found");
                return;
            }

            // 使用道具
            bool used = item.Use(player);

            if (used)
            {
                Log.Info($"成功使用 {item.ItemName}");

                // 如果道具数量为 0，从列表中移除
                if (item.StackCount <= 0)
                {
                    GameDataManager.Instance.RemoveItem(item);
                    // 重新生成按钮
                    InitializeItemButtons();
                }
            }
            else
            {
                Log.Info($"{item.ItemName} 无法使用");
            }
        }

        private void OnItemButtonPressed(Button button)
        {
            if (button.GetMeta("ItemData").Obj is Item item)
            {
                OnItemPressed(item);
            }
        }

        /// <summary>
        /// 刷新道具列表
        /// </summary>
        public void RefreshItemList()
        {
            InitializeItemButtons();
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            // ESC 键回到主菜单
            if (@event.IsActionPressed("ui_cancel"))
            {
                if (SkillList.Visible)
                {
                    SkillList.Visible = false;
                    SkillButton.GrabFocus();
                }
                if (ItemList.Visible)
                {
                    ItemList.Visible = false;
                    ItemsButton.GrabFocus();
                }
                return;
            }

            // 回车键进入下级菜单
            if (@event.IsActionPressed("ui_accept"))
            {
                // 如果焦点在技能按钮上，显示技能列表
                if (SkillButton.HasFocus())
                {
                    OnSkillPressed();
                    // 焦点移动到技能列表的第一个按钮
                    if (SkillListVBox != null && SkillListVBox.GetChildCount() > 0)
                    {
                        var firstSkillButton = SkillListVBox.GetChild(0) as Button;
                        firstSkillButton?.GrabFocus();
                    }
                }
                // 如果焦点在道具按钮上，显示道具列表
                else if (ItemsButton.HasFocus())
                {
                    OnItemsPressed();
                    // 焦点移动到道具列表的第一个按钮
                    if (_itemButtons.Count > 0)
                    {
                        _itemButtons[0].GrabFocus();
                    }
                }
                return;
            }

            // 上下左右键导航
            if (@event.IsActionPressed("ui_up"))
            {
                // NavigateButtons();
            }
            else if (@event.IsActionPressed("ui_down"))
            {
                // NavigateButtons();
            }
            else if (@event.IsActionPressed("ui_left"))
            {
                // 可以根据需要添加左右导航逻辑
            }
            else if (@event.IsActionPressed("ui_right"))
            {
                // 可以根据需要添加左右导航逻辑
            }
        }

        private void NavigateButtons()
        {
            // 主菜单按钮列表
            var mainButtons = new List<Button>
            {
                AttackButton,
                SkillButton,
                ItemsButton,
                DefentButton,
                PassButton
            };

            // 找到当前有焦点的按钮
            int currentIndex = mainButtons.FindIndex(b => b != null && b.HasFocus());

            // 移除旧焦点，设置新焦点
            mainButtons[currentIndex].GrabFocus();
        }
    }
}
