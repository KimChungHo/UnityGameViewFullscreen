# 게임 뷰 전체화면

Windows Unity 에디터에서 플레이를 시작하면 기존 메인 에디터 창의 Game 탭을 모니터 전체 크기로 표시합니다. 플레이 중 F12를 누르면 기본 화면과 전체화면 사이를 전환합니다. 플레이를 종료하면 창 위치, 크기, Game 탭 최대화 상태와 툴바를 복원합니다.

Game 탭은 기본 에디터 창에 있어야 합니다. 분리된 Game 창만 있는 경우 기본 창에 Game 탭을 도킹해 주세요. 전환 메뉴는 `테스트 도구/게임 화면 전체화면 전환`입니다.

에디터 전용 MVC 코드는 `Assets/Editor/GameViewFullscreen/`에 있습니다. 설정 수치는 `GameViewFullscreenSettingsTable.json`, 한국어와 영어 안내 문구는 `GameViewFullscreenStringTable.txt`에서 관리합니다. 씬이나 프리팹에 컴포넌트를 추가할 필요가 없습니다.

## 자동 검증 결과

Unity 6000.3.22f1에서 컴파일 오류가 없으며 `ValidateGameViewFullscreen.Main` 검증이 통과했습니다.

- 실제 Game 뷰 영역과 3840 × 2160 모니터 영역의 픽셀 좌표와 크기가 일치합니다.
- 에디터 창 크기가 아닌 Unity가 마지막 렌더에서 기록한 게임 출력 영역을 검사합니다. 기존 구현에서 출력 시작점이 아래로 36픽셀 밀리는 현상을 재현했고, 수정 후 출력 시작점은 (0, 0), 끝점은 (3840, 2160)입니다.
- Game 탭 헤더를 제외한 출력 영역을 모니터에 맞추며 실제 게임 이미지의 상하좌우가 화면 밖으로 잘리지 않는지 확인합니다. 추가 레이아웃 갱신 이후에도 같은 상태를 유지합니다.
- 최대화된 탭과 기본 도킹 배치 모두 전체화면 전환 후 원래 상태로 복원됩니다.
- 게임 툴바와 에디터 창 위치 및 크기가 복원됩니다.
- F12를 누르고 있는 동안 반복 전환을 차단하며 다른 앱에서 시작한 키 입력을 무시합니다.
- 전환과 복원 중 다른 앱의 포커스를 유지합니다.
- 현재 씬과 저장되지 않은 변경 상태가 유지됩니다.
- 보조 모니터의 음수 좌표를 포함한 창 확장 계산이 통과했습니다.

자동 검증은 플레이 모드에 진입하지 않고 기존 에디터 창을 확장 및 복원했습니다. 실제 Play 버튼, 물리 F12 입력, 플레이 중 재컴파일 및 종료 이벤트의 전체 연결 동작은 다음 수동 절차로 확인할 수 있습니다.

1. Game 탭을 기본 에디터 창에 도킹하고 Play 버튼을 누릅니다.
2. 게임 화면이 모니터 전체에 표시되는지 확인합니다.
3. F12를 눌러 기본 화면으로 돌아오고 다시 눌러 전체화면으로 전환합니다.
4. F12를 길게 눌러 한 번만 전환되는지 확인합니다.
5. 기본 화면으로 돌아와 플레이를 종료하고 원래 배치가 복원되는지 확인합니다.

## 구현 참고

- Unity 내부 GameView API 의존성은 View에 한정합니다. [Unity 6000.3 GameView 소스](https://github.com/Unity-Technologies/UnityCsReference/blob/6000.3/Editor/Mono/GameView/GameView.cs)
- Windows 창 변경은 포커스와 창 순서를 유지하는 SetWindowPos 플래그를 사용합니다. [Microsoft SetWindowPos](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setwindowpos)
- F12는 현재 키 상태의 눌림 경계를 확인하므로 Game 뷰에서 Unity 단축키를 꺼도 처리됩니다. [Microsoft GetAsyncKeyState](https://learn.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-getasynckeystate)
