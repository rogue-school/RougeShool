## CoreSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/CoreSystem/`  
**목적**: 게임 전역 코어 시스템 (오디오, 씬 전환, 코어 매니저/인터페이스 등) 관리  
**비고**: 전체 게임에서 공용으로 사용되는 기반 시스템, 다른 시스템에서 참조하는 중심 계층  
**최신 업데이트**: SaveSystem과 Statistics 시스템이 제거되었습니다 (2024년)

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **CoreSystemInstaller** | `Game.CoreSystem` | `CoreSystemInstaller.cs` | 코어 시스템 Zenject 인스톨러, 코어 서비스 DI 바인딩 | `InstallBindings()` | 코어 매니저/서비스 SerializeField 참조 | `MonoInstaller`로 CoreScene/ProjectContext에서 실행, 코어 매니저·서비스·유틸리티·인터페이스를 AsSingle로 바인딩 | 전체 시스템 (DI 컨테이너 초기화) | ✅ 사용 중 |
| **MainSceneInstaller** | `Game.CoreSystem.Manager` | `Manager/MainSceneInstaller.cs` | 메인 씬 전용 Zenject 인스톨러, 씬 레벨 종속성 재바인딩 | `InstallBindings()` | - | `MonoInstaller`로 MainScene에서 실행, `IGameStateManager`·`SettingsManager`·`IPlayerCharacterSelectionManager`·`ISceneTransitionManager`를 `FromMethod(FindFirstObjectByType)` AsSingle로 바인딩 | MainScene 내 Player/UI 컴포넌트, CharacterSystem 진입 흐름 | ✅ 사용 중 |
| **BaseCoreManager** | `Game.CoreSystem.Manager` | `Manager/BaseCoreManager.cs` | 코어 매니저 공통 베이스 클래스 (초기화/해제 패턴 정의) | `InitCoreSystem()` 등 | 초기화 상태 플래그 등 | 직접 DI 바인딩 없음 (상속 기반) | `CoreSystemInitializer`, 각 코어 매니저들의 베이스 | ✅ 사용 중 |
| **CoreSystemInitializer** | `Game.CoreSystem.Manager` | `Manager/CoreSystemInitializer.cs` | 게임 시작 시 코어 시스템 초기화/부트스트랩 담당 | `InitializeAsync()` 등 | 초기화 대상 리스트, 플래그 | `CoreSystemInstaller`에서 `CoreSystemInitializer` 타입을 AsSingle로 바인딩, `ICoreSystemInitializable` 리스트를 `FromMethod`로 주입받아 순차 초기화 | 초기 진입 씬, 전역 코어 시스템 시작 지점 | ✅ 사용 중 |
| **SceneTransitionManager** | `Game.CoreSystem.Manager` | `Manager/SceneTransitionManager.cs` | 씬 전환 요청/페이드 연출 관리 | `TransitionToScene(...)` | 현재 씬 이름, 전환 중 상태 | `CoreSystemInstaller`에서 `SceneTransitionManager` 및 `ISceneTransitionManager`를 FromInstance.AsSingle로 바인딩, `MainSceneInstaller`에서 `ISceneTransitionManager`를 재바인딩(FindFirstObjectByType) | Stage 전환, Combat 진입/복귀, UI 씬 전환 로직 | ✅ 사용 중 |
| **ISceneTransitionManager** | `Game.CoreSystem.Interface` | `Interface/ISceneTransitionManager.cs` | 씬 전환 매니저 인터페이스 | `TransitionToScene(...)` 등 | - | `CoreSystemInstaller`: `ISceneTransitionManager ← SceneTransitionManager` AsSingle, `MainSceneInstaller`: `ISceneTransitionManager`를 `FromMethod(FindFirstObjectByType<SceneTransitionManager>)`로 바인딩 | StageSystem, CombatSystem, UISystem의 씬 전환 요청 DI | ✅ 사용 중 |
| **ICoreSystemInitializable** | `Game.CoreSystem.Interface` | `Interface/ICoreSystemInitializable.cs` | 코어 시스템 초기화 인터페이스 | `InitCoreSystem()` | - | `CoreSystemInstaller`에서 `FindObjectsByType`로 모든 구현체를 수집하여 `List<ICoreSystemInitializable>` AsSingle로 바인딩 | CoreSystem 내 각 매니저, 일부 외부 시스템 초기화 훅 | ✅ 사용 중 |
| **GameStateManager** | `Game.CoreSystem.Manager` | `Manager/GameStateManager.cs` | 게임 진행 상태(메인 메뉴/전투/스테이지 등) 관리 | `SetState(...)`, `GetState()` | 현재 게임 상태 enum, 상태 변경 이벤트 | `CoreSystemInstaller`에서 `GameStateManager` 및 `IGameStateManager`를 FromInstance.AsSingle로 바인딩, `MainSceneInstaller`에서 `IGameStateManager`를 재바인딩(FindFirstObjectByType) | StageSystem, CombatSystem, UISystem (메뉴/전투/결과 화면 전환 제어) | ✅ 사용 중 |
| **IGameStateManager** | `Game.CoreSystem.Interface` | `Interface/IGameStateManager.cs` | 게임 상태 매니저 인터페이스 | `SetState(...)`, `GetState()` | - | `CoreSystemInstaller`: `IGameStateManager ← GameStateManager` AsSingle, `MainSceneInstaller`: `IGameStateManager`를 `FromMethod(FindFirstObjectByType<GameStateManager>)`로 바인딩 | StageManager, CombatStateMachine, MainMenuController 등 상태 기반 흐름 제어 | ✅ 사용 중 |
| **PlayerCharacterSelectionManager** | `Game.CoreSystem.Manager` | `Manager/PlayerCharacterSelectionManager.cs` | 플레이어 캐릭터 선택/슬롯 관리 | `SelectCharacter(...)` 등 | 선택된 캐릭터 ID/슬롯 정보 | `CoreSystemInstaller`에서 `PlayerCharacterSelectionManager` 및 `IPlayerCharacterSelectionManager`를 FromInstance.AsSingle로 바인딩, `MainSceneInstaller`에서 `IPlayerCharacterSelectionManager` 재바인딩(FindFirstObjectByType) | CharacterSystem, StageSystem의 시작 캐릭터 설정 | ✅ 사용 중 |
| **IPlayerCharacterSelectionManager** | `Game.CoreSystem.Interface` | `Interface/IPlayerCharacterSelectionManager.cs` | 플레이어 캐릭터 선택 매니저 인터페이스 | `SelectCharacter(...)` 등 | - | `CoreSystemInstaller`: `IPlayerCharacterSelectionManager ← PlayerCharacterSelectionManager` AsSingle, `MainSceneInstaller`: `IPlayerCharacterSelectionManager`를 `FromMethod(FindFirstObjectByType<PlayerCharacterSelectionManager>)`로 바인딩 | 캐릭터 선택 UI, Stage 진입 로직 | ✅ 사용 중 |
| **AudioManager** | `Game.CoreSystem.Audio` | `Audio/AudioManager.cs` | BGM/SFX 재생과 볼륨 관리 담당 오디오 매니저 | `PlayBgm(...)`, `PlaySfx(...)` 등 | BGM/SFX 클립/믹서, 볼륨 설정 | `CoreSystemInstaller`에서 `AudioManager` 및 `IAudioManager`를 FromInstance.AsSingle로 바인딩 | CombatSystem, SkillCardSystem, UISystem, VFXSystem 등에서 효과음/배경음 재생 | ✅ 사용 중 |
| **IAudioManager** | `Game.CoreSystem.Interface` | `Interface/IAudioManager.cs` | 오디오 매니저 인터페이스 | `PlayBgm(...)`, `PlaySfx(...)` | - | `CoreSystemInstaller`: `IAudioManager ← AudioManager` AsSingle | 전투 연출, 카드 사용, UI 클릭 사운드 등 전역 오디오 DI | ✅ 사용 중 |
| **AudioEventTrigger** | `Game.CoreSystem.Audio` | `Audio/AudioEventTrigger.cs` | Unity 이벤트에서 오디오 재생 트리거용 컴포넌트 | `Play()` 등 | AudioClip, AudioManager 참조 | DI 바인딩 없음 (CoreScene에는 없을 수 있음, 사용하는 쪽에서 `[InjectOptional]` 사용) | 버튼/트리거 오브젝트에서 이벤트 기반 사운드 재생 | ✅ 사용 중 |
| **AudioPoolManager** | `Game.CoreSystem.Audio` | `Audio/AudioPoolManager.cs` | SFX 풀링 관리, 동시 재생 최적화 | `PlayOneShotPooled(...)` 등 | 오디오 소스 풀, 동시 재생 제한 값 | `AudioManager` 내부에서 조합되어 사용 (별도 DI 바인딩 없음 또는 내부 생성) | 전역 SFX 재생 경로 최적화 | ✅ 사용 중 |
| **SettingsManager** | `Game.CoreSystem.UI` | `UI/SettingsManager.cs` | 설정값(볼륨, 해상도 등) 관리 및 PlayerPrefs 저장 연동 | `ApplySettings()`, `LoadSettings()` 등 | 현재 설정값 프로퍼티, 저장 키 | `CoreSystemInstaller`에서 FromInstance.AsSingle로 바인딩, `MainSceneInstaller`에서 `SettingsManager`를 `FromMethod(FindFirstObjectByType<SettingsManager>)`로 바인딩 | SettingsPanelController (옵션 저장/로드는 PlayerPrefs 사용) | ✅ 사용 중 |
| **SettingsPanelController** | `Game.CoreSystem.UI` | `UI/SettingsPanelController.cs` | 설정 패널 UI 제어, 슬라이더/토글 ↔ 설정값 동기화 | `OnApply()`, `OnOpen()` 등 | 각종 UI 컴포넌트 참조 | 직접 DI 바인딩 없음, `SettingsManager`를 필드 참조 또는 DI로 사용 | 메인 메뉴/옵션 UI 씬 | ✅ 사용 중 |
| **TransitionEffectController** | `Game.CoreSystem.UI` | `UI/TransitionEffectController.cs` | 화면 전환 연출(페이드 등) 전용 컨트롤러 | `PlayFadeIn()`, `PlayFadeOut()` 등 | CanvasGroup, 애니메이션 설정 | 직접 DI 바인딩 없음, `SceneTransitionManager` 또는 씬 전환 흐름에서 호출 | 씬 전환, 전투 시작/종료 연출 | ✅ 사용 중 |
| **GameLogger** | `Game.CoreSystem.Utility` | `Utility/GameLogger.cs` | 프로젝트 공용 로그 유틸리티 (카테고리/레벨 구분, 한국어 메시지) | `LogInfo(...)`, `LogWarning(...)`, `LogError(...)` | 카테고리 enum, 로그 필터 설정 | 정적 클래스, DI 바인딩 없음 (모든 시스템에서 직접 호출) | 전체 시스템 공통 (예외/경고/정보 로그) | ✅ 사용 중 |
| **ComponentInteractionOptimizer** | `Game.CoreSystem.Utility` | `Utility/ComponentInteractionOptimizer.cs` | 컴포넌트 간 상호작용 최적화/역할 충돌 검사 | `ValidateRoles(...)` 등 | 역할 정의/충돌 규칙 컬렉션 | 직접 DI 바인딩 없음 (필요 시 CoreSystemInstaller 또는 에디터/런타임에서 호출) | CharacterSystem, CombatSystem, StageSystem의 컴포넌트 설계 검증 | ✅ 사용 중 |
| **ComponentRoleManager** | `Game.CoreSystem.Utility` | `Utility/ComponentRoleManager.cs` | 컴포넌트 역할 정의/조회 유틸리티 | `RegisterRole(...)`, `GetRoles(...)` | 역할 정의 테이블 | 정적/유틸 성격, DI 바인딩 없음 | ComponentInteractionOptimizer, 각 시스템의 역할 조회 로직 | ✅ 사용 중 |
| **DIOptimizationUtility** | `Game.CoreSystem.Utility` | `Utility/DIOptimizationUtility.cs` | Zenject DI 최적화/검증용 유틸리티 | `ValidateBindings(...)` 등 | DI 설정 관련 상수/헬퍼 | 정적 유틸, DI 바인딩 없음 | CoreSystemInstaller, 기타 Installer 최적화/검증 시 | ✅ 사용 중 |
| **KoreanTextHelper** | `Game.CoreSystem.Utility` | `Utility/KoreanTextHelper.cs` | 한글 텍스트 처리/조사(은/는/이/가) 등 헬퍼 | 조사 선택 함수 등 | 문자열 유틸 메서드 | 정적 유틸, DI 바인딩 없음 | UI 텍스트, 로그 메시지 생성 시 | ✅ 사용 중 |
| **CoroutineRunner** | `Game.CoreSystem.Utility` | `Utility/CoroutineRunner.cs` | 전역 코루틴 실행기(비MonoBehaviour 호출용) | `RunCoroutine(...)` | 코루틴 호스트 GameObject 참조 | `CoreSystemInstaller`의 `BindCoreUtilities`에서 `CoroutineRunner` 및 `ICoroutineRunner`를 `EnsureAndBindCoreManagerWithInterface`로 바인딩 (필요 시 자동 생성) | CoreSystem, CombatSystem, SkillCardSystem 등에서 전역 코루틴 실행 | ✅ 사용 중 |
| **ICoroutineRunner** | `Game.CoreSystem.Utility` | `Utility/ICoroutineRunner.cs` | 코루틴 실행기 인터페이스 | `RunCoroutine(...)` | - | `CoreSystemInstaller`: `ICoroutineRunner ← CoroutineRunner` AsSingle | 코루틴이 필요한 순수 서비스/매니저 (MonoBehaviour 비종속 로직) | ✅ 사용 중 |

