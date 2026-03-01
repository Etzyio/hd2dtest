using Godot;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.UI
{
	public partial class DialogueUI : CanvasLayer
	{
		public VBoxContainer VBoxContainer;
		public Button AttackButton;
		public Button SkillButton;
		public Button ItemsButton;
		public Button DefentButton;
		public Button PassButton;
		public VBoxContainer SkillList;
		public VBoxContainer ItemlList;

		public override void _Ready()
		{
			if (VBoxContainer == null) VBoxContainer = GetNode<VBoxContainer>("VBoxContainer");
		if (AttackButton == null) AttackButton = GetNode<Button>("VBoxContainer/Attack/Attack");
		if (SkillButton == null) SkillButton = GetNode<Button>("VBoxContainer/Skill/Skill");
		if (ItemsButton == null) ItemsButton = GetNode<Button>("VBoxContainer/Items/Items");
		if (DefentButton == null) DefentButton = GetNode<Button>("VBoxContainer/Defent/Defent");
		if (PassButton == null) PassButton = GetNode<Button>("VBoxContainer/Pass/Pass");
		if (SkillList == null) SkillList = GetNode<VBoxContainer>("SkillList");
		if (ItemlList == null) ItemlList = GetNode<VBoxContainer>("ItemlList");

			AttackButton.Pressed += OnAttackPressed;
			SkillButton.Pressed += OnSkillPressed;
			ItemsButton.Pressed += OnItemsPressed;
			DefentButton.Pressed += OnDefentPressed;
			PassButton.Pressed += OnPassPressed;

			// 初始焦点设置
			AttackButton.GrabFocus();
		}

		private void OnAttackPressed()
		{
			Log.Info("攻击按钮被按下");
		}

		private void OnSkillPressed()
		{
			Log.Info("技能按钮被按下");
			SkillList.Visible = true;
			ItemlList.Visible = false;
		}

		private void OnItemsPressed()
		{
			Log.Info("道具按钮被按下");
			ItemlList.Visible = true;
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

		public override void _Input(InputEvent @event)
		{
			if (!Visible) return;

			// ESC 键回到主菜单
			if (@event.IsActionPressed("ui_cancel"))
			{
				if(SkillList.Visible)
				{
					SkillList.Visible = false;
					SkillButton.GrabFocus();
				}
				if(ItemlList.Visible)
				{
					ItemlList.Visible = false;
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
					if (SkillList != null && SkillList.GetChildCount() > 0)
					{
						var firstSkillButton = SkillList.GetChild(0) as Button;
						firstSkillButton?.GrabFocus();
					}
				}
				// 如果焦点在道具按钮上，显示道具列表
				else if (ItemsButton.HasFocus())
				{
					OnItemsPressed();
					// 焦点移动到道具列表的第一个按钮
					if (ItemlList != null && ItemlList.GetChildCount() > 0)
					{
						var firstItemButton = ItemlList.GetChild(0) as Button;
						firstItemButton?.GrabFocus();
					}
				}
				return;
			}

			// 上下左右键导航
			if (@event.IsActionPressed("ui_up"))
			{
				NavigateButtons(-1);
			}
			else if (@event.IsActionPressed("ui_down"))
			{
				NavigateButtons(1);
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

		private void NavigateButtons(int direction)
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
			mainButtons[currentIndex].GrabFocus();
		}
	}
}
