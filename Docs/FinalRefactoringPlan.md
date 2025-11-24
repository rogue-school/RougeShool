# RougeShool 최종 리팩토링 계획

> 작성일: 2025-11-24  
> 목적: 기존 코드 개선이 아닌 **완전히 새로운 아키텍처와 스크립트를 작성**하는 최종 실행 계획  
> 상태: 🚀 실행 준비 완료

---

## 📋 문서 개요

이 문서는 RougeShool 프로젝트의 **완전 재작성 리팩토링**을 위한 최종 실행 계획입니다.

### 핵심 원칙

1. **완전 재작성**: 기존 코드를 수정하는 것이 아닌, 새로운 아키텍처로 처음부터 작성
2. **Hybrid 아키텍처**: 레이어드 + 기능 기반 구조로 확장성과 유지보수성 확보
3. **제로 레거시**: 모든 레거시 코드 제거, 호환성 유지 불필요
4. **유저룰 100% 준수**: 모든 함수/변수/구조를 새 유저룰에 맞게 작성

### 관련 문서

이 문서는 다음 문서들의 내용을 통합하여 작성되었습니다:
- 아키텍처 리팩토링 계획 (Hybrid 구조 제안)
- 완전 재작성 리팩토링 계획 (레거시 제거)
- 코드 품질 진단 리포트 (개선 사항)
- 전체 스크립트 체크리스트 (326개 스크립트 분석)
- 마이그레이션 실행 계획 (단계별 가이드)

---

## 🏗️ 새로운 아키텍처 구조

### Hybrid 구조 (레이어드 + 기능 기반)

```
Assets/Script/
├── Domain/                    # 도메인 레이어 (비즈니스 로직)
│   ├── Character/            # 캐릭터 도메인
│   │   ├── Entities/        # Character, PlayerCharacter, EnemyCharacter
│   │   ├── ValueObjects/    # CharacterStats, Resource
│   │   └── Interfaces/      # ICharacter, IPlayerCharacter
│   ├── Combat/               # 전투 도메인
│   │   ├── Entities/        # CombatSlot, Turn, CombatSession
│   │   ├── ValueObjects/    # CombatPhase, SlotPosition, TurnType
│   │   └── Interfaces/      # ITurnManager, ICombatExecutor
│   ├── Card/                 # 카드 도메인
│   │   ├── Entities/        # SkillCard, CardEffect
│   │   ├── ValueObjects/    # CardDefinition, CardStats
│   │   └── Interfaces/      # ISkillCard, ICardEffect
│   ├── Item/                 # 아이템 도메인
│   └── Stage/                # 스테이지 도메인
├── Application/              # 애플리케이션 레이어 (유스케이스)
│   ├── Battle/              # 전투 유스케이스
│   │   ├── StartCombat.cs
│   │   ├── ExecuteCard.cs
│   │   ├── EndTurn.cs
│   │   └── MoveSlot.cs
│   ├── Character/           # 캐릭터 유스케이스
│   │   ├── InitializeCharacter.cs
│   │   ├── TakeDamage.cs
│   │   ├── Heal.cs
│   │   └── ApplyEffect.cs
│   ├── Card/                # 카드 유스케이스
│   │   ├── DrawCard.cs
│   │   ├── PlayCard.cs
│   │   └── DiscardCard.cs
│   └── Services/            # 공통 서비스
│       ├── EventBus.cs
│       └── CommandBus.cs
├── Infrastructure/          # 인프라스트럭처 레이어
│   ├── Unity/               # Unity 특화
│   │   ├── MonoBehaviour/   # CharacterMonoBehaviour, CardMonoBehaviour
│   │   ├── ScriptableObject/ # CharacterDataSO, CardDataSO
│   │   └── Coroutine/       # CoroutineRunner
│   ├── Persistence/         # 저장/로드
│   │   ├── SaveManager.cs
│   │   └── LoadManager.cs
│   ├── Audio/               # 오디오
│   │   └── AudioManager.cs
│   └── DI/                  # DI 바인딩
│       ├── DomainInstaller.cs
│       ├── ApplicationInstaller.cs
│       └── InfrastructureInstaller.cs
└── Presentation/            # 프레젠테이션 레이어
    ├── UI/                  # UI 컨트롤러
    │   ├── Battle/         # 전투 UI
    │   ├── Character/      # 캐릭터 UI
    │   ├── Card/           # 카드 UI
    │   └── Common/         # 공통 UI
    ├── VFX/                 # VFX 시스템
    └── Input/               # 입력 처리
```

