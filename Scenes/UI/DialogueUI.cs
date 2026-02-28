using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Managers;
using hd2dtest.Scripts.Modules.Dialogue;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scenes.UI
{
    public partial class DialogueUI : Control
    {
        [Export] public RichTextLabel SpeakerNameLabel;
        [Export] public RichTextLabel DialogueTextLabel;
        [Export] public TextureRect PortraitRect;
        [Export] public TextureRect BackgroundRect;
        [Export] public VBoxContainer OptionsContainer;
        [Export] public Button OptionButtonTemplate;
        [Export] public AudioStreamPlayer AudioPlayer;

        private Tween _textTween;
        private bool _isTyping = false;
        private DialogueEffectHandler _effectHandler;

        public override void _Ready()
        {
            _effectHandler = new DialogueEffectHandler(this);
            // Fallback for exported variables if not set in editor
            if (SpeakerNameLabel == null) SpeakerNameLabel = GetNode<RichTextLabel>("DialoguePanel/SpeakerNameLabel");
            if (DialogueTextLabel == null) DialogueTextLabel = GetNode<RichTextLabel>("DialoguePanel/DialogueTextLabel");
            if (PortraitRect == null) PortraitRect = GetNode<TextureRect>("PortraitRect");
            if (BackgroundRect == null) BackgroundRect = GetNode<TextureRect>("BackgroundRect");
            if (OptionsContainer == null) OptionsContainer = GetNode<VBoxContainer>("OptionsContainer");
            if (OptionButtonTemplate == null) OptionButtonTemplate = GetNode<Button>("OptionsContainer/OptionButtonTemplate");
            if (AudioPlayer == null && HasNode("AudioPlayer")) AudioPlayer = GetNode<AudioStreamPlayer>("AudioPlayer");

            // Hide initially
            Visible = false;

            // Connect to DialogueManager signals
            if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeChanged += OnNodeChanged;
                DialogueManager.Instance.OnDialogueEnded += OnDialogueEnded;
                DialogueManager.Instance.OnEventTriggered += OnEventTriggered;
            }
            else
            {
                Log.Error("DialogueManager instance is null. Make sure it is autoloaded.");
            }

            // Hide the template button
            if (OptionButtonTemplate != null)
            {
                OptionButtonTemplate.Visible = false;
            }
        }

        public override void _ExitTree()
        {
             if (DialogueManager.Instance != null)
            {
                DialogueManager.Instance.OnNodeChanged -= OnNodeChanged;
                DialogueManager.Instance.OnDialogueEnded -= OnDialogueEnded;
                DialogueManager.Instance.OnEventTriggered -= OnEventTriggered;
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (!Visible) return;

            if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("attack")) // Use attack/interact key to advance
            {
                // If text is still typing, finish it immediately
                if (_isTyping && _textTween != null && _textTween.IsRunning())
                {
                    _textTween.Kill();
                    DialogueTextLabel.VisibleRatio = 1.0f;
                    _isTyping = false;
                }
                else
                {
                    // Advance dialogue
                    DialogueManager.Instance.Advance();
                }
            }
        }

        private void OnNodeChanged(DialogueNode node)
        {
            if (!Visible) Visible = true;

            UpdateUI(node);
        }

        private void OnDialogueEnded()
        {
            Visible = false;
        }

        private void OnEventTriggered(DialogueEvent evt)
        {
            if (_effectHandler != null)
            {
                _effectHandler.HandleEvent(evt);
            }
        }

        private void UpdateUI(DialogueNode node)
        {
            // Update Text
            SpeakerNameLabel.Text = node.SpeakerName;
            DialogueTextLabel.Text = node.Text;
            DialogueTextLabel.VisibleRatio = 0;

            // Typewriter effect
            if (_textTween != null) _textTween.Kill();
            _textTween = CreateTween();
            _isTyping = true;
            float duration = node.Text.Length * 0.03f; // Faster typing
            _textTween.TweenProperty(DialogueTextLabel, "visible_ratio", 1.0f, duration);
            _textTween.Finished += () => _isTyping = false;

            // Update Portrait
            if (!string.IsNullOrEmpty(node.PortraitPath))
            {
                // Async load could be used here
                if (ResourceLoader.Exists(node.PortraitPath))
                {
                    var texture = GD.Load<Texture2D>(node.PortraitPath);
                    PortraitRect.Texture = texture;
                }
                else
                {
                    Log.Warning($"Portrait not found: {node.PortraitPath}");
                }
            }
            else
            {
                PortraitRect.Texture = null;
            }

            // Update Background
            if (!string.IsNullOrEmpty(node.BackgroundPath))
            {
                 if (ResourceLoader.Exists(node.BackgroundPath))
                {
                    var texture = GD.Load<Texture2D>(node.BackgroundPath);
                    BackgroundRect.Texture = texture;
                }
            }

            // Play Audio if specified
            if (!string.IsNullOrEmpty(node.AudioPath) && AudioPlayer != null)
            {
                 if (ResourceLoader.Exists(node.AudioPath))
                {
                    var stream = GD.Load<AudioStream>(node.AudioPath);
                    AudioPlayer.Stream = stream;
                    AudioPlayer.Play();
                }
            }

            // Update Options
            ClearOptions();
            if (node.Options != null && node.Options.Count > 0)
            {
                CreateOptions(node.Options);
            }
        }

        private void ClearOptions()
        {
            foreach (Node child in OptionsContainer.GetChildren())
            {
                if (child != OptionButtonTemplate)
                {
                    child.QueueFree();
                }
            }
        }

        private void CreateOptions(List<DialogueOption> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                
                // Todo: check conditions
                
                var btn = (Button)OptionButtonTemplate.Duplicate();
                btn.Visible = true;
                btn.Text = option.Text;
                OptionsContainer.AddChild(btn);

                // Capture index for closure
                int index = i;
                btn.Pressed += () => OnOptionSelected(index);
            }
            
            // Focus the first option
            // We need to wait for the frame to update or just call deferred?
            if (options.Count > 0)
            {
                // (OptionsContainer.GetChild(1) as Control)?.GrabFocus();
            }
        }

        private void OnOptionSelected(int index)
        {
            DialogueManager.Instance.SelectOption(index);
        }
    }
}
