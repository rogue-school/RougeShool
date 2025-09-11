# CombatSystem 개발 문서

## 📋 시스템 개요
CombatSystem은 게임의 전투 로직을 관리하는 핵심 시스템입니다. 전투 상태, 턴 관리, 카드 드래그 앤 드롭, 슬롯 관리 등 다양한 기능을 통합적으로 관리합니다.

## 🏗️ 폴더 구조
```
CombatSystem/
├── Core/             # 핵심 로직 (2개 파일)
├── Manager/          # 매니저 클래스 (3개 파일)
├── Interface/        # 인터페이스 (8개 파일)
├── State/            # 상태 패턴 (6개 파일)
├── Service/          # 서비스 클래스 (3개 파일)
├── Data/             # 데이터 클래스 (1개 파일)
├── Event/            # 이벤트 시스템 (1개 파일)
├── Utility/          # 유틸리티 (2개 파일)
├── Context/          # 컨텍스트 (2개 파일)
└── UI/               # UI 관련 (1개 파일)
```

## 📁 주요 컴포넌트

### Core 폴더 (2개 파일)
- **CombatInstaller.cs**: 전투 시스템 의존성 주입 설정
- **CombatContext.cs**: 전투 컨텍스트

### Manager 폴더 (3개 파일)
- **CombatFlowCoordinator.cs**: 전투 플로우 조정
- **CombatTurnManager.cs**: 턴 매니저
- **CombatSlotManager.cs**: 전투 슬롯 매니저

### Interface 폴더 (8개 파일)
- **ICombatState.cs**: 전투 상태 인터페이스
- **ICombatAction.cs**: 전투 행동 인터페이스
- **ICombatEffect.cs**: 전투 효과 인터페이스
- **ICombatCard.cs**: 전투 카드 인터페이스
- **ICombatCharacter.cs**: 전투 캐릭터 인터페이스
- **ICombatUI.cs**: 전투 UI 인터페이스
- **ICombatValidator.cs**: 전투 검증 인터페이스
- **ICombatExecutor.cs**: 전투 실행 인터페이스

### State 폴더 (6개 파일)
- **CombatFirstAttackState.cs**: 첫 번째 공격 상태
- **CombatPlayerInputState.cs**: 플레이어 입력 상태
- **CombatPrepareState.cs**: 준비 상태
- **CombatResultState.cs**: 결과 상태
- **CombatSecondAttackState.cs**: 두 번째 공격 상태
- **CombatVictoryState.cs**: 승리 상태

### Service 폴더 (3개 파일)
- **CombatExecutorService.cs**: 전투 실행 서비스
- **CombatPreparationService.cs**: 전투 준비 서비스
- **TurnCardRegistry.cs**: 턴 카드 등록기

### Data 폴더 (1개 파일)
- **SlotOwner.cs**: 슬롯 소유자 열거형

### Event 폴더 (1개 파일)
- **CombatEventSystem.cs**: 전투 이벤트 시스템

### Utility 폴더 (2개 파일)
- **CardValidator.cs**: 카드 검증기
- **CharacterDeathHandler.cs**: 캐릭터 사망 핸들러

### Context 폴더 (2개 파일)
- **CombatContext.cs**: 전투 컨텍스트
- **TurnContext.cs**: 턴 컨텍스트

### UI 폴더 (1개 파일)
- **CombatUI.cs**: 전투 UI 컨트롤러

## 🎯 주요 기능

### 1. 전투 상태 관리
- **상태 패턴**: 다양한 전투 상태를 상태 패턴으로 관리
- **상태 전환**: 조건에 따른 자동 상태 전환

### 2. 턴 관리
- **턴 순서**: 캐릭터 속도에 따른 턴 순서 결정
- **턴 제한**: 턴당 행동 제한

### 3. 카드 시스템
- **카드 실행**: 카드 효과 실행
- **카드 검증**: 실행 가능 여부 검증

### 4. 전투 플로우
- **전투 준비**: 전투 시작 전 준비
- **입력 제어**: 플레이어 입력 처리