---

## 스크립트 상세 분석 (레벨 3)

### CoreSystemInstaller

#### 클래스 구조

```csharp
MonoBehaviour
  └── MonoInstaller<CoreSystemInstaller>
        └── CoreSystemInstaller
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `coreSystemInitializer` | `CoreSystemInitializer` | `private` (SerializeField) | `null` | 코어 초기화 매니저 | 게임 시작 시 코어 시스템들을 순차 초기화하는 매니저 참조 |
| `gameStateManager` | `GameStateManager` | `private` (SerializeField) | `null` | 게임 상태 매니저 | 메인 메뉴/전투/스테이지 등의 전역 상태 관리 |
| `sceneTransitionManager` | `SceneTransitionManager` | `private` (SerializeField) | `null` | 씬 전환 매니저 | 페이드 연출과 함께 씬 전환을 수행 |
| `audioManager` | `AudioManager` | `private` (SerializeField) | `null` | 오디오 매니저 | BGM/SFX 재생을 담당 |
| `settingsManager` | `SettingsManager` | `private` (SerializeField) | `null` | 설정 매니저 | 게임 옵션(볼륨, 해상도 등) 관리 |
| `coroutineRunner` | `CoroutineRunner` | `private` (SerializeField) | `null` | 코루틴 실행기 | 비 MonoBehaviour 서비스에서 코루틴을 실행할 수 있게 함 |
| `playerCharacterSelectionManager` | `PlayerCharacterSelectionManager` | `private` (SerializeField) | `null` | 캐릭터 선택 매니저 | 플레이어 시작 캐릭터 선택/슬롯 관리 |
| `enableLazyInitialization` | `bool` | `private` (SerializeField) | `true` | DI 최적화 옵션 | 지연 초기화 여부 (향후 사용 예정) |
| `enableCircularDependencyCheck` | `bool` | `private` (SerializeField) | `true` | 순환 의존성 검사 옵션 | DI 사이클 검사 활성화 여부 |
| `enablePerformanceLogging` | `bool` | `private` (SerializeField) | `false` | 성능 로그 출력 | 바인딩 시간 측정 로그 출력 여부 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `InstallBindings` | `void` | 없음 | `public` (override) | 1. `Stopwatch`로 성능 측정 시작<br>2. `BindCoreServices()` 호출<br>3. `BindCoreUtilities()` 호출<br>4. `BindCoreManagers()` 호출<br>5. `BindCoreInterfaces()` 호출<br>6. 옵션에 따라 바인딩 시간 로그 출력 | CoreSystem에 필요한 모든 서비스/유틸리티/매니저/인터페이스를 한 번에 바인딩 |
| `BindCoreServices` | `void` | 없음 | `private` | 1. `IItemService`를 새 GameObject 컴포넌트로 AsSingle 바인딩<br>2. `IRewardGenerator`를 순수 싱글톤으로 바인딩<br>3. `ICombatStatsProvider`를 `CombatStatsAggregator` 탐색 기반으로 바인딩 | Item/보상/전투 통계 등 코어 외부 서비스들을 DI 컨테이너에 등록 |
| `BindCoreUtilities` | `void` | 없음 | `private` | 1. `CoroutineRunner`와 `ICoroutineRunner`를 보장 및 바인딩<br>2. `UnityMainThreadDispatcher`를 계층에서 찾아 인터페이스와 함께 바인딩<br>3. `GameLogger`는 정적 클래스이므로 DI 제외 | 코루틴/스레드 관련 유틸리티를 바인딩하고, 정적 로거는 예외 처리 |
| `BindCoreManagers` | `void` | 없음 | `private` | 1. 매니저 인스턴스와 인터페이스 타입을 튜플 배열로 정의<br>2. 각 인스턴스를 AsSingle로 바인딩<br>3. 인터페이스가 있으면 인터페이스도 함께 FromInstance.AsSingle 바인딩<br>4. `QueueForInject`로 필드/프로퍼티 주입 예약<br>5. 할당 누락 시 경고 로그 출력 | 코어 매니저 및 관련 매니저들을 일괄적으로 최적화된 방식으로 바인딩 |
| `BindCoreInterfaces` | `void` | 없음 | `private` | 1. `ICoreSystemInitializable` 구현체들을 모든 씬에서 검색<br>2. 이미 매니저로 바인딩된 타입은 제외<br>3. 나머지 컴포넌트를 `BindInterfacesAndSelfTo`로 AsSingle 바인딩<br>4. `List<ICoreSystemInitializable>`를 `FromMethod`로 구성해 AsSingle 바인딩 | 코어 초기화 인터페이스 구현체들을 자동으로 등록하고, 초기화 대상 리스트를 구성 |
| `EnsureAndBindCoreManagerWithInterface<TConcrete, TInterface>` | `void` | `TConcrete instance, string gameObjectName` | `private` (generic) | 1. 인스턴스 null이면 계층에서 탐색<br>2. 그래도 없으면 새 GameObject를 생성해 컴포넌트 추가 및 `DontDestroyOnLoad` 설정<br>3. `BindInterfacesAndSelfTo`로 타입과 인터페이스를 AsSingle 바인딩<br>4. `QueueForInject`로 주입 예약 | 코어 매니저 인스턴스를 보장하고, 구현 타입과 인터페이스를 동시에 DI에 등록 |

#### 로직 흐름도

```text
InstallBindings()
  ↓
  [성능 측정 시작]
  ↓
  BindCoreServices()
    ↓ IItemService / IRewardGenerator / ICombatStatsProvider 바인딩
  ↓
  BindCoreUtilities()
    ↓ CoroutineRunner / ICoroutineRunner / UnityMainThreadDispatcher 바인딩
  ↓
  BindCoreManagers()
    ↓ CoreSystemInitializer / SceneTransitionManager / GameStateManager / AudioManager ...
       각 매니저 및 관련 인터페이스 AsSingle 바인딩
  ↓
  BindCoreInterfaces()
    ↓ ICoreSystemInitializable 구현체 자동 검색 및 바인딩
    ↓ List<ICoreSystemInitializable> 구성
  ↓
  [옵션: 바인딩 시간 로그 출력]
  ↓
  (CoreSystem DI 구성 완료)
