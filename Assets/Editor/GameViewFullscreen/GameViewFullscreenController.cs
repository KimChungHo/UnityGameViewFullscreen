#if UNITY_EDITOR_WIN
using System;
using UnityEditor;

namespace UltimateTrpgSimulator.Editor.GameViewFullscreen
{
    [InitializeOnLoad]
    internal static class GameViewFullscreenController
    {
        private const string DesiredKey = "UltimateTrpgSimulator.GameViewFullscreen.Desired";
        private static GameViewFullscreenModel model;
        private static bool enterPending;

        static GameViewFullscreenController()
        {
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.update += Update;
            AssemblyReloadEvents.beforeAssemblyReload += BeforeReload;
            EditorApplication.quitting += OnQuit;
            enterPending = EditorApplication.isPlaying && SessionState.GetBool(DesiredKey, false);
        }

        internal static bool IsFullscreen => model != null && model.IsFullscreen;

        internal static void Toggle()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isCompiling
                || EditorApplication.isPlaying != EditorApplication.isPlayingOrWillChangePlaymode)
                return;
            enterPending = false;
            SessionState.SetBool(DesiredKey, !IsFullscreen);
            if (IsFullscreen)
                Restore();
            else
                Enter();
        }

        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SessionState.SetBool(DesiredKey, true);
                enterPending = true;
            }
            else if (state == PlayModeStateChange.ExitingPlayMode || state == PlayModeStateChange.EnteredEditMode)
            {
                enterPending = false;
                SessionState.SetBool(DesiredKey, false);
                Restore();
            }
        }

        private static void Update()
        {
            if (EditorApplication.isCompiling || EditorApplication.isUpdating)
                return;
            if (enterPending && EditorApplication.isPlaying)
            {
                enterPending = false;
                Enter();
            }
            if (model == null)
                return;

            if (IsFullscreen && model.WaitUpdates > 0 && --model.WaitUpdates == 0)
            {
                try
                {
                    if (!GameViewFullscreenView.Align(model))
                    {
                        if (++model.AlignmentAttempts >= model.Settings.maximumAlignmentAttempts)
                            throw new InvalidOperationException(model.GetText("fullscreen.alignment_failed"));
                        model.WaitUpdates = model.Settings.layoutWaitUpdates;
                    }
                }
                catch (Exception exception)
                {
                    GameViewFullscreenView.ShowError(model, exception);
                    SessionState.SetBool(DesiredKey, false);
                    Restore();
                }
            }

            if (!EditorApplication.isPlaying)
                return;
            IntPtr handle = model.Window?.Handle ?? GameViewFullscreenNative.MainWindowHandle;
            if (model.ReadTogglePress(GameViewFullscreenNative.IsKeyDown(model.Settings.toggleVirtualKey),
                GameViewFullscreenNative.HasForeground(handle)))
                Toggle();
        }

        private static void Enter()
        {
            if (IsFullscreen)
                return;
            try
            {
                model ??= new GameViewFullscreenModel();
                GameViewFullscreenView.Enter(model);
            }
            catch (Exception exception)
            {
                if (model != null)
                    GameViewFullscreenView.ShowError(model, exception);
                else
                    UnityEngine.Debug.LogException(exception);
                SessionState.SetBool(DesiredKey, false);
                Restore();
            }
        }

        private static void Restore()
        {
            if (model == null)
                return;
            try
            {
                GameViewFullscreenView.Restore(model);
            }
            catch (Exception exception)
            {
                GameViewFullscreenView.ShowError(model, exception);
            }
        }

        private static void BeforeReload()
        {
            enterPending = false;
            Restore();
        }

        private static void OnQuit()
        {
            SessionState.SetBool(DesiredKey, false);
            Restore();
        }
    }
}
#endif
