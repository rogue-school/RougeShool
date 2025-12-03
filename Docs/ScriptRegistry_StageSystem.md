## StageSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/StageSystem/`  
**목적**: 스테이지 데이터/진행 상태 관리, 적 생성/소환 흐름, 전투 진입·종료, 보상 트리거, UI 연동을 담당하는 스테이지 전용 시스템  
**비고**: CombatSystem, CharacterSystem, SkillCardSystem, ItemSystem, SaveSystem, CoreSystem(통계/오디오)와 강하게 연결된 상위 흐름 제어 계층

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **StageManager** | `Game.StageSystem.Manager` | `Manager/StageManager.cs` | 스테이지 진행 총괄 매니저 (적 생성/소환/완료, 저장/로드, 통계/보상 연동) | `CreateEnemyAsync(...)`, `CreateEnemyForSummonAsync(...)`, `PeekNextEnemyData()`, `IsSummonedEnemyActive()`, `GetSummonTarget()`, `GetOriginalEnemyHP()`, `SetStageProgressState(...)` 등 | `StageSettings`, `DebugSettings`, `currentStage`, `progressState`, `currentEnemyIndex`, 소환/완료 플래그들 | `CombatInstaller.BindFactories()`에서 `IStageManager`로 바인딩 (`stageManager`를 씬에서 찾거나 새로 생성) | CombatStateMachine(소환/전투 시작), EnemyManager(적 등록), AudioManager(적별 BGM), SaveManager/StageProgressCollector, RewardOnEnemyDeath, TurnManager/CombatStats 등 | ✅ 사용 중 |
| **IStageManager** | `Game.StageSystem.Interface` | `Interface/IStageManager.cs` | StageManager 인터페이스 | `LoadDefaultStage()`, `LoadStageByNumber(...)`, `GetCurrentStageNumber()` 등 | - | `CombatInstaller.BindFactories`에서 `IStageManager ← StageManager` AsSingle | CombatSystem, GameFlow(전투 진입/복귀), Save/통계 | ✅ 사용 중 |
| **StageData** | `Game.StageSystem.Data` | `Data/StageData.cs` | 스테이지 구성 ScriptableObject (적 목록, 배치, 연출 등) | - | 스테이지 이름, 적 리스트, 보상/환경 설정 | 에셋, DI 없음 | StageManager(적 생성/소환), AudioManager(적별 BGM 설정), 통계/보상 로직 | ✅ 사용 중 |
| **StageProgressState** | `Game.StageSystem.Data` | `Data/StageProgressState.cs` | 스테이지 진행 상태 enum (NotStarted/InProgress/Cleared 등) | - | enum 값 | DI 없음 | StageManager 내부 상태, Save/통계/진행 표시 | ✅ 사용 중 |
| **StageProgressController** | `Game.StageSystem.Manager` | `Manager/StageProgressController.cs` | 스테이지 진행 상태/저장 연동 보조 매니저 | `SaveProgress(...)`, `LoadProgress(...)` 등 | 현재 스테이지 번호/진행도 | SaveSystem와 연동, DI 또는 Find 사용 | StageManager, AutoSaveManager | ✅ 사용 중 |
| **StageFlowStateMachine** | `Game.StageSystem.State` | `State/StageFlowStateMachine.cs` | 스테이지 흐름 상태 머신 (예: 준비→전투→보상→다음 스테이지) | `ChangeState(...)`, `StartStageFlow(...)` 등 | 현재 상태, 컨텍스트 | `CombatInstaller.BindStageFlowStateMachine()`에서 바인딩 (CombatInstaller 내) | StageManager, GameFlow/UISystem | ✅ 사용 중 |
| **StageEnemyIndexDisplay** | `Game.StageSystem.UI` | `UI/StageEnemyIndexDisplay.cs` | 현재 적 인덱스를 UI로 표시하는 컴포넌트 | `UpdateDisplay(...)` 등 | 텍스트 UI 참조, StageManager 참조 | 씬 컴포넌트, DI/Find | StageManager의 `currentEnemyIndex` 표시 | ✅ 사용 중 |
| **StageUIController** | `Game.StageSystem.UI` | `UI/StageUIController.cs` | 스테이지 관련 UI(진행도/남은 적 등) 통합 컨트롤러 | `UpdateStageInfo(...)` 등 | StageData/StageManager 참조, UI 요소들 | 씬 컴포넌트, DI/Find | Stage 진행 상황 표시, 결과 화면 전환 등 | ✅ 사용 중 |

