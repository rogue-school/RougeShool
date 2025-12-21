## SaveSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/SaveSystem/`  
**목적**: 스테이지/전투 진행 상황과 카드 슬롯 상태를 저장/복원하고, 자동 저장을 이벤트 기반으로 수행하는 세이브 전담 서브시스템  
**비고**: CoreSystem.SaveManager와 StageSystem/CombatSystem/CharacterSystem을 연결하는 보조 계층

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **SaveSystemInstaller** | `Game.SaveSystem.Installer` | `Installer/SaveSystemInstaller.cs` | 세이브 시스템용 Zenject 설치자 (필요한 세이브 컴포넌트 바인딩) | `InstallBindings()` | - | Save 관련 매니저/데이터를 AsSingle로 바인딩 (CoreSystem.SaveManager와 협업) | StageScene/CombatScene 세이브 구성, AutoSaveManager/StageProgressCollector 설정 | ✅ 사용 중 |
| **AutoSaveManager** | `Game.SaveSystem.Manager` | `Manager/AutoSaveManager.cs` | 자동 저장 매니저 (턴 종료/스테이지 완료/적 처치 시 이벤트 기반으로 SaveManager 호출) | `TriggerManualAutoSave()`, `SetAutoSaveEnabled(...)`, `GetLastSaveInfo()` 등 | `autoSaveEnabled`, `turnBasedAutosaveEnabled`, `lastSavedFrame`, `lastSavedTrigger` | `CombatInstaller.BindIntegratedManagers`에서 `AutoSaveManager`를 씬에서 찾거나 새 GameObject로 생성, InterfacesAndSelfTo AsSingle 바인딩 | TurnManager(턴 변경 이벤트), StageManager(스테이지 완료/적 처치 이벤트), CoreSystem.Save.SaveManager | ✅ 사용 중 |
| **StageProgressCollector** | `Game.SaveSystem.Manager` | `Manager/StageProgressCollector.cs` | 스테이지 진행 상황(현재 스테이지, 적 인덱스, 카드/슬롯 상태 등)을 수집해서 StageProgressData/CardSlotData로 변환 | `CollectProgress(...)`, `RestoreProgress(...)` 등 | StageManager/Combat 관련 참조, 현재 진행 데이터 캐시 | SaveManager의 `Awake`에서 `GetComponent<StageProgressCollector>()` 또는 `AddComponent`로 사용 | SaveManager.SaveCurrentProgress/LoadSavedScene, StageManager 진행 복원 | ✅ 사용 중 |
| **StageProgressData** | `Game.SaveSystem.Data` | `Data/StageProgressData.cs` | 스테이지 진행도(스테이지 번호, 적 인덱스, 클리어 플래그 등)를 표현하는 저장 데이터 구조 | - | 스테이지 번호, 진행 상태, 적 인덱스 등 | 순수 데이터, DI 없음 | StageProgressCollector, SaveManager | ✅ 사용 중 |
| **CardSlotData** | `Game.SaveSystem.Data` | `Data/CardSlotData.cs` | 카드 슬롯 상태(슬롯 위치, 카드 ID, 소유자 등)를 저장하기 위한 데이터 구조 | - | 슬롯 포지션, 카드 ID/Owner 등 | 순수 데이터, DI 없음 | StageProgressCollector/SaveManager, SkillCardSystem/CombatSystem과 연동 | ✅ 사용 중 |

> **사용 여부 메모**: SaveSystem 폴더의 스크립트는 CoreSystem.SaveManager와 긴밀히 연결되어 있으며, CombatInstaller/SaveSystemInstaller를 통해 DI 컨테이너에 등록되고 Stage/Combat 진행 저장/복원 경로에서 실제 사용되고 있습니다.  
> 별도의 “죽은” 세이브 스크립트는 보이지 않고, 모든 구성 요소가 AutoSave/StageProgress/Slot 저장 경로에 참여하고 있습니다.

---

## 스크립트 상세 분석 (레벨 3)

### AutoSaveManager

#### 클래스 구조

