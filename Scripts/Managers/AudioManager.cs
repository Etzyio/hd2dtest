using Godot;
using hd2dtest.Scripts.Utilities;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Managers
{
    public partial class AudioManager : Node
    {
        public static AudioManager Instance { get; private set; }

        private AudioStreamPlayer _bgmPlayer;
        private AudioStreamPlayer _sfxPlayer;
        private AudioStreamPlayer _uiPlayer;
        
        private Dictionary<string, AudioStream> _sfxCache = new Dictionary<string, AudioStream>();
        private Dictionary<string, AudioStream> _bgmCache = new Dictionary<string, AudioStream>();

        private float _bgmVolume = 0.7f;
        private float _sfxVolume = 1.0f;
        private float _uiVolume = 1.0f;

        private string _currentBgm = string.Empty;
        private bool _isBgmPaused = false;

        public override void _Ready()
        {
            if (Instance == null)
            {
                Instance = this;
                InitializeAudioPlayers();
                Log.Info("AudioManager initialized");
            }
            else
            {
                Log.Warning("AudioManager instance already exists");
                QueueFree();
            }
        }

        private void InitializeAudioPlayers()
        {
            // BGM播放器
            _bgmPlayer = new AudioStreamPlayer();
            _bgmPlayer.Name = "BgmPlayer";
            _bgmPlayer.Autoplay = false;
            _bgmPlayer.VolumeDb = Mathf.LinearToDb(_bgmVolume);
            AddChild(_bgmPlayer);

            // SFX播放器
            _sfxPlayer = new AudioStreamPlayer();
            _sfxPlayer.Name = "SfxPlayer";
            _sfxPlayer.Autoplay = false;
            _sfxPlayer.VolumeDb = Mathf.LinearToDb(_sfxVolume);
            AddChild(_sfxPlayer);

            // UI音效播放器
            _uiPlayer = new AudioStreamPlayer();
            _uiPlayer.Name = "UiPlayer";
            _uiPlayer.Autoplay = false;
            _uiPlayer.VolumeDb = Mathf.LinearToDb(_uiVolume);
            AddChild(_uiPlayer);

            Log.Info("Audio players initialized");
        }

        public void PlayBgm(string bgmPath)
        {
            if (_currentBgm == bgmPath && _bgmPlayer.Playing && !_isBgmPaused)
            {
                return; // 已经在播放同一首BGM
            }

            _currentBgm = bgmPath;
            _isBgmPaused = false;

            try
            {
                AudioStream bgm;
                
                // 检查缓存
                if (_bgmCache.ContainsKey(bgmPath))
                {
                    bgm = _bgmCache[bgmPath];
                }
                else
                {
                    bgm = ResourceLoader.Load<AudioStream>(bgmPath);
                    if (bgm == null)
                    {
                        Log.Error($"BGM file not found: {bgmPath}");
                        return;
                    }
                    _bgmCache[bgmPath] = bgm;
                }

                // 停止当前播放
                if (_bgmPlayer.Playing)
                {
                    _bgmPlayer.Stop();
                }

                _bgmPlayer.Stream = bgm;
                _bgmPlayer.Autoplay = true;
                _bgmPlayer.Play();

                Log.Info($"Playing BGM: {bgmPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to play BGM: {ex.Message}");
            }
        }

        public void PlaySfx(string sfxPath, float volume = 1.0f)
        {
            try
            {
                AudioStream sfx;

                if (_sfxCache.ContainsKey(sfxPath))
                {
                    sfx = _sfxCache[sfxPath];
                }
                else
                {
                    sfx = ResourceLoader.Load<AudioStream>(sfxPath);
                    if (sfx == null)
                    {
                        Log.Error($"SFX file not found: {sfxPath}");
                        return;
                    }
                    _sfxCache[sfxPath] = sfx;
                }

                // 如果当前正在播放，创建临时播放器
                if (_sfxPlayer.Playing)
                {
                    var tempPlayer = new AudioStreamPlayer();
                    tempPlayer.Stream = sfx;
                    tempPlayer.VolumeDb = Mathf.LinearToDb(_sfxVolume * volume);
                    tempPlayer.Finished += () => tempPlayer.QueueFree();
                    AddChild(tempPlayer);
                    tempPlayer.Play();
                }
                else
                {
                    _sfxPlayer.Stream = sfx;
                    _sfxPlayer.VolumeDb = Mathf.LinearToDb(_sfxVolume * volume);
                    _sfxPlayer.Play();
                }

                Log.Info($"Playing SFX: {sfxPath}");
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to play SFX: {ex.Message}");
            }
        }

        public void PlayUiSound(string soundName)
        {
            string sfxPath = $"res://Resources/Audio/UI/{soundName}.wav";
            PlaySfx(sfxPath, _uiVolume);
        }

        public void SetBgmVolume(float volume)
        {
            _bgmVolume = Mathf.Clamp(volume, 0, 1);
            _bgmPlayer.VolumeDb = Mathf.LinearToDb(_bgmVolume);
            Log.Info($"BGM volume set to: {_bgmVolume}");
        }

        public void SetSfxVolume(float volume)
        {
            _sfxVolume = Mathf.Clamp(volume, 0, 1);
            _sfxPlayer.VolumeDb = Mathf.LinearToDb(_sfxVolume);
            Log.Info($"SFX volume set to: {_sfxVolume}");
        }

        public void SetUiVolume(float volume)
        {
            _uiVolume = Mathf.Clamp(volume, 0, 1);
            _uiPlayer.VolumeDb = Mathf.LinearToDb(_uiVolume);
            Log.Info($"UI volume set to: {_uiVolume}");
        }

        public void PauseBgm()
        {
            if (_bgmPlayer.Playing)
            {
                _bgmPlayer.Stop();
                _isBgmPaused = true;
                Log.Info("BGM paused");
            }
        }

        public void ResumeBgm()
        {
            if (_isBgmPaused)
            {
                _bgmPlayer.Play();
                _isBgmPaused = false;
                Log.Info("BGM resumed");
            }
        }

        public void StopBgm()
        {
            _bgmPlayer.Stop();
            _currentBgm = string.Empty;
            _isBgmPaused = false;
            Log.Info("BGM stopped");
        }

        public void StopSfx()
        {
            _sfxPlayer.Stop();
            Log.Info("SFX stopped");
        }

        public bool IsBgmPlaying()
        {
            return _bgmPlayer.Playing;
        }

        public string GetCurrentBgm()
        {
            return _currentBgm;
        }

        public void PreloadSfx(string sfxPath)
        {
            try
            {
                if (!_sfxCache.ContainsKey(sfxPath))
                {
                    AudioStream sfx = ResourceLoader.Load<AudioStream>(sfxPath);
                    if (sfx != null)
                    {
                        _sfxCache[sfxPath] = sfx;
                        Log.Info($"Preloaded SFX: {sfxPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to preload SFX: {ex.Message}");
            }
        }

        public void PreloadBgm(string bgmPath)
        {
            try
            {
                if (!_bgmCache.ContainsKey(bgmPath))
                {
                    AudioStream bgm = ResourceLoader.Load<AudioStream>(bgmPath);
                    if (bgm != null)
                    {
                        _bgmCache[bgmPath] = bgm;
                        Log.Info($"Preloaded BGM: {bgmPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to preload BGM: {ex.Message}");
            }
        }

        public void ClearCache()
        {
            _sfxCache.Clear();
            _bgmCache.Clear();
            Log.Info("Audio cache cleared");
        }
    }
}