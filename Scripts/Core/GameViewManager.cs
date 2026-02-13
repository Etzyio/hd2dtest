using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;
using hd2dtest.Scripts.Utilities;
using hd2dtest.Scripts.Managers;

namespace hd2dtest.Scripts.Core
{
	/// <summary>
	/// 游戏视图管理器，负责场景切换和弹窗管理
	/// </summary>
	/// <remarks>
	/// 该类提供了场景的加载、切换、暂停和恢复功能，以及弹窗的打开和关闭功能。
	/// 它通过管理场景层和弹窗层的可见性和处理状态，实现了平滑的场景过渡和弹窗交互。
	/// </remarks>
	public class GameViewManager
	{
		//场景层
		private static Control _sceneLayer;
		//弹窗层
		private static Control _popupLayer;
		private static readonly Queue<Node> _popupQueue = new();
		//弹窗标识
		private static int _identifier;
		//当前场景
		private static Node _nowScene;

		/// <summary>
		/// 当前场景属性
		/// </summary>
		/// <value>当前显示的场景节点</value>
		/// <remarks>
		/// 设置场景时，会自动处理旧场景的清理和新场景的添加。
		/// 如果场景有Exit方法，会在移除前调用该方法。
		/// </remarks>
		public static Node NowScene
		{
			get => _nowScene;
			set
			{
				if (_sceneLayer == null)
				{
					Log.Error("Scene layer is null, cannot set NowScene");
					return;
				}

				if (_nowScene != null)
				{
					// 检查节点是否有Exit方法
					if (_nowScene.HasMethod("Exit"))
					{
						_ = _nowScene.Call("Exit");
						Log.Info($"Called Exit method on scene: {_nowScene.Name}");
					}
					else
					{
						Log.Info($"Scene {_nowScene.Name} does not have Exit method, skipping call");
					}
					_sceneLayer.RemoveChild(_nowScene);
					_nowScene.QueueFree();  //释放资源
				}

				_nowScene = value;
				if (_nowScene != null)
				{
					_sceneLayer.AddChild(_nowScene);
					// 如果场景层是隐藏的，新场景也应该隐藏
					if (!_sceneLayer.Visible && _nowScene is CanvasItem canvas)
					{
						canvas.Visible = false;
					}
					Log.Info($"New scene added to scene layer: {_nowScene.Name}");
				}
			}
		}

		public static bool PopupStatus => _popupLayer.Visible;

		/// <summary>
		/// 初始化游戏视图管理器
		/// </summary>
		/// <param name="sceneLayer">场景层控件</param>
		/// <param name="popupLayer">弹窗层控件</param>
		/// <remarks>
		/// 该方法需要在游戏启动时调用，用于设置场景层和弹窗层的引用。
		/// 这些引用将用于后续的场景切换和弹窗管理操作。
		/// </remarks>
		public static void Init(Control sceneLayer, Control popupLayer)
		{
			_sceneLayer = sceneLayer;
			_popupLayer = popupLayer;
			_popupLayer.Visible = false;
		}

		/// <summary>
		/// 隐藏场景层（用于场景切换）
		/// </summary>
		/// <remarks>
		/// 该方法会隐藏场景层和弹窗层，以及当前场景（如果是CanvasItem类型）。
		/// 用于场景切换时的过渡效果。
		/// </remarks>
		private static void HideSceneLayer()
		{
			if (_sceneLayer != null)
			{
				_sceneLayer.Visible = false;
			}
			if (_popupLayer != null)
			{
				_popupLayer.Visible = false;
			}
			if (_nowScene is CanvasItem canvas)
			{
				canvas.Visible = false;
			}

			Log.Info("Scene layer hidden (CanvasItem)");
		}

		/// <summary>
		/// 显示场景层（用于场景切换）
		/// </summary>
		/// <remarks>
		/// 该方法会显示场景层和弹窗层，用于场景切换完成后的显示。
		/// </remarks>
		private static void ShowSceneLayer()
		{
			if (_sceneLayer != null)
			{
				_sceneLayer.Visible = true;
			}
			if (_popupLayer != null)
			{
				_popupLayer.Visible = false;
			}
			Log.Info("Scene layer shown (CanvasItem)");
		}

