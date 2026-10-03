# Game View Fullscreen

English | [한국어](README.ko.md)

An Editor extension that automatically displays the Unity Game view across the entire monitor when you enter Play Mode on Windows. Press F12 during Play Mode to switch between fullscreen and the normal Editor layout.

## Features

- Automatic fullscreen when entering Play Mode
- F12 to toggle fullscreen and the normal Editor layout
- Hidden Game tab header and toolbar
- Alignment based on the actual rendered viewport to prevent clipping at the bottom
- Restoration of the window size, position, and tab state when leaving Play Mode or before script recompilation
- No repeated toggling when F12 is held down
- F12 input from other applications is ignored
- Editor-only functionality with no scene or prefab components to add

## Supported environment

| Item | Details |
| --- | --- |
| Operating system | Windows |
| Validated Unity version | 6000.3.22f1 |
| Validated resolution | 3840 × 2160 |
| Execution scope | Play Mode in the Unity Editor |
| Additional packages | None |
| Architecture | Feature-specific MVC and an independent Editor assembly |

This extension uses internal Unity GameView APIs. Compatibility with other Unity versions needs verification. It does not change fullscreen settings in built games.

## Installation

### Install from a unitypackage

1. Download [GameViewFullscreen.unitypackage](Releases/GameViewFullscreen.unitypackage).
2. In the target Unity project, select `Assets > Import Package > Custom Package...`.
3. Select the downloaded package and import all included files.
4. Once compilation finishes, dock a Game tab in the main Editor window.

### Install from source

Copy this repository's `Assets/Editor/GameViewFullscreen` folder and `GameViewFullscreen.meta` into the target project's `Assets/Editor/` directory. Include the `.meta` files, settings table, string table, and `.asmdef` inside the folder.

Settings and strings are loaded from `Assets/Editor/GameViewFullscreen/`, so keep that installation path unchanged.

## Usage

1. Dock a Game tab in the main Editor window and press Play.
2. The game fills the monitor.
3. Press F12 to return to the normal Editor layout.
4. Press F12 again to enter fullscreen.
5. Stop Play Mode to restore the original window and tab state.

You can also toggle through `테스트 도구 > 게임 화면 전체화면 전환` during Play Mode. This Korean menu label means `Test Tools > Toggle Game View Fullscreen`. Press F12 to return to the normal layout when you need other Editor tools or the Play button.

## Open as a standalone Unity project

Add this repository folder to Unity Hub and open it with Unity 6000.3.22f1. The repository includes `ProjectSettings/ProjectVersion.txt` and a minimal `Packages/manifest.json`.

This project is for maintaining the tool's source and does not include a sample game scene. Verify game behavior in a Play Mode scene of the project where you install the extension.

## Repository layout

```text
GameViewFullscreen/
├── Assets/
│   └── Editor/
│       ├── GameViewFullscreen/      # Distributed Editor feature and data
│       └── Distribution/            # unitypackage export utility
├── Documentation/
│   └── VALIDATION.md               # Validation results and manual checks
├── Packages/
│   └── manifest.json               # Minimal built-in Unity modules
├── ProjectSettings/
│   └── ProjectVersion.txt          # Reference Unity version
├── Releases/
│   └── GameViewFullscreen.unitypackage
├── Tests/
│   └── Editor/
│       └── ValidateGameViewFullscreen.cs
├── .gitattributes
├── .gitignore
├── README.md
└── README.ko.md
```

### Code responsibilities

| File | Responsibility |
| --- | --- |
| `GameViewFullscreenController.cs` | Play Mode events, F12 input, fullscreen transitions, and restoration |
| `GameViewFullscreenModel.cs` | Fullscreen state and the state to restore |
| `GameViewFullscreenView.cs` | Existing Game view discovery, rendered viewport calculation, and presentation |
| `GameViewFullscreenNative.cs` | Windows window position, size, and visible region handling |
| `GameViewFullscreenSettings.cs` | Settings data loading and validation |
| `GameViewFullscreenStringTable.cs` | Internal Korean and English string handling |
| `UltimateTrpgSimulator.Editor.GameViewFullscreen.asmdef` | Independent Editor assembly with no runtime project references |

The namespace and assembly name are retained for compatibility with the existing distribution package. The extension has no dependency on the original game's runtime code.

## Configuration

Edit numerical settings in `Assets/Editor/GameViewFullscreen/GameViewFullscreenSettingsTable.json`.

| Setting | Default | Description |
| --- | --- | --- |
| `toggleVirtualKey` | 123 | Windows virtual-key code; the default is F12 |
| `layoutWaitUpdates` | 2 | Editor updates to wait before reading the rendered viewport |
| `maximumAlignmentAttempts` | 4 | Maximum monitor alignment attempts |
| `pixelTolerance` | 2 | Allowed pixel error when checking monitor alignment |
| `memo` | Korean note | Purpose of the settings and reasons for changes |

Configuration changes take effect after script recompilation or an Editor restart.

`GameViewFullscreenStringTable.txt` uses tab-separated `key`, `ko`, `en`, and `description` columns. Add Korean and English strings together, and write descriptions in Korean. The current Editor menu and messages use Korean.

## Rebuild the package

Open this repository as a Unity project. In the Project window, select `Assets/Editor/GameViewFullscreen`, then choose `Assets > Export Package...`. Disable `Include Dependencies`, include all files in the feature folder, and save the package as `Releases/GameViewFullscreen.unitypackage`.

For automated export, pass the following method to Unity's `-executeMethod` option:

```text
GameViewFullscreen.Distribution.GameViewFullscreenPackageExporter.Export
```

The exporter includes only the feature folder and writes the result to `Releases/GameViewFullscreen.unitypackage`.

## Validation

Independent Editor assembly compilation, rendered viewport alignment, window and tab restoration, key input state handling, foreground focus preservation, and scene state preservation were verified in the original development environment. The distributed package's code, data, and `.meta` files were also checked against the source.

See the [validation record](Documentation/VALIDATION.md) for details and manual verification steps. That record is currently in Korean. `Tests/Editor/ValidateGameViewFullscreen.cs` is a validation script for Unity Pipeline's `run_script`, using the entry point `ValidateGameViewFullscreen.Main`. It expands and restores the existing Game view while preserving the current scene and without forcing the Editor to the foreground.

Opening the new standalone project in the Editor and compatibility with other Unity versions have not yet been verified.
