using System;
using System.Reflection;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class ValidateGameViewFullscreen
{
    private const BindingFlags Members = BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

    public static async Task<string> Main()
    {
        Assembly assembly = Assembly.Load("UltimateTrpgSimulator.Editor.GameViewFullscreen");
        Type modelType = assembly.GetType("UltimateTrpgSimulator.Editor.GameViewFullscreen.GameViewFullscreenModel", true);
        Type viewType = assembly.GetType("UltimateTrpgSimulator.Editor.GameViewFullscreen.GameViewFullscreenView", true);
        Type nativeType = assembly.GetType("UltimateTrpgSimulator.Editor.GameViewFullscreen.GameViewFullscreenNative", true);
        object model = Activator.CreateInstance(modelType, true);
        var readPress = modelType.GetMethod("ReadTogglePress", Members);
        Assert((bool)readPress.Invoke(model, new object[] { true, true }), "첫 F12 입력");
        Assert(!(bool)readPress.Invoke(model, new object[] { true, true }), "F12 반복 입력 차단");
        Assert(!(bool)readPress.Invoke(model, new object[] { false, true }), "키 해제");
        Assert(!(bool)readPress.Invoke(model, new object[] { true, false }), "다른 앱에서 누른 F12 무시");
        Assert(!(bool)readPress.Invoke(model, new object[] { true, true }), "누른 상태로 포커스 복귀 시 무시");
        readPress.Invoke(model, new object[] { false, true });
        Assert((bool)readPress.Invoke(model, new object[] { true, true }), "새 F12 입력");

        var fit = modelType.GetMethod("ExpandWindowToFit", Members);
        var fitted = (Rect)fit.Invoke(null, new object[] {
            new Rect(-1920, 0, 1920, 1080), new Rect(-1920, 80, 1920, 960), new Rect(-1920, 0, 1920, 1080) });
        Assert(fitted == new Rect(-1920, -80, 1920, 1200), "보조 모니터의 음수 좌표와 여백 계산");

        if (EditorApplication.isPlaying)
            throw new InvalidOperationException("플레이를 멈춘 상태에서 검증해 주세요.");
        object foreground = nativeType.GetProperty("ForegroundWindow", Members).GetValue(null);
        var view = (EditorWindow)viewType.GetMethod("FindGameView", Members).Invoke(null, new[] { model });
        Rect originalViewBounds = view.position;
        bool originalMaximized = view.maximized;
        object handle = nativeType.GetProperty("MainWindowHandle", Members).GetValue(null);
        var readBounds = nativeType.GetMethod("ReadBounds", Members);
        Rect originalWindowBounds = (Rect)readBounds.Invoke(null, new[] { handle });
        var originalScene = EditorSceneManager.GetActiveScene();
        bool originalDirty = originalScene.isDirty;
        var toolbar = view.GetType().GetProperty("showToolbar", Members);
        bool originalToolbar = (bool)toolbar.GetValue(view);
        Rect fullscreenBounds = default;
        Rect monitorBounds = default;
        string alignmentTrace = string.Empty;
        try
        {
            viewType.GetMethod("Enter", Members).Invoke(null, new[] { model });
            bool aligned = false;
            for (int attempt = 0; attempt < 4; attempt++)
            {
                await WaitForEditorUpdates(3);
                object activeSnapshot = modelType.GetProperty("Window", Members).GetValue(model);
                alignmentTrace += $"\n{attempt}: game={viewType.GetMethod("GetGameBounds", Members).Invoke(null, new[] { model })}; outer={readBounds.Invoke(null, new[] { handle })}; monitor={activeSnapshot.GetType().GetField("Monitor", Members).GetValue(activeSnapshot)}";
                aligned = (bool)viewType.GetMethod("Align", Members).Invoke(null, new[] { model });
                if (aligned)
                    break;
            }
            Assert(aligned, "모니터 전체 영역 정렬" + alignmentTrace);
            await WaitForEditorUpdates(3);
            fullscreenBounds = GetRenderedViewport(view);
            object snapshot = modelType.GetProperty("Window", Members).GetValue(model);
            monitorBounds = (Rect)snapshot.GetType().GetField("Monitor", Members).GetValue(snapshot);
            Assert(Approximately(fullscreenBounds, monitorBounds), $"탭 헤더를 제외한 실제 렌더 영역 일치: render={fullscreenBounds}, monitor={monitorBounds}");
            AssertImageFitsViewport(view, monitorBounds);
            Assert(foreground.Equals(nativeType.GetProperty("ForegroundWindow", Members).GetValue(null)), "전체화면 진입 시 다른 앱의 포커스 유지");
        }
        finally
        {
            viewType.GetMethod("Restore", Members).Invoke(null, new[] { model });
        }
        await WaitForEditorUpdates(3);
        Assert(Approximately(originalWindowBounds, (Rect)readBounds.Invoke(null, new[] { handle })), "원래 창 위치 및 크기 복원");
        Assert(originalMaximized == view.maximized, "원래 탭 최대화 상태 복원");
        Assert(originalToolbar == (bool)toolbar.GetValue(view), "원래 게임 툴바 상태 복원");
        Assert(Approximately(originalViewBounds, view.position), "원래 Game 뷰 영역 복원");
        Assert(foreground.Equals(nativeType.GetProperty("ForegroundWindow", Members).GetValue(null)), "복원 시 다른 앱의 포커스 유지");
        Assert(originalScene == EditorSceneManager.GetActiveScene() && originalDirty == originalScene.isDirty, "현재 씬과 저장 상태 유지");
        // 실제 기본 탭 배치에서도 최대화와 복원이 가능한지 확인한 뒤 사용자의 상태로 되돌립니다.
        try
        {
            view.maximized = false;
            await WaitForEditorUpdates(3);
            Rect dockedBounds = view.position;
            try
            {
                viewType.GetMethod("Enter", Members).Invoke(null, new[] { model });
                bool aligned = false;
                for (int attempt = 0; attempt < 4; attempt++)
                {
                    await WaitForEditorUpdates(3);
                    if ((bool)viewType.GetMethod("Align", Members).Invoke(null, new[] { model }))
                    {
                        aligned = true;
                        break;
                    }
                }
                Assert(aligned, "기본 탭 배치에서 전체화면 정렬");
                await WaitForEditorUpdates(3);
                Assert(Approximately(GetRenderedViewport(view), monitorBounds), "기본 탭의 실제 렌더 영역 일치");
                AssertImageFitsViewport(view, monitorBounds);
            }
            finally
            {
                viewType.GetMethod("Restore", Members).Invoke(null, new[] { model });
            }
            await WaitForEditorUpdates(3);
            Assert(!view.maximized && Approximately(dockedBounds, view.position), "기본 탭 배치 복원");
            Assert(originalToolbar == (bool)toolbar.GetValue(view), "기본 탭 툴바 복원");
            Assert(foreground.Equals(nativeType.GetProperty("ForegroundWindow", Members).GetValue(null)), "기본 탭 전환 시 포커스 유지");
        }
        finally
        {
            view.maximized = originalMaximized;
        }
        return $"PASS: F12 입력/보조 모니터 좌표/전체화면 정렬/창 및 탭 복원/포커스 유지/씬 보존. Game={fullscreenBounds}, Monitor={monitorBounds}";
    }

    private static Task WaitForEditorUpdates(int count)
    {
        var completion = new TaskCompletionSource<bool>();
        void Update()
        {
            if (--count > 0)
                return;
            EditorApplication.update -= Update;
            completion.SetResult(true);
        }
        EditorApplication.update += Update;
        return completion.Task;
    }

    private static Rect GetRenderedViewport(EditorWindow view)
    {
        object parent = typeof(EditorWindow).GetField("m_Parent", Members).GetValue(view);
        Rect screen = (Rect)parent.GetType().GetProperty("screenPosition", Members).GetValue(parent);
        Rect viewport = (Rect)typeof(EditorWindow).GetField("m_GameViewClippedRect", Members).GetValue(view);
        float scale = (float)view.GetType().GetProperty("backingScale", Members).GetValue(view);
        return new Rect((screen.position + viewport.position) * scale, viewport.size * scale);
    }

    private static void AssertImageFitsViewport(EditorWindow view, Rect monitor)
    {
        object parent = typeof(EditorWindow).GetField("m_Parent", Members).GetValue(view);
        Rect screen = (Rect)parent.GetType().GetProperty("screenPosition", Members).GetValue(parent);
        Rect image = (Rect)typeof(EditorWindow).GetField("m_GameViewRect", Members).GetValue(view);
        float scale = (float)view.GetType().GetProperty("backingScale", Members).GetValue(view);
        image = new Rect((screen.position + image.position) * scale, image.size * scale);
        Assert(image.xMin >= monitor.xMin - 2 && image.yMin >= monitor.yMin - 2
            && image.xMax <= monitor.xMax + 2 && image.yMax <= monitor.yMax + 2,
            $"게임 이미지의 상하좌우가 모니터 밖으로 잘리지 않아야 합니다: image={image}, monitor={monitor}");
        Assert((bool)view.GetType().GetProperty("showToolbar", Members).GetValue(view) == false, "게임 툴바 숨김 유지");
    }

    private static bool Approximately(Rect first, Rect second) =>
        Mathf.Abs(first.x - second.x) <= 2 && Mathf.Abs(first.y - second.y) <= 2
        && Mathf.Abs(first.width - second.width) <= 2 && Mathf.Abs(first.height - second.height) <= 2;

    private static void Assert(bool condition, string message)
    {
        if (!condition)
            throw new InvalidOperationException(message);
    }
}
