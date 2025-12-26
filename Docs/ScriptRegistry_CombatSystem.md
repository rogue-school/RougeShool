## CombatSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/CombatSystem/`
**목적**: 전투 흐름(턴, 상태 머신, 카드 실행), 슬롯/드랍/검증, 전투 UI, 통계 수집 등을 담당하는 전투 전용 시스템
**비고**: StageSystem, CharacterSystem, SkillCardSystem, ItemSystem과 강하게 연동되는 핵심 런타임 시스템

---

## 스크립트 목록

| 스크립트 이름                                                                               | 네임스페이스                                                    | 상대 경로                                                       | 역할                                                                                         | 주요 공개 메서드(대표)                                                   | 주요 필드/프로퍼티(대표)                                                                          | Zenject 바인딩(있으면)                                                                                                                                                                                                                                                                                                 | 주요 참조자(사용처)                                                                           | 상태       |
| ------------------------------------------------------------------------------------------- | --------------------------------------------------------------- | --------------------------------------------------------------- | -------------------------------------------------------------------------------------------- | ------------------------------------------------------------------------ | ------------------------------------------------------------------------------------------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- | --------------------------------------------------------------------------------------------- | ---------- |
| **CombatInstaller**                                                                   | `Game.CombatSystem.Core`                                      | `Core/CombatInstaller.cs`                                     | 전투 씬용 Zenject 설치자, 전투 관련 서비스/매니저/상태머신 DI 바인딩                         | `InstallBindings()`                                                    | 카드 UI 프리팹, DI 설정 플래그                                                                    | `MonoInstaller`로 전투 씬에서 실행, `PlayerManager`/`EnemyManager`/`CombatExecutionManager`/`CombatFlowManager`/`CardCirculationSystem`/`PlayerHandManager`/`PlayerDeckManager`/`TurnContext`/`IStageManager`/`CardDropService`/`CombatStatsAggregator` 등 AsSingle/InterfacesAndSelfTo 바인딩 | Combat 씬 전체, Stage/SkillCard/Item/VFX 연동 진입점                                          | ✅ 사용 중 |
| **CombatStateMachine**                                                                | `Game.CombatSystem.State`                                     | `State/CombatStateMachine.cs`                                 | 전투 상태 머신 (PlayerTurn/EnemyTurn/CardExecution/Summon 등 상태 전환 관리)                 | `CheckSummonTriggerAtSafePoint()`, `StartCombat(...)`, `IsInSafeStateForPhaseTransition()`, `WaitForSafeStateForPhaseTransition()` 등             | `_currentState`, `CombatStateContext`, 디버그 플래그, 부활/처치 딜레이                        | `CombatInstaller`에서 `BindCombatStateMachine()` (파일 내 메서드)로 바인딩 (상태 머신 인스턴스 AsSingle)                                                                                                                                                                                                           | StageManager(소환 플래그/Enemy 데이터), TurnManager, CombatEvents, EnemyManager/PlayerManager, `EnemyCharacter`(페이즈 전환 안전 상태 확인) | ✅ 사용 중 |
| **ICombatState**                                                                      | `Game.CombatSystem.State`                                     | `State/ICombatState.cs`                                       | 전투 상태 인터페이스                                                                         | `OnEnter(...)`, `OnUpdate(...)`, `OnExit(...)`                     | `StateName` 등                                                                                  | 상태 구현체들에 대한 공통 인터페이스                                                                                                                                                                                                                                                                                   | `CombatStateMachine`, 각 상태 구현 클래스                                                   | ✅ 사용 중 |
| **BaseCombatState**                                                                   | `Game.CombatSystem.State`                                     | `State/BaseCombatState.cs`                                    | 전투 상태 기본 베이스 클래스                                                                 | `OnEnter(...)`, `OnUpdate(...)`, `OnExit(...)` 기본 구현           | 공통 디버그/헬퍼                                                                                  | DI 없음(상속용)                                                                                                                                                                                                                                                                                                        | PlayerTurnState, EnemyTurnState, CardExecutionState 등                                        | ✅ 사용 중 |
| **PlayerTurnState / EnemyTurnState**                                                  | `Game.CombatSystem.State`                                     | `State/PlayerTurnState.cs`, `State/EnemyTurnState.cs`       | 플레이어/적 턴 상태 구현                                                                     | `OnEnter(...)`, `OnUpdate(...)` 등                                   | 턴 타입/컨텍스트 참조                                                                             | 상태 객체로만 사용, DI 없음                                                                                                                                                                                                                                                                                            | CombatStateMachine의 상태 전환 경로                                                           | ✅ 사용 중 |
| **CardExecutionState**                                                                | `Game.CombatSystem.State`                                     | `State/CardExecutionState.cs`                                 | 카드 사용/효과 실행 상태                                                                     | `OnEnter(...)` 등                                                      | 실행 컨텍스트 참조                                                                                | 상태 객체로만 사용                                                                                                                                                                                                                                                                                                     | CombatExecutionManager, SkillCardSystem 실행 흐름                                             | ✅ 사용 중 |
| **SummonState / SummonReturnState**                                                   | `Game.CombatSystem.State`                                     | `State/SummonState.cs`, `State/SummonReturnState.cs`        | 적 소환/복귀 상태                                                                            | `OnEnter(...)`, `OnUpdate(...)` 등                                   | 소환 대상 데이터, 원래 HP 등                                                                      | 상태 객체로만 사용                                                                                                                                                                                                                                                                                                     | CombatStateMachine, StageManager 소환 플래그                                                  | ✅ 사용 중 |
| **EnemyDefeatedState / SlotMovingState**                                            | `Game.CombatSystem.State`                                     | `State/EnemyDefeatedState.cs`, `State/SlotMovingState.cs`   | 적 처치 상태, 슬롯 이동 상태                                                                 | `OnEnter(...)`, `OnUpdate(...)` 등                                   | 상태별 컨텍스트 참조                                                                              | 상태 객체로만 사용                                                                                                                                                                                                                                                                                                     | CombatStateMachine                                                                            | ✅ 사용 중 |
| **BattleEndState / CombatVictoryState / CombatResultState / CombatGameOverState**     | `Game.CombatSystem.State`                                     | `State/BattleEndState.cs` 등                                  | 전투 종료/결과/게임오버 상태                                                                 | `OnEnter(...)`                                                         | 승리/패배 플래그, 결과 데이터                                                                     | 상태 객체, DI 없음                                                                                                                                                                                                                                                                                                     | CombatStateMachine                                                                            | ✅ 사용 중 |
| **CombatInitState / CombatPrepareState / CombatPlayerInputState / CombatAttackState** | `Game.CombatSystem.State`                                     | `State/CombatInitState.cs` 등                                 | 전투 준비/입력/공격 진행 상태들                                                              | `OnEnter(...)`, `OnUpdate(...)`                                      | 컨텍스트/매니저 참조                                                                              | 상태 객체, DI 없음                                                                                                                                                                                                                                                                                                     | CombatStateMachine                                                                            | ✅ 사용 중 |
| **DefaultCombatState**                                                                    | `Game.CombatSystem.Core`                                      | `Core/DefaultCombatState.cs`                                 | 기본 전투 상태 (초기 상태/디버깅용)                                                          | -                                                                        | 턴 매니저 참조                                                                                     | DI 없음 (상태 객체로만 사용)                                                                                                                                                                                                                                                                                        | CombatStateMachine 초기 상태/예외 상황 처리                                                   | ✅ 사용 중 |
| **CombatStateFactory**                                                                    | `Game.CombatSystem.Factory`                                    | `Core/CombatStateFactory.cs`                                 | 전투 상태 객체들을 생성하는 팩토리                                                            | `CreatePrepareState()`, `CreateInputState()` 등                         | 각 상태 팩토리 참조                                                                                | `CombatInstaller`에서 바인딩 (현재는 주석 처리됨)                                                                                                                                                                                                                                                                    | CombatStateMachine 상태 생성                                                                  | ✅ 사용 중 |
| **CombatStateContext**                                                                | `Game.CombatSystem.State`                                     | `State/CombatStateContext.cs`                                 | 상태 간 공유되는 컨텍스트 (매니저/컨트롤러 묶음)                                             | `Initialize(...)`, `ValidateManagers()` 등                           | ExecutionManager, TurnManager, Player/EnemyManager, TurnController, SlotRegistry, SlotMovement 등 | DI 없음 (CombatStateMachine에서 초기화)                                                                                                                                                                                                                                                                                | 모든 ICombatState 구현체                                                                      | ✅ 사용 중 |
| **TurnManager**                                                                       | `Game.CombatSystem.Manager`                                   | `Manager/TurnManager.cs`                                      | 레거시 인터페이스 `ICombatTurnManager`를 새 TurnController/슬롯 시스템에 어댑트하는 매니저 | `StartGame()`, `EndGame()`, `SetTurn(...)` 등                      | `_turnController`, `_slotRegistry`, `_slotMovement` 및 각종 이벤트 핸들러 컬렉션            | `CombatInstaller`에서 `ICombatTurnManager`/`TurnManager`로 바인딩 (파일 내 BindTurnManager 계열 메서드)                                                                                                                                                                                                          | CombatStateMachine, StageManager, SkillCardSystem(턴 기반 로직)                               | ✅ 사용 중 |
| **ITurnController / TurnController**                                                  | `Game.CombatSystem.Interface` / `Game.CombatSystem.Manager` | `Interface/ITurnController.cs`, `Manager/TurnController.cs` | 새 턴 제어/상태 관리 로직                                                                    | `StartGame()`, `ProcessAllCharacterTurnEffects()` 등                 | 현재 턴, 턴 수, 이벤트                                                                            | `CombatInstaller`에서 `ITurnController` 바인딩                                                                                                                                                                                                                                                                     | TurnManager, CombatStateMachine                                                               | ✅ 사용 중 |
| **ICombatTurnManager**                                                                | `Game.CombatSystem.Interface`                                 | `Interface/ICombatTurnManager.cs`                             | 외부 시스템에 노출되는 전투 턴 인터페이스                                                    | `CurrentTurn`, `TurnCount`, 이벤트들                                 | -                                                                                                 | `TurnManager`를 구현체로 바인딩                                                                                                                                                                                                                                                                                      | StageSystem, SkillCardSystem 등 전투 턴 의존 코드                                             | ✅ 사용 중 |
| **CardSlotRegistry**                                                                  | `Game.CombatSystem.Manager`                                   | `Manager/CardSlotRegistry.cs`                                 | 슬롯별 카드 및 카드 UI 레지스트리                                                            | `RegisterCard(...)`, `GetCardInSlot(...)`, `ClearSlot(...)` 등     | 내부 슬롯→카드/카드UI 매핑                                                                       | `CombatInstaller`에서 `ICardSlotRegistry`로 바인딩                                                                                                                                                                                                                                                                 | TurnManager, SkillCardSystem Drag&Drop, SlotMovementController                                | ✅ 사용 중 |
| **ICardSlotRegistry**                                                                 | `Game.CombatSystem.Interface`                                 | `Interface/ICardSlotRegistry.cs`                              | 카드 슬롯 레지스트리 인터페이스                                                              | `RegisterCard(...)`, `ClearAllSlots()` 등                            | -                                                                                                 | `CardSlotRegistry` 구현체 바인딩                                                                                                                                                                                                                                                                                     | TurnManager, CardDropService, SlotMovementController                                          | ✅ 사용 중 |
| **SlotMovementController**                                                            | `Game.CombatSystem.Manager`                                   | `Manager/SlotMovementController.cs`                           | 슬롯 이동/애니메이션/큐 제어, 적 덱 캐시 관리                                                                 | `MoveAllSlotsForwardRoutine(...)`, `SetupInitialEnemyQueueRoutine(...)`, `AdvanceQueueAtTurnStartRoutine(...)`, `UpdateEnemyCache(...)`, `ClearEnemyCache()`, `RefillAllCombatSlotsWithEnemyDeckCoroutine()` 등          | 큐, 코루틴, 참조 매니저들, `_cachedEnemyData`, `_cachedEnemyName`                                                                         | `CombatInstaller`에서 `ISlotMovementController`/자체 타입으로 바인딩                                                                                                                                                                                                                                               | TurnManager, CombatStateMachine, StageManager (초기 적 큐), `EnemyCharacter`(페이즈 전환 후 덱 캐시 업데이트 및 슬롯 재채우기)                                    | ✅ 사용 중 |
| **ISlotMovementController**                                                           | `Game.CombatSystem.Interface`                                 | `Interface/ISlotMovementController.cs`                        | 슬롯 이동 컨트롤러 인터페이스                                                                | `MoveAllSlotsForwardRoutine(...)`, `SetupInitialEnemyQueueRoutine(...)`, `AdvanceQueueAtTurnStartRoutine(...)`, `UpdateEnemyCache(...)`, `ClearEnemyCache()`, `RefillAllCombatSlotsWithEnemyDeckCoroutine()` 등                                                  | -                                                                                                 | `SlotMovementController` 구현체 바인딩                                                                                                                                                                                                                                                                               | TurnManager, CombatStateMachine, `EnemyCharacter`(페이즈 전환)                                                               | ✅ 사용 중 |
| **CombatFlowManager**                                                                 | `Game.CombatSystem.Manager`                                   | `Manager/CombatFlowManager.cs`                                | 전투 전반의 흐름 관리 (입장→턴→종료까지)                                                   | `StartCombat(...)` 등                                                  | 현재 상태, 콜백                                                                                   | `CombatInstaller`에서 `ICombatFlowManager`/자체 타입으로 바인딩                                                                                                                                                                                                                                                    | CombatStateMachine, StageManager, UI                                                          | ✅ 사용 중 |
| **ICombatFlowManager**                                                                | `Game.CombatSystem.Interface`                                 | `Interface/ICombatFlowManager.cs`                             | 전투 플로우 인터페이스                                                                       | `StartCombat(...)` 등                                                  | -                                                                                                 | `CombatFlowManager` 구현체 바인딩                                                                                                                                                                                                                                                                                    | StageSystem, 게임 흐름 매니저                                                                 | ✅ 사용 중 |
| **CombatExecutionManager**                                                            | `Game.CombatSystem.Manager`                                   | `Manager/CombatExecutionManager.cs`                           | 카드/행동 실행과 전투 해석 로직 중앙 관리                                                    | `ExecuteCardAsync(...)` 등                                             | 현재 실행 상태, 큐 등                                                                             | `CombatInstaller`에서 `ICombatExecutionManager`/자체 타입으로 바인딩                                                                                                                                                                                                                                               | CombatStateMachine, SkillCardExecutor, CardDropService                                        | ✅ 사용 중 |
| **ICombatExecutionManager**                                                           | `Game.CombatSystem.Interface`                                 | `Interface/ICombatExecutionManager.cs`                        | 전투 실행 인터페이스                                                                         | `ExecuteCardAsync(...)` 등                                             | -                                                                                                 | `CombatExecutionManager` 구현체 바인딩                                                                                                                                                                                                                                                                               | 카드 실행/효과 시스템                                                                         | ✅ 사용 중 |
| **TurnContext**                                                                       | `Game.CombatSystem.Context`                                   | `Context/TurnContext.cs`                                      | 턴 컨텍스트 (현재 턴, 타겟, 캐릭터 등)                                                       | -                                                                        | 현재 턴 관련 데이터                                                                               | `CombatInstaller`에서 AsSingle 바인딩                                                                                                                                                                                                                                                                                | TurnManager, CharacterDeathHandler 등                                                         | ✅ 사용 중 |
| **DefaultCardExecutionContext**                                                       | `Game.CombatSystem.Context`                                   | `Context/DefaultCardExecutionContext.cs`                      | 카드 실행 시 사용하는 기본 컨텍스트 구현                                                     | -                                                                        | 카드/타겟 정보                                                                                    | `CardExecutor` 쪽에서 사용                                                                                                                                                                                                                                                                                           | SkillCard 실행 로직                                                                           | ✅ 사용 중 |
| **CardSlotHelper**                                                                    | `Game.CombatSystem.Utility`                                   | `Utility/CardSlotHelper.cs`                                   | 슬롯 관련 헬퍼 함수 모음                                                                     | `GetSlotByOwnerAndIndex(...)` 등                                       | -                                                                                                 | 정적/유틸, DI 없음                                                                                                                                                                                                                                                                                                     | TurnManager, CardSlotRegistry, Drag&Drop                                                      | ✅ 사용 중 |
| **SlotValidator / SlotSelector**                                                      | `Game.CombatSystem.Utility`                                   | `Utility/SlotValidator.cs`, `Utility/SlotSelector.cs`       | 슬롯 선택/검증 도우미                                                                        | `IsValidTargetSlot(...)` 등                                            | -                                                                                                 | 정적/유틸                                                                                                                                                                                                                                                                                                              | SkillCard 타겟팅, 드랍 위치 검증                                                              | ✅ 사용 중 |
| **UnityMainThreadDispatcher**                                                         | `Game.CombatSystem.Utility`                                   | `Utility/UnityMainThreadDispatcher.cs`                        | 메인 스레드에서 실행 보장용 유틸리티                                                         | `Enqueue(...)` 등                                                      | 큐, 싱글턴 인스턴스                                                                               | `CoreSystemInstaller.BindCoreUtilities()`에서 InterfacesAndSelfTo 바인딩                                                                                                                                                                                                                                             | Save/Combat/Item 등에서 메인 스레드 실행 필요 시 사용                                         | ✅ 사용 중 |
| **CombatEvents**                                                                      | `Game.CombatSystem`                                           | `Event/CombatEvents.cs`                                       | 전투 관련 전역 이벤트 집합                                                                   | `OnEnemyCharacterDamaged`, `OnPlayerCharacterDamaged` 등             | static event 필드들                                                                               | 정적 클래스, DI 없음                                                                                                                                                                                                                                                                                                   | CombatStatsAggregator, CombatStateMachine, VFX/Audio 등                                       | ✅ 사용 중 |
| **CombatStatsAggregator**                                                             | `Game.CombatSystem.Manager`                                   | `Manager/CombatStatsAggregator.cs`                            | 전투 중 발생하는 통계를 수집/누적                                                            | `HandleEnemyDamaged(...)` 등                                           | 누적 스탯 필드                                                                                    | `CombatInstaller.BindIntegratedManagers`에서 DontDestroyOnLoad GameObject로 생성 후 InterfacesAndSelfTo 바인딩                                                                                                                                                                                                       | 전투 통계 수집                                                                                | ✅ 사용 중 |
| **CardDropService** | `Game.CombatSystem.Service` | `SkillCardSystem/DragDrop/CardDropService.cs` (파일 위치는 SkillCardSystem이지만 네임스페이스는 CombatSystem.Service) | 카드 드래그&드롭 처리 서비스 | `TryDropCard(...)` | 슬롯/카드 참조, validator, registrar, turnManager, executionManager, stateMachine | `CombatInstaller.BindFactories`에서 `Container.Bind<CardDropService>().AsSingle()` 바인딩 | SkillCardSystem Drag&Drop 경로, CardDropToSlotHandler | ✅ 사용 중 |
| **DefaultCardDropValidator / CardDropRegistrar**                    | `Game.CombatSystem.DragDrop`                                  | `DragDrop/DefaultCardDropValidator.cs`, `DragDrop/CardDropRegistrar.cs`                                               | 카드 드래그&드롭 검증, 등록                                                            | `RegisterDropHandler(...)` 등                                          | 슬롯/카드 참조                                                                                    | `CombatInstaller.BindFactories`에서 Validator/Registrar는 유틸/컴포넌트로 사용                                                                                                                                                                                                         | SkillCardSystem Drag&Drop 경로                                                                | ✅ 사용 중 |
| **PlayerInputController**                                                             | `Game.CombatSystem.Service`                                   | `Service/PlayerInputController.cs`                            | 전투 중 플레이어 입력 처리 서비스                                                            | `HandleInput(...)` 등                                                  | 입력 상태, 참조 매니저들                                                                          | `CombatInstaller.BindCoreServices`에서 AsSingle/FromNew... 바인딩 (코드 내)                                                                                                                                                                                                                                          | CombatStateMachine, TurnController                                                            | ✅ 사용 중 |
| **DefaultEnemySpawnValidator**                                                        | `Game.CombatSystem.Service`                                   | `Service/DefaultEnemySpawnValidator.cs`                       | 적 스폰 조건 검증                                                                            | `CanSpawn(...)` 등                                                     | -                                                                                                 | AsSingle 또는 유틸 사용 (CombatInstaller 내)                                                                                                                                                                                                                                                                           | StageManager/EnemySpawnerManager와 연동                                                       | ✅ 사용 중 |
| **DefaultTurnStartConditionChecker**                                                  | `Game.CombatSystem.Service`                                   | `Service/DefaultTurnStartConditionChecker.cs`                 | 턴 시작 조건 검증 서비스                                                                     | `CanStartTurn(...)` 등                                                 | -                                                                                                 | AsSingle 바인딩                                                                                                                                                                                                                                                                                                        | TurnController, TurnManager                                                                   | ✅ 사용 중 |
| **CombatConstants**                                                                   | `Game.CombatSystem.Core`                                      | `Core/CombatConstants.cs`                                     | 전투 관련 상수 모음                                                                          | -                                                                        | 상수 필드                                                                                         | 정적/유틸                                                                                                                                                                                                                                                                                                              | 여러 전투 관련 스크립트                                                                       | ✅ 사용 중 |
| **SlotOwner**                                                                         | `Game.CombatSystem.Data`                                      | `Data/SlotOwner.cs`                                           | 슬롯 소유자(Player/Enemy/None) enum                                                          | -                                                                        | enum 값                                                                                           | DI 없음                                                                                                                                                                                                                                                                                                                | CardSlotRegistry, SlotMovementController, 카드 배치 로직                                      | ✅ 사용 중 |
| **ExecutionResult / ExecutionCommand / CombatPhase**                                  | `Game.CombatSystem.Interface`                                 | `Interface/*.cs`                                              | 전투 실행 결과/명령/페이즈 관련 타입                                                         | -                                                                        | 필드/enum                                                                                         | DI 없음                                                                                                                                                                                                                                                                                                                | CombatExecutionManager, 상태 머신, SkillCard 실행                                             | ✅ 사용 중 |
| **VictoryUI / GameOverUI / DamageTextUI / TurnStartButtonHandler**                    | `Game.CombatSystem.UI` / `Game.CombatSystem.Core`           | `UI/*.cs`, `Core/TurnStartButtonHandler.cs`                 | 전투 결과/게임오버/데미지 텍스트/턴 시작 버튼 UI                                             | `ShowVictory(...)`, `ShowGameOver(...)`, `SpawnDamageText(...)` 등 | UI 참조 필드                                                                                      | 씬 UI 컴포넌트, DI 없음                                                                                                                                                                                                                                                                                                | CombatFlowManager/CombatStateMachine 이벤트와 연동                                            | ✅ 사용 중 |
| **SlotInitializationStep**                                                            | `Game.CombatSystem.Initialization`                            | `Initialization/SlotInitializationStep.cs`                    | 전투 시작 시 슬롯 초기화 단계 구현                                                           | `InitializeSlots(...)` 등                                              | 슬롯/카드 참조                                                                                    | `CombatInstaller` 또는 Turn/State 초기화 로직에서 사용                                                                                                                                                                                                                                                               | Combat 시작 준비 단계                                                                         | ✅ 사용 중 |
| **CardExecutionStateFactory / Combat*StateFactory 들**                                | `Game.CombatSystem.Factory`                                   | `Factory/*.cs`                                                | 각 전투 상태 생성용 팩토리                                                                   | `Create(...)`                                                          | -                                                                                                 | CombatInstaller에서 팩토리들 AsSingle로 바인딩                                                                                                                                                                                                                                                                         | CombatStateMachine, 상태 전환 로직                                                            | ✅ 사용 중 |

