# Game View Fullscreen

[English](README.md) | 한국어

Unity 에디터에서 플레이를 시작하면 기존 Game 탭의 게임 화면을 모니터 전체에 표시하고, F12로 기본 화면과 전체화면을 전환하는 에디터 확장입니다. Game 탭 헤더와 툴바를 화면 밖으로 숨기며 하단 게임 화면이 잘리지 않도록 실제 렌더 영역을 맞춥니다.

## 설치 및 사용

1. 대상 프로젝트의 Unity 에디터에서 `Assets > Import Package > Custom Package...`를 선택합니다.
2. `GameViewFullscreen.unitypackage`를 선택하고 포함된 파일을 모두 가져옵니다.
3. 컴파일이 끝나면 Game 탭을 기본 에디터 창에 도킹합니다.
4. Play 버튼을 누르면 자동으로 전체화면이 됩니다.
5. 플레이 중 F12를 누르면 기본 화면으로 돌아옵니다. 다시 누르면 전체화면으로 전환합니다.
6. 플레이를 종료하면 원래 창 위치와 크기, 탭 배치 및 툴바 상태로 복원합니다.

전환 메뉴는 `테스트 도구/게임 화면 전체화면 전환`입니다. 씬 및 프리팹에 컴포넌트를 추가할 필요가 없으며 추가 Unity 패키지도 필요하지 않습니다.

## 적용 환경

- Windows Unity 에디터 전용입니다. 빌드된 게임에는 포함되지 않습니다.
- Unity 6000.3.22f1에서 독립 어셈블리 컴파일, 실제 3840 × 2160 렌더 영역 정렬 및 원래 창과 탭 상태 복원을 검증했습니다.
- Unity 내부 GameView API를 사용합니다. 다른 Unity 버전에서는 내부 API 및 레이아웃 차이로 호환성 확인이 필요합니다.
- 분리된 Game 창만 있는 경우 기본 에디터 창에 도킹해야 합니다.
- 설정 및 문자열 테이블은 `Assets/Editor/GameViewFullscreen/` 경로에서 읽습니다. 폴더 경로를 유지해 주세요.

## 구성

- Controller: 플레이 이벤트, F12 입력과 전체화면 전환을 관리합니다.
- Model: 전체화면 상태 및 복원할 상태를 관리합니다.
- View: 기존 Game 탭의 렌더 영역을 검사하고 전체화면을 표시합니다.
- Native: Windows 창 크기, 위치 및 영역을 변경하며 다른 앱의 포커스를 가져오지 않습니다.
- Settings: 설정 테이블을 읽습니다.
- StringTable: 기능 내부의 한국어와 영어 문자열을 읽습니다. 프로젝트 공통 현지화 코드에 의존하지 않습니다.
- asmdef: 런타임 코드 참조 없이 에디터 전용 어셈블리로 컴파일합니다.

`GameViewFullscreenSettingsTable.json`의 `toggleVirtualKey`는 Windows 가상 키 코드이며 기본값 123은 F12입니다. `layoutWaitUpdates`, `maximumAlignmentAttempts`, `pixelTolerance`는 레이아웃 정렬 설정입니다. 메모는 한국어로 작성합니다.

`GameViewFullscreenStringTable.txt`는 탭으로 구분된 `key`, `ko`, `en`, `description` 열을 사용합니다. 한국어와 영어를 함께 관리하며 현재 에디터 안내는 한국어를 사용합니다.
