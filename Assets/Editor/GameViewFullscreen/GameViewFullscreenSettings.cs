#if UNITY_EDITOR_WIN
using System;
using UnityEditor;
using UnityEngine;

namespace UltimateTrpgSimulator.Editor.GameViewFullscreen
{
    [Serializable]
    internal sealed class GameViewFullscreenSettings
    {
        private const string TablePath = "Assets/Editor/GameViewFullscreen/GameViewFullscreenSettingsTable.json";

        public string memo;
        public int toggleVirtualKey;
        public int layoutWaitUpdates;
        public int maximumAlignmentAttempts;
        public int pixelTolerance;

        internal static GameViewFullscreenSettings Load()
        {
            TextAsset table = AssetDatabase.LoadAssetAtPath<TextAsset>(TablePath);
            if (table == null)
                throw new InvalidOperationException(TablePath);

            var settings = JsonUtility.FromJson<GameViewFullscreenSettings>(table.text);
            if (settings == null || settings.toggleVirtualKey <= 0 || settings.layoutWaitUpdates < 1
                || settings.maximumAlignmentAttempts < 1 || settings.pixelTolerance < 0)
                throw new InvalidOperationException(TablePath);
            return settings;
        }
    }
}
#endif