## 📊 주요 클래스 및 메서드

### CombatFlowCoordinator 클래스
- **StartCombat()**: 전투 시작
- **PrepareCombat()**: 전투 준비
- **ExecuteCombatPhase()**: 전투 페이즈 실행
- **HandleCombatResult()**: 전투 결과 처리
- **EnablePlayerInput()**: 플레이어 입력 활성화
- **DisablePlayerInput()**: 플레이어 입력 비활성화
- **IsEnemyFirst**: 적 선공 여부 (프로퍼티)

### CombatTurnManager 클래스
- **Initialize()**: 턴 매니저 초기화
- **SetState(ICombatTurnState state)**: 상태 설정
- **CanProceedToNextTurn()**: 다음 턴 진행 가능 여부
- **ProceedToNextTurn()**: 다음 턴으로 진행
- **RegisterCard(ISkillCard card, CombatSlotPosition position)**: 카드 등록
- **ClearRegisteredCards()**: 등록된 카드 초기화
- **OnTurnReadyChanged**: 턴 준비 상태 변경 이벤트

### CombatExecutorService 클래스
- **ExecuteCombatPhase()**: 전체 전투 페이즈 실행 (코루틴)
- **PerformAttack(CombatSlotPosition position)**: 지정 슬롯 공격 실행 (코루틴)
- **ExecuteCard(ISkillCard card, CombatSlotPosition position)**: 카드 실행
- **CreateExecutionContext()**: 실행 컨텍스트 생성
- **ProcessCardExecution()**: 카드 실행 처리

### CombatPreparationService 클래스
- **PrepareCombat()**: 전투 준비 (코루틴)
- **SpawnEnemy()**: 적 스폰
- **SetupPlayerCards()**: 플레이어 카드 설정
- **InitializeTurnSystem()**: 턴 시스템 초기화
- **RegisterCombatSlots()**: 전투 슬롯 등록

### CombatStartupManager 클래스
- **FindInitializerSteps()**: 초기화 스텝 수집
- **ExecuteInitializationSteps()**: 초기화 스텝 실행 (코루틴)
- **EnablePlayerInput()**: 플레이어 입력 활성화
- **OnInitializationComplete()**: 초기화 완료 처리

### ICombatTurnState 인터페이스
- **ExecuteState()**: 상태 실행
- **CanTransitionTo(ICombatTurnState nextState)**: 상태 전환 가능 여부
- **OnEnter()**: 상태 진입 시 호출
- **OnExit()**: 상태 종료 시 호출

### ICombatFlowCoordinator 인터페이스
- **StartCombat()**: 전투 시작
- **PrepareCombat()**: 전투 준비
- **ExecuteCombatPhase()**: 전투 페이즈 실행
- **HandleCombatResult()**: 전투 결과 처리
- **EnablePlayerInput()**: 플레이어 입력 활성화
- **DisablePlayerInput()**: 플레이어 입력 비활성화

### ICombatTurnManager 인터페이스
- **SetState(ICombatTurnState state)**: 상태 설정
- **CanProceedToNextTurn()**: 다음 턴 진행 가능 여부
- **ProceedToNextTurn()**: 다음 턴으로 진행
- **RegisterCard(ISkillCard card, CombatSlotPosition position)**: 카드 등록
- **ClearRegisteredCards()**: 등록된 카드 초기화

## 🔧 사용 방법

### 기본 사용법
```csharp
// 전투 시작
CombatFlowCoordinator combatCoordinator = FindObjectOfType<CombatFlowCoordinator>();
combatCoordinator.StartCombat(stageData);

// 턴 매니저를 통한 상태 관리
CombatTurnManager turnManager = FindObjectOfType<CombatTurnManager>();
turnManager.Initialize();
turnManager.SetState(new CombatPrepareState());

// 카드 등록
turnManager.RegisterCard(skillCard, CombatSlotPosition.FIRST);

// 턴 진행 가능 여부 확인
if (turnManager.CanProceedToNextTurn())
{
    turnManager.ProceedToNextTurn();
}
```