```

#### Zenject 연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `IItemService` | `Bind<IItemService>().To<ItemService>().FromNewComponentOnNewGameObject().AsSingle().NonLazy()` | 새 GameObject에 `ItemService` 컴포넌트 생성 후 인터페이스로 노출 | 아이템 관련 전역 서비스 |
| `IRewardGenerator` | `Bind<IRewardGenerator>().To<RewardGenerator>().AsSingle()` | 순수 C# 싱글톤 | 보상 생성 로직 제공 |
| `ICombatStatsProvider` | `Bind<ICombatStatsProvider>().FromMethod(FindFirstObjectByType<CombatStatsAggregator>).AsSingle().NonLazy()` | 씬에서 `CombatStatsAggregator`를 찾아 주입 | 전투 통계 수집기 |
| 코어 매니저들 (예: `GameStateManager`) | `Bind(type).FromInstance(instance).AsSingle()` | SerializeField로 지정된 인스턴스를 싱글톤으로 등록 | 코어 매니저 인스턴스 DI 등록 |
| 코어 인터페이스들 (예: `IGameStateManager`) | `Bind(interfaceType).FromInstance(instance).AsSingle()` | 매니저 인스턴스를 해당 인터페이스로 노출 | 다른 시스템에서 인터페이스 기반 의존성 주입 |
| `ICoroutineRunner` | `BindInterfacesAndSelfTo<CoroutineRunner>().AsSingle()` | `CoroutineRunner` 인스턴스를 구현 타입/인터페이스에 모두 바인딩 | 전역 코루틴 실행기 |
| `List<ICoreSystemInitializable>` | `Bind<List<ICoreSystemInitializable>>().FromMethod(...)` | 초기화 대상 컴포넌트 목록 구성 | `CoreSystemInitializer`에서 사용 |

---

### MainSceneInstaller

#### 클래스 구조

```csharp
MonoBehaviour
  └── MonoInstaller<MainSceneInstaller>
        └── MainSceneInstaller
