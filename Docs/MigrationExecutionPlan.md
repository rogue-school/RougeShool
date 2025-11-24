# RougeShool 아키텍처 마이그레이션 실행 계획

> 작성일: 2025-11-24  
> 목적: Hybrid 구조로 실제 마이그레이션을 단계별로 실행하는 구체적인 가이드

---

## 🎯 마이그레이션 전략

### 원칙

1. **점진적 마이그레이션**: 기존 코드와 병행하며 단계적으로 전환
2. **기능 단위 이동**: 한 번에 하나의 기능만 이동하여 테스트 가능
3. **하위 호환성 유지**: 기존 코드가 동작하는 상태 유지
4. **자동화된 검증**: 각 단계마다 컴파일 및 기본 테스트

---

## 📋 Phase 1: Domain 레이어 구축 (1주)

### 1.1 폴더 구조 생성

```
Assets/Script/
├── Domain/                    # 새로 생성
│   ├── Character/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   ├── Combat/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   ├── Card/
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   └── Interfaces/
│   ├── Item/
│   └── Stage/
```

### 1.2 Character 도메인 마이그레이션

#### Step 1: 인터페이스 이동

**이동 대상**:
- `CharacterSystem/Interface/ICharacter.cs` → `Domain/Character/Interfaces/ICharacter.cs`
- `CharacterSystem/Interface/IPlayerCharacter.cs` → `Domain/Character/Interfaces/IPlayerCharacter.cs`
- `CharacterSystem/Interface/IEnemyCharacter.cs` → `Domain/Character/Interfaces/IEnemyCharacter.cs`

**작업**:
1. 새 위치에 인터페이스 복사
2. 네임스페이스 변경: `Game.CharacterSystem.Interface` → `Game.Domain.Character.Interfaces`
3. Unity 의존성 제거 (MonoBehaviour 참조 제거)
4. 기존 파일은 레거시로 유지 (나중에 삭제)

#### Step 2: 엔티티 이동

**이동 대상**:
- `CharacterSystem/Core/CharacterBase.cs` → `Domain/Character/Entities/Character.cs`
- `CharacterSystem/Core/PlayerCharacter.cs` → `Domain/Character/Entities/PlayerCharacter.cs`
- `CharacterSystem/Core/EnemyCharacter.cs` → `Domain/Character/Entities/EnemyCharacter.cs`

