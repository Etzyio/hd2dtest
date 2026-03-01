using Godot;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.UI
{
	public partial class DialogueUI : CanvasLayer
	{
		[Export] public VBoxContainer VBoxContainer;
		[Export] public Button AttackButton;
		[Export] public Button SkillButton;
		[Export] public Button ItemsButton;
		[Export] public Button DefentButton;
		[Export] public Button PassButton;

		public override void _Ready()
		{
			if (VBoxContainer == null) VBoxContainer = GetNode<VBoxContainer>("VBoxContainer");
			if (AttackButton == null) AttackButton = GetNode<Button>("VBoxContainer/Attack/Attack");
			if (SkillButton == null) SkillButton = GetNode<Button>("VBoxContainer/Skill/Skill");
			if (ItemsButton == null) ItemsButton = GetNode<Button>("VBoxContainer/Items/Items");
			if (DefentButton == null) DefentButton = GetNode<Button>("VBoxContainer/Defent/Defent");
			if (PassButton == null) PassButton = GetNode<Button>("VBoxContainer/Pass/Pass");

			AttackButton.Pressed += OnAttackPressed;
			SkillButton.Pressed += OnSkillPressed;
			ItemsButton.Pressed += OnItemsPressed;
			DefentButton.Pressed += OnDefentPressed;
			PassButton.Pressed += OnPassPressed;
		}

		private void OnAttackPressed()
		{
			Log.Info("攻击按钮被按下");
		}

		private void OnSkillPressed()
		{
			Log.Info("技能按钮被按下");
		}

		private void OnItemsPressed()
		{
			Log.Info("道具按钮被按下");
		}

		private void OnDefentPressed()
		{
			Log.Info("防御按钮被按下");
		}

		private void OnPassPressed()
		{
			Log.Info("跳过按钮被按下");
		}
	}
}
