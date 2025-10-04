# Combat State Machine System

전투 흐름을 명확한 상태로 관리하는 상태 패턴 기반 시스템입니다.

## 개요

기존 시스템의 문제점:
- 전투 흐름이 TurnManager와 CombatExecutionManager에 분산되어 있음
- 카드 드래그, 슬롯 이동, 카드 실행 등의 상태 구분이 없음
- 디버깅이 어렵고 버그 추적이 힘듦

**CombatStateMachine**은 이러한 문제를 해결합니다:
- 명확한 전투 상태 정의 (PlayerTurn, EnemyTurn, Execution 등)
- 각 상태별 허용/차단 액션 명시
- 상태 전환 로직 중앙 관리
- 디버깅 및 로깅 용이

## 아키텍처

### 핵심 컴포넌트

```
CombatStateMachine (MonoBehaviour)
├─ CombatStateContext (데이터 공유)
├─ ICombatState (인터페이스)
│  ├─ BaseCombatState (추상 클래스)
│  │  ├─ CombatInitState
│  │  ├─ PlayerTurnState
│  │  ├─ EnemyTurnState
│  │  ├─ CardExecutionState
│  │  ├─ SlotMovingState
│  │  ├─ EnemyDefeatedState
│  │  └─ BattleEndState
```

### 상태 흐름

**상태 패턴 완전 전환**

- **초기화**: CombatInitState에서 전투/대기 슬롯 초기화
- **플레이어 턴**: PlayerTurnState에서 플레이어 손패 생성 → 카드 드래그/드롭
- **카드 실행**: CardExecutionState에서 플레이어 손패 클리어 → 카드 효과 실행
- **슬롯 이동**: SlotMovingState에서 슬롯 전진 → 다음 턴 결정
- **적 턴**: EnemyTurnState에서 적 카드 자동 실행

**턴 시스템: 플레이어 1턴 ↔ 적 1턴 교대**

```
CombatInitState (초기화)
    ↓
PlayerTurnState (플레이어 턴 시작, 손패 생성)
    ↓ 플레이어 카드 드래그/드롭
CardExecutionState (손패 클리어, 플레이어 카드 실행)
    ↓
SlotMovingState (슬롯 전진)
    ↓ 배틀 슬롯에 적 카드 있음?
    ├─ YES → EnemyTurnState (적 턴)
    │        ↓
    │        CardExecutionState (적 카드 실행)
    │        ↓
    │        SlotMovingState (슬롯 전진)
    │        ↓
    │        PlayerTurnState (플레이어 턴, 손패 재생성)
    │
    └─ NO → PlayerTurnState (플레이어 턴 계속)

적 처치 → EnemyDefeatedState → CombatInitState (새 적 셋업)
승리/패배 → BattleEndState
```

## 상태 설명

### 1. CombatInitState
전투 초기화 상태
- 게임 시작
- 턴 리셋
- 초기 슬롯 셋업 대기
- 완료 후 PlayerTurnState로 전환

### 2. PlayerTurnState
플레이어 턴 상태
- **허용**: 플레이어 카드 드래그 (핸드에서 슬롯으로)
- **차단**: 적 카드 자동 실행, 슬롯 이동, 턴 전환
- **주요 기능**:
  - TurnManager를 Player 턴으로 설정
  - 플레이어 손패 생성 (PlayerHandManager.GenerateInitialHand)
- 카드 배치 시 CardExecutionState로 전환

### 3. EnemyTurnState
적 턴 상태
- **허용**: 적 카드 자동 실행
- **차단**: 플레이어 카드 드래그, 슬롯 이동, 턴 전환
- **주요 기능**:
  - TurnManager를 Enemy 턴으로 설정
  - 플레이어 손패 완전 정리 (적 턴에는 플레이어가 카드를 낼 수 없음)
  - 배틀 슬롯의 적 카드를 즉시 CardExecutionState로 전환

### 4. CardExecutionState
카드 실행 상태
- **차단**: 모든 사용자 입력
- **주요 기능**:
  - 플레이어 카드 실행 시: 손패 클리어 → 카드 효과 실행
  - 적 카드 실행 시: 카드 효과 실행
  - 애니메이션 재생
  - CombatExecutionManager를 통한 실행
- 완료 후 SlotMovingState로 전환