### 네임스페이스 구조

```
Game.
├── Domain.                    # 도메인 레이어
│   ├── Character.
│   │   ├── Entities
│   │   ├── ValueObjects
│   │   └── Interfaces
│   ├── Combat.
│   ├── Card.
│   ├── Item.
│   └── Stage.
├── Application.              # 애플리케이션 레이어
│   ├── Battle.
│   ├── Character.
│   ├── Card.
│   └── Services.
├── Infrastructure.           # 인프라스트럭처 레이어
│   ├── Unity.
│   ├── Persistence.
│   ├── Audio.
│   └── DI.
└── Presentation.            # 프레젠테이션 레이어
    ├── UI.
    ├── VFX.
    └── Input.
```

---

## 🎯 리팩토링 목표

### 1. 아키텍처 개선

| 항목 | 현재 구조 | 새 구조 | 개선 효과 |
|------|----------|---------|----------|
| **폴더 수** | 11개 시스템 | 4개 레이어 | 구조 단순화 |
| **의존성 방향** | 순환 의존성 | 단방향 의존성 | 테스트 용이성 향상 |
| **테스트 용이성** | 어려움 | 쉬움 | Domain 레이어는 Unity 없이 테스트 가능 |
| **확장성** | 중간 | 높음 | 새 기능 추가 시 해당 Feature 폴더에만 추가 |
| **코드 재사용** | 낮음 | 높음 | 도메인 로직 재사용 가능 |

### 2. 코드 품질 개선

#### 제거 대상
- ❌ `FindObjectOfType` 사용 (2개 파일)
- ❌ `Update()` 루프 (9개 파일 → 이벤트 기반 전환)
- ❌ `Resources.Load` (17개 파일 → Addressables 전환)
- ❌ 레거시 타입 및 변환 코드
- ❌ 사용하지 않는 스크립트 (12개)
- ❌ 순환 의존성 (3개)

#### 개선 사항
- ✅ Zenject DI 전면 적용
- ✅ 이벤트 기반 아키텍처
- ✅ DOTween 메모리 안전
- ✅ 3-계층 예외 처리
- ✅ Inspector 한글화
- ✅ XML 문서화

---

## 📋 단계별 실행 계획

### Phase 1: Domain 레이어 구축 (1주)

#### 1.1 Character 도메인

**작업 내용**:
1. 폴더 구조 생성
   ```
   Assets/Script/Domain/Character/
   ├── Entities/
   ├── ValueObjects/
   └── Interfaces/
   ```

2. 인터페이스 작성 (Unity 의존성 제거)
   - `ICharacter.cs`: MonoBehaviour 참조 제거, 순수 인터페이스
   - `IPlayerCharacter.cs`: 플레이어 전용 인터페이스
   - `IEnemyCharacter.cs`: 적 전용 인터페이스

3. 엔티티 작성 (MonoBehaviour 제거)
   - `Character.cs`: CharacterBase에서 비즈니스 로직만 추출
   - `PlayerCharacter.cs`: Unity 의존성 제거
   - `EnemyCharacter.cs`: Unity 의존성 제거

4. ValueObjects 작성
   - `CharacterStats.cs`: 캐릭터 스탯
   - `Resource.cs`: 리소스 (마나, 화살 등)

**기존 코드 참고**:
- `CharacterSystem/Interface/ICharacter.cs`
- `CharacterSystem/Core/CharacterBase.cs`
- `CharacterSystem/Core/PlayerCharacter.cs`
- `CharacterSystem/Core/EnemyCharacter.cs`

