# Game View Fullscreen

Windows Unity 에디터에서 Play를 시작할 때 Game 뷰를 모니터 전체화면으로 표시하는 에디터 확장입니다. 플레이 중 F12로 기본 화면과 전체화면을 전환할 수 있습니다.

## 주요 기능

- Play 시작 시 자동 전체화면
- F12로 전체화면 및 기본 화면 전환
- Game 탭 헤더와 툴바 숨김
- 실제 렌더 영역 기준 정렬로 하단 잘림 방지
- Play 종료 및 스크립트 재컴파일 전 창 크기, 위치와 탭 상태 복원
- F12를 길게 눌렀을 때 반복 전환 차단
- 다른 앱에서 누른 F12 입력 무시
- 씬 및 프리팹에 컴포넌트를 추가하지 않는 에디터 전용 기능

## 지원 환경

| 항목 | 내용 |
| --- | --- |
| 운영체제 | Windows |
| 검증한 Unity 버전 | 6000.3.22f1 |
| 검증한 해상도 | 3840 × 2160 |
| 실행 범위 | Unity 에디터의 Play Mode |
| 추가 패키지 | 없음 |
| 구조 | 기능별 MVC, 독립 에디터 어셈블리 |

Unity 내부 GameView API를 사용하므로 다른 Unity 버전에서는 호환성 확인이 필요합니다. 빌드된 게임의 전체화면 설정에는 영향을 주지 않습니다.

## 설치

### unitypackage로 설치

1. [GameViewFullscreen.unitypackage](Releases/GameViewFullscreen.unitypackage)를 내려받습니다.
2. 대상 Unity 프로젝트에서 `Assets > Import Package > Custom Package...`를 선택합니다.
3. 내려받은 파일을 선택하고 포함된 파일을 모두 가져옵니다.
4. 컴파일이 끝나면 Game 탭을 기본 에디터 창에 도킹합니다.

### 소스 파일로 설치

이 저장소의 `Assets/Editor/GameViewFullscreen` 폴더와 `GameViewFullscreen.meta` 파일을 대상 프로젝트의 `Assets/Editor/`에 복사합니다. 폴더 안의 `.meta`, 설정 테이블, 문자열 테이블, `.asmdef`도 함께 복사해 주세요.

설정과 문자열 파일은 `Assets/Editor/GameViewFullscreen/`에서 읽으므로 설치 경로를 유지해 주세요.

## 사용

1. Game 탭이 기본 에디터 창에 도킹된 상태에서 Play를 누릅니다.
2. 게임 화면이 모니터 전체에 표시됩니다.
3. F12를 누르면 기본 에디터 화면으로 돌아옵니다.
4. 다시 F12를 누르면 전체화면으로 전환합니다.
5. Play를 종료하면 원래 창과 탭 상태로 복원됩니다.

메뉴로 전환하려면 플레이 중 `테스트 도구 > 게임 화면 전체화면 전환`을 선택합니다. 다른 에디터 도구를 사용하거나 Play 버튼을 다시 누를 때는 F12로 기본 화면으로 돌아오시면 됩니다.

## 독립 Unity 프로젝트로 열기

이 저장소 폴더를 Unity Hub의 프로젝트 추가 기능으로 등록하고 Unity 6000.3.22f1로 열 수 있습니다. `ProjectSettings/ProjectVersion.txt`와 최소 `Packages/manifest.json`을 포함합니다.

도구 소스를 관리하기 위한 프로젝트이며 샘플 게임 씬은 포함하지 않습니다. 전체화면의 게임 동작은 설치 대상 프로젝트의 플레이 씬에서 확인해 주세요.

## 저장소 구성

```text
GameViewFullscreen/
├── Assets/
│   └── Editor/
│       ├── GameViewFullscreen/      # 배포할 에디터 기능과 데이터
│       └── Distribution/            # unitypackage 내보내기 도구
├── Documentation/
│   └── VALIDATION.md               # 검증 결과와 수동 검증 절차
├── Packages/
│   └── manifest.json               # 최소 Unity 내장 모듈
├── ProjectSettings/
│   └── ProjectVersion.txt          # 기준 Unity 버전
├── Releases/
│   └── GameViewFullscreen.unitypackage
├── Tests/
│   └── Editor/
│       └── ValidateGameViewFullscreen.cs
├── .gitattributes
├── .gitignore
└── README.md
```

### 코드 역할

