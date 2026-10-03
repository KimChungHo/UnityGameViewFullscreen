#if UNITY_EDITOR_WIN
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace UltimateTrpgSimulator.Editor.GameViewFullscreen
{
    internal static class GameViewFullscreenView
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Type GameViewType = typeof(EditorWindow).Assembly.GetType("UnityEditor.GameView");
        private static readonly PropertyInfo Toolbar = GameViewType?.GetProperty("showToolbar", Members);
        private static readonly FieldInfo Parent = typeof(EditorWindow).GetField("m_Parent", Members);
        private static readonly FieldInfo RenderedViewport = typeof(EditorWindow).GetField("m_GameViewClippedRect", Members);
        private static readonly PropertyInfo ParentScreenPosition = typeof(EditorWindow).Assembly
            .GetType("UnityEditor.GUIView")?.GetProperty("screenPosition", Members);

        // MenuItem 경로는 컴파일 상수여야 하므로 문자열 테이블의 한국어 경로와 맞춥니다.
        [MenuItem("테스트 도구/게임 화면 전체화면 전환")]
        private static void ToggleMenu() => GameViewFullscreenController.Toggle();

        [MenuItem("테스트 도구/게임 화면 전체화면 전환", true)]
        private static bool ValidateToggleMenu() => EditorApplication.isPlaying && !EditorApplication.isCompiling;

        internal static EditorWindow FindGameView(GameViewFullscreenModel model)
        {
            if (GameViewType == null || Toolbar == null || Parent == null
                || ParentScreenPosition == null || RenderedViewport == null)
                throw new NotSupportedException(model.GetText("fullscreen.unsupported"));

            // 기존 메인 창의 Game 탭만 사용하며 EditorWindow/씬 오브젝트를 새로 만들지 않습니다.
            EditorWindow fallback = null;
            foreach (UnityEngine.Object candidate in Resources.FindObjectsOfTypeAll(GameViewType))
            {
                var view = (EditorWindow)candidate;
                object parent = Parent.GetValue(view);
                object container = parent?.GetType().GetProperty("window", Members)?.GetValue(parent);
                var isMain = container?.GetType().GetMethod("IsMainWindow", Members);
                if (isMain != null && (bool)isMain.Invoke(container, null))
                {
                    fallback ??= view;
                    // 여러 Game 탭이 있을 때 실제 표시되는 탭의 레이아웃을 사용합니다.
                    if (parent.GetType().GetProperty("actualView", Members)?.GetValue(parent) == view)
                        return view;
                }
            }
            if (fallback != null)
                return fallback;
            throw new InvalidOperationException(model.GetText("fullscreen.no_game_view"));
        }

        internal static void Enter(GameViewFullscreenModel model)
        {
            model.GameView = FindGameView(model);
            model.WasMaximized = model.GameView.maximized;
            model.HadToolbar = (bool)Toolbar.GetValue(model.GameView);
            model.Window = GameViewFullscreenNative.Capture();
            model.GameView.maximized = true;
            Toolbar.SetValue(model.GameView, false);
            GameViewFullscreenNative.MakeBorderless(model.Window);
            model.WaitUpdates = model.Settings.layoutWaitUpdates;
            model.AlignmentAttempts = 0;
            model.GameView.Repaint();
        }

        internal static Rect GetGameBounds(GameViewFullscreenModel model)
        {
            object parent = Parent.GetValue(model.GameView);
            Rect host = (Rect)ParentScreenPosition.GetValue(parent);
            Rect content = (Rect)RenderedViewport.GetValue(model.GameView);
            // EditorWindow.position은 HostView의 탭 헤더 여백을 포함하지 않습니다.
            // Unity가 마지막 렌더에서 기록한 게임 출력 영역을 화면 좌표로 변환해야
            // 상단 Game 탭이 화면 밖으로 나가고 하단 게임 화면도 잘리지 않습니다.
            if (content.width <= 0 || content.height <= 0)
                throw new InvalidOperationException(model.GetText("fullscreen.alignment_failed"));
            Rect points = new Rect(host.position + content.position, content.size);
            float scale = GameViewFullscreenNative.GetScale(model.Window.Handle);
            return new Rect(points.position * scale, points.size * scale);
        }

        internal static bool Align(GameViewFullscreenModel model)
        {
            if (model.GameView == null || !model.GameView.maximized)
                throw new InvalidOperationException(model.GetText("fullscreen.alignment_failed"));
            Rect game = GetGameBounds(model);
            Rect monitor = model.Window.Monitor;
            int tolerance = model.Settings.pixelTolerance;
            if (Mathf.Abs(game.x - monitor.x) <= tolerance && Mathf.Abs(game.y - monitor.y) <= tolerance
                && Mathf.Abs(game.width - monitor.width) <= tolerance && Mathf.Abs(game.height - monitor.height) <= tolerance)
            {
                GameViewFullscreenNative.CropToMonitor(model.Window);
                return true;
            }

            Rect outer = GameViewFullscreenNative.ReadBounds(model.Window.Handle);
            GameViewFullscreenNative.Move(model.Window.Handle,
                GameViewFullscreenModel.ExpandWindowToFit(outer, game, monitor));
            GameViewFullscreenNative.CropToMonitor(model.Window);
            model.GameView.Repaint();
            return false;
        }

        internal static void Restore(GameViewFullscreenModel model)
        {
            try
            {
                if (model.Window != null)
                    GameViewFullscreenNative.Restore(model.Window);
            }
            finally
            {
                model.Window = null;
                if (model.GameView != null)
                {
                    Toolbar.SetValue(model.GameView, model.HadToolbar);
                    model.GameView.maximized = model.WasMaximized;
                    model.GameView.Repaint();
                }
                model.GameView = null;
            }
        }

        internal static void ShowError(GameViewFullscreenModel model, Exception exception) =>
            Debug.LogError(string.Format(model.GetText("fullscreen.failed"), exception.Message));
    }
}
#endif