> **사용 여부 메모**: CombatSystem 내 스크립트는 StageManager/SkillCardSystem/ItemSystem과 강하게 연결되어 있으며, grep/Installer/StateMachine 기준으로 **전부 실제 실행 경로가 확인**되었습니다.
> 순수 데이터/헬퍼/enum은 코드 참조 여부만으로 확인했고, UI/컴포넌트 스크립트는 전투 씬 프리팹/캔버스에 부착된 전제를 기준으로 `✅ 사용 중`으로 표기했습니다.

---

## 스크립트 상세 분석 (레벨 3)

### CombatInstaller

#### 클래스 구조

```csharp
MonoBehaviour
  └── MonoInstaller<CombatInstaller>
        └── CombatInstaller
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_cardUIPrefab` | `SkillCardUI` | `private` (SerializeField) | `null` | 카드 UI 프리팹 | 전투 씬에서 사용할 카드 UI 기본 프리팹 |
| `_stageManagerPrefab` | `StageManager` | `private` (SerializeField) | `null` | StageManager 프리팹 | 스테이지 매니저가 없을 때 생성용 (싱글 게임 구조에서 사용) |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `InstallBindings()` | `override void` | 없음 | `public` | 1. `BindCoreServices()` 호출<br>2. `BindIntegratedManagers()` 호출<br>3. `BindFactories()` 호출<br>4. `BindCombatStateMachine()`/`BindStageFlowStateMachine()` 호출 | 전투 씬에서 필요한 모든 서비스/매니저/상태 머신/팩토리를 DI 컨테이너에 등록 |
| `BindCoreServices()` | `void` | 없음 | `private` | 1. `ITurnController`, `ICombatExecutionManager`, `ICombatFlowManager` 등 코어 전투 서비스들을 AsSingle로 바인딩<br>2. PlayerInputController, DefaultEnemySpawnValidator 등 서비스 바인딩 | 전투 핵심 서비스 계층 구성 |
| `BindIntegratedManagers()` | `void` | 없음 | `private` | 1. `PlayerManager`/`EnemyManager`/`CombatStatsAggregator` 등 전역 또는 통합 매니저를 씬에서 찾거나 프리팹 통해 생성<br>2. `DontDestroyOnLoad` 설정 및 `BindInterfacesAndSelfTo`로 AsSingle 바인딩 | 전투와 다른 시스템을 잇는 통합 매니저 구성 |
| `BindFactories()` | `void` | 없음 | `private` | 1. `SkillCardFactory`, `CardCirculationSystem`, `PlayerDeckManager`, `PlayerHandManager`, `CardDropService` 등 카드 관련 팩토리/레지스트리 바인딩 | SkillCardSystem과 전투를 연결하는 카드 생성/순환 계층 구성 |

