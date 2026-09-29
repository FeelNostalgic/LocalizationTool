# Localization Tool

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)
[![Unity](https://img.shields.io/badge/Unity-2022.3.7f1-000000.svg)](https://unity.com)
[![TextMeshPro](https://img.shields.io/badge/TextMeshPro-3.0.6-000000.svg)](https://docs.unity3d.com/Packages/com.unity.textmeshpro@3.0/manual/index.html)

A complete localization workflow for Unity: a windowed editor tool to manage keys, categories and languages, plus a lightweight runtime API that swaps languages in your game and auto-fills TextMeshPro text.

Built as a full Unity project so you can open it, run the demo, and read the working implementation.

---

## Table of contents

- [Features](#features)
- [Requirements](#requirements)
- [Installation](#installation)
- [Quick start](#quick-start)
- [The four editor tabs](#the-four-editor-tabs)
- [Runtime API](#runtime-api)
- [Data layout on disk](#data-layout-on-disk)
- [Import / export workflow](#import--export-workflow)
- [Demo scene](#demo-scene)
- [Project structure](#project-structure)
- [Current state](#current-state)
- [Known limitations](#known-limitations)
- [Branches](#branches)
- [Contributing](#contributing)
- [License](#license)

---

## Features

**Editor**

- Windowed manager with four tabs: **Dictionary**, **Languages**, **Categories**, **Configuration**
- Keys grouped by category, one value per language, editable inline
- A rich text editor per value (bold, italic, underline, strikethrough, color, font size, zoom) with a live preview
- Search by key, by value, or by both, optionally filtered by category
- Per-language favourite (default) language that cannot be deleted or emptied
- Per-category default that new keys are assigned to
- Reorder languages and categories by dragging the up/down controls
- Import and export to **CSV**, **JSON** and **XML**
- Undo/redo for translation edits, key renames and language/category operations
- Configurable delete confirmations, clear-on-add fields, and console verbosity

**Runtime**

- `LocalizationToolAPI` — get a value, get a value in a specific language, switch language, list languages/categories/keys
- `LocalizationToolAddon` — drop onto any `TMP_Text` to have its text filled and kept in sync when the language changes
- A single language switch updates every addon in the scene
- Data is read from disk once at startup and cached in memory

---

## Requirements

| | |
|---|---|
| Unity | **2022.3.7f1** (the version this project targets) |
| TextMeshPro | 3.0.6 (already in `Packages/manifest.json`) |
| Render pipeline | Built-in. URP support is untested |

The tool targets the built-in render pipeline. TMP shaders in this repo were imported for the built-in pipeline, so switching to URP will need the TMP essentials re-imported for that pipeline.

---

## Installation

**Clone the repository**

```bash
git clone https://github.com/FeelNostalgic/LocalizationTool.git
```

Then open the folder with Unity **2022.3.7f1**. The first import will take a while (TextMeshPro essentials and UniRx are included in the repo).

**Or copy the tool into your own project**

Everything the tool needs lives in:

```
Assets/LocalizationTool/
```

Copy that folder into your project and you are done. There are no assembly definitions, so the scripts integrate into your default `Assembly-CSharp` / `Assembly-CSharp-Editor` automatically.

**Requirements you must add yourself**

`com.unity.editorcoroutines` is used at runtime by the import flow but is not declared in `manifest.json` — it currently resolves transitively through `com.unity.feature.development`. If you hit a `Unity.EditorCoroutines` namespace error in your own project, add it explicitly:

```json
"com.unity.editorcoroutines": "1.0.0"
```

---

## Quick start

### 1. Open the manager

`Tools > LocalizationTool > Manager`

This is a dockable window (minimum size 1400×1000) with the four tabs at the top.

### 2. Install the runtime API

`Tools > LocalizationTool > Install`

This creates a `LocalizationToolAPI` GameObject in the active scene. It is `DontDestroyOnLoad` and sets execution order to `-900`, so it initialises before your UI. If you open the tool before playing, you can also just add the component manually via `Add Component > Localization Tool > API > Auto translation`.

Keep this object in your **first loaded scene** — it must exist before anything that reads a translation.

### 3. Add an addon to your text

Select a `TMP_Text` in the hierarchy, then `Tools > LocalizationTool > UI > Add Addon To TMPro`.

This adds a `LocalizationToolAddon` component. In the Inspector you set the `Key`; at runtime the component looks the key up in the active language and writes the result into the text. When the language changes, every addon refreshes automatically.

To do it from code instead:

```csharp
using LocalizationTool.Scripts.Addons;

GetComponent<TMP_Text>().gameObject.AddComponent<LocalizationToolAddon>()
    .SetKey("Play_Button");
```

### 4. Switch language

```csharp
using LocalizationTool.Scripts.API;

LocalizationToolAPI.ChangeLanguage("Spanish");
```

Every addon in the scene updates to Spanish. There is no per-object call to make.

---

## The four editor tabs

### Dictionary

The main work surface.

- **Left column** — add a new key and pick its category, then a searchable, category-filtered list of existing keys.
- **Right column** — the translation for the language currently selected in the toolbar, one editable field per key.
- Clicking a key name copies it to the clipboard.
- The pencil icon opens the **rich text editor** for that value, where you can apply formatting tags and see a live preview of the result. Changes save automatically.
- Translation edits register with Unity's undo system, so `Ctrl+Z` works.

### Languages

Add, rename, reorder, and delete languages. The starred language is the **default**: it is applied at startup and cannot be deleted or emptied. You can also choose whether deleting a language removes it entirely or only clears its translations.

### Categories

Same shape as Languages, for categories. The starred category is the **default** and is assigned automatically to new keys. The default category cannot be deleted.

### Configuration

Editor preferences (delete confirmations, clear-on-add, search mode, console logs) plus the **import/export** panel.

---

## Runtime API

`Assets/LocalizationTool/Scripts/API/LocalizationToolAPI.cs`

All methods are static. The full public surface:

```csharp
// Get a value in the active language.
// Returns an empty string and sets found = false if the key or language is missing.
string value = LocalizationToolAPI.GetValueByKey("Play_Button", out bool found);

// Get a value in a specific language, regardless of the active one.
string spanish = LocalizationToolAPI.GetValueByKeyAndLanguage("Play_Button", "Spanish", out bool ok);

// Switch language. Returns false if that language does not exist.
bool success = LocalizationToolAPI.ChangeLanguage("English");

// Read the current active language.
string current = LocalizationToolAPI.ActiveLanguage;

// Introspection, for building a language selector.
List<string> languages  = LocalizationToolAPI.GetAvailableLanguages();
List<string> categories = LocalizationToolAPI.GetAllCategories();
List<string> keys       = LocalizationToolAPI.GetAllKeys();
```

A minimal language dropdown:

```csharp
using System.Linq;
using LocalizationTool.Scripts.API;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LanguageSelector : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        var languages = LocalizationToolAPI.GetAvailableLanguages();
        dropdown.options = languages.Select(l => new TMP_Dropdown.OptionData(l)).ToList();
        dropdown.value = languages.IndexOf(LocalizationToolAPI.ActiveLanguage);
        dropdown.onValueChanged.AddListener(OnChanged);
    }

    private void OnChanged(int index) =>
        LocalizationToolAPI.ChangeLanguage(LocalizationToolAPI.GetAvailableLanguages()[index]);
}
```

### LocalizationToolAddon

`Assets/LocalizationTool/Scripts/Addons/LocalizationToolAddon.cs`

Requires a `TMP_Text` on the same GameObject. Execution order `-999`, so it runs right after the API.

| Member | Purpose |
|---|---|
| `Key` | The key this text renders |
| `KeyIndex` | Cached position in the key list |
| `SetKey(string)` | Assign a key and immediately render its value |
| `LanguageUpdate(string)` | Called by the API on every language change |
| `OnKeyRemoved(string)` | Called when a key is deleted in the editor |
| `OnKeyUpdate(old, new)` | Called when a key is renamed in the editor |

---

## Data layout on disk

Translations are stored as ScriptableObject assets, one file per key, language and category, plus a root index:

```
Assets/LocalizationTool/Database/
├── LocalizationData.asset      # root index: keys + language + category dictionaries
├── Keys/                       # one .asset per translation key
├── Languages/                  # English.asset, Spanish.asset, ...
└── Categories/                 # None.asset, Main Menu.asset, ...
```

The per-item assets mean every key is version-controlled, diffable, and mergeable — unlike a single serialized blob.

Two more locations:

```
Assets/LocalizationTool/PersistentData/DoNotTouch/
├── Configuration/LocalizationConfiguration.bin   # binary editor preferences
└── Info/{Readme,License}.txt                      # shown in the Configuration tab

Assets/LocalizationTool/Exports/                   # generated export files
```

`DoNotTouch` is written to by the tool at runtime; commit it but don't hand-edit it.

---

## Import / export workflow

**Configuration > Export** — pick a format, pick a location, done.

| Format | Shape | Notes |
|---|---|---|
| **CSV** | `Key;Category;English;French;Spanish` | One row per key, one column per language. Line breaks are stripped on export. |
| **JSON** | Nested dictionaries | Built with `JsonUtility` |
| **XML** | Nested elements | Built with `XmlSerializer` |

CSV separators are selectable: `;` `,` `.` `:` `|` `=`.

**The outsourcing workflow this was built for:**

1. Export to CSV or JSON.
2. Send the file to your translator.
3. They fill in the new language columns and return the file.
4. Import it back — the import window reports progress per item.

Import is partial by design: you can import a single language, a single category, or a single key, so you do not overwrite good data with a partial file. Existing keys are updated rather than duplicated.

---

## Demo scene

```
Assets/LocalizationTool/Demo/Scenes/LocalizationToolDemoScene_1.unity
```

A main menu with **Play / Options / Exit**, a language dropdown, and a volume label. `LocalizationToolDemoScene_2.unity` is an empty scene used for transitions.

The demo works by swapping data at play mode: on `Awake` the `UIManager` backs up the real database to `PersistentData/Backups/CurrentBackup.json`, loads `Demo/Data/DemoData.json` instead, and restores the backup on disable. This keeps the demo data from contaminating your actual translations.

To run it: open the scene and press Play.

> The scene is **not** in `ProjectSettings/EditorBuildSettings.asset` (`m_Scenes` is empty). If you want to build the demo, add it to Build Settings first.

---

## Project structure

```
Assets/
├── LocalizationTool/                  # the tool
│   ├── Scripts/
│   │   ├── API/                       # LocalizationToolAPI — runtime entry point
│   │   ├── Addons/                    # LocalizationToolAddon — TMP component
│   │   ├── Commons/                   # strings, GUIStyles, icons, helpers
│   │   ├── Data/                      # CacheDataSO, ScriptableObjects, config
│   │   ├── Editors/                   # the four tab implementations
│   │   ├── ExportSerializer/          # CSV serializer
│   │   ├── General/                   # LocalizationManager, menu bar
│   │   ├── Serializer/                # JSON / XML / binary serializers
│   │   └── Test/                      # empty on this branch
│   ├── Database/                      # your translation data (ScriptableObjects)
│   ├── Demo/                          # demo scenes and data
│   ├── Exports/                       # generated files
│   └── PersistentData/                # config, backups, in-editor docs
├── CustomDebug/                       # colored logging helper
├── Tarfmagougou/                      # editor log-trace helper
├── Plugins/UniRx/                     # reactive extensions
├── Scenes/SampleScene.unity
└── TextMesh Pro/                      # TMP essentials
```

The heavy lifting is in `LocalizationManager.cs` (~750 lines, all editor-only) and `CacheDataSO.cs` (~640 lines), which is the single source of truth for keys, languages and categories.

---

## Current state

**The current version, Unity 2022.3.7f1 on the `main` branch, is the working one.**

The last commits on `main` are WIP on undo support for key renaming. Specifically:

- Undo works for: translation edits, key renames, language add/remove, category add/remove, name changes, and display order, in both Languages and Categories.
- Undo is **incomplete for key rename** — the last commit is literally `WIP on UNDO key name change`, and the fix notes say it needs either an update to the language dictionary or a change to store translations under keys instead of languages. The existing undo stack in `CacheDataSO.cs` (`UndoRenameKeyTracker`) pushes the old/new name pair, but the redo path is not finished.

Everything else in the four tabs is functional.

**There are no tests on `main`.** The `Scripts/Test/` folder is empty. See [Branches](#branches).

---

## Known limitations

Be aware of these before adopting the tool:

1. **No player builds.** `CacheDataSO.cs` calls `AssetDatabase` and `EditorUtility` in 30 places without an `#if UNITY_EDITOR` guard, so the assembly will not compile for a standalone player. It works in the editor and in Play mode, which is what the demo exercises, but shipping a build that includes this code will fail to compile. Fixing it means either guarding the editor calls or moving the storage layer out of `AssetDatabase` — this is the single largest piece of work left.

2. **No `.asmdef`.** Everything compiles into `Assembly-CSharp`. Adding assembly definitions would isolate editor code from runtime code and make point 1 tractable.

3. **Performance past a few hundred keys.** The dictionary renders every row each repaint through IMGUI. An earlier branch addressed lag beyond 400 items, but that work is not on `main`.

4. **`com.unity.editorcoroutines` is an undeclared dependency** (see [Installation](#installation)).

5. **The Documentation button does nothing.** In `ConfigurationEditor.cs` it is still a `//TODO`. The Readme and License buttons work.

6. **CSV export strips line breaks.** Multi-line translations will not survive a CSV round trip. Use JSON or XML if you need them.

7. **CSV import is delimiter-sensitive.** The separator must match what the file actually uses or the import will silently produce wrong results.

8. **Untested outside the built-in render pipeline.**

---

## Branches

| Branch | State | Notes |
|---|---|---|
| `main` | **Active, this is the working version** | Unity 2022.3.7f1. WIP on undo for key rename. |
| `Tests` | Parked | Adds NUnit tests (`LocalizationTool.Test.asmdef`) and a SQLite-backed store. Stale — last touched September 2024, before the ScriptableObject rewrite landed on `main`. |
| ~~`Unity-6`~~ | **Deleted** | A failed Unity 6 migration. It was not a forward port of `main`: it was built on the older SQLite architecture (`CacheData.cs` plus a native `sqlite3.dll`) that `main` had already replaced with ScriptableObjects, then updated to Unity 6 on top. The two architectures were never reconciled, so the branch does not compile. Recoverable from commit `1183436` if the Unity 6 upgrade is ever worth redoing from the current architecture. |

---

## Contributing

Issues and pull requests are welcome.

A few things that will make your contribution much easier to merge:

- **Match the existing structure.** Files under `Scripts/Editors/` are UI-only and wrapped in `#if UNITY_EDITOR`; files under `Scripts/API/` and `Scripts/Addons/` are runtime code. Do not mix them.
- **User-facing strings go in `Commons/EditorStrings.cs`.** Every label, tooltip and log message is a `const` there, not a literal in the layout code. This is what makes the tool's copy centrally editable.
- **GUIStyles go in `Commons/CustomStyles.cs`,** registered by name in `Data/Enums.cs` under `CustomStyleName`. Never construct a `GUIStyle` inline in a layout method.
- **Route all data mutation through `LocalizationManager`.** The editors call into it; they do not touch `CacheDataSO` mutating methods directly.
- **When adding a ScriptableObject asset, commit its `.meta` file** with the same commit. The per-asset storage depends on stable GUIDs.

Note that the project has no test coverage on `main`. If you touch data mutation logic, a manual pass through all four tabs is the current bar.

---

## License

MIT — see [LICENSE](LICENSE).

Copyright (c) 2025 Francisco Aragonés