```csharp
MonoBehaviour
  └── AutoSaveManager
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `saveManager` | `SaveManager` | `[Inject] private` | `null` | 코어 세이브 매니저 | 실제 저장/로드를 수행하는 CoreSystem.SaveManager |
| `turnManager` | `TurnManager` | `[Inject(Optional = true)] private` | `null` | 전투 턴 매니저 | 턴 변경 이벤트를 통해 자동 저장 트리거 |
| `stageManager` | `StageManager` | `[Inject(Optional = true)] private` | `null` | 스테이지 매니저 | 스테이지 완료/적 처치 이벤트를 통해 자동 저장 트리거 |
| `autoSaveEnabled` | `bool` | `private` (SerializeField) | `true` | 자동 저장 전체 On/Off | 옵션/디버그에서 자동 저장을 끄는 플래그 |
| `turnBasedAutosaveEnabled` | `bool` | `private` (SerializeField) | `true` | 턴/스테이지 이벤트 기반 저장 여부 | 시간 기반 자동 저장은 사용하지 않고 이벤트 기반만 활성화 |
| `lastSavedFrame` | `int` | `private` | `-1` | 마지막 저장이 일어난 프레임 | 중복 저장 방지용 |
| `lastSavedTrigger` | `string` | `private` | 빈 문자열 | 마지막 저장 트리거 이름 | 디버그/툴팁에서 최근 저장 원인 확인용 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Start()` | `void` | 없음 | `private` | 1. `Initialize()` 코루틴 시작 | 씬 시작 시 의존성 연결/이벤트 구독을 지연 초기화 |
| `Initialize()` | `IEnumerator` | 없음 | `private` | 1. `autoSaveEnabled`/`turnBasedAutosaveEnabled` 체크<br>2. 조건 만족 시 `HookRuntimeDependencies()` 호출<br>3. `SceneManager.sceneLoaded`에 `OnSceneLoaded` 구독<br>4. `isInitialized = true` 설정 | 전투/스테이지 씬에서 자동 저장 준비 |
| `OnTurnChanged(TurnManager.TurnType turnType)` | `async void` | `TurnType turnType` | `private` | 1. 자동 저장/초기화 여부 체크<br>2. 동일 프레임 중복 호출 방지<br>3. `lastSavedFrame`/`lastSavedTrigger` 갱신<br>4. `saveManager.SaveCurrentProgress("TurnChanged")` 호출<br>5. 예외를 GameLogger로 로깅 | 턴 변경 시 자동 저장 |
| `OnStageCompleted(StageData stageData)` | `async void` | `StageData stageData` | `private` | 1. 조건 체크 후 `SaveCurrentProgress("StageCompleted")` 호출 | 스테이지 완료 시 자동 저장 |
| `OnEnemyDefeated(ICharacter enemy)` | `async void` | `ICharacter enemy` | `private` | 1. 조건 체크 후 `SaveCurrentProgress("EnemyDefeated")` 호출 | 적 처치 시 자동 저장 |
| `TriggerManualAutoSave()` | `Task` | 없음 | `public` | 1. 조건 체크 후 `SaveCurrentProgress("Manual")` 호출<br>2. 예외 로깅 | 디버그/옵션에서 수동으로 자동 저장을 강제 실행 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `TurnManager` | DI 주입 및 이벤트 구독 | `OnTurnChanged` 이벤트 | 턴이 바뀔 때마다 자동 저장 트리거 |
| `StageManager` | DI 주입 및 이벤트 구독 | `OnStageCompleted`/`OnEnemyDefeated` 이벤트 | 스테이지 진행과 세이브 시스템을 연결 |
| `SaveManager` | `[Inject]` 필드 | `SaveCurrentProgress(reason)` 호출 | 실제 파일 저장/진행 데이터 직렬화 수행 |
| `SceneManager.sceneLoaded` | 이벤트 구독 | 씬 로드 → `ReinitializeForScene()` | StageScene 로드시 턴/스테이지 매니저 재연결 |

---

## 레거시/미사용 코드 정리

현재 SaveSystem 폴더 내에서는 CoreSystem.SaveManager/CombatInstaller/StageSystem 연계 및 grep 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
`StageProgressCollector`/`AutoSaveManager` 모두 실제 이벤트 경로와 SaveManager의 세이브/로드 흐름에 적극적으로 참여합니다.

---

## 폴더 구조

```text
Assets/Script/SaveSystem/
├── Data/
│   ├── CardSlotData.cs
│   └── StageProgressData.cs
├── Installer/
│   └── SaveSystemInstaller.cs
└── Manager/
    ├── AutoSaveManager.cs
    └── StageProgressCollector.cs
```


