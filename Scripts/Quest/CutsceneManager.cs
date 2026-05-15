/*
 * File: CutsceneManager.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * 过场动画管理器，负责管理游戏中的剧情过场动画。
 * 支持动画播放、帧管理、跳过功能以及事件通知。
 * 
 * Key Features:
 * - 单例模式，全局访问
 * - 支持多个过场动画定义
 * - 帧级别的动画控制
 * - 可跳过动画功能
 * - 动画开始/结束事件通知
 */
using Godot;
using System;
using System.Collections.Generic;
using hd2dtest.Scripts.Utilities;

namespace hd2dtest.Scripts.Quest
{
    /// <summary>
    /// 过场动画管理器
    /// 负责管理游戏中的剧情过场动画，包括播放、帧管理和跳过功能
    /// </summary>
    public partial class CutsceneManager : Node
    {
        /// <summary>
        /// 单例实例
        /// </summary>
        public static CutsceneManager Instance { get; private set; }

        /// <summary>
        /// 所有过场动画列表
        /// </summary>
        private List<Cutscene> _cutscenes = new List<Cutscene>();

        /// <summary>
        /// 当前播放的过场动画
        /// </summary>
        private Cutscene _currentCutscene;

        /// <summary>
        /// 是否正在播放动画
        /// </summary>
        private bool _isPlaying = false;

        /// <summary>
        /// 当前帧索引
        /// </summary>
        private int _currentFrame = 0;

        /// <summary>
        /// 当前帧计时器（秒）
        /// </summary>
        private double _frameTimer = 0;

        /// <summary>
        /// 节点就绪时的回调
        /// 初始化单例实例并加载过场动画数据
        /// </summary>
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

        /// <summary>
        /// 初始化过场动画数据
        /// 加载所有内置的过场动画定义
        /// </summary>
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

        /// <summary>
        /// 播放指定的过场动画
        /// </summary>
        /// <param name="cutsceneId">过场动画ID</param>
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
            OnCutsceneStarted?.Invoke();
        }

        /// <summary>
        /// 每帧更新方法
        /// 处理过场动画的帧切换逻辑
        /// </summary>
        /// <param name="delta">时间增量（秒）</param>
        public override void _Process(double delta)
        {
            if (!_isPlaying || _currentCutscene == null) return;

            _frameTimer += delta;

            // 检查当前帧是否播放完毕
            if (_frameTimer >= _currentCutscene.Frames[_currentFrame].Duration)
            {
                _currentFrame++;
                _frameTimer = 0;

                // 检查是否播放完所有帧
                if (_currentFrame >= _currentCutscene.Frames.Count)
                {
                    EndCutscene();
                    return;
                }
            }
        }

        /// <summary>
        /// 跳过当前播放的过场动画
        /// 只有标记为可跳过的动画才能被跳过
        /// </summary>
        public void SkipCutscene()
        {
            if (_currentCutscene != null && _currentCutscene.IsSkippable)
            {
                EndCutscene();
            }
        }

        /// <summary>
        /// 结束当前过场动画
        /// 重置状态并触发结束事件
        /// </summary>
        private void EndCutscene()
        {
            _isPlaying = false;
            OnCutsceneEnded?.Invoke();
            Log.Info($"Cutscene ended: {_currentCutscene.Name}");
            _currentCutscene = null;
            _currentFrame = 0;
            _frameTimer = 0;
        }

        /// <summary>
        /// 检查是否正在播放过场动画
        /// </summary>
        /// <returns>是否正在播放</returns>
        public bool IsPlaying()
        {
            return _isPlaying;
        }

        /// <summary>
        /// 获取当前帧数据
        /// </summary>
        /// <returns>当前帧数据，如果没有播放则返回null</returns>
        public CutsceneFrame GetCurrentFrame()
        {
            if (_currentCutscene != null && _currentFrame < _currentCutscene.Frames.Count)
            {
                return _currentCutscene.Frames[_currentFrame];
            }
            return null;
        }

        /// <summary>
        /// 获取当前播放的过场动画
        /// </summary>
        /// <returns>当前过场动画，如果没有播放则返回null</returns>
        public Cutscene GetCurrentCutscene()
        {
            return _currentCutscene;
        }

        /// <summary>
        /// 过场动画开始事件
        /// </summary>
        public event Action OnCutsceneStarted;

        /// <summary>
        /// 过场动画结束事件
        /// </summary>
        public event Action OnCutsceneEnded;
    }

    /// <summary>
    /// 过场动画数据类
    /// </summary>
    public class Cutscene
    {
        /// <summary>
        /// 过场动画ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 过场动画名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 总时长（秒）
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// 是否可跳过
        /// </summary>
        public bool IsSkippable { get; set; }

        /// <summary>
        /// 帧列表
        /// </summary>
        public List<CutsceneFrame> Frames { get; set; } = new List<CutsceneFrame>();
    }

    /// <summary>
    /// 过场动画帧数据类
    /// </summary>
    public class CutsceneFrame
    {
        /// <summary>
        /// 帧持续时间（秒）
        /// </summary>
        public double Duration { get; set; }

        /// <summary>
        /// 文本内容
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// 背景图片
        /// </summary>
        public Texture2D Image { get; set; }

        /// <summary>
        /// 文本位置
        /// </summary>
        public CutsceneTextPosition Position { get; set; } = CutsceneTextPosition.Bottom;

        /// <summary>
        /// 文本颜色
        /// </summary>
        public Color TextColor { get; set; } = new Color(1, 1, 1);

        /// <summary>
        /// 文本大小
        /// </summary>
        public float TextSize { get; set; } = 24;
    }

    /// <summary>
    /// 过场动画文本位置枚举
    /// </summary>
    public enum CutsceneTextPosition
    {
        /// <summary>
        /// 顶部
        /// </summary>
        Top,

        /// <summary>
        /// 居中
        /// </summary>
        Center,

        /// <summary>
        /// 底部
        /// </summary>
        Bottom
    }
}
