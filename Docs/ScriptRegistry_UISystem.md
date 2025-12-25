## UISystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/UISystem/`  
**목적**: 메인 메뉴/설정/무기 선택 등 게임 전역 UI 흐름을 관리하고, CoreSystem/StageSystem/SkillCardSystem과 연결되는 상위 UI 계층 제공  
**비고**: Scene 전환, 캐릭터 선택, 새 게임/이어하기, 옵션 패널 등 유저 진입 경험 중심

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **MainMenuController** | `Game.UISystem` | `MainMenuController.cs` | 메인 메뉴/캐릭터 선택/새 게임/튜토리얼 스킵/크레딧 UI를 통합 제어하는 메인 컨트롤러 | `Start()`, `OnClickNewGame()`, `OnClickContinue()`, `OnClickStartGame()`, `SelectCharacterFromExternal(...)` 등 | `mainMenuPanel`, `characterSelectionPanel`, `gameStartPanel`, 캐릭터 버튼들, `swordCharacter/bowCharacter/staffCharacter`, `skipTutorialToggle`, `skillCardUIPrefab` 등 | `[Inject] IGameStateManager`, `IPlayerCharacterSelectionManager`, `ISceneTransitionManager`, `AudioManager` 주입 (Zenject) | CoreSystem(상태/씬 전환), CharacterSystem(캐릭터 데이터), SkillCardSystem(미리보기), StageSystem(새 게임 플래그) | ✅ 사용 중 |
| **BaseUIController** | `Game.UISystem` | `BaseUIController.cs` | 공통 UI 컨트롤러 베이스 클래스 (패널 열기/닫기, 공용 헬퍼) | `Show()`, `Hide()` 등 | CanvasGroup/Animation 참조 (예상) | DI 없음 (상속용) | SettingsUIController, PanelManager 등 공통 베이스 | ✅ 사용 중 |
| **SettingsUIController** | `Game.UISystem` | `SettingsUIController.cs` | 설정(볼륨/그래픽 등) UI를 제어하고 CoreSystem.SettingsManager와 연동 | `ApplySettings()`, `Open()`, `Close()` 등 | 슬라이더/토글 UI 참조, 설정 값 캐시 | `[Inject] SettingsManager` 주입 | Main 메뉴/인게임 설정 패널 | ✅ 사용 중 |
| **PanelManager** | `Game.UISystem` | `PanelManager.cs` | 여러 UI 패널 열기/닫기를 중앙에서 관리하는 유틸 컨트롤러 | `ShowPanel(...)`, `HidePanel(...)` 등 | 패널 리스트/맵 | 씬 컴포넌트, DI 없음 | MainMenu/Settings/기타 패널 토글 | ✅ 사용 중 |
| **WeaponSelector** | `Game.UISystem` | `WeaponSelector.cs` | 메인 메뉴/캐릭터 선택 UI에서 무기(검/활/지팡이 등) 선택을 보조 | `SelectWeapon(...)` 등 | 버튼/아이콘 참조 | 씬 컴포넌트, DI 없음 | MainMenuController와 함께 캐릭터/무기 선택 연동 | ✅ 사용 중 |
| **ButtonHoverEffect / UnderlineHoverEffect** | `Game.UISystem` | `ButtonHoverEffect.cs`, `UnderlineHoverEffect.cs` | 버튼/텍스트에 마우스 오버 효과(색/밑줄/스케일 등)를 주는 UI 효과 스크립트 | `OnPointerEnter(...)`, `OnPointerExit(...)` 등 | 색상/애니메이션 파라미터 | 씬/프리팹 컴포넌트, DI 없음 | 모든 UI 버튼/텍스트 호버 연출 | ✅ 사용 중 |
| **play / Xbutton / Newgame / ExitGame** | `Game.UISystem` | `play.cs`, `Xbutton.cs`, `Newgame.cs`, `ExitGame.cs` | 간단한 버튼 핸들러(플레이/닫기/새 게임/종료) - 레거시/보조용 | 버튼 OnClick 핸들러 메서드 | 버튼/패널 참조 | 씬 컴포넌트, DI 없음 (필요 시 MainMenuController로 점진 통합 예정) | 일부 UI 버튼에 직접 연결된 OnClick 이벤트 | ✅ 사용 중 (레거시/통합 후보) |

> **사용 여부 메모**: UISystem은 주로 씬/프리팹에 직접 부착된 컴포넌트 형태로 사용되며, `MainMenuController`는 Zenject DI를 통해 Core/Stage/Character/SkillCard와 연결되는 핵심 진입 UI입니다.  
> `play/Xbutton/Newgame/ExitGame`는 레거시 스타일이지만, 씬 OnClick에 여전히 연결된 전제를 기준으로 `✅ 사용 중`으로 표기했습니다.

---

## 스크립트 상세 분석 (레벨 3)

### MainMenuController

#### 클래스 구조