**제거 대상**:
- MonoBehaviour 상속
- Unity 의존성 (Transform, GameObject 등)
- UI 관련 코드
- VFX 관련 코드

#### 1.2 Combat 도메인

**작업 내용**:
1. ValueObjects 작성
   - `TurnType.cs`: 턴 타입 (Player/Enemy)
   - `SlotPosition.cs`: 슬롯 위치
   - `CombatPhase.cs`: 전투 페이즈

2. 엔티티 작성
   - `CombatSlot.cs`: 전투 슬롯
   - `Turn.cs`: 턴 정보
   - `CombatSession.cs`: 전투 세션

3. 인터페이스 작성
   - `ITurnManager.cs`: 턴 관리 인터페이스
   - `ICombatExecutor.cs`: 전투 실행 인터페이스
   - `ISlotRegistry.cs`: 슬롯 레지스트리 인터페이스

**기존 코드 참고**:
- `CombatSystem/Interface/TurnType.cs`
- `CombatSystem/Slot/CombatSlotPosition.cs`
- `CombatSystem/State/CombatStateContext.cs`

**제거 대상**:
- 레거시 `TurnType` enum (TurnManager.cs)
- 변환 메서드 (ConvertToLegacyTurnType, ConvertToNewTurnType)

#### 1.3 Card 도메인

**작업 내용**:
1. 인터페이스 작성
   - `ISkillCard.cs`: 스킬 카드 인터페이스
   - `ICardEffect.cs`: 카드 효과 인터페이스

2. 엔티티 작성
   - `SkillCard.cs`: 스킬 카드 엔티티
   - `CardEffect.cs`: 카드 효과 엔티티

3. ValueObjects 작성
   - `CardDefinition.cs`: 카드 정의
   - `CardStats.cs`: 카드 스탯

**기존 코드 참고**:
- `SkillCardSystem/Interface/ISkillCard.cs`
- `SkillCardSystem/Runtime/SkillCard.cs`

#### 1.4 Item 도메인

**작업 내용**:
1. 엔티티 작성
   - `Item.cs`: 아이템 엔티티
   - `ItemEffect.cs`: 아이템 효과 엔티티

2. 인터페이스 작성
   - `IItem.cs`: 아이템 인터페이스

#### 1.5 Stage 도메인

**작업 내용**:
1. 엔티티 작성
   - `Stage.cs`: 스테이지 엔티티

2. 인터페이스 작성
   - `IStage.cs`: 스테이지 인터페이스

---

### Phase 2: Application 레이어 구축 (1주)

#### 2.1 Battle 유스케이스

**새로 작성할 클래스**:
- `Application/Battle/StartCombat.cs`: 전투 시작
- `Application/Battle/ExecuteCard.cs`: 카드 실행
- `Application/Battle/EndTurn.cs`: 턴 종료
- `Application/Battle/MoveSlot.cs`: 슬롯 이동

**기존 코드에서 추출할 로직**:
- `CombatSystem/Manager/CombatExecutionManager.cs`
- `CombatSystem/Manager/TurnManager.cs`

#### 2.2 Character 유스케이스

**새로 작성할 클래스**:
- `Application/Character/InitializeCharacter.cs`: 캐릭터 초기화
- `Application/Character/TakeDamage.cs`: 데미지 처리
- `Application/Character/Heal.cs`: 힐 처리
- `Application/Character/ApplyEffect.cs`: 효과 적용

#### 2.3 Card 유스케이스

**새로 작성할 클래스**:
- `Application/Card/DrawCard.cs`: 카드 뽑기
- `Application/Card/PlayCard.cs`: 카드 사용
- `Application/Card/DiscardCard.cs`: 카드 버리기
- `Application/Card/ShuffleDeck.cs`: 덱 셔플

#### 2.4 이벤트 시스템

