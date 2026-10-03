# Game View Fullscreen

English | [한국어](README.ko.md)

An Editor extension that displays the existing Game view across the entire monitor when you enter Play Mode. Press F12 to switch between fullscreen and the normal Editor layout. It hides the Game tab header and toolbar and aligns the actual rendered viewport to prevent clipping at the bottom.

## Installation and usage

1. In the target Unity project, select `Assets > Import Package > Custom Package...`.
2. Select `GameViewFullscreen.unitypackage` and import all included files.
3. Once compilation finishes, dock a Game tab in the main Editor window.
4. Press Play to enter fullscreen automatically.
5. Press F12 to return to the normal layout. Press it again to enter fullscreen.
6. Stop Play Mode to restore the original window position, size, Game tab state, and toolbar.

You can also toggle through `테스트 도구/게임 화면 전체화면 전환`, the Korean menu label for `Test Tools/Toggle Game View Fullscreen`. No scene or prefab components and no additional Unity packages are required.

## Supported environment

- Windows Unity Editor only. The tool is not included in built games.
- Independent assembly compilation, actual 3840 × 2160 rendered viewport alignment, and restoration of the original window and tab state were verified with Unity 6000.3.22f1.
- This tool uses internal Unity GameView APIs. Other Unity versions require compatibility checks because the APIs and layouts may differ.
- A Game view in a separate floating window must be docked in the main Editor window.
- Settings and string tables are loaded from `Assets/Editor/GameViewFullscreen/`. Keep this folder path unchanged.

## Components

- Controller: manages Play Mode events, F12 input, and fullscreen transitions.
- Model: manages fullscreen state and the state to restore.
- View: inspects the existing Game view's rendered viewport and displays fullscreen.
- Native: changes Windows window size, position, and visible region without taking foreground focus from another application.
- Settings: loads the settings table.
- StringTable: reads Korean and English strings within the feature, without a dependency on project-wide localization code.
- asmdef: compiles the feature as an Editor-only assembly with no runtime project references.

The `toggleVirtualKey` setting in `GameViewFullscreenSettingsTable.json` is a Windows virtual-key code. Its default value, 123, selects F12. `layoutWaitUpdates`, `maximumAlignmentAttempts`, and `pixelTolerance` configure viewport alignment. Write settings notes in Korean.

`GameViewFullscreenStringTable.txt` uses tab-separated `key`, `ko`, `en`, and `description` columns. Maintain Korean and English strings together. The current Editor messages use Korean.