		/// <summary>
		/// 暂停场景层（用于弹窗）
		/// </summary>
		/// <remarks>
		/// 该方法会暂停场景层的所有处理，并添加视觉反馈（场景暗化效果）和音频处理（降低背景音乐音量）。
		/// 用于弹窗显示时的效果处理。
		/// </remarks>
		private static void PauseSceneLayer()
		{
			if (_sceneLayer != null)
			{
				// 完全暂停场景层的所有处理
				_sceneLayer.ProcessMode = Node.ProcessModeEnum.Disabled;
				
				// 添加视觉反馈 - 场景暗化效果
				AddPauseVisualEffect();
				
				// 处理音频 - 降低背景音乐音量
				HandleAudioPause();
				
				Log.Info("Scene layer paused with visual and audio effects");
			}
		}

		/// <summary>
		/// 恢复场景层（用于弹窗）
		/// </summary>
		/// <remarks>
		/// 该方法会恢复场景层的处理，并移除视觉反馈和恢复音频。
		/// 用于弹窗关闭后的恢复处理。
		/// </remarks>
		private static void ResumeSceneLayer()
		{
			if (_sceneLayer != null)
			{
				// 恢复场景层的处理
				_sceneLayer.ProcessMode = Node.ProcessModeEnum.Inherit;
				
				// 移除视觉反馈
				RemovePauseVisualEffect();
				
				// 恢复音频
				HandleAudioResume();
				
				Log.Info("Scene layer resumed");
			}
		}