### 전투 실행 서비스 사용법
```csharp
// CombatExecutorService를 통한 전투 실행
CombatExecutorService executorService = new CombatExecutorService(
    combatSlotRegistry, 
    contextProvider, 
    cardExecutor, 
    enemyHandManager
);

// 전체 전투 페이즈 실행
StartCoroutine(executorService.ExecuteCombatPhase());

// 특정 슬롯 공격 실행
StartCoroutine(executorService.PerformAttack(CombatSlotPosition.FIRST));

// 카드 실행
executorService.ExecuteCard(skillCard, CombatSlotPosition.SECOND);
```

### 전투 준비 서비스 사용법
```csharp
// CombatPreparationService를 통한 전투 준비
CombatPreparationService preparationService = new CombatPreparationService(
    playerManager, 
    enemySpawnerManager, 
    enemyManager, 
    enemyHandManager, 
    turnCardRegistry, 
    placementService, 
    turnManager, 
    slotSelector, 
    slotRegistry
);

// 전투 준비 실행
StartCoroutine(preparationService.PrepareCombat());

// 적 스폰
preparationService.SpawnEnemy();

// 플레이어 카드 설정
preparationService.SetupPlayerCards();
```

### 초기화 시스템 사용법
```csharp
// CombatStartupManager를 통한 초기화
CombatStartupManager startupManager = FindObjectOfType<CombatStartupManager>();

// 초기화 스텝 실행
StartCoroutine(startupManager.ExecuteInitializationSteps());

// 초기화 완료 후 플레이어 입력 활성화
startupManager.OnInitializationComplete();
```

### 상태 패턴 사용법
```csharp
// 전투 상태 구현
public class CustomCombatState : ICombatTurnState
{
    public void ExecuteState()
    {
        // 상태별 로직 실행
    }
    
    public bool CanTransitionTo(ICombatTurnState nextState)
    {
        // 상태 전환 조건 확인
        return true;
    }
    
    public void OnEnter()
    {
        // 상태 진입 시 처리
    }
    
    public void OnExit()
    {
        // 상태 종료 시 처리
    }
}

// 상태 설정
turnManager.SetState(new CustomCombatState());
```

### 의존성 주입 설정
```csharp
// CombatInstaller를 통한 DI 설정
public class CustomCombatInstaller : MonoInstaller
{
    public override void InstallBindings()
    {
        Container.Bind<ICombatFlowCoordinator>().To<CombatFlowCoordinator>().AsSingle();
        Container.Bind<ICombatTurnManager>().To<CombatTurnManager>().AsSingle();
        Container.Bind<ICombatExecutorService>().To<CombatExecutorService>().AsSingle();
        Container.Bind<ICombatPreparationService>().To<CombatPreparationService>().AsSingle();
    }
}
```

## 🏗️ 아키텍처 패턴

### 1. 상태 패턴 (State Pattern)
- **전투 상태**: 각 전투 단계별 상태 관리
- **상태 전환**: 조건에 따른 자동 상태 전환

### 2. 서비스 패턴 (Service Pattern)
- **전투 서비스**: 특정 기능을 담당하는 서비스
- **의존성 주입**: 서비스 간 의존성 관리

### 3. 옵저버 패턴 (Observer Pattern)
- **이벤트 시스템**: 전투 이벤트 발생 및 구독
- **상태 변경**: 상태 변경 시 알림

## 🔧 기술적 구현 세부사항

### 성능 최적화
- **메모리 관리**: 상태 객체 풀링을 통한 GC 압박 최소화
- **프레임 최적화**: 상태 전환 시 Update() 호출 최소화
- **렌더링 최적화**: UI 업데이트 빈도 최적화
- **로딩 최적화**: 전투 데이터 사전 로딩 및 캐싱

### 스레드 안전성
- **동시성 제어**: 상태 전환 시 락을 통한 동시성 제어
- **비동기 처리**: 코루틴을 통한 비동기 전투 처리
- **이벤트 처리**: 스레드 안전한 전투 이벤트 시스템
- **데이터 동기화**: 전투 상태 데이터 동기화