> **사용 여부 메모**: StageSystem에는 소수의 스크립트만 있으며, `StageManager`와 `IStageManager`가 CombatInstaller 및 여러 시스템에서 직접 참조되고, 나머지 UI/보조 클래스들 역시 StageManager/Save/통계/CombatStateMachine과 연결되어 있어 **모두 실행 경로가 확인된 상태**입니다.

---

## 스크립트 상세 분석 (레벨 3)

### StageManager

#### 클래스 구조

```csharp
MonoBehaviour
  └── StageManager : IStageManager
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_stageSettings` | `StageSettings` 또는 `StageData[]` | `private` (SerializeField) | 에셋 참조 | 스테이지 설정 | 스테이지별 적 목록/환경/보상 등을 담은 데이터 |
| `_currentStage` | `StageData` | `private` | `null` | 현재 스테이지 데이터 | 진행 중인 스테이지에 대한 참조 |
| `_progressState` | `StageProgressState` | `private` | `NotStarted` | 현재 진행 상태 | InProgress/Cleared 등 상태 플래그 |
| `_currentEnemyIndex` | `int` | `private` | `0` | 현재 적 인덱스 | 스테이지 적 리스트 중 몇 번째인지 |
| `_summonTarget` | `EnemyCharacterData` | `private` | `null` | 소환 대상 적 데이터 | SummonState에서 사용할 적 정보 |
| `_originalEnemyHP` | `int` | `private` | `0` | 소환 전 적 HP | SummonReturnState에서 되돌릴 HP |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `LoadDefaultStage()` | `void` | 없음 | `public` | 1. 기본 스테이지 번호/데이터 결정<br>2. `LoadStageByNumber(defaultNumber)` 호출 | 게임 시작 시 초기 스테이지를 로드 |
| `LoadStageByNumber(int stageNumber)` | `void` | `int stageNumber` | `public` | 1. 유효 범위 검사<br>2. `_currentStage`를 해당 `StageData`로 설정<br>3. `currentEnemyIndex`/`progressState` 초기화 | 특정 스테이지로 점프/로드 |
| `CreateEnemyAsync()` | `Task<ICharacter>` 또는 `Task` | 없음 또는 인덱스 | `public` | 1. 현재 스테이지의 `currentEnemyIndex` 위치에서 적 데이터 조회<br>2. EnemySpawner/EnemyManager를 통해 실제 EnemyCharacter 생성<br>3. BGM/통계/세이브와 연동 | 스테이지 적을 실제 전장에 배치하는 핵심 로직 |
| `PeekNextEnemyData()` | `EnemyCharacterData` | 없음 | `public` | 1. `currentEnemyIndex + 1` 위치의 적 데이터 반환 (없으면 null) | 다음 적/소환 대상 미리보기 |
| `SetStageProgressState(StageProgressState state)` | `void` | `StageProgressState state` | `public` | 1. 상태 갱신<br>2. 통계/SaveSystem에 변경 알림 | 스테이지 진행 상태 변경 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `CombatInstaller` / `IStageManager` | DI 바인딩 | StageScene 전역 → CombatScene | 전투 상태 머신/Installer에서 스테이지 정보를 조회 |
| `CombatStateMachine` (SummonState/ReturnState) | 메서드 호출 | 소환 대상/원래 HP 조회 | 전투 상태 전환 시 스테이지 소환/복귀 로직과 연동 |
| `AutoSaveManager` / `StageProgressCollector` | 이벤트/메서드 호출 | Stage 완료/진행 → 세이브 데이터 | 스테이지 진행과 세이브 시스템을 연결 |
| `StageUIController` / `StageEnemyIndexDisplay` | 참조/이벤트 | 진행도/인덱스 → UI | 현재 적 인덱스/스테이지 번호를 HUD에 표시 |

---

## 레거시/미사용 코드 정리

현재 StageSystem 폴더 내에서는 CombatInstaller/SaveSystem/통계/오디오 연계 및 grep 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
`StageProgressController`는 SaveSystem과의 경계 계층으로, AutoSave/StageProgressCollector와 함께 실사용 중입니다.

---

## 폴더 구조

```text
Assets/Script/StageSystem/
├── Data/
│   ├── StageData.cs
│   └── StageProgressState.cs
├── Interface/
│   └── IStageManager.cs
├── Manager/
│   ├── StageManager.cs
│   └── StageProgressController.cs
├── State/
│   └── StageFlowStateMachine.cs
└── UI/
    ├── StageEnemyIndexDisplay.cs
    └── StageUIController.cs
```


