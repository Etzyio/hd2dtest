using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace hd2dtest.Scripts.Core
{
    public class GameViewManager
    {
        //场景层
        private static Node _sceneLayer;
        //弹窗层
        private static Node _popupLayer;
        private static readonly Queue<Node> _弹窗队列 = new();
        //弹窗标识
        private static int _identifier = 0;
        //当前弹窗
        private static Node _nowPopup;
        //当前场景
        private static Node _nowScene;

        //当前新场景
        public static Node nowScene
        {
            get => _nowScene;
            set
            {
                if (_nowScene != null)
                {
                    _nowScene.Call("Exit");
                    _sceneLayer.RemoveChild(_nowScene);
                    _nowScene.QueueFree();  //释放资源
                }

                _nowScene = value;
                if (_nowScene != null)
                {
                    _sceneLayer.AddChild(_nowScene);
                }
            }
        }


        public static void Init(Node sceneLayer, Node popupLayer)
        {
            _sceneLayer = sceneLayer;
            _popupLayer = popupLayer;
        }

        //场景管理
        public static Node SwitchScene(string sceneName)
        {

            PackedScene scene = GameViewRegister.GetScene(sceneName);

            if (scene != null)
            {
                nowScene = scene.Instantiate<Node>();
                // 使用Godot内置的_ready方法初始化，不需要额外调用Init
                return nowScene;
            }
            return null;
        }

        /*************************弹窗管理*****************************/
        //获取识别码
        private static int _getIdentifier()
        {
            _identifier += 1;
            return _identifier;
        }

        public static Node GetPopUp(Godot.Variant id)
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
            if (_nowPopup != null) return null;

            var scene = GameViewRegister.GetScene(popUpName);
            if (scene != null)
            {
                var sceneObj = scene.Instantiate<Node>();
                // sceneObj.PopupName = popUpName;
                // sceneObj.Identifier = _getIdentifier();
                sceneObj.Set("PopupName", popUpName);
                sceneObj.Set("Identifier", _getIdentifier());

                _popupLayer.AddChild(sceneObj);
                _nowPopup = sceneObj;
                // 使用Godot内置的_ready方法初始化，不需要额外调用Init

                return sceneObj;
            }
            return null;
        }

        public static void ClosePopup(int identifier)
        {
            var popup = GetPopUp(identifier);
            if (popup != null)
            {
                popup.Call("Exit");
                _popupLayer.RemoveChild(popup);
                popup.QueueFree();
                _nowPopup = null;
            }
        }

        public static void ClosePopup(string popName)
        {
            var popup = GetPopUp(popName);
            if (popup != null)
            {

                popup.Call("Exit");
                _popupLayer.RemoveChild(popup);
                popup.QueueFree();
                _nowPopup = null;
            }
        }
    }
}