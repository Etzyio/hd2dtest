# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

HD-2D style RPG game built on Godot 4.6 (C#), targeting .NET 10.0. Features turn-based combat, skill/buff systems, quest chains, save/load, dialogue, AI behavior trees, and localization (zh_CN, en_US, ja_JP).

## Build & Development Commands

```bash
# Build (default: Debug with Steam)
dotnet build

# Build without Steam
dotnet build -c NoSteam

# Build release
dotnet build -c Release

# Generate version.json from git metadata
bash build_version.sh
```

Godot editor is the primary development environment. The entry scene is `main.tscn`.

## Architecture

### Autoload Singletons (boot order in `project.godot`)

Seven nodes are auto-registered and available globally:

| Singleton | Purpose |
|---|---|
| `ConfigManager` | User preferences (audio, graphics, language, keybinds), persisted to `user://config.json` |
| `GameManager` | Game state, item inventory, teammate management, UI cancel/popup handling |
| `ResourcesManager` | Loads all static JSON data into typed `Dictionary<string, T>` caches |
| `SaveManager` | JSON save/load to `user://saves/`, 20-slot limit, auto-save |
| `DialogueManager` | Dialogue parsing, typewriter effect, branching options |
| `VersionManager` | Version tracking from `version.json` for save compatibility |
| `HD2DRenderManager` | HD-2D rendering pipeline |

### Application Entry Point (`Main.cs`)

`Main` is the root node of `main.tscn`. It owns a `_sceneLayer` (active scene container) and a `_popupLayer` (menu overlay). Scene switching goes through `Main.SwitchScene("sceneName")`, which looks up the `.tscn` path from `ViewRegister.json` via `GameViewRegister.GetScene()`.

### Data Flow

```
Resources/Static/*.json  →  ResourcesManager (async priority loading)  →  static Dictionary caches
```

All game data (skills, items, monsters, weapons, equipment, NPCs, levels, quests) is authored as JSON files under `Resources/Static/` and loaded into static caches like `ResourcesManager.SkillsCache`, `ResourcesManager.ItemsCache`, etc. Quest data lives under `Resources/Static/Quests/`.

`ViewRegister.json` is loaded **synchronously** first (before any async loading) because scene switching depends on it.

### Key Subsystems

- **Combat** (`Scripts/Modules/Battle/BattleManager.cs`): Persona-style turn-based system. `SkillExecutor` handles skill flow (check cost → play animation → trigger effects → cooldown). `BuffSystem` manages buff lifecycle (apply, tick, remove) with stacking/refresh rules. `DamageCalculator` is a static utility for damage formulas.
- **Skill System** (`Scripts/Modules/SkillSystem/`): `SkillEvents` acts as an event bus (`OnSkillCast`, `OnDamageCalculated`, `OnBuffApplied`). `SkillManager` (static) holds skill templates.
- **Quests** (`Scripts/Quest/`): `QuestManager` handles runtime quest state. `QuestLineManager` manages quest dependencies and story progression nodes. `QuestTriggerSystem` and `NotificationManager` handle in-world triggers and UI notifications.
- **AI** (`Scripts/Modules/AI/`): Behavior tree (`BehaviorTreeRunner` + nodes: Selector, Sequence, Inverter, Repeater) with a blackboard for decision-making. `SensorySystem` simulates sight/hearing detection.
- **Save System** (`Scripts/Modules/SaveModels.cs`): `SaveData` is the root save object. `PlayerSaveData` holds individual player state. Saves are JSON with camelCase, stored at `user://saves/save_{id}.json`. `SaveInfo` is a lightweight struct for save-list display.
- **Localization**: CSV-based via Godot's `TranslationServer`. Translation files are in `Resources/Localization/`. Use `TranslationServer.Translate("key")`.

### Namespace & Folder Mapping

- `hd2dtest.Scripts.Core` → `Scripts/Core/` (framework: view management, level loading)
- `hd2dtest.Scripts.Managers` → `Scripts/Managers/` (global singleton managers)
- `hd2dtest.Scripts.Modules` → `Scripts/Modules/` (game entities, skill system, battle, AI, dialogue data)
- `hd2dtest.Scripts.Modules.Battle` → `Scripts/Modules/Battle/`
- `hd2dtest.Scripts.Modules.SkillSystem` → `Scripts/Modules/SkillSystem/`
- `hd2dtest.Scripts.Quest` → `Scripts/Quest/`
- `hd2dtest.Scripts.Utilities` → `Scripts/Utilities/`
- `hd2dtest.Scenes` → `Scripts/Scenes/`
- `hd2dtest.Scripts.World` → `Scripts/World/`

## Code Conventions

- **Logging**: Always use `Log.Info()`, `Log.Error()`, `Log.Warning()`, `Log.Debug()`, `Log.Critical()` from `hd2dtest.Scripts.Utilities`. Never use `Console.WriteLine` or `GD.Print` directly. All log messages must be in English.
- **Comments**: All classes, methods, properties, and public fields should have XML doc comments (`///`) describing purpose and parameters.
- **Exception handling**: Wrap operations that can fail in try/catch. Log errors and degrade gracefully rather than crashing.
- **File headers**: Each `.cs` file starts with a standardized header block noting file name, author, last modified date, purpose, and key features.
- **Singleton pattern**: Managers use either `Instance` (static property) or `GetInstance()` (lazy-init method). The `SingletonHelper` utility provides safe access patterns (`ExecuteIfAvailable`, `TryGetInstance`).
- Do not commit code changes without the user explicitly asking. Do not modify content the user has authored without asking.
