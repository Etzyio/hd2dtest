using Godot;
using hd2dtest.Scripts.Managers;
using System.Collections.Generic;

namespace hd2dtest.Scenes.UI
{
	public partial class GameUI : CanvasLayer
	{
		private VBoxContainer _teammatesContainer;
		private PackedScene _teammateScene;
		private List<Teammate> _teammateUIs = [];

		public override void _Ready()
		{
			InitializeUIReferences();
			SetProcess(true);
		}

		public override void _Process(double delta)
		{
			UpdatePlayerStatus();
			UpdateTeammateStatus();
		}

		private void InitializeUIReferences()
		{
			_teammatesContainer = GetNode<VBoxContainer>("VBoxContainer");
			_teammateScene = GD.Load<PackedScene>("res://Scenes/UI/Teammate.tscn");
		}

		private void UpdatePlayerStatus()
		{
		}

		private void UpdateTeammateStatus()
		{
			var teammates = GameDataManager.Instance.Teammates.Get();
			
			if (teammates.Count != _teammateUIs.Count)
			{
				RefreshTeammateUI();
				return;
			}

			for (int i = 0; i < teammates.Count; i++)
			{
				if (i < _teammateUIs.Count && teammates[i] != null)
				{
					var teammate = teammates[i];
					_teammateUIs[i].UpdateStatus(
						teammate.CreatureName,
						teammate.Health,
						teammate.MaxHealth,
						teammate.Mana,
						teammate.MaxMana
					);
				}
			}
		}

		public void RefreshTeammateUI()
		{
			foreach (var ui in _teammateUIs)
			{
				ui.QueueFree();
			}
			_teammateUIs.Clear();

			var teammates = GameDataManager.Instance.Teammates.Get();
			foreach (var teammate in teammates)
			{
				var teammateInstance = _teammateScene.Instantiate<Teammate>();
				_teammatesContainer.AddChild(teammateInstance);
				_teammateUIs.Add(teammateInstance);
				
				teammateInstance.UpdateStatus(
					teammate.CreatureName,
					teammate.Health,
					teammate.MaxHealth,
					teammate.Mana,
					teammate.MaxMana
				);
			}
		}
	}
}
