using Godot;

namespace hd2dtest.Scenes.UI
{
    public partial class Teammate : VBoxContainer
    {
        private Label _nameLabel;
        private Label _healthValue;
        private TextureProgressBar _hpBar;
        private Label _mpValue;
        private TextureProgressBar _mpBar;

        public override void _Ready()
        {
            _nameLabel = GetNode<Label>("TeammateHeader/TeammateLabel");
            _healthValue = GetNode<Label>("TeammateStats/TeammateHealthValue");
            _hpBar = GetNode<TextureProgressBar>("TeammateStats/HPBar");
            _mpValue = GetNode<Label>("TeammateStats2/TeammateMpValue");
            _mpBar = GetNode<TextureProgressBar>("TeammateStats2/MPBar");
        }

        public void UpdateStatus(string name, float health, float maxHealth, float mana, float maxMana)
        {
            _nameLabel.Text = name;
            _healthValue.Text = $"{health:F0}/{maxHealth:F0}";
            _hpBar.Value = (health / maxHealth) * 100;
            _mpValue.Text = $"{mana:F0}/{maxMana:F0}";
            _mpBar.Value = (mana / maxMana) * 100;
        }
    }
}