```

#### 함수 상세

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `InstallBindings` | `void` | 없음 | `public` (override) | 1. `IGameStateManager`를 `GameStateManager` 인스턴스 탐색 기반으로 바인딩<br>2. `SettingsManager`를 씬에서 탐색해 바인딩<br>3. `IPlayerCharacterSelectionManager`를 `PlayerCharacterSelectionManager` 탐색 기반으로 바인딩<br>4. `ISceneTransitionManager`를 `SceneTransitionManager` 탐색 기반으로 바인딩 | CoreScene에서 살아있는 전역 매니저들을 MainScene DI 컨테이너에 재노출하여, 씬 내 컴포넌트에서 안전하게 주입받을 수 있게 함 |

#### 로직 흐름도

```text
MainSceneInstaller.InstallBindings()
  ↓
  [전역 GameStateManager 탐색] → IGameStateManager 바인딩
  ↓
  [전역 SettingsManager 탐색] → SettingsManager 바인딩
  ↓
  [전역 PlayerCharacterSelectionManager 탐색] → IPlayerCharacterSelectionManager 바인딩
  ↓
  [전역 SceneTransitionManager 탐색] → ISceneTransitionManager 바인딩
  ↓
  (MainScene 내 컴포넌트들이 코어 매니저를 DI로 사용 가능)