#### Zenject 연결 관계 (요약)

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `ITurnController` / `ICombatTurnManager` | `BindInterfacesAndSelfTo<TurnController>()` / `Bind<ICombatTurnManager>().To<TurnManager>()` | 전투 턴 상태 → 외부 시스템 인터페이스 | 새 턴 컨트롤러 + 레거시 인터페이스 어댑터를 함께 구성 |
| `ICombatExecutionManager` | `BindInterfacesAndSelfTo<CombatExecutionManager>().AsSingle()` | 카드 실행 요청 → 실제 실행 | SkillCardSystem/StateMachine에서 카드 실행을 통합 |
| `IStageManager` | `Bind<IStageManager>().FromMethod(FindOrCreateStageManager)` | StageScene 전역 → CombatScene | StageSystem과 CombatSystem의 경계 연결 |

---

### CombatStateMachine

#### 클래스 구조

```csharp
MonoBehaviour
  └── CombatStateMachine : ICoreSystemInitializable
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_currentState` | `ICombatState` | `private` | `null` | 현재 전투 상태 | PlayerTurn/EnemyTurn/CardExecution 등 현재 활성 상태 |
| `_stateContext` | `CombatStateContext` | `private` | `null` | 상태 컨텍스트 | ExecutionManager, TurnManager, Player/EnemyManager 등 묶음 |
| `_isInitialized` | `bool` | `private` | `false` | 초기화 여부 | CoreSystem/Installer 초기화 완료 여부 플래그 |
| `_pendingSummon` | `bool` | `private` | `false` | 소환 대기 플래그 | SummonState 전환 여부 판단에 사용 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Initialize(CombatStateContext context)` | `void` | `CombatStateContext context` | `public` | 1. `context` 검증<br>2. `_stateContext` 설정<br>3. 기본 상태(예: Init/Prepare)로 전환<br>4. `_isInitialized` = true | CombatInstaller에서 상태 머신을 준비할 때 호출 |
| `StartCombat()` | `void` | 없음 | `public` | 1. 초기 상태에서 PlayerTurn 또는 PrepareState로 전환<br>2. 상태 `OnEnter` 호출 | 실제 전투 시작 진입점 |
| `ChangeState(ICombatState nextState)` | `void` | `ICombatState nextState` | `public` | 1. 현재 상태 `OnExit` 호출<br>2. `_currentState`를 `nextState`로 교체<br>3. 새 상태 `OnEnter` 호출 | 상태 전환의 공통 처리 |
| `CheckSummonTriggerAtSafePoint()` | `void` | 없음 | `public` | 1. StageManager/Enemy 데이터 기준 소환 조건 확인<br>2. 조건 만족 시 SummonState로 전환 준비 | SummonState/ReturnState로의 전환 타이밍 제어 |
| `IsInSafeStateForPhaseTransition()` | `bool` | 없음 | `public` | 1. 현재 상태가 안전한 상태인지 확인<br>2. PlayerTurn/EnemyTurn은 안전, CardExecution/SlotMoving은 안전하지 않음 | 페이즈 전환이 허용되는 안전한 상태인지 확인 |
| `WaitForSafeStateForPhaseTransition()` | `IEnumerator` | 없음 | `public` | 1. `IsInSafeStateForPhaseTransition()`이 true가 될 때까지 대기<br>2. 최대 대기 프레임 제한 | 페이즈 전환이 허용되는 안전한 상태가 될 때까지 대기 |

#### 로직 흐름도 (전투 시작~턴 진행)

```text
StartCombat()
  ↓
