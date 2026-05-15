using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    public partial class CutsceneManager : Node
    {
        public static CutsceneManager Instance { get; private set; }

        private List<Cutscene> _cutscenes = new List<Cutscene>();
        private Cutscene _currentCutscene;
        private bool _isPlaying = false;
        private int _currentFrame = 0;
        private double _frameTimer = 0;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeCutscenes();
                Log.Info("CutsceneManager initialized");
            }
            else
            {
                QueueFree();
            }
        }

        private void InitializeCutscenes()
        {
            _cutscenes.Add(new Cutscene
            {
                Id = "cs_intro",
                Name = "游戏开场",
                Duration = 15.0,
                IsSkippable = true,
                Frames = new List<CutsceneFrame>
                {
                    new CutsceneFrame { Duration = 3.0, Text = "很久很久以前...", Position = CutsceneTextPosition.Center },
                    new CutsceneFrame { Duration = 3.0, Text = "在一片神秘的武侠世界中...", Position = CutsceneTextPosition.Center },
                    new CutsceneFrame { Duration = 3.0, Text = "武林中流传着一个传说...", Position = CutsceneTextPosition.Center },
                    new CutsceneFrame { Duration = 3.0, Text = "找到传说中的神器，就能称霸江湖！", Position = CutsceneTextPosition.Center },
                    new CutsceneFrame { Duration = 3.0, Text = "你的冒险即将开始！", Position = CutsceneTextPosition.Center }
                }
            });

            _cutscenes.Add(new Cutscene
            {
                Id = "cs_main_001_start",
                Name = "主线任务1开场",
                Duration = 8.0,
                IsSkippable = true,
                Frames = new List<CutsceneFrame>
                {
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 年轻人，你终于来了！", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 最近村里不太太平...", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 有一群山贼在附近作乱！", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 你能帮我们解决这个麻烦吗？", Position = CutsceneTextPosition.Bottom }
                }
            });

            _cutscenes.Add(new Cutscene
            {
                Id = "cs_main_001_end",
                Name = "主线任务1完成",
                Duration = 6.0,
                IsSkippable = true,
                Frames = new List<CutsceneFrame>
                {
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 太好了！山贼被击退了！", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 谢谢你，勇敢的侠客！", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 2.0, Text = "村长: 这是你应得的奖励！", Position = CutsceneTextPosition.Bottom }
                }
            });

            _cutscenes.Add(new Cutscene
            {
                Id = "cs_side_001",
                Name = "支线任务开场",
                Duration = 5.0,
                IsSkippable = true,
                Frames = new List<CutsceneFrame>
                {
                    new CutsceneFrame { Duration = 2.0, Text = "村民小李: 我的小猫走丢了...", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 1.5, Text = "村民小李: 你能帮我找找吗？", Position = CutsceneTextPosition.Bottom },
                    new CutsceneFrame { Duration = 1.5, Text = "村民小李: 它最喜欢去后山玩...", Position = CutsceneTextPosition.Bottom }
                }
            });
        }

        public void PlayCutscene(string cutsceneId)
        {
            Cutscene cutscene = _cutscenes.Find(c => c.Id == cutsceneId);
            if (cutscene == null)
            {
                Log.Error($"Cutscene not found: {cutsceneId}");
                return;
            }

            _currentCutscene = cutscene;
            _currentFrame = 0;
            _frameTimer = 0;
            _isPlaying = true;

            Log.Info($"Playing cutscene: {cutscene.Name}");
            OnCutsceneStarted();
        }

        public override void _Process(double delta)
        {
            if (!_isPlaying || _currentCutscene == null) return;

            _frameTimer += delta;

            if (_frameTimer >= _currentCutscene.Frames[_currentFrame].Duration)
            {
                _currentFrame++;
                _frameTimer = 0;

                if (_currentFrame >= _currentCutscene.Frames.Count)
                {
                    EndCutscene();
                    return;
                }
            }
        }

        public void SkipCutscene()
        {
            if (_currentCutscene != null && _currentCutscene.IsSkippable)
            {
                EndCutscene();
            }
        }

        private void EndCutscene()
        {
            _isPlaying = false;
            OnCutsceneEnded();
            Log.Info($"Cutscene ended: {_currentCutscene.Name}");
            _currentCutscene = null;
            _currentFrame = 0;
            _frameTimer = 0;
        }

        public bool IsPlaying()
        {
            return _isPlaying;
        }

        public CutsceneFrame GetCurrentFrame()
        {
            if (_currentCutscene != null && _currentFrame < _currentCutscene.Frames.Count)
            {
                return _currentCutscene.Frames[_currentFrame];
            }
            return null;
        }

        public Cutscene GetCurrentCutscene()
        {
            return _currentCutscene;
        }

        public event Action OnCutsceneStarted;
        public event Action OnCutsceneEnded;
    }

    public class Cutscene
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public double Duration { get; set; }
        public bool IsSkippable { get; set; }
        public List<CutsceneFrame> Frames { get; set; } = new List<CutsceneFrame>();
    }

    public class CutsceneFrame
    {
        public double Duration { get; set; }
        public string Text { get; set; }
        public Texture2D Image { get; set; }
        public CutsceneTextPosition Position { get; set; } = CutsceneTextPosition.Bottom;
        public Color TextColor { get; set; } = new Color(1, 1, 1);
        public float TextSize { get; set; } = 24;
    }

    public enum CutsceneTextPosition
    {
        Top,
        Center,
        Bottom
    }
}