		/// <summary>
		/// 添加暂停视觉效果
		/// </summary>
		/// <remarks>
		/// 该方法会创建并添加暗化效果和暂停标签到场景层，并添加淡入动画。
		/// 如果已经存在暂停效果，则不会重复添加。
		/// </remarks>
		private static void AddPauseVisualEffect()
		{
			if (_sceneLayer == null)
			{
				return;
			}
			// 检查是否已经有暂停效果
			foreach (Node child in _sceneLayer.GetChildren())
			{
				if (child.Name == "PauseEffect")
				{
					return; // 已经有暂停效果
				}
			}

			// 创建暗化效果
			var darkenEffect = new ColorRect
			{
				Name = "PauseEffect",
				Color = new Color(0, 0, 0, 0.5f),
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			darkenEffect.SetAnchorsPreset(Control.LayoutPreset.FullRect);

			// 添加暂停图标
			var pauseLabel = new Label
			{
				Name = "PauseLabel",
				Text = TranslationServer.Translate("paused"),
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Center,
				MouseFilter = Control.MouseFilterEnum.Ignore
			};
			pauseLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
			pauseLabel.AddThemeColorOverride("font_color", Colors.White);
			pauseLabel.AddThemeFontSizeOverride("font_size", 32);

			// 添加到场景层
			_sceneLayer.AddChild(darkenEffect);
			_sceneLayer.AddChild(pauseLabel);

			// 创建淡入动画
			var tween = _sceneLayer.CreateTween();
			tween.TweenProperty(darkenEffect, "color:a", 0.5f, 0.3f);
			tween.TweenProperty(pauseLabel, "modulate:a", 1.0f, 0.3f);
		}

		/// <summary>
		/// 移除暂停视觉效果
		/// </summary>
		/// <remarks>
		/// 该方法会查找并移除场景层中的暂停效果（暗化效果和暂停标签），并添加淡出动画。
		/// </remarks>
		private static void RemovePauseVisualEffect()
		{
			if (_sceneLayer == null)
			{
				return;
			}
			// 查找并移除暂停效果
			foreach (Node child in _sceneLayer.GetChildren())
			{
				if (child.Name == "PauseEffect" || child.Name == "PauseLabel")
				{
					// 创建淡出动画
					var tween = _sceneLayer.CreateTween();
					if (child is ColorRect colorRect)
					{
						tween.TweenProperty(colorRect, "color:a", 0, 0.3f);
					}
					else if (child is Label label)
					{
						tween.TweenProperty(label, "modulate:a", 0, 0.3f);
					}
					tween.TweenCallback(Callable.From(() => child.QueueFree()));
				}
			}
		}

		/// <summary>
		/// 处理音频暂停
		/// </summary>
		/// <remarks>
		/// 该方法用于处理音频的暂停逻辑，如降低背景音乐音量。
		/// 实际实现需要根据游戏的音频系统来调整。
		/// </remarks>
		private static void HandleAudioPause()
		{
			// 这里可以添加音频处理逻辑
			// 例如：降低背景音乐音量
			// 实际实现需要根据游戏的音频系统来调整
			Log.Info("Audio paused - background music volume reduced");
		}

		/// <summary>
		/// 处理音频恢复
		/// </summary>
		/// <remarks>
		/// 该方法用于处理音频的恢复逻辑，如恢复背景音乐音量。
		/// 实际实现需要根据游戏的音频系统来调整。
		/// </remarks>
		private static void HandleAudioResume()
		{
			// 这里可以添加音频处理逻辑
			// 例如：恢复背景音乐音量
			// 实际实现需要根据游戏的音频系统来调整
			Log.Info("Audio resumed - background music volume restored");
		}

		/// <summary>
		/// 切换场景
		/// </summary>
		/// <param name="sceneName">场景名称</param>
		/// <remarks>
		/// 该方法会隐藏场景层，加载指定名称的场景，实例化并设置为当前场景，最后返回新场景节点。
		/// 如果加载失败或发生异常，会记录错误并返回null。
		/// </remarks>
		/// <exception cref="System.Exception">场景切换过程中可能发生的异常</exception>
		public static void SwitchScene(string sceneName)
		{
			Log.Info($"Starting to switch scene: {sceneName}");

			// 隐藏场景层
			HideSceneLayer();
			try
			{
				PackedScene scene = GameViewRegister.GetScene(sceneName);

				if (scene != null)
				{
					NowScene = scene.Instantiate<Node>();
					// 使用Godot内置的_ready方法初始化，不需要额外调用Init
				}
				Log.Info($"Scene {sceneName} loaded successfully");
			}
			catch (System.Exception ex)
			{
				Log.Error($"Exception during scene switch: {ex.Message}");
			}
			finally
			{
				ShowSceneLayer();
			}
		}

		/// <summary>
		/// 触发场景就绪，显示场景层
		/// </summary>
		/// <remarks>
		/// 该方法用于在场景初始化完成后显示场景层，通常在场景的_ready方法中调用。
		/// </remarks>
		public static void TriggerSceneReady()
		{
			Log.Info("Scene ready triggered, showing scene layer");
			// 显示场景层
			ShowSceneLayer();
		}

		/*************************弹窗管理*****************************/
		/// <summary>
		/// 获取弹窗识别码
		/// </summary>
		/// <returns>新的弹窗识别码</returns>
		/// <remarks>
		/// 该方法会递增并返回当前的弹窗识别码，用于唯一标识每个弹窗。
		/// </remarks>
		private static int GetIdentifier()
		{
			_identifier += 1;
			return _identifier;
		}

		/// <summary>
		/// 打开弹窗
		/// </summary>
		/// <param name="popUpName">弹窗名称</param>
		/// <remarks>
		/// 该方法会加载指定名称的弹窗场景，实例化并添加到弹窗层，设置标识符，暂停场景层，最后返回弹窗节点。
		/// 如果已有弹窗打开，则返回null。
		/// </remarks>
		public static void OpenPopup()
		{
			if (_popupLayer != null)
			{
				_popupLayer.Visible=true;
			}
		}

		/// <summary>
		/// 根据名称关闭弹窗
		/// </summary>
		/// <remarks>
		/// 该方法会根据名称查找弹窗，调用其Exit方法，从弹窗层移除并释放资源，最后恢复场景层。
		/// </remarks>
		public static void ClosePopup()
		{
			if (_popupLayer != null)
			{
				_popupLayer.Visible = false;
			}
		}

		/// <summary>
		/// 显示弹窗层
		/// </summary>
		/// <remarks>
		/// 该方法会显示弹窗层，使其可见。
		/// </remarks>
		public static void ShowPopupLayer()
		{
			if (_popupLayer != null)
			{
				_popupLayer.Visible = true;
				Log.Info("Popup layer shown");
			}
		}

		/// <summary>
		/// 隐藏弹窗层
		/// </summary>
		/// <remarks>
		/// 该方法会隐藏弹窗层，使其不可见。
		/// </remarks>
		public static void HidePopupLayer()
		{
			if (_popupLayer != null)
			{
				_popupLayer.Visible = false;
				Log.Info("Popup layer hidden");
			}
		}

		/// <summary>
		/// 切换弹窗层的可见性
		/// </summary>
		/// <remarks>
		/// 该方法会切换弹窗层的可见性，如果当前可见则隐藏，否则显示。
		/// </remarks>
		public static void TogglePopupLayerVisibility()
		{
			if (_popupLayer != null)
			{
				_popupLayer.Visible = !_popupLayer.Visible;
				Log.Info($"Popup layer visibility toggled to: {_popupLayer.Visible}");
			}
		}
	}
}