[Init/Prepare 상태 OnEnter]
  ↓
첫 턴 진입 조건 만족 시 PlayerTurnState로 ChangeState
  ↓
PlayerTurnState.OnEnter → 입력/카드 사용 처리
  ↓
턴 종료 조건 시 EnemyTurnState로 전환
  ↓
EnemyTurnState.OnEnter → 적 행동 처리
  ↓
승리/패배/소환 조건에 따라
  ├─ SummonState / SummonReturnState
  ├─ BattleEndState / CombatResultState / CombatVictoryState / CombatGameOverState
  └─ 다시 PlayerTurnState
```

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `CombatStateContext` | `Initialize(context)` | 매니저/컨트롤러 묶음 → 상태들 | 모든 `ICombatState` 구현체가 공통 컨텍스트를 통해 매니저에 접근 |
| `TurnManager` / `ITurnController` | 컨텍스트 필드 | 턴 변경 이벤트 ↔ 상태 머신 | PlayerTurn/EnemyTurn 전환 조건 및 턴 카운트에 사용 |
| `StageManager` | 컨텍스트/직접 참조 | 스테이지 진행/소환 플래그 | SummonState/ReturnState 진입 조건을 제공 |
| `CombatEvents` | 상태 내부에서 사용 | 전투 이벤트 브로드캐스트 | UI/VFX/Audio/통계와 연결 |

---

### SlotMovementController

#### 클래스 구조

```csharp
public class SlotMovementController : ISlotMovementController
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_cachedEnemyData` | `EnemyCharacterData` | `private` | `null` | 적 덱 캐시 | 자동 보충 시 사용할 적 덱 데이터 캐시 |
| `_cachedEnemyName` | `string` | `private` | 빈 문자열 | 적 이름 캐시 | 자동 보충 시 사용할 적 이름 캐시 |
| `_registry` | `ICardSlotRegistry` | `private` | `null` | 카드 슬롯 레지스트리 | 슬롯별 카드 조회/등록 |
| `_enemyManager` | `EnemyManager` | `private` | `null` | 적 매니저 | 현재 적 캐릭터 조회 |
| `_cardFactory` | `ISkillCardFactory` | `private` | `null` | 카드 팩토리 | 적 카드 생성 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `UpdateEnemyCache(EnemyCharacterData enemyData, string enemyName)` | `void` | `EnemyCharacterData enemyData, string enemyName` | `public` | 1. `_cachedEnemyData`와 `_cachedEnemyName` 갱신<br>2. 디버그 로그 출력 | 페이즈 전환 시 새로운 페이즈의 덱을 사용하도록 캐시 업데이트 |
| `ClearEnemyCache()` | `void` | 없음 | `public` | 1. `_cachedEnemyData`와 `_cachedEnemyName` 초기화 | 적 교체 시 캐시 초기화 |
| `RefillAllCombatSlotsWithEnemyDeckCoroutine()` | `IEnumerator` | 없음 | `public` | 1. 모든 전투/대기 슬롯의 적 카드 제거<br>2. 적 덱 정보 가져오기 (캐시 또는 런타임)<br>3. 모든 전투/대기 슬롯을 적 카드로 채우기 | 페이즈 전환 시 모든 전투/대기 슬롯을 초기화하고 새로운 페이즈의 덱으로 재채우기 |
| `RefillWaitSlot4IfNeededRoutine()` | `IEnumerator` | 없음 | `private` | 1. WAIT_SLOT_4가 비어있는지 확인<br>2. 다음 카드 타입 결정 (플레이어 마커/적 카드)<br>3. 캐시된 적 덱 또는 런타임 적 덱에서 카드 생성<br>4. 슬롯에 배치 | 빈 슬롯 자동 보충 (페이즈 전환 후에도 동작, `MoveAllSlotsForwardRoutine`에서 자동 호출) |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `EnemyCharacter` | DI 주입 (`ISlotMovementController`) | 페이즈 전환 → `UpdateEnemyCache` 호출 → `RefillAllCombatSlotsWithEnemyDeckCoroutine` 호출 | 페이즈 전환 후 새로운 페이즈의 덱을 사용하도록 캐시 업데이트 및 슬롯 재채우기 |
| `ICardSlotRegistry` | DI 주입 | 슬롯 조회/등록 | 카드 슬롯 상태 확인 및 카드 배치 |
| `ISkillCardFactory` | DI 주입 | 적 카드 생성 | 자동 보충 시 적 카드 인스턴스 생성 |
| `EnemyManager` | DI 주입 | 현재 적 캐릭터 조회 | 런타임 적 덱 폴백 |

---

## 레거시/미사용 코드 정리

현재 CombatSystem 폴더 내에서는 Installer/StateMachine/DI 바인딩/grep 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
TurnManager는 기존 인터페이스를 유지하는 어댑터이지만, 실제로 `ICombatTurnManager` 의존 코드들이 여전히 사용하는 활성 경로이므로 레거시로 보지 않고 `✅ 사용 중`으로 유지합니다.

---

## 폴더 구조

```text
Assets/Script/CombatSystem/
├── Context/
│   ├── DefaultCardExecutionContext.cs
│   └── TurnContext.cs
├── Core/
│   ├── CombatConstants.cs
│   ├── CombatInstaller.cs
│   ├── CombatStateFactory.cs
│   ├── DefaultCombatState.cs
│   └── TurnStartButtonHandler.cs
├── Data/
│   └── SlotOwner.cs
├── Docs/
│   └── CombatSystem_개발문서.md
├── DragDrop/
│   ├── CardDropRegistrar.cs
│   └── DefaultCardDropValidator.cs
├── Event/
│   └── CombatEvents.cs
├── Factory/
│   ├── CombatAttackStateFactory.cs
│   ├── CombatGameOverStateFactory.cs
│   ├── CombatPlayerInputStateFactory.cs
│   ├── CombatPrepareStateFactory.cs
│   ├── CombatResultStateFactory.cs
│   └── CombatVictoryStateFactory.cs
├── Initialization/
│   └── SlotInitializationStep.cs
├── Interface/
│   ├── CombatPhase.cs
│   ├── ExecutionCommand.cs
│   ├── ExecutionResult.cs
│   ├── ICardSlotRegistry.cs
│   ├── ICombatExecutionManager.cs
│   ├── ICombatFlowManager.cs
│   ├── ICombatTurnManager.cs
│   ├── ISlotMovementController.cs
│   └── ITurnController.cs
├── Manager/
│   ├── CardSlotRegistry.cs
│   ├── CombatExecutionManager.cs
│   ├── CombatFlowManager.cs
│   ├── CombatStatsAggregator.cs
│   ├── SlotMovementController.cs
│   └── TurnManager.cs
├── Service/
│   ├── DefaultEnemySpawnValidator.cs
│   ├── DefaultTurnStartConditionChecker.cs
│   └── PlayerInputController.cs
├── State/
│   ├── BaseCombatState.cs
│   ├── BattleEndState.cs
│   ├── CardExecutionState.cs
│   ├── CombatAttackState.cs
│   ├── CombatGameOverState.cs
│   ├── CombatInitState.cs
│   ├── CombatPlayerInputState.cs
│   ├── CombatPrepareState.cs
│   ├── CombatResultState.cs
│   ├── CombatStateContext.cs
│   ├── CombatStateMachine.cs
│   ├── CombatVictoryState.cs
│   ├── EnemyDefeatedState.cs
│   ├── EnemyTurnState.cs
│   ├── PlayerTurnState.cs
│   ├── SlotMovingState.cs
│   ├── SummonReturnState.cs
│   └── SummonState.cs
├── UI/
│   ├── DamageTextUI.cs
│   ├── GameOverUI.cs
│   └── VictoryUI.cs
└── Utility/
    ├── CardSlotHelper.cs
    ├── SlotSelector.cs
    ├── SlotValidator.cs
    └── UnityMainThreadDispatcher.cs
```
