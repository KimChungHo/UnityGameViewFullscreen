#if UNITY_EDITOR_WIN
using System;
using UnityEditor;
using UnityEngine;

namespace UltimateTrpgSimulator.Editor.GameViewFullscreen
{
    internal sealed class GameViewFullscreenModel
    {
        private const string StringTablePath = "Assets/Editor/GameViewFullscreen/GameViewFullscreenStringTable.txt";
        private readonly GameViewFullscreenStringTable strings;

        internal GameViewFullscreenModel()
        {
            Settings = GameViewFullscreenSettings.Load();
            strings = new GameViewFullscreenStringTable(AssetDatabase.LoadAssetAtPath<TextAsset>(StringTablePath),
                SystemLanguage.Korean);
        }

        internal GameViewFullscreenSettings Settings { get; }
        internal EditorWindow GameView { get; set; }
        internal bool WasMaximized { get; set; }
        internal bool HadToolbar { get; set; }
        internal GameViewFullscreenNative.WindowSnapshot Window { get; set; }
        internal bool IsFullscreen => Window != null;
        internal int WaitUpdates { get; set; }
        internal int AlignmentAttempts { get; set; }
        internal bool ToggleKeyWasDown { get; private set; }
        internal string GetText(string key) => strings.Get(key);

        internal bool ReadTogglePress(bool isDown, bool editorHasForeground)
        {
            bool pressed = isDown && !ToggleKeyWasDown && editorHasForeground;
            ToggleKeyWasDown = isDown;
            return pressed;
        }

        internal static Rect ExpandWindowToFit(Rect window, Rect gameView, Rect monitor)
        {
            return new Rect(window.x + monitor.x - gameView.x,
                window.y + monitor.y - gameView.y,
                window.width + monitor.width - gameView.width,
                window.height + monitor.height - gameView.height);
        }
    }
}
#endif
