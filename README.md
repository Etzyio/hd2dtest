# HD2D Game Project

这是一个使用Godot引擎开发的HD2D（2D角色+3D背景）风格游戏项目。

## 项目结构

```
├── .github/                # GitHub Actions配置
│   └── workflows/
│       └── build-release.yml  # 构建和发布工作流
├── .godot/                 # Godot引擎生成的文件
├── Resources/              # 游戏资源
│   ├── Examples/           # 示例资源文件
│   │   ├── CharacterExample.json  # 角色示例
│   │   ├── EquipmentExample.json  # 装备示例
│   │   ├── ItemExample.json       # 道具示例
│   │   ├── MonsterExample.json    # 怪物示例
│   │   └── SkillExample.json      # 技能示例
│   └── Localization/       # 本地化资源
│       └── zh.json         # 中文翻译
├── Scripts/                # 游戏脚本
│   ├── Core/               # 核心系统
│   │   ├── ConfigManager.cs    # 配置管理器
│   │   ├── GameManager.cs      # 游戏管理器
│   │   ├── Log.cs              # 日志系统
│   │   ├── SaveManager.cs      # 存档管理器
│   │   └── VersionManager.cs   # 版本管理器
│   ├── Modules/            # 游戏模块
│   │   ├── Character.cs        # 角色类
│   │   ├── Creature.cs         # 生物基类
│   │   ├── Equipment.cs        # 装备类
│   │   ├── Monster.cs          # 怪物类
│   │   ├── NPC.cs              # NPC类
│   │   ├── Player.cs           # 玩家类（单例）
│   │   ├── Skill.cs            # 技能类
│   │   └── Weapon.cs           # 武器类
│   ├── Player/             # 玩家相关
│   │   ├── CameraController.cs # 相机控制器
│   │   └── PlayerController.cs # 玩家控制器
│   └── World/              # 世界相关
│       ├── Background3D.cs     # 3D背景管理
│       └── LevelManager.cs     # 关卡管理器
├── main.cs                 # 主入口脚本
├── main.tscn               # 主场景
└── hd2dtest.csproj         # C#项目配置
```

## 核心功能

### 1. 核心系统

#### 配置管理（ConfigManager）
- 音量设置（主音量、音乐、音效、语音）
- 图形设置（亮度、对比度、饱和度）
- 分辨率和全屏设置
- 垂直同步设置
- 游戏玩法设置（自动存档、文本速度、显示FPS）
- 自定义按键绑定
- 配置自动保存到`user://config.json`

#### 游戏管理（GameManager）
- 游戏初始化和状态管理
- 场景切换和加载
- 游戏暂停/继续

#### 日志系统（Log）
- 支持多种日志级别（Debug、Info、Warning、Error）
- 单例模式，全局访问
- 可扩展的日志处理

#### 存档管理（SaveManager）
- 支持多存档槽位
- 存档自动按ID排序
- 支持自定义数据保存
- 存档文件保存到`user://saves/`目录
- 存档信息包含：
  - 基本信息（ID、名称、保存时间）
  - 玩家状态（位置、等级、经验、属性）
  - 游戏进度（当前场景、分数、游戏时间）
  - 已完成任务和已发现区域
  - 物品和装备
  - 已学习技能和技能等级

#### 版本管理（VersionManager）
- 版本信息管理
- 支持基于git tags的版本号
- 版本信息显示

### 2. 游戏模块

#### 生物系统
- **Creature（生物基类）**：包含生命值、攻击力、防御力、等级、经验值、是否存活等属性
- **Character（角色类）**：继承自Creature，添加了移动、攻击、防御等功能
- **NPC（非玩家角色类）**：继承自Character，添加了交互功能
- **Player（玩家类）**：继承自Character，单例模式，玩家控制
- **Monster（怪物类）**：继承自Character，添加了AI行为

#### 装备系统
- **Weapon（武器类）**：包含武器类型、攻击力、特殊效果等
- **Equipment（装备类）**：包含装备类型、属性加成、特殊效果等

#### 技能系统
- **Skill（技能类）**：包含技能类型、伤害、冷却时间、效果等

### 3. 世界系统

#### 3D背景（Background3D）
- 管理3D背景环境
- 支持动态背景效果

#### 关卡管理（LevelManager）
- 关卡加载和切换
- 关卡进度管理

### 4. 玩家系统

#### 相机控制（CameraController）
- 跟随玩家移动
- 平滑相机过渡

#### 玩家控制（PlayerController）
- 玩家输入处理
- 移动、跳跃、攻击等动作

## 技术特点

1. **HD2D风格**：2D角色动画与3D背景结合
2. **模块化设计**：清晰的代码结构，便于扩展和维护
3. **单例模式**：核心管理器使用单例模式，便于全局访问
4. **存档系统**：支持多存档，自动保存到用户目录
5. **配置系统**：支持各种游戏设置，自动保存
6. **国际化支持**：已准备好中文本地化
7. **CI/CD集成**：GitHub Actions自动构建和发布

## 开发环境

- Godot 4.x
- .NET 8.0
- C#

## 构建和运行

### 构建项目
```bash
dotnet build
```

### 运行游戏
在Godot编辑器中打开项目，然后点击运行按钮。

## 存档系统说明

### 存档位置
存档文件保存到系统的用户目录，路径为：
- Windows: `%APPDATA%/Godot/app_userdata/hd2dtest/saves/`
- macOS: `~/Library/Application Support/Godot/app_userdata/hd2dtest/saves/`
- Linux: `~/.local/share/godot/app_userdata/hd2dtest/saves/`

### 存档文件格式
每个存档文件是一个JSON文件，命名格式为`save_{id}.json`，其中`{id}`是存档槽位ID。

### 存档内容
- 基本信息：ID、名称、保存时间
- 玩家状态：位置、等级、经验、生命值、魔法值、属性
- 游戏进度：当前场景、分数、游戏时间
- 已完成任务和已发现区域
- 物品和装备
- 已学习技能和技能等级

## 配置系统说明

### 配置位置
配置文件保存到：`user://config.json`

### 配置内容
- 音量设置
- 图形设置
- 游戏玩法设置
- 按键绑定

## 版本控制

版本号基于git tags，格式为`v{major}.{minor}.{patch}`，例如`v1.0.0`。

## 贡献

欢迎提交Issue和Pull Request！

## 许可证

MIT License
