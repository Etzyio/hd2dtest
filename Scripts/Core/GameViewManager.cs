using System.Collections.Generic;
using Godot;

namespace hd2dtest.Scripts.Core
{
    public class GameViewManager
    {
        //场景层
        private static Node _sceneLayer;
        //弹窗层
        private static Node _popupLayer;
        private static readonly Queue<Node> _popupQueue = new();
        //弹窗标识
        private static int _identifier;
        //当前弹窗
        private static Node _nowPopup;
        //当前场景
        private static Node _nowScene;

        //当前新场景
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
                    Log.Info($"New scene added to scene layer: {_nowScene.Name}");
                }
            }
        }


        public static void Init(Node sceneLayer, Node popupLayer)
        {
            _sceneLayer = sceneLayer;
            _popupLayer = popupLayer;
            
            if (_sceneLayer != null)
            {
                Log.Info($"GameViewManager initialized with scene layer: {_sceneLayer.Name}");
                Log.Info($"Scene layer parent: {_sceneLayer.GetParent()?.Name}");
                Log.Info($"Scene layer has {_sceneLayer.GetChildCount()} children");
            }
            else
            {
                Log.Error("GameViewManager initialized with null scene layer!");
            }
        }

        // 隐藏场景层（用于场景切换）
        private static void HideSceneLayer()
        {
            if (_sceneLayer is not null and CanvasItem canvasItem)
            {
                canvasItem.Visible = false;
                Log.Info("Scene layer hidden");
            }
        }

        // 显示场景层（用于场景切换）
        private static void ShowSceneLayer()
        {
            if (_sceneLayer is not null and CanvasItem canvasItem)
            {
                canvasItem.Visible = true;
                Log.Info("Scene layer shown");
            }
        }

        // 暂停场景层（用于弹窗）
        private static void PauseSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.ProcessMode = Node.ProcessModeEnum.Disabled;
                Log.Info("Scene layer paused");
            }
        }

        // 恢复场景层（用于弹窗）
        private static void ResumeSceneLayer()
        {
            if (_sceneLayer != null)
            {
                _sceneLayer.ProcessMode = Node.ProcessModeEnum.Inherit;
                Log.Info("Scene layer resumed");
            }
        }

        //场景管理
        public static Node SwitchScene(string sceneName)
        {
            Log.Info($"Starting to switch scene: {sceneName}");
            
            // 检查场景层状态
            if (_sceneLayer == null)
            {
                Log.Error("Scene layer is null before switching scene!");
            }
            else
            {
                Log.Info($"Scene layer status before switch: {_sceneLayer.Name}, Parent: {_sceneLayer.GetParent()?.Name}, Children: {_sceneLayer.GetChildCount()}");
            }

            // 显示加载动画
            LoadingManager.Instance?.ShowLoading();

            // 隐藏场景层
            HideSceneLayer();

            try
            {
                PackedScene scene = GameViewRegister.GetScene(sceneName);

                if (scene != null)
                {
                    Log.Info($"Scene packed scene loaded: {sceneName}");
                    NowScene = scene.Instantiate<Node>();
                    // 使用Godot内置的_ready方法初始化，不需要额外调用Init
                    Log.Info($"Scene switch completed: {sceneName}");
                    return NowScene;
                }
                Log.Error($"Failed to load scene: {sceneName}");
                return null;
            }
            catch (System.Exception ex)
            {
                Log.Error($"Exception during scene switch: {ex.Message}");
                return null;
            }
            finally
            {
                // 检查场景层状态
                if (_sceneLayer == null)
                {
                    Log.Error("Scene layer is null after switching scene!");
                }
                else
                {
                    Log.Info($"Scene layer status after switch: {_sceneLayer.Name}, Parent: {_sceneLayer.GetParent()?.Name}, Children: {_sceneLayer.GetChildCount()}");
                }
                
                // 显示场景层
                ShowSceneLayer();

                // 隐藏加载动画
                LoadingManager.Instance?.HideLoading();
                
                Log.Info($"Scene switch process completed for: {sceneName}");
            }
        }

        /*************************弹窗管理*****************************/
        //获取识别码
        private static int GetIdentifier()
        {
            _identifier += 1;
            return _identifier;
        }

        public static Node GetPopUp(Variant id)
        {
            foreach (Node popup in _popupLayer.GetChildren())
            {

                if (id.VariantType == Variant.Type.Int &&
                    (int)popup.Get("Identifier") == (int)id)
                {
                    return popup;
                }

                if (id.VariantType == Variant.Type.String &&
                    (string)popup.Get("PopupName") == (string)id)
                {
                    return popup;
                }
            }
            return null;
        }

        public static Node OpenPopup(string popUpName)
        {
            if (_nowPopup != null)
            {
                return null;
            }

            PackedScene scene = GameViewRegister.GetScene(popUpName);
            if (scene != null)
            {
                Node sceneObj = scene.Instantiate<Node>();
                // sceneObj.PopupName = popUpName;
                // sceneObj.Identifier = _getIdentifier();
                sceneObj.Set("PopupName", popUpName);
                sceneObj.Set("Identifier", GetIdentifier());

                _popupLayer.AddChild(sceneObj);
                _nowPopup = sceneObj;

                // 暂停场景层
                PauseSceneLayer();

                // 使用Godot内置的_ready方法初始化，不需要额外调用Init

                return sceneObj;
            }
            return null;
        }

        public static void ClosePopup(int identifier)
        {
            Node popup = GetPopUp(identifier);
            if (popup != null)
            {
                _ = popup.Call("Exit");
                _popupLayer.RemoveChild(popup);
                popup.QueueFree();
                _nowPopup = null;

                // 恢复场景层
                ResumeSceneLayer();
            }
        }

        public static void ClosePopup(string popName)
        {
            Node popup = GetPopUp(popName);
            if (popup != null)
            {
                _ = popup.Call("Exit");
                _popupLayer.RemoveChild(popup);
                popup.QueueFree();
                _nowPopup = null;

                // 恢复场景层
                ResumeSceneLayer();
            }
        }
    }
}