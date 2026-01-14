## TutorialSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/TutorialSystem/`  
**목적**: 전투 튜토리얼(턴 진행, 카드 드래그/드랍, 액티브 아이템 사용 등)을 제어하고, 오버레이 UI로 플레이어에게 가이드를 제공하는 시스템  
**비고**: PlayerPrefs 기반 게이트(`TUTORIAL_SKIP`), CombatSystem/ItemSystem/UISystem과 긴밀히 연동

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **TutorialManager** | `Game.TutorialSystem` | `TutorialManager.cs` | 전투 튜토리얼 매니저 (턴/아이템 이벤트를 구독하여 오버레이 튜토리얼 실행) | `CompleteTutorial()`, `StartTutorialIfReady()` (내부), `PrepareOverlay()` 등 | `_turnController`, `_itemService`, `_overlayPrefab`, `_shouldRun`, `_isRunning`, `_step` | `[Inject(Optional = true)] ITurnController`, `ItemService` 주입 (CombatInstaller/ItemSystem 바인딩 활용) | CombatStateMachine/TurnController(턴 이벤트), ItemService(액티브 아이템 사용 이벤트), StageManager(PlayerPrefs `TUTORIAL_SHOULD_RUN` 설정) | ✅ 사용 중 |
| **TutorialOverlayView** | `Game.TutorialSystem` | `TutorialOverlayView.cs` | 튜토리얼 오버레이 UI 뷰 (CanvasGroup/페이지 전환/Completed 이벤트 제공) | `ShowFirstPage()`, `Hide()` 등 | CanvasGroup, 페이지 UI 참조 | 씬/프리팹 컴포넌트, DI 없음 (`TutorialManager`에서 Instantiate/Find) | TutorialManager에서 튜토리얼 페이지 표시/완료 이벤트 수신 | ✅ 사용 중 |
| **TutorialOverlayViewEditor** | `Game.TutorialSystem.Editor` | `Editor/TutorialOverlayViewEditor.cs` | 튜토리얼 오버레이 커스텀 인스펙터/에디터 툴 | OnInspectorGUI 등 | SerializedProperty 참조 | 에디터 전용, DI 없음 | 디자이너가 튜토리얼 페이지를 설정/미리보기 할 때 사용 | ✅ 사용 중 |

> **사용 여부 메모**: `TutorialManager`는 Combat/Item 이벤트를 구독하고 PlayerPrefs로 실행 여부를 제어하며, `TutorialOverlayView`와 함께 실제 게임 플레이 중 튜토리얼을 실행합니다.  
> 에디터 스크립트(`TutorialOverlayViewEditor`)는 튜토리얼 뷰 설정을 돕는 에디터 도구로, 현재 에셋/프리팹을 대상으로 동작하는 것으로 확인됩니다.

---

## 스크립트 상세 분석 (레벨 3)

### TutorialManager

#### 클래스 구조

