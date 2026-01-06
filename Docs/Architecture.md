# RougeShool 아키텍처 문서

**작성일**: 2024년  
**대상 프로젝트**: RougeShool (Unity 로그라이크 카드 게임)  
**목적**: 프로젝트의 전체 아키텍처 구조, 설계 원칙, 시스템 간 관계를 문서화

---

## 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [아키텍처 원칙](#아키텍처-원칙)
3. [시스템 계층 구조](#시스템-계층-구조)
4. [시스템별 상세 설명](#시스템별-상세-설명)
5. [의존성 주입 구조](#의존성-주입-구조)
6. [데이터 흐름](#데이터-흐름)
7. [주요 디자인 패턴](#주요-디자인-패턴)
8. [아키텍처 다이어그램](#아키텍처-다이어그램)

---

## 프로젝트 개요

### 프로젝트 정보

- **게임 장르**: 로그라이크 카드 배틀 게임
- **엔진**: Unity (2D)
- **언어**: C#
- **DI 프레임워크**: Zenject
- **애니메이션**: DOTween Pro
- **총 클래스 수**: 288개
- **시스템 수**: 10개

### 핵심 특징

- **인터페이스 기반 설계**: 모든 시스템이 인터페이스를 통해 통신
- **의존성 주입**: Zenject를 통한 완전한 DI 패턴
- **이벤트 기반 아키텍처**: Update() 루프 최소화, 이벤트 기반 통신
- **시스템 분리**: 명확한 책임 분리와 느슨한 결합

---

## 아키텍처 원칙

### SOLID 원칙

#### 1. Single Responsibility Principle (SRP)
- 각 클래스는 단일 책임만 가짐
- 예: `CardValidator`는 카드 검증만, `CardExecutor`는 실행만 담당

#### 2. Open-Closed Principle (OCP)
- 확장에는 열려있고 수정에는 닫혀있음
- 예: `CardEffect` 추상 클래스를 상속하여 새로운 효과 추가

#### 3. Liskov Substitution Principle (LSP)
- 인터페이스 구현체는 언제든 교체 가능
- 예: `ICharacter` 인터페이스로 `PlayerCharacter`와 `EnemyCharacter` 교체 가능

#### 4. Interface Segregation Principle (ISP)
- 클라이언트는 사용하지 않는 인터페이스에 의존하지 않음
- 예: `ICardValidator`, `ICardExecutor` 등 세분화된 인터페이스

#### 5. Dependency Inversion Principle (DIP)
- 구체 클래스가 아닌 추상화에 의존
- 예: 모든 매니저가 인터페이스를 통해 주입됨

### 설계 원칙

#### 1. 의존성 주입 (Dependency Injection)
- **Zenject 사용**: 모든 의존성은 Zenject를 통해 주입
- **싱글톤 금지**: 싱글톤 패턴 대신 Zenject의 `AsSingle()` 사용
- **인터페이스 우선**: 구체 클래스가 아닌 인터페이스에 의존

#### 2. 이벤트 기반 통신
- **Update() 최소화**: 이벤트 기반으로 상태 변경 전파
- **느슨한 결합**: 시스템 간 직접 참조 최소화, 이벤트로 통신

#### 3. 계층 구조
```
┌─────────────────────────────────────┐
│      CoreSystem (기반 계층)          │
│  - GameStateManager                  │
│  - SceneTransitionManager            │
│  - AudioManager                      │
│  - GameLogger                        │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   CharacterSystem (도메인 계층)     │
│  - PlayerCharacter                  │
│  - EnemyCharacter                   │
│  - CharacterBase                    │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   CombatSystem (전투 계층)           │
│  - CombatStateMachine               │
│  - TurnManager                      │
│  - CombatExecutionManager           │
└─────────────────────────────────────┘
              ↓
┌─────────────────────────────────────┐
│   SkillCardSystem (카드 계층)       │
│  - SkillCardFactory                │
│  - CardExecutor                    │
│  - CardCirculationSystem           │
└─────────────────────────────────────┘
```

---

## 시스템 계층 구조

### 계층별 역할

| 계층 | 시스템 | 역할 | 의존성 |
|------|--------|------|--------|
| **기반 계층** | CoreSystem | 게임 전역 서비스 (오디오, 씬 전환, 로깅) | 없음 |
| **도메인 계층** | CharacterSystem | 캐릭터 데이터 및 로직 | CoreSystem |
| **전투 계층** | CombatSystem | 전투 흐름 및 상태 관리 | CoreSystem, CharacterSystem |
| **카드 계층** | SkillCardSystem | 카드 정의, 실행, 순환 | CoreSystem, CharacterSystem, CombatSystem |
| **아이템 계층** | ItemSystem | 아이템 및 보상 시스템 | CoreSystem, CharacterSystem |
| **스테이지 계층** | StageSystem | 스테이지 진행 및 적 생성 | CoreSystem, CharacterSystem, CombatSystem |
| **UI 계층** | UISystem | 메뉴 및 설정 UI | CoreSystem |
| **유틸리티 계층** | UtilitySystem | 공통 유틸리티 및 헬퍼 | 없음 |
| **VFX 계층** | VFXSystem | 시각 효과 및 풀링 | CoreSystem |
| **튜토리얼 계층** | TutorialSystem | 튜토리얼 시스템 | CoreSystem |

---

## 시스템별 상세 설명

### 1. CoreSystem (기반 시스템)

**위치**: `Assets/Script/CoreSystem/`  
**클래스 수**: 25개  
**역할**: 게임 전역 기반 서비스 제공

#### 주요 컴포넌트

- **CoreSystemInstaller**: 코어 시스템 DI 바인딩
- **GameStateManager**: 게임 상태 관리 (메뉴/전투/스테이지)
- **SceneTransitionManager**: 씬 전환 및 페이드 연출
- **AudioManager**: BGM/SFX 재생 관리
- **GameLogger**: 통합 로깅 시스템 (카테고리별)
- **CoroutineRunner**: 전역 코루틴 실행기

#### 특징

- 모든 시스템의 기반이 되는 최하위 계층
- 다른 시스템에 의존하지 않음
- 전역 서비스 제공 (싱글톤 패턴 대신 Zenject AsSingle)

---

### 2. CharacterSystem (캐릭터 시스템)

**위치**: `Assets/Script/CharacterSystem/`  
**클래스 수**: 41개  
**역할**: 플레이어/적 캐릭터 데이터 및 로직

#### 주요 컴포넌트

- **CharacterBase**: 캐릭터 공통 베이스 클래스
- **PlayerCharacter**: 플레이어 캐릭터 구현
- **EnemyCharacter**: 적 캐릭터 구현
- **PlayerManager**: 플레이어 매니저
- **EnemyManager**: 적 매니저
- **EnemyPhaseData**: 적 페이즈 데이터 (ScriptableObject)

#### 특징

- `ICharacter` 인터페이스로 통일된 캐릭터 추상화
- ScriptableObject 기반 데이터 설계
- 효과 시스템 (`ICharacterEffect`) 지원

---

### 3. CombatSystem (전투 시스템)

**위치**: `Assets/Script/CombatSystem/`  
**클래스 수**: 63개  
**역할**: 전투 흐름, 턴 관리, 상태 머신

#### 주요 컴포넌트

- **CombatInstaller**: 전투 씬 DI 바인딩
- **CombatStateMachine**: 전투 상태 머신
  - `PlayerTurnState`: 플레이어 턴
  - `EnemyTurnState`: 적 턴
  - `CardExecutionState`: 카드 실행
  - `SummonState`: 적 소환
  - `CombatVictoryState`: 승리
  - `CombatGameOverState`: 게임 오버
- **TurnManager**: 턴 관리 및 이벤트
- **CombatExecutionManager**: 카드 실행 관리
- **SlotMovementController**: 슬롯 이동 및 애니메이션

#### 특징

- 상태 패턴을 통한 전투 흐름 관리
- 이벤트 기반 턴 시스템
- 슬롯 기반 카드 배치 시스템

---

### 4. SkillCardSystem (스킬 카드 시스템)

**위치**: `Assets/Script/SkillCardSystem/`  
**클래스 수**: 103개  
**역할**: 카드 정의, 팩토리, 효과, 덱/핸드 관리

#### 주요 컴포넌트

- **CardInstaller**: 카드 시스템 DI 바인딩
- **SkillCardFactory**: 카드 생성 팩토리
- **CardExecutor**: 카드 실행 엔진
- **CardCirculationSystem**: 카드 순환 시스템 (덱→핸드→묘지)
- **PlayerHandManager**: 플레이어 핸드 관리
- **PlayerDeckManager**: 플레이어 덱 관리
- **SkillCardDefinition**: 카드 정의 (ScriptableObject)

#### 특징

- 팩토리 패턴으로 카드 생성
- 전략 패턴으로 효과 시스템 (`IEffectStrategy`)
- 커맨드 패턴으로 효과 실행 (`IEffectCommand`)

---

### 5. ItemSystem (아이템 시스템)

**위치**: `Assets/Script/ItemSystem/`  
**클래스 수**: 52개  
**역할**: 액티브/패시브 아이템, 보상 시스템

#### 주요 컴포넌트

- **ItemService**: 아이템 서비스 (전역)
- **RewardGenerator**: 보상 생성기
- **DefaultRewardService**: 기본 보상 서비스
- **ItemDefinition**: 아이템 정의 (ScriptableObject)
- **ActiveItem**: 액티브 아이템 런타임
- **PassiveItemDefinition**: 패시브 아이템 정의

#### 특징

- 서비스 패턴으로 아이템 관리
- 보상 시스템과 통합
- ScriptableObject 기반 데이터

---

### 6. StageSystem (스테이지 시스템)

**위치**: `Assets/Script/StageSystem/`  
**클래스 수**: 8개  
**역할**: 스테이지 진행, 적 생성, 보상 트리거

#### 주요 컴포넌트

- **StageManager**: 스테이지 매니저
- **StageProgressController**: 스테이지 진행 컨트롤러
- **StageData**: 스테이지 데이터 (ScriptableObject)
- **StageFlowStateMachine**: 스테이지 흐름 상태 머신

#### 특징

- CombatSystem과 강하게 연동
- 적 생성 및 보상 트리거 관리
- 스테이지 진행 상태 관리

---

### 7. UISystem (UI 시스템)

**위치**: `Assets/Script/UISystem/`  
**클래스 수**: 11개  
**역할**: 메뉴, 설정 UI

#### 주요 컴포넌트

- **MainMenuController**: 메인 메뉴 컨트롤러
- **SettingsUIController**: 설정 UI 컨트롤러
- **PanelManager**: 패널 관리

#### 특징

- CoreSystem 인터페이스를 DI로 사용
- 씬 컴포넌트 기반

---

### 8. UtilitySystem (유틸리티 시스템)

**위치**: `Assets/Script/UtilitySystem/`  
**클래스 수**: 9개  
**역할**: 공통 유틸리티 및 헬퍼

#### 주요 컴포넌트

- **GameContext**: 게임 컨텍스트
- **BaseTooltipManager**: 툴팁 매니저 베이스
- **UIAnimationHelper**: UI 애니메이션 헬퍼
- **HoverEffectHelper**: 호버 효과 헬퍼
- **TransformExtensions**: Transform 확장 메서드

#### 특징

- 정적 유틸리티 및 확장 메서드
- 다른 시스템에 의존하지 않음

---

### 9. VFXSystem (VFX 시스템)

**위치**: `Assets/Script/VFXSystem/`  
**클래스 수**: 7개  
**역할**: 시각 효과 및 풀링

#### 주요 컴포넌트

- **VFXManager**: VFX 매니저
- **DamageTextPool**: 데미지 텍스트 풀
- **BuffIconPool**: 버프 아이콘 풀
- **SkillCardUIPool**: 카드 UI 풀

#### 특징

- 오브젝트 풀링 패턴
- CoreSystem 오디오와 연동

---

### 10. TutorialSystem (튜토리얼 시스템)

**위치**: `Assets/Script/TutorialSystem/`  
**클래스 수**: 3개  
**역할**: 튜토리얼 오버레이

#### 주요 컴포넌트

- **TutorialManager**: 튜토리얼 매니저
- **TutorialOverlayView**: 튜토리얼 오버레이 뷰

#### 특징

- 간단한 구조
- CoreSystem에 의존

---

## 의존성 주입 구조

### Zenject Installer 계층

```
ProjectContext (전역)
  └── CoreSystemInstaller
        ├── GameStateManager
        ├── SceneTransitionManager
        ├── AudioManager
        ├── IItemService
        └── IRewardGenerator

MainScene
  └── MainSceneInstaller
        └── (CoreSystem 재바인딩)

CombatScene
  └── CombatInstaller
        ├── PlayerManager
        ├── EnemyManager
        ├── CombatStateMachine
        ├── TurnManager
        ├── CardExecutor
        ├── IStageManager
        └── (SkillCardSystem 바인딩)
```

### 주요 바인딩 패턴

#### 1. 인터페이스 바인딩
```csharp
Container.Bind<IGameStateManager>()
    .FromInstance(gameStateManager)
    .AsSingle();
```

#### 2. 팩토리 바인딩
```csharp
Container.Bind<ISkillCardFactory>()
    .To<SkillCardFactory>()
    .AsSingle();
```

#### 3. 자동 검색 바인딩
```csharp
Container.Bind<ICoreSystemInitializable>()
    .FromMethod(FindAllImplementations)
    .AsSingle();
```

### DI 원칙

1. **인터페이스 우선**: 모든 의존성은 인터페이스를 통해 주입
2. **AsSingle 사용**: 싱글톤 패턴 대신 Zenject의 `AsSingle()` 사용
3. **생명주기 관리**: Zenject가 객체 생명주기 관리
4. **순환 의존성 방지**: 인터페이스 분리로 순환 의존성 최소화

---

## 데이터 흐름

### 전투 시작 흐름

```
1. StageManager.StartStage()
   ↓
2. CombatFlowManager.StartCombat()
   ↓
3. CombatStateMachine.ChangeState(CombatInitState)
   ↓
4. CombatInitState.OnEnter()
   ├── PlayerManager.Initialize()
   ├── EnemyManager.SpawnEnemy()
   └── TurnManager.StartGame()
   ↓
5. CombatStateMachine.ChangeState(CombatPrepareState)
   ↓
6. CombatPrepareState.OnEnter()
   ├── SlotMovementController.SetupInitialQueue()
   └── PlayerHandManager.DrawCards()
   ↓
7. CombatStateMachine.ChangeState(PlayerTurnState)
```

### 카드 실행 흐름

```
1. PlayerHandCardSlotUI.OnCardDropped()
   ↓
2. CardDropService.TryDropCard()
   ↓
3. CombatExecutionManager.ExecuteCardAsync()
   ↓
4. CardExecutor.ExecuteCard()
   ├── CardValidator.Validate()
   ├── EffectCommandFactory.Create()
   └── IEffectCommand.Execute()
   ↓
5. CombatEvents.OnCardExecuted (이벤트 발생)
   ↓
6. UI 업데이트 (VFX, 사운드 등)
```

### 적 턴 흐름

```
1. TurnManager.EndPlayerTurn()
   ↓
2. CombatStateMachine.ChangeState(EnemyTurnState)
   ↓
3. EnemyTurnState.OnEnter()
   ├── SlotMovementController.AdvanceQueue()
   └── EnemyCharacter.PlayTurn()
   ↓
4. EnemyCharacter.ExecuteSkillCard()
   ├── CardExecutor.ExecuteCard()
   └── CombatEvents.OnEnemyCardExecuted
   ↓
5. TurnManager.EndEnemyTurn()
   ↓
6. CombatStateMachine.ChangeState(PlayerTurnState)
```

---

## 주요 디자인 패턴

### 1. 상태 패턴 (State Pattern)

**사용 위치**: `CombatSystem/State/`

```csharp
public interface ICombatState
{
    void OnEnter(CombatStateContext context);
    void OnUpdate(CombatStateContext context);
    void OnExit(CombatStateContext context);
}

public class CombatStateMachine
{
    private ICombatState _currentState;
    
    public void ChangeState(ICombatState newState)
    {
        _currentState?.OnExit(_context);
        _currentState = newState;
        _currentState?.OnEnter(_context);
    }
}
```

**장점**:
- 전투 흐름을 명확하게 관리
- 상태 전환 로직이 한 곳에 집중
- 새로운 상태 추가가 용이

---

### 2. 팩토리 패턴 (Factory Pattern)

**사용 위치**: `SkillCardSystem/Factory/`, `CombatSystem/Factory/`

```csharp
public interface ISkillCardFactory
{
    ISkillCard CreateCard(SkillCardDefinition definition);
}

public class SkillCardFactory : ISkillCardFactory
{
    public ISkillCard CreateCard(SkillCardDefinition definition)
    {
        // 카드 생성 로직
    }
}
```

**장점**:
- 객체 생성 로직 캡슐화
- 생성 과정 변경 시 영향 최소화
- 테스트 용이

---

### 3. 전략 패턴 (Strategy Pattern)

**사용 위치**: `SkillCardSystem/Effect/`

```csharp
public interface IEffectStrategy
{
    void Apply(ICharacter target, EffectData data);
}

public class DamageEffectStrategy : IEffectStrategy
{
    public void Apply(ICharacter target, EffectData data)
    {
        // 데미지 적용 로직
    }
}
```

**장점**:
- 효과 로직을 런타임에 교체 가능
- 새로운 효과 추가가 용이
- 효과별 독립적인 테스트 가능

---

### 4. 커맨드 패턴 (Command Pattern)

**사용 위치**: `SkillCardSystem/Effect/`

```csharp
public interface IEffectCommand
{
    ExecutionResult Execute(ICardExecutionContext context);
}

public class DamageEffectCommand : IEffectCommand
{
    public ExecutionResult Execute(ICardExecutionContext context)
    {
        // 데미지 실행 로직
    }
}
```

**장점**:
- 실행 로직을 객체로 캡슐화
- 실행 취소/재실행 가능 (향후 확장)
- 실행 로직의 순서 제어 가능

---

### 5. 오브젝트 풀링 패턴 (Object Pooling)

**사용 위치**: `VFXSystem/Pool/`

```csharp
public class DamageTextPool
{
    private Queue<DamageTextUI> _pool = new Queue<DamageTextUI>();
    
    public DamageTextUI Get()
    {
        if (_pool.Count > 0)
            return _pool.Dequeue();
        return Instantiate(_prefab);
    }
    
    public void Return(DamageTextUI instance)
    {
        instance.gameObject.SetActive(false);
        _pool.Enqueue(instance);
    }
}
```

**장점**:
- GC 압력 감소
- 성능 향상 (빈번한 생성/삭제 최적화)
- 메모리 사용량 최적화

---

### 6. 서비스 로케이터 패턴 (Service Locator)

**사용 위치**: Zenject DI 컨테이너

```csharp
[Inject]
private IAudioManager _audioManager;

public void PlaySound()
{
    _audioManager.PlaySfx("button_click");
}
```

**장점**:
- 의존성 명시적 선언
- 테스트 시 Mock 객체 주입 용이
- 순환 의존성 방지

---

## 아키텍처 다이어그램

### 전체 시스템 관계도

```
┌─────────────────────────────────────────────────────────────┐
│                        CoreSystem                             │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────┐     │
│  │GameStateMgr  │  │SceneTransMgr  │  │ AudioManager  │     │
│  └──────────────┘  └──────────────┘  └──────────────┘     │
└─────────────────────────────────────────────────────────────┘
           │                    │                    │
           │                    │                    │
    ┌──────▼──────┐      ┌──────▼──────┐      ┌──────▼──────┐
    │CharacterSys │      │ CombatSys   │      │  UISystem   │
    │             │      │             │      │             │
    │PlayerChar   │◄─────┤StateMachine │      │MainMenuCtrl │
    │EnemyChar    │      │TurnManager  │      │SettingsCtrl │
    └──────┬──────┘      └──────┬──────┘      └─────────────┘
           │                    │
           │                    │
    ┌──────▼──────┐      ┌──────▼──────┐
    │SkillCardSys │      │  ItemSystem  │
    │             │      │             │
    │CardFactory  │      │ItemService  │
    │CardExecutor │      │RewardGen    │
    └─────────────┘      └──────┬──────┘
                                │
                         ┌──────▼──────┐
                         │ StageSystem  │
                         │             │
                         │StageManager │
                         └─────────────┘
```

### 전투 상태 머신 다이어그램

```
                    [CombatInitState]
                         │
                         ▼
                    [CombatPrepareState]
                         │
                         ▼
                    [PlayerTurnState]
                         │
                    ┌────┴────┐
                    │         │
                    ▼         ▼
            [CardExecutionState]  [EnemyTurnState]
                    │         │
                    │         ▼
                    │    [SummonState]
                    │         │
                    └────┬────┘
                         │
                         ▼
                    [CombatResultState]
                         │
                    ┌────┴────┐
                    │         │
                    ▼         ▼
            [CombatVictoryState]  [CombatGameOverState]
```

### 카드 시스템 다이어그램

```
                    [SkillCardDefinition]
                         │ (ScriptableObject)
                         ▼
                    [SkillCardFactory]
                         │
                         ▼
                    [SkillCard] (Runtime)
                         │
                    ┌────┴────┐
                    │         │
                    ▼         ▼
            [CardExecutor]  [CardCirculationSystem]
                    │              │
                    │              ▼
                    │         [PlayerHandManager]
                    │              │
                    │              ▼
                    │         [PlayerDeckManager]
                    │
                    ▼
            [EffectCommandFactory]
                    │
            ┌───────┼───────┐
            │       │       │
            ▼       ▼       ▼
    [DamageEffect] [HealEffect] [BuffEffect]
```

---

## 아키텍처 품질 지표

### 긍정적 지표

- ✅ **SOLID 원칙 준수**: 인터페이스 기반 설계, 단일 책임 원칙 준수
- ✅ **의존성 주입**: Zenject를 통한 완전한 DI 패턴
- ✅ **시스템 분리**: 10개 시스템으로 명확한 책임 분리
- ✅ **코드 중복**: 중복 코드 없음
- ✅ **순환 복잡도**: 임계값(10) 이상 없음
- ✅ **문서화**: 시스템별 상세 문서화 완료

### 개선 필요 사항

- ⚠️ **Update 루프**: 10개 파일에서 Update() 사용 (이벤트 기반 전환 권장)
- ⚠️ **순환 의존성**: 3개 자기 참조 순환 (SlotMovementController, TurnController, SkillCardFactory)
- ⚠️ **금지된 API**: 2개 파일에서 FindObjectOfType 사용 (DI로 전환 필요)

---

## 확장성 고려사항

### 새로운 시스템 추가 시

1. **인터페이스 정의**: `ISystemName` 인터페이스 생성
2. **Installer 생성**: `SystemNameInstaller` 생성 및 DI 바인딩
3. **의존성 관리**: CoreSystem에만 의존하도록 설계
4. **문서화**: `ScriptRegistry_SystemName.md` 문서 작성

### 새로운 효과 추가 시

1. **EffectStrategy 구현**: `IEffectStrategy` 인터페이스 구현
2. **EffectCommand 구현**: `IEffectCommand` 인터페이스 구현
3. **팩토리 등록**: `EffectCommandFactory`에 등록
4. **ScriptableObject 생성**: `EffectSO` 상속하여 데이터 정의

---

## 참고 문서

- [ScriptRegistry_Master.md](./ScriptRegistry_Master.md): 전체 스크립트 레지스트리 마스터 문서
- [ScriptRegistry_CoreSystem.md](./ScriptRegistry_CoreSystem.md): CoreSystem 상세 문서
- [ScriptRegistry_CombatSystem.md](./ScriptRegistry_CombatSystem.md): CombatSystem 상세 문서
- [ScriptRegistry_SkillCardSystem.md](./ScriptRegistry_SkillCardSystem.md): SkillCardSystem 상세 문서
- [ScriptRegistry_CharacterSystem.md](./ScriptRegistry_CharacterSystem.md): CharacterSystem 상세 문서
- [ScriptRegistry_ItemSystem.md](./ScriptRegistry_ItemSystem.md): ItemSystem 상세 문서

---

## 변경 이력

| 날짜 | 변경 내용 | 작성자 |
|------|----------|--------|
| 2024년 | 초기 문서 작성 | - |

---

**문서 버전**: 1.0  
**최종 업데이트**: 2024년