### 메모리 관리
- **생명주기 관리**: 전투 상태 객체의 생성/소멸 관리
- **리소스 해제**: 전투 종료 시 리소스 정리
- **메모리 누수 방지**: 이벤트 구독 해제, 상태 참조 해제
- **프로파일링**: 전투 중 메모리 사용량 모니터링

## 🏗️ 시스템 아키텍처

### 의존성 다이어그램
```mermaid
graph TD
    A[CombatFlowCoordinator] --> B[CombatTurnManager]
    A --> C[CombatSlotManager]
    A --> D[CombatExecutorService]
    
    B --> E[TurnCardRegistry]
    C --> F[CombatSlotPosition]
    D --> G[CardValidator]
    
    H[CombatStateMachine] --> I[CombatFirstAttackState]
    H --> J[CombatSecondAttackState]
    H --> K[CombatPlayerInputState]
    H --> L[CombatResultState]
    
    A --> H
    
    style A fill:#ff9999
    style H fill:#ffcc99
    style B fill:#99ccff
    style C fill:#99ccff
    style D fill:#99ccff
```

### 클래스 다이어그램
```mermaid
classDiagram
    class ICombatState {
        <<interface>>
        +Enter() void
        +Exit() void
        +Update() void
    }
    
    class CombatStateMachine {
        -currentState: ICombatState
        -states: Dictionary~string, ICombatState~
        +ChangeState(state) void
        +GetCurrentState() ICombatState
    }
    
    class CombatFlowCoordinator {
        -turnManager: CombatTurnManager
        -slotManager: CombatSlotManager
        -stateMachine: CombatStateMachine
        +StartCombat(stageData) void
        +ProcessTurn() void
    }
    
    class CombatTurnManager {
        -currentTurn: int
        -turnOrder: List~ICharacter~
        +NextTurn() void
        +GetCurrentCharacter() ICharacter
    }
    
    ICombatState <|.. CombatFirstAttackState
    ICombatState <|.. CombatSecondAttackState
    ICombatState <|.. CombatPlayerInputState
    CombatStateMachine --> ICombatState
    CombatFlowCoordinator --> CombatStateMachine
    CombatFlowCoordinator --> CombatTurnManager
```

### 시퀀스 다이어그램
```mermaid
sequenceDiagram
    participant Player as Player
    participant CFC as CombatFlowCoordinator
    participant CSM as CombatStateMachine
    participant CTM as CombatTurnManager
    participant CSL as CombatSlotManager
    
    Player->>CFC: StartCombat()
    CFC->>CSM: ChangeState(PrepareState)
    CFC->>CTM: InitializeTurnOrder()
    CFC->>CSL: InitializeSlots()
    
    Player->>CFC: PlaceCard(card, slot)
    CFC->>CSL: RegisterCard(slot, card)
    
    Player->>CFC: StartTurn()
    CFC->>CSM: ChangeState(FirstAttackState)
    CSM->>CTM: ExecuteFirstAttack()
    CTM-->>CSM: Attack Complete
    CSM->>CFC: ChangeState(SecondAttackState)
    CSM->>CTM: ExecuteSecondAttack()
    CTM-->>CSM: Attack Complete
    CSM->>CFC: ChangeState(ResultState)
```

## 📚 참고 자료

### 관련 문서
- [Unity 코루틴](https://docs.unity3d.com/Manual/Coroutines.html)
- [상태 패턴](https://refactoring.guru/design-patterns/state)
- [Zenject 의존성 주입](https://github.com/modesttree/Zenject)

## 📝 변경 기록(Delta)
- 형식: `YYYY-MM-DD | 작성자 | 변경 요약 | 영향도(코드/씬/문서)`

- 2025-01-27 | Maintainer | CombatSystem 개발 문서 초기 작성 | 문서
- 2025-01-27 | Maintainer | 실제 폴더 구조 반영 및 파일 수 정정 | 문서
- 2025-01-27 | Maintainer | 실제 코드 분석 기반 구체적 클래스/메서드/서비스 정보 추가 | 문서