### 5. SlotMovingState
슬롯 이동 상태
- **허용**: 슬롯 이동
- **차단**: 카드 드래그, 적 카드 실행, 턴 전환
- **주요 기능**:
  - TurnManager.AdvanceQueueAtTurnStartRoutine() 호출
  - 대기 슬롯 → 배틀 슬롯 이동 (애니메이션 포함)
  - 다음 턴 결정 (현재 턴과 배틀 슬롯 상태 기반):
    - 플레이어 턴 종료 → 적 카드 있으면 EnemyTurnState, 없으면 PlayerTurnState
    - 적 턴 종료 → 항상 PlayerTurnState

### 6. EnemyDefeatedState
적 처치 상태
- **트리거**: 적이 사망했을 때
- **차단**: 모든 액션
- **새로운 기능**: 
  - 플레이어 핸드 완전 정리
  - 모든 전투/대기 슬롯 정리 (플레이어 카드 + 적 카드 모두 제거)
  - 적 캐시 초기화
- 완료 후 CombatInitState로 전환하여 새로운 적 셋업 시작

### 7. BattleEndState
전투 종료 상태
- **차단**: 모든 액션
- 승리/패배 처리
- 게임 종료

## 사용 방법

### Zenject 바인딩 (자동)

`CombatInstaller.cs`에서 자동으로 바인딩됩니다:

```csharp
// CombatInstaller.cs
private void BindCombatStateMachine()
{
    var stateMachine = FindFirstObjectByType<CombatStateMachine>();
    if (stateMachine == null)
    {
        var go = new GameObject("CombatStateMachine");
        stateMachine = go.AddComponent<CombatStateMachine>();
    }
    Container.Bind<CombatStateMachine>().FromInstance(stateMachine).AsSingle();
    Container.Inject(stateMachine);
}
```

### 전투 시작

```csharp
// GameStartupController 등에서
var stateMachine = FindFirstObjectByType<CombatStateMachine>();
if (stateMachine != null)
{
    stateMachine.StartCombat();
}
```

### 상태 쿼리

```csharp
// 현재 플레이어가 카드를 드래그할 수 있는지 확인
if (stateMachine.CanPlayerDragCard())
{
    // 카드 드래그 허용
}

// 현재 적 카드 자동 실행이 가능한지 확인
if (stateMachine.CanEnemyAutoExecute())
{
    // 적 카드 실행
}

// 현재 상태 확인
var currentState = stateMachine.GetCurrentState();
Debug.Log($"현재 상태: {currentState.StateName}");
```

### 이벤트 구독

```csharp
stateMachine.OnStateChanged += (prevState, newState) =>
{
    Debug.Log($"상태 전환: {prevState?.StateName} → {newState.StateName}");
};

stateMachine.OnCombatStarted += () =>
{
    Debug.Log("전투 시작!");
};

stateMachine.OnCombatEnded += (isVictory) =>
{
    Debug.Log($"전투 종료 - {(isVictory ? "승리" : "패배")}");
};
```

## 기존 시스템과의 통합

### CardDropService 통합

`CardDropService`는 상태 머신을 확인하여 드롭 허가를 결정합니다:

```csharp
// CardDropService.cs
public bool TryDropCard(ISkillCard card, SkillCardUI ui, object slot, out string message)
{
    // 상태 머신 검증
    if (stateMachine != null)
    {
        if (!stateMachine.CanPlayerDragCard())
        {
            message = "현재 상태에서 카드 배치가 허용되지 않습니다.";
            return false;
        }
    }

    // ... 카드 드롭 처리

    // 상태 머신에 알림
    if (stateMachine != null)
    {
        stateMachine.OnPlayerCardPlaced(card, slotPosition);
    }
}
```

### TurnManager 통합

**리팩토링 완료: 완전한 책임 분리**

- **TurnManager**: 순수 슬롯 물리적 이동만 담당
  - `AdvanceQueueAtTurnStartRoutine()`: 슬롯 전진 처리
  - `SetTurn()`: 턴 타입 설정
  - `GetCardInSlot()`: 슬롯 카드 조회
  - **제거됨**: `SwitchTurn()`, `NextTurn()`, `ProceedToNextTurn()` (상태 패턴으로 이동)

- **CombatStateMachine**: 모든 전투 흐름 제어
  - 턴 전환 결정
  - 손패 생성/정리 타이밍 제어
  - 상태 전환 관리

**슬롯 이동 흐름:**
1. SlotMovingState가 TurnManager.AdvanceQueueAtTurnStartRoutine() 호출
2. TurnManager가 슬롯 전진 처리 (애니메이션 포함)
3. SlotMovingState가 다음 턴 결정 (현재 턴과 배틀 슬롯 상태 기반)

## 새로운 상태 추가 방법

1. **상태 클래스 생성**

