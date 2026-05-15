/*
 * File: HD2DCharacter.cs
 * Author: hd2dtest Team
 * Last Modified: 2026-05-15
 * 
 * Purpose:
 * HD2D角色渲染组件，负责角色的3D渲染和动画控制。
 * 使用多层Sprite3D实现2D精灵在3D空间中的渲染，支持角色朝向相机、阴影效果和图层管理。
 * 
 * Key Features:
 * - 图层系统：支持基础层、身体层、头部层、武器层、特效层
 * - 碰撞体管理：自动创建胶囊碰撞体
 * - 阴影系统：动态阴影创建和强度控制
 * - 朝向相机：角色自动面向主相机
 * - 纹理管理：支持图层纹理设置和颜色调整
 * - 缩放和翻转：支持角色缩放和水平翻转
 */

using Godot;
using System;
using System.Collections.Generic;

namespace hd2dtest.Scripts.Modules
{
    /// <summary>
    /// 角色图层枚举
    /// </summary>
    /// <remarks>
    /// 定义角色渲染的图层顺序，用于管理角色的各个视觉组件
    /// </remarks>
    public enum CharacterLayer
    {
        /// <summary>基础层（背景）</summary>
        Base,
        /// <summary>身体层</summary>
        Body,
        /// <summary>头部层</summary>
        Head,
        /// <summary>武器层</summary>
        Weapon,
        /// <summary>特效层（最上层）</summary>
        Effects
    }

    /// <summary>
    /// HD2D角色类，继承自CharacterBody3D，实现2D精灵在3D空间中的渲染
    /// </summary>
    /// <remarks>
    /// 使用多层Sprite3D实现角色渲染，支持自动朝向相机、阴影效果和图层管理
    /// </remarks>
    public partial class HD2DCharacter : CharacterBody3D
    {
        [Export] public string CharacterId = "";
        [Export] public float CharacterScale = 1.0f;
        [Export] public float CollisionHeight = 2.0f;
        [Export] public float CollisionRadius = 0.3f;

        private Dictionary<CharacterLayer, Sprite3D> _layers = new Dictionary<CharacterLayer, Sprite3D>();
        private CollisionShape3D _collisionShape;
        private Node3D _shadowNode;
        private Sprite3D _shadowSprite;
        private Node3D _spriteRoot;
        private Camera3D _mainCamera;

        public override void _Ready()
        {
            InitializeCollision();
            InitializeSpriteLayers();
            CreateShadow();
            _mainCamera = GetViewport().GetCamera3D();
        }

        private void InitializeCollision()
        {
            CapsuleShape3D capsuleShape = new CapsuleShape3D();
            capsuleShape.Radius = CollisionRadius;
            capsuleShape.Height = CollisionHeight;

            _collisionShape = new CollisionShape3D();
            _collisionShape.Name = "CollisionShape";
            _collisionShape.Shape = capsuleShape;
            AddChild(_collisionShape);
        }

        private void InitializeSpriteLayers()
        {
            _spriteRoot = new Node3D();
            _spriteRoot.Name = "SpriteRoot";
            _spriteRoot.Position = new Vector3(0, CollisionHeight / 2, 0);
            AddChild(_spriteRoot);

            foreach (CharacterLayer layer in Enum.GetValues(typeof(CharacterLayer)))
            {
                Sprite3D sprite = new Sprite3D();
                sprite.Name = layer.ToString();
                sprite.Centered = true;
                sprite.FlipH = false;
                sprite.FlipV = false;
                sprite.Scale = new Vector3(CharacterScale, CharacterScale, 1);
                _spriteRoot.AddChild(sprite);
                _layers[layer] = sprite;
            }
        }

        private void CreateShadow()
        {
            _shadowNode = new Node3D();
            _shadowNode.Name = "Shadow";
            _shadowNode.Position = new Vector3(0, 0.01f, 0);
            AddChild(_shadowNode);

            _shadowSprite = new Sprite3D();
            _shadowSprite.Name = "ShadowSprite";
            _shadowSprite.Scale = new Vector3(CharacterScale * 1.5f, CharacterScale * 0.3f, 1);
            _shadowSprite.Modulate = new Color(0, 0, 0, 0.4f);
            _shadowSprite.Centered = true;
            _shadowNode.AddChild(_shadowSprite);
        }

        public override void _Process(double delta)
        {
            FaceCamera();
        }

        private void FaceCamera()
        {
            if (_mainCamera == null || _spriteRoot == null) return;

            Vector3 cameraDirection = GlobalPosition - _mainCamera.GlobalPosition;
            cameraDirection.Y = 0;
            cameraDirection = cameraDirection.Normalized();

            if (cameraDirection != Vector3.Zero)
            {
                _spriteRoot.GlobalRotation = new Vector3(0, Mathf.Atan2(cameraDirection.X, cameraDirection.Z), 0);
            }
        }

        public void SetLayerTexture(CharacterLayer layer, Texture2D texture)
        {
            if (_layers.TryGetValue(layer, out Sprite3D sprite))
            {
                sprite.Texture = texture;
            }
        }

        public void FlipHorizontal(bool flip)
        {
            foreach (var layer in _layers.Values)
            {
                layer.FlipH = flip;
            }
        }

        public void SetScale(float scale)
        {
            CharacterScale = scale;
            foreach (var layer in _layers.Values)
            {
                layer.Scale = new Vector3(scale, scale, 1);
            }
            if (_shadowSprite != null)
            {
                _shadowSprite.Scale = new Vector3(scale * 1.5f, scale * 0.3f, 1);
            }
        }

        public void SetColor(CharacterLayer layer, Color color)
        {
            if (_layers.TryGetValue(layer, out Sprite3D sprite))
            {
                sprite.Modulate = color;
            }
        }

        public void ShowLayer(CharacterLayer layer, bool visible)
        {
            if (_layers.TryGetValue(layer, out Sprite3D sprite))
            {
                sprite.Visible = visible;
            }
        }

        public Sprite3D GetLayer(CharacterLayer layer)
        {
            if (_layers.TryGetValue(layer, out Sprite3D sprite))
            {
                return sprite;
            }
            return null;
        }

        public void SetShadowEnabled(bool enabled)
        {
            if (_shadowNode != null)
            {
                _shadowNode.Visible = enabled;
            }
        }

        public void SetShadowIntensity(float intensity)
        {
            if (_shadowSprite != null)
            {
                Color currentColor = _shadowSprite.Modulate;
                _shadowSprite.Modulate = new Color(currentColor.R, currentColor.G, currentColor.B, intensity);
            }
        }

        public void Move(Vector3 direction, float speed)
        {
            Velocity = direction.Normalized() * speed;
            MoveAndSlide();
        }

        public void LookAtDirection(Vector3 direction)
        {
            if (direction.X < 0)
            {
                FlipHorizontal(true);
            }
            else if (direction.X > 0)
            {
                FlipHorizontal(false);
            }
        }
    }
}