**새로 작성할 클래스**:
- `Application/Services/EventBus.cs`: 이벤트 버스
- `Application/Services/CommandBus.cs`: 명령 버스
- `Application/Services/QueryBus.cs`: 쿼리 버스

---

### Phase 3: Infrastructure 레이어 구축 (1주)

#### 3.1 Unity 어댑터

**새로 작성할 클래스**:
- `Infrastructure/Unity/MonoBehaviour/CharacterMonoBehaviour.cs`
  - Domain의 Character를 Unity에서 사용하기 위한 래퍼
  - MonoBehaviour 상속, Domain Character 참조
- `Infrastructure/Unity/ScriptableObject/CharacterDataSO.cs`
  - ScriptableObject를 Domain 모델로 변환하는 어댑터

#### 3.2 Persistence 재작성

**재작성 대상**:
- `CoreSystem/Save/SaveManager.cs` → `Infrastructure/Persistence/SaveManager.cs`

**제거 대상**:
- FindObjectOfType 캐싱 (8개 매니저)
- 레거시 호환 메서드
- 주석 처리된 코드

**새 구조**:
```csharp
namespace Game.Infrastructure.Persistence
{
    public class SaveManager : MonoBehaviour, ISaveManager
    {
        #region Dependency Injection
        
        [Inject] private IStageManager stageManager;
        [Inject] private ITurnManager turnManager;
        [Inject] private ICombatFlowManager combatFlowManager;
        [Inject] private IPlayerManager playerManager;
        [Inject] private IEnemyManager enemyManager;
        [Inject] private ICardSlotRegistry slotRegistry;
        [Inject] private IPlayerHandManager playerHandManager;
        
        #endregion
        
        // FindObjectOfType 완전 제거
        // Domain 모델을 저장 형식으로 변환하는 로직 추가
    }
}
```

#### 3.3 Audio 재작성

**재작성 대상**:
- `CoreSystem/Audio/AudioManager.cs` → `Infrastructure/Audio/AudioManager.cs`

**개선 사항**:
- Resources.Load → Addressables 전환
- DOTween 메모리 안전 적용

#### 3.4 DI 바인딩 재구성

**재작성 대상**:
- `CoreSystem/CoreSystemInstaller.cs` → `Infrastructure/DI/DomainInstaller.cs`
- `CombatSystem/Core/CombatInstaller.cs` → `Infrastructure/DI/ApplicationInstaller.cs`
- 새 Installer 작성: `Infrastructure/DI/InfrastructureInstaller.cs`

---

### Phase 4: Presentation 레이어 구축 (1주)

#### 4.1 UI 재구성

**이동 대상**:
- `CharacterSystem/UI/*` → `Presentation/UI/Character/`
- `CombatSystem/UI/*` → `Presentation/UI/Battle/`
- `SkillCardSystem/UI/*` → `Presentation/UI/Card/`
- `UISystem/*` → `Presentation/UI/Common/`

**변경 사항**:
- UI 로직만 유지
- 도메인 로직 제거
- 이벤트 기반 통신으로 변경
- Update() 제거 (이벤트 기반 전환)

**재작성 대상**:
- `UISystem/ExitGame.cs` → `Presentation/UI/Common/ExitGameController.cs`
- `UISystem/Newgame.cs` → `Presentation/UI/Common/NewGameController.cs`
- `UISystem/WeaponSelector.cs` → `Presentation/UI/Common/WeaponSelectorController.cs`
- `UISystem/SettingsUIController.cs` → `Presentation/UI/Common/SettingsUIController.cs`

#### 4.2 VFX 통합

**이동 대상**:
- `VFXSystem/*` → `Presentation/VFX/`

**개선 사항**:
- DOTween 메모리 안전 적용
- OnDisable/OnDestroy에서 Kill 필수

#### 4.3 Input 처리

**새로 작성할 클래스**:
- `Presentation/Input/InputHandler.cs`: 입력 처리
- `Presentation/Input/DragDropHandler.cs`: 드래그앤드롭

---