```csharp
using Game.CombatSystem.State;

public class MyCustomState : BaseCombatState
{
    public override string StateName => "MyCustom";

    // 허용/차단 설정
    public override bool AllowPlayerCardDrag => false;
    public override bool AllowEnemyAutoExecution => false;

    public override void OnEnter(CombatStateContext context)
    {
        base.OnEnter(context);
        LogStateTransition("내 커스텀 상태 진입");

        // 상태별 로직 실행
        DoCustomLogic(context);
    }

    private void DoCustomLogic(CombatStateContext context)
    {
        // 커스텀 로직

        // 다음 상태로 전환
        var nextState = new PlayerTurnState();
        RequestTransition(context, nextState);
    }
}
```

2. **상태 전환**

```csharp
// 다른 상태에서
var customState = new MyCustomState();
RequestTransition(context, customState);
```

## 폴백 (Fallback) 메커니즘

상태 머신이 없어도 기존 시스템이 동작하도록 폴백이 구현되어 있습니다:

```csharp
// CardDropService.cs
if (stateMachine != null)
{
    // 상태 머신으로 검증
    if (!stateMachine.CanPlayerDragCard())
        return false;
}
else
{
    // 기존 방식으로 검증
    if (!turnManager.IsPlayerTurn())
        return false;
}
```

이를 통해:
- 점진적 마이그레이션 가능
- 테스트 중에도 안정성 유지
- 상태 머신 비활성화 시에도 동작

## 디버깅

### 1. 상태 정보 출력

Inspector에서 Context Menu 사용:
- CombatStateMachine → 우클릭 → "현재 상태 정보 출력"

코드에서:
```csharp
stateMachine.LogCurrentState();
```

### 2. 상태 전환 로그

모든 상태 전환은 자동으로 로깅됩니다:
```
[CombatStateMachine] 상태 전환: PlayerTurn → CardExecution
[CombatState] 진입: CardExecution
[CombatState] 종료: CardExecution
[CombatStateMachine] 상태 전환: CardExecution → SlotMoving
```

### 3. 허가/차단 로그

상태별 액션 차단 시 경고 로그:
```
[CardDropService] 현재 상태에서 카드 배치가 허용되지 않습니다. (상태: CardExecution)
[TurnManager] 현재 상태에서 적 카드 자동 실행 불가: CardExecution
```

## 성능 고려사항

- **상태 객체**: 매번 new로 생성 (가비지 발생 최소)
- **컨텍스트**: 재사용 (단일 인스턴스)
- **FindFirstObjectByType**: 지연 초기화로 최소화
- **이벤트**: C# Action 사용 (Unity Event보다 빠름)

## 주의사항

1. **상태 전환은 항상 RequestTransition() 사용**
   ```csharp
   // Good
   RequestTransition(context, nextState);

   // Bad - 직접 호출하지 말것
   context.StateMachine.ChangeState(nextState);
   ```

2. **컨텍스트 검증**
   ```csharp
   if (context == null || !context.ValidateManagers())
   {
       LogError("컨텍스트 검증 실패");
       return;
   }
   ```

3. **비동기 작업은 코루틴 사용**
   ```csharp
   private MonoBehaviour _coroutineRunner;

   public override void OnEnter(CombatStateContext context)
   {
       _coroutineRunner = context.StateMachine;
       _coroutineRunner.StartCoroutine(MyAsyncTask(context));
   }
   ```

## 향후 확장

### 계획된 기능

1. **상태 저장/복원**
   - 전투 중단 후 재개
   - 세이브/로드 시스템 통합

2. **상태 히스토리**
   - 상태 전환 기록
   - 되돌리기 기능

3. **조건부 전환**
   - 특정 조건에서만 상태 전환 허용
   - 전환 검증 로직

4. **서브스테이트**
   - 상태 내부의 세부 단계
   - 계층적 상태 관리

## 문제 해결

### Q: 상태 머신이 초기화되지 않음
**A**: CombatInstaller가 실행되었는지 확인. Scene Context가 있어야 함.

### Q: 카드 드래그가 안 됨
**A**: 현재 상태 확인. PlayerTurnState가 아니면 드래그 불가.

### Q: 상태 전환이 안 됨
**A**: 로그 확인. 컨텍스트 검증 실패 또는 매니저 누락 가능성.

### Q: 적 카드가 자동 실행 안 됨
**A**: EnemyTurnState인지 확인. 다른 상태에서는 실행 차단됨.

## 참고 자료

- [상태 패턴 (Design Patterns)](https://refactoring.guru/design-patterns/state)
- [Unity Zenject 문서](https://github.com/modesttree/Zenject)
- 프로젝트 내 `CombatStateMachine.cs` 주석 참고