**변경 사항**:
- MonoBehaviour 제거 (순수 C# 클래스로)
- Unity 의존성 제거
- 비즈니스 로직만 유지

#### Step 3: ValueObjects 이동

**이동 대상**:
- `CharacterSystem/Data/CharacterStats.cs` → `Domain/Character/ValueObjects/CharacterStats.cs`
- 리소스 관련 클래스 → `Domain/Character/ValueObjects/Resource.cs`

### 1.3 Combat 도메인 마이그레이션

#### Step 1: ValueObjects 먼저 이동

**이동 대상**:
- `CombatSystem/Interface/TurnType.cs` → `Domain/Combat/ValueObjects/TurnType.cs`
- `CombatSystem/Slot/CombatSlotPosition.cs` → `Domain/Combat/ValueObjects/SlotPosition.cs`
- `CombatSystem/Core/CombatConstants.cs` → `Domain/Combat/ValueObjects/CombatPhase.cs`

#### Step 2: 엔티티 이동

**이동 대상**:
- `CombatSystem/State/CombatStateContext.cs` → `Domain/Combat/Entities/CombatSession.cs`
- Turn 관련 로직 → `Domain/Combat/Entities/Turn.cs`

### 1.4 Card 도메인 마이그레이션

#### Step 1: 인터페이스 이동

**이동 대상**:
- `SkillCardSystem/Interface/ISkillCard.cs` → `Domain/Card/Interfaces/ISkillCard.cs`
- `SkillCardSystem/Interface/ICardEffect.cs` → `Domain/Card/Interfaces/ICardEffect.cs`

#### Step 2: 엔티티 이동

**이동 대상**:
- `SkillCardSystem/Runtime/SkillCard.cs` → `Domain/Card/Entities/SkillCard.cs`
- 효과 관련 클래스 → `Domain/Card/Entities/CardEffect.cs`

---

## 📋 Phase 2: Application 레이어 구축 (1주)

### 2.1 폴더 구조 생성

```
Assets/Script/
├── Application/              # 새로 생성
│   ├── Battle/
│   ├── Character/
│   ├── Card/
│   └── Services/
```

### 2.2 Battle 유스케이스 작성

**새로 작성할 클래스**:
- `Application/Battle/StartCombat.cs`
- `Application/Battle/ExecuteCard.cs`
- `Application/Battle/EndTurn.cs`
- `Application/Battle/MoveSlot.cs`

**기존 코드에서 추출**:
- `CombatSystem/Manager/CombatExecutionManager.cs`의 로직을 유스케이스로 분리
- `CombatSystem/Manager/TurnManager.cs`의 로직을 유스케이스로 분리

### 2.3 Character 유스케이스 작성

**새로 작성할 클래스**:
- `Application/Character/InitializeCharacter.cs`
- `Application/Character/TakeDamage.cs`
- `Application/Character/Heal.cs`
- `Application/Character/ApplyEffect.cs`

### 2.4 이벤트 시스템 구축

**새로 작성할 클래스**:
- `Application/Services/EventBus.cs`
- `Application/Services/CommandBus.cs`

---

## 📋 Phase 3: Infrastructure 레이어 구축 (1주)

### 3.1 Unity 어댑터 작성

**새로 작성할 클래스**:
- `Infrastructure/Unity/MonoBehaviour/CharacterMonoBehaviour.cs`
  - Domain의 Character를 Unity에서 사용하기 위한 래퍼
- `Infrastructure/Unity/ScriptableObject/CharacterDataSO.cs`
  - ScriptableObject를 Domain 모델로 변환

### 3.2 Persistence 재작성

**재작성 대상**:
- `CoreSystem/Save/SaveManager.cs` → `Infrastructure/Persistence/SaveManager.cs`
- Domain 모델을 저장 형식으로 변환하는 로직 추가

### 3.3 DI 바인딩 재구성

**재작성 대상**:
- `CoreSystem/CoreSystemInstaller.cs` → `Infrastructure/DI/DomainInstaller.cs`
- `CombatSystem/Core/CombatInstaller.cs` → `Infrastructure/DI/ApplicationInstaller.cs`

---

## 📋 Phase 4: Presentation 레이어 구축 (1주)

### 4.1 UI 재구성

**이동 대상**:
- `CharacterSystem/UI/*` → `Presentation/UI/Character/`
- `CombatSystem/UI/*` → `Presentation/UI/Battle/`
- `SkillCardSystem/UI/*` → `Presentation/UI/Card/`

**변경 사항**:
- UI 로직만 유지
- 도메인 로직 제거
- 이벤트 기반 통신으로 변경

### 4.2 VFX 통합

**이동 대상**:
- `VFXSystem/*` → `Presentation/VFX/`

---

## 📋 Phase 5: 레거시 코드 제거 (1주)

### 5.1 기존 시스템 폴더 삭제

**삭제 대상**:
- `CharacterSystem/` (Domain으로 이동 완료 후)
- `CombatSystem/` (Domain, Application으로 이동 완료 후)
- `SkillCardSystem/` (Domain으로 이동 완료 후)
- `ItemSystem/` (Domain으로 이동 완료 후)
- `StageSystem/` (Domain으로 이동 완료 후)

### 5.2 네임스페이스 정리

**변경 사항**:
- 모든 네임스페이스를 새 구조에 맞게 변경
- using 문 정리

---

## 🚀 즉시 시작 가능한 작업

### 우선순위 1: Domain.Character 구축

가장 간단하고 독립적인 Character 도메인부터 시작:

1. **폴더 생성**
   ```
   Assets/Script/Domain/Character/Entities/
   Assets/Script/Domain/Character/ValueObjects/
   Assets/Script/Domain/Character/Interfaces/
   ```

2. **인터페이스 이동 및 정리**
   - ICharacter, IPlayerCharacter, IEnemyCharacter
   - Unity 의존성 제거

3. **엔티티 이동 및 정리**
   - CharacterBase → Character (MonoBehaviour 제거)
   - PlayerCharacter, EnemyCharacter 정리

4. **테스트**
   - 컴파일 확인
   - 기본 동작 확인

---

## 📝 마이그레이션 체크리스트

### Domain 레이어
- [ ] Character 도메인 완료
- [ ] Combat 도메인 완료
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
- [ ] Persistence 완료
- [ ] DI 바인딩 완료

### Presentation 레이어
- [ ] UI 재구성 완료
- [ ] VFX 통합 완료

### 정리 작업
- [ ] 레거시 코드 삭제
- [ ] 네임스페이스 정리
- [ ] 최종 테스트

---

## 🔗 관련 문서

- [아키텍처 리팩토링 계획](./ArchitectureRefactoringPlan.md)
- [완전 재작성 리팩토링 계획](./CompleteRefactoringPlan.md)