| 파일 | 역할 |
| --- | --- |
| `GameViewFullscreenController.cs` | Play Mode 이벤트, F12 입력, 전환 및 복원 흐름 |
| `GameViewFullscreenModel.cs` | 전체화면 상태와 복원할 상태 |
| `GameViewFullscreenView.cs` | 기존 Game 뷰 탐색, 실제 렌더 영역 계산과 표시 |
| `GameViewFullscreenNative.cs` | Windows 창 위치, 크기 및 표시 영역 처리 |
| `GameViewFullscreenSettings.cs` | 설정 데이터 로드와 유효성 검사 |
| `GameViewFullscreenStringTable.cs` | 기능 내부의 한국어 및 영어 문자열 처리 |
| `UltimateTrpgSimulator.Editor.GameViewFullscreen.asmdef` | 런타임 코드 참조 없는 독립 에디터 어셈블리 |

네임스페이스와 어셈블리 이름은 기존 배포 패키지와의 호환성을 위해 유지합니다. 원본 게임 프로젝트의 런타임 코드에는 의존하지 않습니다.

## 설정

`Assets/Editor/GameViewFullscreen/GameViewFullscreenSettingsTable.json`에서 수치를 변경할 수 있습니다.

| 항목 | 기본값 | 설명 |
| --- | --- | --- |
| `toggleVirtualKey` | 123 | Windows 가상 키 코드, 기본값은 F12 |
| `layoutWaitUpdates` | 2 | 렌더 영역을 읽기 전 기다리는 에디터 갱신 횟수 |
| `maximumAlignmentAttempts` | 4 | 모니터 정렬 재시도 한도 |
| `pixelTolerance` | 2 | 모니터 정렬 검사 시 허용하는 픽셀 오차 |
| `memo` | 한국어 메모 | 설정의 목적과 변경 이유 |

설정 변경은 스크립트 재컴파일 또는 에디터 재시작 후 반영됩니다.

`GameViewFullscreenStringTable.txt`는 탭으로 구분한 `key`, `ko`, `en`, `description` 열을 사용합니다. 문자열을 추가할 때 한국어와 영어를 함께 작성하고 설명은 한국어로 작성합니다. 현재 에디터 메뉴와 안내는 한국어를 사용합니다.

## 패키지 다시 만들기

이 저장소를 Unity 프로젝트로 연 뒤 Project 창에서 `Assets/Editor/GameViewFullscreen` 폴더를 선택하고 `Assets > Export Package...`를 실행합니다. `Include Dependencies`를 끄고 기능 폴더의 모든 파일을 포함하여 `Releases/GameViewFullscreen.unitypackage`로 저장합니다.

자동으로 내보낼 때는 Unity의 `-executeMethod`에 다음 메서드를 지정합니다.

```text
GameViewFullscreen.Distribution.GameViewFullscreenPackageExporter.Export
```

내보내기 도구는 기능 폴더만 패키지에 포함하며 결과를 `Releases/GameViewFullscreen.unitypackage`에 저장합니다.

## 검증

독립 에디터 어셈블리 컴파일, 실제 렌더 영역 정렬, 창과 탭 복원, 키 입력 상태 처리, 포커스 유지 및 씬 상태 보존을 원본 작업 환경에서 검증했습니다. 배포 패키지의 코드, 데이터 및 `.meta`가 소스와 일치하는지도 확인했습니다.

자세한 결과와 수동 검증 절차는 [검증 기록](Documentation/VALIDATION.md)을 참고해 주세요. `Tests/Editor/ValidateGameViewFullscreen.cs`는 Unity Pipeline의 `run_script`에서 `ValidateGameViewFullscreen.Main`을 실행하는 검증 스크립트입니다. 에디터 포커스를 강제로 가져오지 않고 현재 씬을 유지한 채 기존 Game 뷰를 확장 및 복원합니다.

새 독립 프로젝트에서의 에디터 실행과 다른 Unity 버전의 호환성은 아직 검증하지 않았습니다.

## English quick start

Download [GameViewFullscreen.unitypackage](Releases/GameViewFullscreen.unitypackage) and import all files through `Assets > Import Package > Custom Package...`. Dock a Game tab in the main Editor window. Press Play to enter fullscreen automatically, then press F12 to toggle between fullscreen and the normal Editor layout. Stopping Play Mode restores the original window and tab state.

You can also copy `Assets/Editor/GameViewFullscreen` and its folder `.meta` into an existing Unity project. Keep the installation path unchanged. This tool is for the Windows Unity Editor and has been validated with Unity 6000.3.22f1. It uses internal GameView APIs, so other Unity versions need compatibility verification.