```csharp
MonoBehaviour
  └── TutorialManager
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_turnController` | `ITurnController` | `[Inject(Optional = true)] private` | `null` | 전투 턴 컨트롤러 | 턴 변경/턴 카운트 이벤트를 통해 튜토리얼 시작 조건 판단 |
| `_itemService` | `ItemService` | `[Inject(Optional = true)] private` | `null` | 아이템 서비스 | 액티브 아이템 사용 이벤트 구독에 사용 |
| `overlayPrefab` | `TutorialOverlayView` | `private` (SerializeField) | `null` | 튜토리얼 오버레이 프리팹 | 페이지/가이드 내용을 담은 UI 프리팹 |
| `overlayParent` | `RectTransform` | `private` (SerializeField) | `null` | 오버레이 부모 Transform | 미설정 시 활성 Canvas를 자동 탐색 |
| `_shouldRun` | `bool` | `private` | `false` | 실행 여부 플래그 | PlayerPrefs `TUTORIAL_SKIP` 값 기반 게이트 |
| `_isRunning` | `bool` | `private` | `false` | 현재 실행 중 여부 | 중복 시작 방지 |
| `_overlay` | `TutorialOverlayView` | `private` | `null` | 런타임 오버레이 인스턴스 | 튜토리얼를 실제로 표시하는 UI |
| `_step` | `Step` | `private` | `Step.None` | 현재 튜토리얼 단계 | 페이지/드래그 안내/액티브 아이템 안내 등 단계 관리 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Awake()` | `void` | 없음 | `private` | 1. `PlayerPrefs.GetInt("TUTORIAL_SKIP", 0)` 읽기<br>2. `_shouldRun` 설정 | 메인 메뉴의 튜토리얼 스킵 토글 상태를 반영 |
| `Start()` | `void` | 없음 | `private` | 1. `TrySubscribe()`로 턴/아이템 이벤트 구독<br>2. `_shouldRun`이 false면 조용히 대기 | 이벤트 기반 구조를 준비 |
| `TrySubscribe()` | `void` | 없음 | `private` | 1. `_turnController`/`_itemService`가 있으면 각각 이벤트 구독<br>2. 없으면 `TurnManager`를 FindFirstObjectByType으로 찾아 어댑트 | DI 실패 시에도 최대한 튜토리얼을 동작시키기 위한 방어 로직 |
| `StartTutorialIfReady()` | `void` | 없음 | `private` | 1. `_shouldRun`/`_isRunning`/`_startedOnce` 체크<br>2. 필요 시 `PrepareOverlay()`로 오버레이 생성<br>3. 첫 페이지를 보여주고 `_step = Step.PagesIntroAndTooltip` 설정 | Player 턴/TurnCount 조건이 만족되었을 때 실질적인 튜토리얼 시작 |
| `PrepareOverlay()` | `void` | 없음 | `private` | 1. 씬에 이미 존재하는 `TutorialOverlayView` 우선 사용<br>2. 없으면 `overlayPrefab`을 부모(지정 또는 캔버스 탐색) 아래에 Instantiate<br>3. RectTransform을 전체 화면으로 스트레치 | 오버레이 UI를 안전하게 생성/배치 |
| `OnOverlayCompleted()` | `void` | 없음 | `private` | 1. `Completed` 이벤트 구독 해제<br>2. 현재 `_step`에 따라 다음 단계 또는 종료 결정 | 페이지 순서/액티브 아이템 안내 등 단계를 마무리 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `ITurnController` / `TurnManager` | 이벤트 구독 | 턴 변경/턴 카운트 → 튜토리얼 시작 | 첫 Player 턴/TurnCount 조건에서 튜토리얼 시작 시점을 결정 |
| `ItemService` | 이벤트 구독 | 액티브 아이템 사용 → ActiveItem 단계 처리 | 액티브 아이템 사용 튜토리얼 단계와 연동 |
| `TutorialOverlayView` | 프리팹/씬 검색 | Completed 이벤트 ↔ Manager | 페이지별 안내/완료 시그널 전달 |
| `PlayerPrefs "TUTORIAL_SKIP"` | 키 조회 | 메인 메뉴 옵션 → 튜토리얼 실행 여부 | 메인 메뉴에서 설정한 “튜토리얼 스킵” 선택을 반영 |

---

## 레거시/미사용 코드 정리

현재 TutorialSystem 폴더 내에서는 Combat/Item/PlayerPrefs 연계 및 grep 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
튜토리얼을 비활성화하더라도 `TutorialManager`/`TutorialOverlayView`는 선택적 기능으로 유지되며, 온보딩 UX를 위해 남겨두는 활성 스크립트로 간주합니다.

---

## 폴더 구조

```text
Assets/Script/TutorialSystem/
├── Editor/
│   └── TutorialOverlayViewEditor.cs
├── TutorialManager.cs
└── TutorialOverlayView.cs
```


