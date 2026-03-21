using Godot;

public partial class TalkUI : Control
{
    private Label _talkLabel;
    private Label _nameLabel;
    private float _typeSpeed = 0.05f; // 每个字的显示间隔时间（秒）

    private string _fullText = "";
    private float _typeTimer = 0;
    private int _currentCharIndex = 0;
    private bool _isTyping = false;
    private System.Action _onTypeComplete;

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

    public override void _Process(double delta)
    {
        if (_isTyping)
        {
            _typeTimer += (float)delta;

            if (_typeTimer >= _typeSpeed)
            {
                _typeTimer = 0;
                _currentCharIndex++;

                if (_currentCharIndex <= _fullText.Length)
                {
                    _talkLabel.Text = _fullText.Substring(0, _currentCharIndex);
                }
                else
                {
                    // 打字完成
                    _isTyping = false;
                    _onTypeComplete?.Invoke();
                }
            }
        }
    }

    /// <summary>
    /// 设置对话内容（立即显示）
    /// </summary>
    /// <param name="text">对话文本</param>
    public void SetTalkText(string text)
    {
        _isTyping = false;
        _fullText = text;
        _talkLabel.Text = text;
        _currentCharIndex = text.Length;
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
    /// 同时设置对话人和对话内容（立即显示）
    /// </summary>
    /// <param name="name">对话人名称</param>
    /// <param name="text">对话文本</param>
    public void SetTalk(string name, string text)
    {
        SetTalkerName(name);
        SetTalkText(text);
    }
    
    /// <summary>
    /// 逐字显示对话内容（打字机效果）
    /// </summary>
    /// <param name="text">对话文本</param>
    /// <param name="onComplete">打字完成后的回调</param>
    public void TypeTalkText(string text, System.Action onComplete = null)
    {
        if (string.IsNullOrEmpty(text))
        {
            _talkLabel.Text = text;
            _isTyping = false;
            onComplete?.Invoke();
            return;
        }
        
        _isTyping = true;
        _fullText = text;
        _currentCharIndex = 0;
        _typeTimer = 0;
        _onTypeComplete = onComplete;
        _talkLabel.Text = "";
    }
    
    /// <summary>
    /// 逐字显示对话（打字机效果）
    /// </summary>
    /// <param name="name">对话人名称</param>
    /// <param name="text">对话文本</param>
    /// <param name="onComplete">打字完成后的回调</param>
    public void TypeTalk(string name, string text, System.Action onComplete = null)
    {
        SetTalkerName(name);
        TypeTalkText(text, onComplete);
    }
    
    /// <summary>
    /// 跳过打字效果，直接显示完整文本
    /// </summary>
    public void SkipTyping()
    {
        if (_isTyping)
        {
            _isTyping = false;
            _talkLabel.Text = _fullText;
            _currentCharIndex = _fullText.Length;
            _onTypeComplete?.Invoke();
        }
    }
    
    /// <summary>
    /// 设置打字速度
    /// </summary>
    /// <param name="speed">每个字的显示间隔时间（秒）</param>
    public void SetTypeSpeed(float speed)
    {
        _typeSpeed = speed;
    }
    
    /// <summary>
    /// 检查是否正在打字
    /// </summary>
    /// <returns>是否正在打字</returns>
    public bool IsTyping()
    {
        return _isTyping;
    }
}