```

#### Zenject 연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `IGameStateManager` | `Bind<IGameStateManager>().FromMethod(_ => FindFirstObjectByType<GameStateManager>(Include)).AsSingle()` | 전역 `GameStateManager` 인스턴스를 찾아 인터페이스로 바인딩 | 메인 씬 내 컴포넌트에서 게임 상태 주입 |
| `SettingsManager` | `Bind<SettingsManager>().FromMethod(_ => FindFirstObjectByType<SettingsManager>(Include)).AsSingle()` | 전역 설정 매니저를 타입 그대로 바인딩 | 옵션 UI에서 설정 접근 |
| `IPlayerCharacterSelectionManager` | `Bind<IPlayerCharacterSelectionManager>().FromMethod(_ => FindFirstObjectByType<PlayerCharacterSelectionManager>(Include)).AsSingle()` | 전역 캐릭터 선택 매니저를 인터페이스로 바인딩 | 캐릭터 선택/스폰 로직에서 사용 |
| `ISceneTransitionManager` | `Bind<ISceneTransitionManager>().FromMethod(_ => FindFirstObjectByType<SceneTransitionManager>(Include)).AsSingle()` | 전역 씬 전환 매니저를 인터페이스로 바인딩 | 메인 씬 내에서 Stage/Combat 씬 전환 요청에 사용 |

---

### GameStateManager

#### 클래스 구조

```csharp
BaseCoreManager<IGameStateManager>
  └── GameStateManager : IGameStateManager
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `currentGameState` | `GameState` | `private` (SerializeField) | `GameState.MainMenu` | 현재 게임 상태 | 메인 메뉴/플레이/일시정지 등 전역 게임 상태 저장 |
| `selectedCharacter` | `PlayerCharacterData` | `private` (SerializeField) | `null` | 선택된 캐릭터 | 현재 세션에서 사용할 플레이어 캐릭터 데이터 |
| `OnGameStateChanged` | `System.Action<GameState>` | `public` | `null` | 상태 변경 이벤트 | 외부에서 게임 상태 변경 알림을 구독 |
| `sceneTransitionManager` | `ISceneTransitionManager` | `private` | `null` | 씬 전환 매니저 | 상태 변경에 따라 메인 씬 전환 등을 수행 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Construct` | `void` | `ISceneTransitionManager sceneTransitionManager` | `public` (`[Inject]`) | 1. null 체크 없이 필드에 할당<br>2. 이후 상태 전환 시 사용 | Zenject를 통해 씬 전환 매니저를 주입받는 생성자 역할 메서드 |
| `ChangeGameState` | `void` | `GameState newState` | `public` | 1. 현재 상태와 동일하면 리턴<br>2. 이전 상태 백업<br>3. `currentGameState` 갱신<br>4. `OnGameStateChanged` 이벤트 호출<br>5. `Debug.Log`로 상태 변경 출력 | 게임 상태를 변경하고, 구독자들에게 알립니다. |
| `ResetProgress` | `Task` | 없음 | `public` (async) | 1. 상태를 메인 메뉴로 변경<br>2. `sceneTransitionManager.TransitionToMainScene()` 호출<br>3. 완료 로그 출력 | 진행 상태를 초기화하고 메인 메뉴로 되돌립니다. |
| `ExitGame` | `void` | 없음 | `public` | 1. 에디터/빌드 환경에 따라 종료 처리<br>2. 에디터에서는 `EditorApplication.isPlaying = false` 호출<br>3. 빌드에서는 `Application.Quit()` 호출 | 게임을 종료합니다. |
| `SelectCharacter` | `void` | `PlayerCharacterData characterData` | `public` | 1. `selectedCharacter` 갱신<br>2. `GameLogger.LogInfo`로 선택 로그 출력 | 플레이어가 선택한 캐릭터를 기록합니다. |
| `PauseGame` | `void` | 없음 | `public` | 1. `Time.timeScale = 0` 설정<br>2. `ChangeGameState(GameState.Paused)` 호출<br>3. 로그 출력 | 게임을 일시정지 상태로 전환합니다. |
| `ResumeGame` | `void` | 없음 | `public` | 1. `Time.timeScale = 1` 설정<br>2. `ChangeGameState(GameState.Playing)` 호출<br>3. 로그 출력 | 일시정지에서 게임을 재개합니다. |
| `ResetSession` | `void` | 없음 | `public` | 1. `selectedCharacter` 초기화<br>2. 타임스케일 1로 복원<br>3. 상태를 메인 메뉴로 변경<br>4. 로그 출력 | 세션 레벨의 진행 상태를 깨끗하게 초기화합니다. |
| `GoToMainMenu` | `void` | 없음 | `public` | 1. 로그 출력<br>2. `sceneTransitionManager.TransitionToMainScene()` 비동기 호출 | 메인 메뉴로 이동하는 유틸리티 메서드입니다. |
| `OnInitialize` | `IEnumerator` | 없음 | `protected override` | 1. 초기 상태를 메인 메뉴로 설정<br>2. `ConnectUI()` 호출<br>3. `ValidateReferences()` 호출<br>4. 한 프레임 대기 | 베이스 코어 매니저 초기화 루틴 구현부 |
| `Reset` | `void` | 없음 | `public override` | 1. 선택된 캐릭터/상태/타임스케일 초기화<br>2. 디버그 모드일 때 로그 출력 | 코어 시스템 리셋 시 GameStateManager 상태를 초기화합니다. |

#### 로직 흐름도 (요약)

```text
ChangeGameState(newState)
  ↓
  [현재 상태와 비교]
  ↓ (다르면)
  currentGameState ← newState
  ↓
  OnGameStateChanged(newState) 이벤트 호출
  ↓
  (필요 시 외부 시스템이 상태 변경에 반응)
