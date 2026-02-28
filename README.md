# HD-2D RPG Project

这是一个基于 Godot 4.x (C#) 开发的 HD-2D 风格 RPG 游戏项目。本项目采用了模块化的架构设计，集成了完整的战斗、技能、任务、存档和对话系统。

## 目录

- [项目结构](#项目结构)
- [功能内部结构](#功能内部结构)
  - [核心架构 (Core)](#核心架构-core)
  - [技能与战斗系统 (Skill & Combat)](#技能与战斗系统-skill--combat)
  - [存档系统 (Save System)](#存档系统-save-system)
  - [任务系统 (Quest System)](#任务系统-quest-system)
  - [AI 系统 (AI System)](#ai-系统-ai-system)
  - [UI 系统 (UI System)](#ui-系统-ui-system)
  - [资源管理 (Resource Management)](#资源管理-resource-management)

---

## 项目结构

以下是项目的目录层级结构及其主要作用说明：

```
/
├── Module/                 # 独立的游戏模块资源
│   ├── House/              # 房屋模块（场景、脚本、资源）
│   └── terrain/            # 地形相关资源
│
├── Resources/              # 游戏静态资源与数据
│   ├── Config/             # 全局配置文件 (如 DesignTokens.json)
│   ├── Examples/           # JSON 数据结构示例 (用于开发参考)
│   ├── Font/               # 字体文件
│   ├── Localization/       # 本地化 CSV 文件 (多语言支持)
│   ├── Material/           # 3D 材质与纹理资源
│   ├── Music/              # 背景音乐与音效
│   ├── Static/             # 静态游戏数据 (JSON格式: 任务、物品、技能、怪物等)
│   ├── Theme/              # Godot UI 主题资源
│   └── player/             # 玩家美术资源
│
├── Scenes/                 # 游戏场景文件 (.tscn)
│   ├── Battle/             # 战斗场景
│   ├── First/              # 初始加载场景
│   ├── Player/             # 玩家预制件
│   ├── Popup/              # 弹出窗口 (菜单、存档选择等)
│   ├── Start/              # 游戏开始菜单
│   ├── UI/                 # 通用 UI 界面 (对话框、HUD)
│   └── test/               # 测试场景
│
├── Scripts/                # 核心 C# 源代码
│   ├── Core/               # 核心系统框架
│   │   ├── UI/             # UI 框架 (焦点管理、交互组件)
│   │   └── [Managers]      # 视图管理、关卡管理等核心逻辑
│   │
│   ├── Managers/           # 全局管理器 (单例模式)
│   │   ├── GameManager.cs      # 游戏主循环与状态管理
│   │   ├── SaveManager.cs      # 存档管理
│   │   ├── DialogueManager.cs  # 对话系统管理
│   │   ├── ConfigManager.cs    # 配置读取
│   │   └── ResourcesManager.cs # 资源加载管理
│   │
│   ├── Modules/            # 业务逻辑模块
│   │   ├── AI/             # AI 行为树与感知系统
│   │   ├── Battle/         # 战斗逻辑实现
│   │   ├── Dialogue/       # 对话数据模型与处理
│   │   ├── Quest/          # 任务系统逻辑
│   │   ├── SkillSystem/    # 技能与 Buff 系统核心
│   │   └── [Entities]      # 实体类 (Player, Creature, NPC, Item)
│   │
│   ├── Scenes/             # 场景特定的脚本逻辑
│   ├── Tools/              # 开发工具 (如 DPS 计算器)
│   └── Utilities/          # 通用工具类 (日志、JSON解析、数学扩展)
│
├── addons/                 # Godot 插件 (如自定义的 SkillEditor)
├── doc/                    # 项目文档
├── godot-ci/               # CI/CD 配置
├── Main.cs                 # 游戏入口脚本
├── project.godot           # Godot 项目配置文件
└── README.md               # 项目说明文档
```

---

## 功能内部结构

### 核心架构 (Core)

项目的核心架构负责生命周期管理、视图切换和全局配置。

*   **GameManager**
    *   **作用**: 全局单例，维护游戏状态（Playing, Paused, Menu 等）。
    *   **依赖**: 协调其他管理器（SaveManager, SceneManager）。
*   **GameViewManager**
    *   **作用**: 管理 UI 视图的堆栈。支持视图的 Push（入栈）、Pop（出栈）和层级管理。
    *   **核心组件**: `ViewRegister` (注册视图路径), `BaseView` (视图基类)。
*   **VersionManager**
    *   **作用**: 版本控制与更新检测，确保存档与游戏版本的兼容性。

### 技能与战斗系统 (Skill & Combat)

基于事件驱动和数据驱动的设计，支持复杂的技能效果和 Buff 机制。

*   **SkillManager**
    *   **核心职责**: 技能的释放流程控制（检查消耗 -> 播放动画 -> 触发效果 -> 进入冷却）。
    *   **数据流**: 读取 `SkillData` -> 实例化 `SkillExecutor` -> 触发 `SkillEvents`。
*   **BuffSystem**
    *   **BuffManager**: 单例，维护所有实体上的 Buff 列表。负责 Buff 的生命周期（应用、Tick、移除）。
    *   **BuffData**: 定义 Buff 属性（ID、类型、持续时间、叠加规则、属性修正值）。
    *   **机制**: 支持 Buff 叠加（Stacking）、刷新持续时间（Refresh）、互斥覆盖。
*   **DamageCalculator**
    *   **作用**: 静态工具类，统一处理伤害公式。
    *   **逻辑**: 基础伤害 * (1 + 攻击加成) - (防御 * (1 + 防御加成)) * 暴击倍率 * 属性克制系数。
*   **SkillEvents**
    *   **作用**: 事件总线，解耦战斗逻辑。
    *   **钩子**: `OnSkillCast`, `OnDamageCalculated`, `OnBuffApplied` 等。

### 存档系统 (Save System)

采用 JSON 序列化的方式存储游戏进度，支持多存档槽位。

*   **SaveManager**
    *   **核心职责**: 序列化/反序列化游戏数据，文件 I/O 操作。
    *   **关键方法**: `SaveGame(slotId)`, `LoadGame(slotId)`, `DeleteSave(slotId)`, `GetSaveMetadata()`.
*   **SaveModels**
    *   **组成**: 
        *   `SaveData`: 根对象，包含时间戳、版本号。
        *   `PlayerData`: 玩家属性、位置、背包。
        *   `QuestData`: 任务进度状态。
        *   `WorldData`: 世界状态（如宝箱开启状态、NPC 状态）。
*   **SaveSlotSelector (UI)**
    *   **作用**: 扫描存档目录，动态生成存档列表 UI，提供加载、覆盖、删除交互。

### 任务系统 (Quest System)

支持线性任务链和分支任务。

*   **QuestManager**
    *   **作用**: 运行时任务状态管理（接受任务、更新目标、完成任务）。
    *   **数据**: 维护 `ActiveQuests` 和 `CompletedQuests` 列表。
*   **QuestLineManager**
    *   **作用**: 管理任务之间的依赖关系（前置任务、后续任务），处理剧情线的推进。
*   **QuestData**
    *   **定义**: 包含任务 ID、标题、描述、目标类型（杀怪/对话/收集）、奖励内容。

### AI 系统 (AI System)

基于行为树（Behavior Tree）和感知系统构建的智能体。

*   **Behavior Tree**
    *   **核心组件**: `BehaviorTreeRunner` (执行器), `BehaviorTreeCore` (节点基类)。
    *   **节点类型**:
        *   **Composites**: Selector, Sequence.
        *   **Decorators**: Inverter, Repeater.
        *   **Actions**: `MoveToTarget`, `DetectTarget`, `Attack`.
*   **SensorySystem**
    *   **作用**: 模拟视听感知，检测范围内的目标（Player）。
    *   **输出**: 更新 AI 的 `Blackboard`（黑板数据），供行为树决策。

### UI 系统 (UI System)

*   **DesignSystem**
    *   **作用**: 集中管理 UI 样式（颜色、字体、间距），确保视觉一致性。
*   **DialogueManager**
    *   **作用**: 解析对话数据，控制对话框的显示，处理玩家选项。
    *   **特性**: 支持打字机效果、分支选项回调。
*   **PopupMenu**
    *   **作用**: 游戏内综合菜单，集成了状态查看、物品栏、任务列表、系统设置等子模块。

### 资源管理 (Resource Management)

*   **ResourcesManager**
    *   **作用**: 统一加载路径，提供资源缓存（可选），封装 `GD.Load`。
    *   **支持**: 预加载常用资源，按需加载大资源。
*   **Localization**
    *   **实现**: 基于 CSV 的键值对映射，支持动态切换语言（`TranslationServer`）。