### Phase 5: 레거시 코드 제거 및 정리 (1주)

#### 5.1 사용하지 않는 파일 삭제

**즉시 삭제 대상 (12개)**:
1. `ItemSystem/Runtime/TestItemButton.cs` - 테스트 코드
2. `UISystem/play.cs` - 네임스페이스 없음, 소문자 클래스명
3. `UISystem/Xbutton.cs` - 네임스페이스 없음, 오타
4. `CombatSystem/Core/DefaultCombatState.cs` - 사용 안함
5. `CharacterSystem/Data/PlayerCharacterTypeHelper.cs` - 사용 안함
6. `SkillCardSystem/Manager/BaseSkillCardManager.cs` - 상속받는 클래스 없음
7. `ItemSystem/Service/Reward/RewardInstaller.cs` - 사용 안함
8. `CoreSystem/Utility/DIOptimizationUtility.cs` - 사용 안함
9. `CoreSystem/Utility/ComponentInteractionOptimizer.cs` - 사용 안함
10. `CoreSystem/Utility/ComponentRoleManager.cs` - 사용 안함
11. `UtilitySystem/DontDestroyOnLoadContainer.cs` - 사용 안함
12. `UtilitySystem/DropHandlerInjector.cs` - 기능 없음

#### 5.2 네임스페이스 수정

**수정 대상 (3개)**:
1. `SkillCardSystem/DragDrop/CardDragHandler.cs`
   - `Game.CombatSystem.DragDrop` → `Game.SkillCardSystem.DragDrop`
2. `CombatSystem/Initialization/SlotInitializationStep.cs`
   - `Game.CombatSystem.Intialization` → `Game.CombatSystem.Initialization` (오타 수정)
3. `SkillCardSystem/Installer/CardInstaller.cs`
   - `Game.SkillCardSystem.Installation` → `Game.SkillCardSystem.Installer`

#### 5.3 기존 시스템 폴더 삭제

**삭제 대상** (Domain/Application/Infrastructure/Presentation으로 이동 완료 후):
- `CharacterSystem/` (Domain.Character로 이동 완료 후)
- `CombatSystem/` (Domain.Combat, Application.Battle로 이동 완료 후)
- `SkillCardSystem/` (Domain.Card로 이동 완료 후)
- `ItemSystem/` (Domain.Item로 이동 완료 후)
- `StageSystem/` (Domain.Stage로 이동 완료 후)

#### 5.4 네임스페이스 정리

**변경 사항**:
- 모든 네임스페이스를 새 구조에 맞게 변경
- using 문 정리
- 컴파일 오류 해결

---

## 🔧 코드 작성 규칙

### 1. SOLID 원칙

#### Single Responsibility Principle (SRP)
- 각 클래스는 하나의 책임만 가짐
- Manager 클래스는 관리만, Service 클래스는 서비스만

#### Open-Closed Principle (OCP)
- 인터페이스 기반 설계
- 확장에는 열려있고 수정에는 닫혀있음

#### Dependency Inversion Principle (DIP)
- 구체 클래스가 아닌 인터페이스에 의존
- Zenject DI를 통한 의존성 주입

### 2. 예외 처리 (3-계층 전략)

#### LEVEL 1: Validation Layer (throw only)
```csharp
public void ProcessCard(ISkillCard card)
{
    if (card == null)
        throw new ArgumentNullException(nameof(card), "카드가 null입니다");
    
    if (!card.IsValid())
        throw new InvalidOperationException("카드 상태가 유효하지 않습니다");
}
```

#### LEVEL 2: Operation Layer (log + wrap)
```csharp
public bool ExecuteCard(ISkillCard card)
{
    try
    {
        ProcessCard(card);
        return card.Execute();
    }
    catch (ArgumentNullException ex)
    {
        GameLogger.LogError($"카드 실행 실패 (null): {ex.Message}", GameLogger.LogCategory.Error);
        throw;
    }
    catch (Exception ex)
    {
        GameLogger.LogError($"카드 실행 중 예상치 못한 오류: {ex.Message}", GameLogger.LogCategory.Error);
        throw new InvalidOperationException("카드 실행 중 오류 발생", ex);
    }
}
```