```

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `ISceneTransitionManager` | `[Inject] Construct(ISceneTransitionManager)` | 상태 변경 시 메인 씬 전환 | 진행 초기화/메인 메뉴 이동 등에서 사용 |
| `BaseCoreManager<IGameStateManager>` | 상속 | 초기화/리셋 루틴 재사용 | 코어 시스템 공통 초기화 패턴 사용 |
| 외부 UI/시스템 | `OnGameStateChanged` 이벤트 구독 | 상태 변경 알림 | UI 표시 변경, 입력 허용/차단 등에 사용 가능 |

---

### AudioManager

#### 클래스 구조

```csharp
MonoBehaviour
  └── AudioManager : ICoreSystemInitializable, IAudioManager
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `bgmSource` | `AudioSource` | `private` (SerializeField) | `null` | BGM 재생 소스 | 배경음 재생용 오디오 소스 |
| `sfxSource` | `AudioSource` | `private` (SerializeField) | `null` | SFX 재생 소스 | 효과음 재생용 오디오 소스 |
| `audioPoolManager` | `AudioPoolManager` | `private` (SerializeField) | `null` | 오디오 풀 매니저 | 다수의 SFX 동시 재생을 위한 풀 관리 |
| `bgmVolume` / `sfxVolume` | `float` | `private` (SerializeField) | `0.7f / 1.0f` | 볼륨 설정 | BGM/SFX 기본 볼륨 값 |
| `mainMenuBGM` | `AudioClip` | `private` (SerializeField) | `null` | 메인 메뉴 BGM | MainScene 자동 재생용 BGM |
| `stageEnemyBGMConfigs` | `List<StageEnemyBGMConfig>` | `private` (SerializeField) | `new` | 스테이지별 적 BGM 설정 | Stage/Enemy 조합별 BGM 매핑 |
| `IsInitialized` | `bool` | `public` | `false` | 초기화 여부 | 오디오 시스템 초기화 완료 플래그 |
| `audioClipCache` | `Dictionary<string, AudioClip>` | `private` | 빈 | AudioClip 캐시 | Resources 로드 결과 캐싱 |
| `sceneBGMMap` | `Dictionary<string, AudioClip>` | `private` | `null` | 씬 이름 → BGM 매핑 | 씬 로드 시 자동 BGM 선택용 |
| `sceneBGMRegistry` | `Dictionary<string, string>` | `private` | 기본 매핑 | 씬 이름 → Resources 경로 매핑 | 리소스 기반 BGM 로드용 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Awake` | `void` | 없음 | `private` | 1. 전역 매니저로 `DontDestroyOnLoad` 설정<br>2. `InitializeAudio()` 호출<br>3. `InitializeSceneBGMMap()` 호출<br>4. `SceneManager.sceneLoaded`에 `OnSceneLoaded` 구독 | 오디오 시스템 초기화 및 전역 유지 설정 |
| `InitializeAudio` | `void` | 없음 | `private` | 1. BGM/SFX AudioSource를 보장 및 설정<br>2. `AudioPoolManager`를 보장 | 런타임 오디오 재생에 필요한 컴포넌트 구성 |
| `InitializeSceneBGMMap` | `void` | 없음 | `private` | 1. `sceneBGMMap` 초기화<br>2. `mainMenuBGM`가 있으면 MainScene 매핑 | 씬별 BGM 매핑 테이블 구성 |
| `OnSceneLoaded` | `void` | `Scene scene, LoadSceneMode mode` | `private` | 1. 씬 이름으로 BGM 매핑 조회<br>2. 있으면 `PlayBGM` 호출 | 씬 로드 시 자동 BGM 재생 처리 |
| `PlayEnemyBGM` | `void` | `EnemyCharacterData enemyData` | `public` | 1. null 체크 후 로그<br>2. `stageEnemyBGMConfigs`에서 적 데이터에 맞는 BGM 검색<br>3. 찾으면 `PlayBGM` 호출, 없으면 경고 로그 | StageManager에서 적 소환 시 호출되는 적별 BGM 재생 함수 |
| `LoadAudioClipCached` | `AudioClip` | `string resourcePath` | `public` | 1. 경로 유효성 검사<br>2. 캐시 조회<br>3. 없으면 `Resources.Load` 후 캐시에 추가<br>4. 실패 시 경고 로그 | Resources 기반 오디오 클립 로딩 + 캐싱 |
| `PlayBGM` (대표) | `void` | `AudioClip bgmClip, bool fadeIn` | `public` | 1. null/동일 클립 체크<br>2. 필요 시 페이드 아웃/인 처리<br>3. BGM 소스에 클립 설정 및 재생 | 배경음 재생의 중심 메서드 |
| `PlaySfx` | `void` | `AudioClip clip` | `public` | 1. null 체크<br>2. 풀 또는 `sfxSource`를 통해 재생 | 일반 효과음 재생 |

#### 로직 흐름도 (요약)

```text
Awake()
  ↓
  [전역 매니저 설정(DontDestroyOnLoad)]
  ↓
  InitializeAudio()
  ↓
  InitializeSceneBGMMap()
  ↓
  SceneManager.sceneLoaded += OnSceneLoaded