```csharp
MonoBehaviour
  └── MainMenuController
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `gameStateManager` | `IGameStateManager` | `[Inject] private` | `null` | 게임 상태 매니저 | 새 게임/종료 등 전역 상태 전환 |
| `playerCharacterSelectionManager` | `IPlayerCharacterSelectionManager` | `[Inject] private` | `null` | 캐릭터 선택 매니저 | 선택된 캐릭터를 Stage/Combat에 전달 |
| `sceneTransitionManager` | `ISceneTransitionManager` (Optional) | `[Inject(Optional = true)] private` | `null` | 씬 전환 매니저 | 메인 메뉴 → StageScene/게임 종료 등 씬 이동 |
| `audioManager` | `AudioManager` (Optional) | `[Inject(Optional = true)] private` | `null` | 오디오 매니저 | 버튼 클릭/메인 메뉴 BGM 재생 |
| `mainMenuPanel` / `characterSelectionPanel` / `gameStartPanel` | `GameObject` | `private` (SerializeField) | 할당 | UI 패널 참조 | 메인 메뉴/캐릭터 선택/게임 시작 패널 토글 관리 |
| `newGameButton` / `continueButton` / `startGameButton` / `reselectCharacterButton` | `Button` | `private` (SerializeField) | 할당 | 주요 버튼 | 버튼 클릭 이벤트를 코드에 연결 |
| `skipTutorialToggle` | `Toggle` | `private` (SerializeField) | 할당 | 튜토리얼 스킵 옵션 | PlayerPrefs `TUTORIAL_SKIP`에 반영 |
| `swordCharacter` / `bowCharacter` / `staffCharacter` | `PlayerCharacterData` | `private` (SerializeField) | 에셋 참조 | 캐릭터 선택 데이터 | 각 무기 버튼이 선택할 실제 게임 캐릭터 데이터 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Start()` | `void` | 없음 | `private` | 1. `InitializeUI()`로 패널/버튼 기본 상태 세팅<br>2. `InitializeCanvasGroups()`로 페이드용 CanvasGroup 구성<br>3. `ValidateInspectorBindings()`로 필수 필드 검사<br>4. `LoadCharacterData()`/`CreateCharacterCards()`/`BindFixedCharacterButtons()` 호출<br>5. `UpdateContinueButtonState()` 및 초기 애니메이션 실행 | 메인 메뉴 화면 초기화 |
| `OnClickNewGame()` | `void` | 없음 | `public` | 1. 캐릭터 선택 패널로 전환<br>2. 필요 시 효과음/애니메이션 재생 | “새 게임” 버튼 클릭 처리 |
| `OnClickContinue()` | `void` | 없음 | `public` | 1. `sceneTransitionManager`를 통해 StageScene 전환 요청<br>2. `OnNewGameButtonClicked()` 호출 | “이어하기” 버튼 클릭 처리 (현재는 새 게임과 동일하게 처리) |
| `OnClickStartGame()` | `void` | 없음 | `public` | 1. 선택된 캐릭터 유효성 검사<br>2. `playerCharacterSelectionManager`에 선택 반영<br>3. `PlayerPrefs`에 튜토리얼 스킵 값 저장<br>4. `sceneTransitionManager`로 StageScene 전환 요청 | 캐릭터 선택 후 실제 게임 시작 처리 |
| `SelectCharacterFromExternal(PlayerCharacterData data)` | `void` | `PlayerCharacterData data` | `public` | 1. null 검사 후 `selectedCharacter` 설정<br>2. 선택 정보 UI 반영<br>3. StartGamePanel 활성화 | 외부 UI(WeaponSelector 등)에서 캐릭터 선택 시 호출되는 진입점 |

#### 로직 흐름도 (새 게임 → 캐릭터 선택 → 게임 시작)

```text
OnClickNewGame()
  ↓
캐릭터 선택 패널 활성화
  ↓
Weapon 버튼 또는 캐릭터 카드 클릭
  ↓
SelectCharacterFromExternal(data)
  ↓
선택 정보 UI 갱신 + StartGamePanel 활성화
  ↓
OnClickStartGame()
  ↓
플레이어 캐릭터 선택 매니저에 선택 반영
  ↓
튜토리얼 스킵 옵션 PlayerPrefs 저장
  ↓
SceneTransitionManager를 통해 StageScene 로드
```

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `IGameStateManager` | DI 주입 | 메인 메뉴 ↔ 게임 진행 상태 | 새 게임/종료 시 상태 전환 |
| `IPlayerCharacterSelectionManager` | DI 주입 | 선택된 캐릭터 데이터 전달 | Stage/Combat 진입 시 어떤 캐릭터를 사용할지 결정 |
| `ISceneTransitionManager` | DI 주입(옵션) | 메인 메뉴 → StageScene | 씬 전환 연출/로딩 관리 |
| `AudioManager` | DI 주입(옵션) | 버튼 클릭/메뉴 BGM 재생 | 유저 경험 향상을 위한 오디오 연출 |

---

## 레거시/미사용 코드 정리

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 상태 | 비고 |
|--------------|--------------|-----------|------|------|
| **play / Xbutton / Newgame / ExitGame** | `Game.UISystem` | `play.cs`, `Xbutton.cs`, `Newgame.cs`, `ExitGame.cs` | 🟡 레거시/통합 후보 | 단일 버튼 OnClick에 직접 연결되는 **레거시 스타일 핸들러**로, 현재는 일부 UI 버튼에서 여전히 사용 중입니다. 향후에는 `MainMenuController`/`PanelManager` 등 중앙 컨트롤러로 기능을 통합하고, 이 스크립트들을 제거하는 방향의 리팩터링 후보입니다. |

그 외 UISystem 스크립트들은 메인 메뉴/설정/무기 선택/효과 연출에 직접 사용되고 있어, 레거시/미사용으로 분류된 항목은 없습니다.

---

## 폴더 구조

```text
Assets/Script/UISystem/
├── BaseUIController.cs
├── ButtonHoverEffect.cs
├── ExitGame.cs
├── MainMenuController.cs
├── Newgame.cs
├── PanelManager.cs
├── SettingsUIController.cs
├── UnderlineHoverEffect.cs
├── WeaponSelector.cs
├── Xbutton.cs
└── play.cs
```


