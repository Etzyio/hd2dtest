using Godot;

public partial class TalkUI : Control
{
	[Export]
	private Label _talkLabel;
	
	[Export]
	private Label _nameLabel;
	
	public override void _Ready()
	{
		// 初始化时获取标签引用（如果没有通过编辑器设置）
		if (_talkLabel == null)
		{
			_talkLabel = GetNode<Label>("TalkLabel");
		}
		
		if (_nameLabel == null)
		{
			_nameLabel = GetNode<Label>("NameLabel");
		}
	}
	
	/// <summary>
	/// 设置对话内容
	/// </summary>
	/// <param name="text">对话文本</param>
	public void SetTalkText(string text)
	{
		if (_talkLabel != null)
		{
			_talkLabel.Text = text;
		}
	}
	
	/// <summary>
	/// 设置对话人名称
	/// </summary>
	/// <param name="name">对话人名称</param>
	public void SetTalkerName(string name)
	{
		if (_nameLabel != null)
		{
			_nameLabel.Text = name;
		}
	}
	
	/// <summary>
	/// 同时设置对话人和对话内容
	/// </summary>
	/// <param name="name">对话人名称</param>
	/// <param name="text">对话文本</param>
	public void SetTalk(string name, string text)
	{
		SetTalkerName(name);
		SetTalkText(text);
	}
}