OnSceneLoaded(scene)
  ↓
  [sceneBGMMap에서 BGM 검색]
  ↓
  있으면 PlayBGM 호출
```

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `AudioPoolManager` | SerializeField / `AddComponent` | SFX 풀 관리 | 다수의 효과음 동시 재생을 최적화 |
| `StageManager` | `StageManager`에서 `IAudioManager` 주입 후 `PlayEnemyBGM` 호출 | 적 소환 → BGM 재생 | 스테이지/적 연출과 연결 |
| `SceneManager.sceneLoaded` | 이벤트 구독 | 씬 로드 시 BGM 자동 재생 | 씬 전환과 오디오를 동기화 |

---

### GameLogger

#### 클래스 구조

```csharp
public static class GameLogger
```

#### 핵심 개념

- **로그 레벨 제어**: `LogLevel` 열거형과 `SetLogLevel`, `GetCurrentLogLevel`을 통해 전체 로그 레벨을 제어합니다.
- **카테고리별 토글**: `LogCategory` 열거형과 `ToggleLogCategory`, `IsLogCategoryEnabled` 등을 통해 카테고리별 활성/비활성을 제어합니다.
- **조건부 로그**: `LogWarning`, `LogInfo`, `LogDebug`, `LogVerbose`는 `UNITY_EDITOR`/`DEVELOPMENT_BUILD` 조건부 컴파일 속성을 사용해 빌드에서 로그 비용을 줄입니다.
- **카테고리 전용 헬퍼**: `LogCombat`, `LogAnimation`, `LogSlot`, `LogCharacter`, `LogSkillCard` 등 특정 시스템 전용 헬퍼 메서드를 제공합니다.

#### 대표 메서드

| 함수 이름 | 반환 타입 | 매개변수 | 설명 |
|----------|----------|---------|------|
| `LogError` | `void` | `string message, LogCategory category = LogCategory.Error` | 항상 출력되는 에러 로그 (조건부 컴파일 없음) |
| `LogWarning` | `void` (`Conditional`) | `string message, LogCategory category = LogCategory.Combat` | 에디터/개발 빌드에서만 출력되는 경고 로그 |
| `LogInfo` | `void` (`Conditional`) | `string message, LogCategory category = LogCategory.Combat` | 정보 로그, 현재 레벨/카테고리 활성 여부에 따라 출력 |
| `ToggleLogCategory` | `void` | `LogCategory category, bool enabled` | 특정 카테고리의 로그 활성/비활성을 토글 |
| `DisableAllLogs` / `EnableAllLogs` | `void` | 없음 | 전체 카테고리 활성/비활성화 (에러 제외) |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| 전역 시스템 전체 | 정적 메서드 호출 | 문자열 메시지 + 카테고리 | Combat/Stage/SkillCard/Save/Audio 등 모든 시스템에서 공통 사용 |
| Unity `Debug` | 내부에서 `Debug.Log*` 호출 | 실제 콘솔 출력 | Unity 로그 시스템과 연동 |

> **사용 여부**: grep 결과 및 여러 스크립트(Combat/Stage/SkillCard/Save 등)에서 다수 호출되고 있어 **완전히 활성 사용 중**입니다.

---

### ComponentInteractionOptimizer

#### 클래스 구조

```csharp
MonoBehaviour
  └── ComponentInteractionOptimizer
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `enableInteractionValidation` | `bool` | `private` (SerializeField) | `true` | 상호작용 검증 플래그 | 최적화 기능 전체 On/Off |
| `enableRoleConflictDetection` | `bool` | `private` (SerializeField) | `true` | 역할 충돌 검사 플래그 | 역할 중복/충돌 검사 수행 여부 |
| `enablePerformanceMonitoring` | `bool` | `private` (SerializeField) | `false` | 성능 모니터링 플래그 | Update 사용 여부/필드 수 검사 등 |
| `enableAutomaticOptimization` | `bool` | `private` (SerializeField) | `true` | 자동 최적화 플래그 | 금지된 책임 발견 시 자동 최적화 처리 여부 |
| `totalComponents` | `int` | `private` (SerializeField) | `0` | 전체 컴포넌트 수 | 초기화 시 찾은 MonoBehaviour 개수 |
| `optimizedComponents` | `int` | `private` (SerializeField) | `0` | 최적화된 컴포넌트 수 | 책임 분리 최적화 결과 카운트 |
| `conflictResolved` | `int` | `private` (SerializeField) | `0` | 해소된 충돌 수 | 금지된 책임 제거 제안 수 |
| `optimizationTime` | `float` | `private` (SerializeField) | `0` | 최적화 수행 시간(ms) | 최적화 작업 시간 측정 |
| `componentRoles` | `Dictionary<MonoBehaviour, ComponentRoleManager.ComponentRole>` | `private` | `null` | 컴포넌트 역할 맵 | 각 컴포넌트의 역할 기록 |
| `componentResponsibilities` | `Dictionary<MonoBehaviour, List<string>>` | `private` | `null` | 책임 목록 맵 | 각 컴포넌트의 책임 리스트 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Awake` | `void` | 없음 | `private` | 1. `enableInteractionValidation`이 true면 `InitializeOptimization()` 호출 | 초기 최적화 준비 작업 수행 |
| `Start` | `void` | 없음 | `private` | 1. `enableInteractionValidation`이 true면 `OptimizeComponentInteractions()` 호출 | 씬 시작 시 실제 최적화 실행 |
| `InitializeOptimization` | `void` | 없음 | `private` | 1. 역할/책임 딕셔너리 초기화<br>2. 모든 `MonoBehaviour` 수집 및 카운트 기록<br>3. 초기화 로그 출력 | 최적화 대상 컴포넌트 정보를 수집 |
| `OptimizeComponentInteractions` | `void` | 없음 | `public` (`[ContextMenu]`) | 1. 스톱워치 시작<br>2. 역할 충돌 검사<br>3. 책임 분리 최적화<br>4. 성능 모니터링<br>5. 시간 측정 및 기록 | 상호작용 최적화의 메인 엔트리 포인트 |
| `DetectRoleConflicts` | `void` | 없음 | `private` | 1. 모든 컴포넌트에 대해 역할 정보 조회<br>2. 역할별로 그룹화<br>3. 같은 역할에 여러 컴포넌트가 있으면 경고 로그 출력 | 역할 정의에 기반한 충돌 탐지 |
| `OptimizeResponsibilitySeparation` | `void` | 없음 | `private` | 1. 모든 컴포넌트 순회<br>2. `OptimizeComponentResponsibilities` 호출<br>3. 최적화된 컴포넌트 수 카운트 | 책임 분리 관점에서 컴포넌트 역할 검토 |
| `OptimizeComponentResponsibilities` | `bool` | `MonoBehaviour component` | `private` | 1. 역할 정보 조회<br>2. 금지된 책임 목록 순회<br>3. 책임이 있으면 경고/자동 최적화 처리 및 카운터 증가 | 개별 컴포넌트의 책임 위반을 탐지/보고 |
| `MonitorComponentPerformance` | `void` | 없음 | `private` | 1. 모든 컴포넌트 순회<br>2. `Update` 메서드 존재 여부 검사<br>3. 필드 수가 많은 컴포넌트 탐지<br>4. 성능 이슈 목록을 로그로 출력 | Update 사용/필드 수 과다 등 잠재적 성능 문제 탐지 |
| `ValidateComponentRoles` | `void` | 없음 | `public` (`[ContextMenu]`) | 1. 모든 컴포넌트에 대해 역할 정보 조회<br>2. 유효/무효 카운트 및 로그 출력 | 역할 정의가 올바르게 지정되어 있는지 검증 |

#### 사용/연결 관계 및 사용 여부

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `ComponentRoleManager` | 정적 메서드 호출 (`GetComponentRoleInfo`) | 컴포넌트 → 역할 정보 | 역할/책임 정의 시스템과 연동 |
| `GameLogger` | 정적 로그 메서드 사용 | 최적화/경고/성능 이슈 로그 | Core 카테고리 로그 다수 출력 |
| 씬 내 컴포넌트들 | `FindObjectsByType<MonoBehaviour>` | 컴포넌트 목록 수집 | 전체 씬에 존재하는 컴포넌트 분석 대상 |

> **사용 여부**: 코드 레벨에서는 외부에서 직접 참조하지 않고 **씬에 컴포넌트로 부착되어 실행/ContextMenu로 호출**되는 유틸리티입니다.  
> Inspector에서 제거되기 전까지는 **“실행 경로가 존재하는 활성 스크립트”**로 간주하여 `✅ 사용 중` 상태로 유지합니다.

---

## 레거시/미사용 코드 정리

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 상태 | 비고 |
|--------------|--------------|-----------|------|------|
| **DIOptimizationUtility** | `Game.CoreSystem.Utility` | `Utility/DIOptimizationUtility.cs` | 🟡 레거시/미사용 헬퍼 | Zenject DI 최적화/검증용 정적 유틸로 설계되었지만, 현재 grep 기준 어디에서도 호출되지 않습니다. 향후 DI 구조 점검 시 재활용하거나, 사용 계획이 없다면 정리(삭제) 후보로 볼 수 있습니다. |

---

## 폴더 구조

```text
Assets/Script/CoreSystem/
├── Audio/
│   ├── AudioManager.cs
│   ├── AudioEventTrigger.cs
│   └── AudioPoolManager.cs
├── Interface/
│   ├── IAudioManager.cs
│   ├── ICoreSystemInitializable.cs
│   ├── IGameStateManager.cs
│   ├── IPlayerCharacterSelectionManager.cs
│   └── ISceneTransitionManager.cs
├── Manager/
│   ├── BaseCoreManager.cs
│   ├── CoreSystemInitializer.cs
│   ├── GameStateManager.cs
│   ├── MainSceneInstaller.cs
│   ├── PlayerCharacterSelectionManager.cs
│   └── SceneTransitionManager.cs
├── UI/
│   ├── SettingsManager.cs
│   ├── SettingsPanelController.cs
│   └── TransitionEffectController.cs
├── Utility/
│   ├── ComponentInteractionOptimizer.cs
│   ├── ComponentRoleManager.cs
│   ├── CoroutineRunner.cs
│   ├── DIOptimizationUtility.cs
│   ├── GameLogger.cs
│   ├── ICoroutineRunner.cs
│   └── KoreanTextHelper.cs
└── CoreSystemInstaller.cs
```