#### LEVEL 3: Boundary Layer (log + handle gracefully)
```csharp
public void OnCardClicked(ISkillCard card)
{
    try
    {
        ExecuteCard(card);
    }
    catch (Exception ex)
    {
        GameLogger.LogError($"UI 카드 클릭 처리 오류: {ex.Message}", GameLogger.LogCategory.UI);
        ShowErrorMessage("카드를 사용할 수 없습니다");
        // DON'T re-throw at UI boundary
    }
}
```

### 3. DOTween 메모리 안전

**필수 사항**:
```csharp
public class SafeAnimationController : MonoBehaviour
{
    private List<Tween> activeTweens = new List<Tween>();

    public void PlayCardAnimation(Transform cardTransform)
    {
        var tween = cardTransform.DOScale(1.2f, 0.3f)
            .SetEase(Ease.OutBack)
            .SetAutoKill(true)  // ✅ 필수
            .OnComplete(() => {
                activeTweens.Remove(tween);
            });

        activeTweens.Add(tween);
    }

    private void OnDisable()
    {
        // ✅ 필수: 모든 Tween Kill
        foreach (var tween in activeTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
        activeTweens.Clear();
    }

    private void OnDestroy()
    {
        // ✅ 필수: 모든 Tween Kill
        foreach (var tween in activeTweens)
        {
            if (tween != null && tween.IsActive())
                tween.Kill();
        }
        activeTweens.Clear();
    }
}
```

### 4. 이벤트 기반 아키텍처

**Update() 제거 원칙**:
- ❌ 금지: 타이머, 주기적 체크
- ✅ 허용: Input 폴링, Physics 계산, 실시간 보간

**대체 방법**:
- 이벤트 기반: `OnCardPlayed`, `OnTurnChanged` 등
- 코루틴: 시간 기반 로직
- DOTween: 애니메이션

### 5. 한국어 로컬라이징

**Inspector 필드**:
```csharp
[Header("전투 설정")]
[Tooltip("플레이어의 최대 체력입니다")]
[SerializeField] private int _maxHealth = 100;
```

**로그 및 예외**:
```csharp
GameLogger.LogInfo("카드 사용 완료", GameLogger.LogCategory.UI);
throw new ArgumentNullException(nameof(card), "카드가 null입니다");
```

**XML 문서화**:
```csharp
/// <summary>
/// 카드를 실행하고 효과를 적용합니다
/// </summary>
/// <param name="card">실행할 카드</param>
/// <returns>실행 성공 여부</returns>
public bool ExecuteCard(ISkillCard card)
{
    // Implementation
}
```

---

## 📊 작업 우선순위 및 일정

### 전체 일정 (5주)

| Phase | 기간 | 작업 내용 | 우선순위 |
|-------|------|----------|---------|
| **Phase 1** | 1주 | Domain 레이어 구축 | 🔥 최우선 |
| **Phase 2** | 1주 | Application 레이어 구축 | 🔥 높음 |
| **Phase 3** | 1주 | Infrastructure 레이어 구축 | 🔥 높음 |
| **Phase 4** | 1주 | Presentation 레이어 구축 | ⚠️ 중간 |
| **Phase 5** | 1주 | 레거시 코드 제거 및 정리 | ⚠️ 중간 |

### Phase 1 상세 일정

| 작업 | 예상 시간 | 우선순위 |
|------|----------|---------|
| Character 도메인 | 2일 | 🔥 최우선 |
| Combat 도메인 | 2일 | 🔥 높음 |
| Card 도메인 | 1일 | ⚠️ 중간 |
| Item 도메인 | 0.5일 | ⚠️ 낮음 |
| Stage 도메인 | 0.5일 | ⚠️ 낮음 |

---

## ✅ 체크리스트

### Domain 레이어
- [ ] Character 도메인 완료
  - [ ] 인터페이스 작성 (Unity 의존성 제거)
  - [ ] 엔티티 작성 (MonoBehaviour 제거)
  - [ ] ValueObjects 작성
- [ ] Combat 도메인 완료
  - [ ] ValueObjects 작성
  - [ ] 엔티티 작성
  - [ ] 인터페이스 작성
- [ ] Card 도메인 완료
- [ ] Item 도메인 완료
- [ ] Stage 도메인 완료

### Application 레이어
- [ ] Battle 유스케이스 완료
- [ ] Character 유스케이스 완료
- [ ] Card 유스케이스 완료
- [ ] 이벤트 시스템 완료

### Infrastructure 레이어
- [ ] Unity 어댑터 완료
- [ ] Persistence 완료 (FindObjectOfType 제거)
- [ ] Audio 완료 (Addressables 전환)
- [ ] DI 바인딩 완료

### Presentation 레이어
- [ ] UI 재구성 완료 (Update() 제거)
- [ ] VFX 통합 완료 (DOTween 메모리 안전)
- [ ] Input 처리 완료

### 정리 작업
- [ ] 사용하지 않는 파일 삭제 (12개)
- [ ] 네임스페이스 수정 (3개)
- [ ] 기존 시스템 폴더 삭제
- [ ] 네임스페이스 정리
- [ ] 컴파일 오류 0개
- [ ] 경고 0개
- [ ] 최종 테스트

---

## 🚨 주의사항

### 위험 관리

1. **백업 필수**: 각 Phase 시작 전 Git 커밋
2. **점진적 진행**: 한 번에 하나씩 작성
3. **테스트**: 각 파일 작성 후 즉시 컴파일 확인
4. **롤백 계획**: 문제 발생 시 즉시 롤백

### 호환성

- **레거시 호환성 유지 불필요**: 완전 재작성이므로 기존 코드와 호환 유지 불필요
- **데이터 호환성**: Save 데이터는 마이그레이션 필요할 수 있음
- **에셋 재사용**: ScriptableObject 에셋은 재사용 (로딩 방식만 개선)

---

## 📝 변경 기록

| 날짜 | 담당 | 내용 |
|------|------|------|
| 2025-11-24 | Cursor AI | 최종 리팩토링 계획 작성 (모든 문서 통합) |

---

## 🔗 참고 문서

이 문서는 다음 문서들의 내용을 통합하여 작성되었습니다:

1. **[아키텍처 리팩토링 계획](./ArchitectureRefactoringPlan.md)**: Hybrid 구조 제안
2. **[완전 재작성 리팩토링 계획](./CompleteRefactoringPlan.md)**: 레거시 코드 제거 계획
3. **[코드 품질 진단 리포트](./CodeQualityDiagnosisReport.md)**: 개선 사항 진단
4. **[전체 스크립트 체크리스트](./CompleteScriptChecklist.md)**: 326개 스크립트 분석
5. **[스크립트 상세 분석](./DetailedScriptAnalysis.md)**: 상세 분석 및 재작성 계획
6. **[마이그레이션 실행 계획](./MigrationExecutionPlan.md)**: 단계별 가이드
7. **[코드 로직 문서](./CodeLogicDocumentation.md)**: 코드 로직 설명
8. **[리팩토링 마스터 플랜](./RefactoringMasterPlan.md)**: 전역 리팩토링 계획

---

## 🎯 다음 단계

1. **Phase 1 시작**: Domain.Character 도메인부터 구축
2. **폴더 구조 생성**: `Assets/Script/Domain/Character/` 생성
3. **인터페이스 작성**: ICharacter, IPlayerCharacter, IEnemyCharacter
4. **엔티티 작성**: Character, PlayerCharacter, EnemyCharacter
5. **테스트**: 컴파일 확인 및 기본 동작 확인

**준비 완료**: 이 문서를 바탕으로 리팩토링을 시작할 수 있습니다. 🚀

