# RougeShool 완전 재작성 리팩토링 계획

> 작성일: 2025-11-24  
> 목적: 기존 코드를 개선하는 것이 아닌, **완전히 새로운 코드로 재작성**하며 레거시 코드를 제거하는 전면 리팩토링 계획

---

## 🎯 리팩토링 철학

### 기존 접근 vs 새로운 접근

| 구분 | 기존 리팩토링 | 완전 재작성 리팩토링 |
|------|--------------|-------------------|
| **방식** | 기존 코드 수정/개선 | 완전히 새로 작성 |
| **레거시 코드** | 호환성 유지 | 완전 제거 |
| **변수/함수명** | 기존 유지 | 새 유저룰에 맞게 재명명 |
| **구조** | 점진적 개선 | 처음부터 새 구조 |
| **위험도** | 낮음 (점진적) | 중간 (전면 재작성) |

### 핵심 원칙

1. **제로 레거시**: 모든 레거시 코드 제거, 호환성 유지 불필요
2. **완전 재작성**: 기존 코드 참고만 하고 새로 작성
3. **유저룰 100% 준수**: 모든 함수/변수/구조를 새 유저룰에 맞게
4. **불필요 코드 제거**: 사용하지 않는 모든 코드 삭제

---

## 📊 발견된 레거시 및 사용하지 않는 코드

### 1. 레거시 타입 및 변환 코드

#### TurnManager.cs
- **레거시 TurnType enum** (420-431줄)
  - `public enum TurnType { Player, Enemy }`
  - 새로운 `Interface.TurnType`로 대체됨
  - **제거 대상**: enum 정의 + 변환 메서드 2개

- **변환 메서드** (400-417줄)
  - `ConvertToLegacyTurnType()`
  - `ConvertToNewTurnType()`
  - **제거 대상**: 완전 삭제

#### 개선 방안
```csharp
// ❌ 제거할 코드
public enum TurnType { Player, Enemy }  // 레거시
private TurnType ConvertToLegacyTurnType(...) { ... }
private Interface.TurnType ConvertToNewTurnType(...) { ... }

// ✅ 새 코드: Interface.TurnType만 사용
// 변환 메서드 불필요
```

---

### 2. 사용하지 않는 레거시 컴포넌트

#### TurnStartButtonHandler.cs
- **문제**: 상태 패턴 전환으로 사용되지 않음
- **증거**: 88-90줄 주석
  ```csharp
  // 레거시: 상태 패턴으로 전환되어 이 버튼은 사용되지 않음
  // turnManager?.NextTurn(); // 제거됨
  GameLogger.LogWarning("[TurnStartButtonHandler] 레거시 버튼...");
  ```
- **제거 대상**: 전체 파일 삭제 또는 완전 재작성

#### 개선 방안
```csharp
// ❌ 제거: TurnStartButtonHandler.cs 전체
// 상태 패턴에서 자동으로 턴 진행되므로 불필요

// ✅ 대체: 상태 패턴이 자동으로 처리
// 별도 버튼 핸들러 불필요
```

---

### 3. 테스트/디버그 코드

#### TestItemButton.cs
- **위치**: `Assets/Script/ItemSystem/Runtime/TestItemButton.cs`
- **문제**: 프로덕션 코드에 테스트 코드 포함
- **제거 대상**: 전체 파일 삭제

#### 개선 방안
```csharp
// ❌ 제거: TestItemButton.cs 전체
// 프로덕션 코드에서 테스트 코드 제거

// ✅ 대체: 필요 시 Editor 폴더로 이동 또는 완전 삭제
```

---

### 4. 순환 의존성 (3개 발견)

#### 발견된 순환 의존성
1. **SlotMovementController** → 자기 자신
2. **TurnController** → 자기 자신  
3. **SkillCardFactory** → 자기 자신

#### 개선 방안
- 각 클래스의 자기 참조 제거
- 인터페이스 도입으로 순환 의존성 해결
- 의존성 방향 재설계

---

### 5. 주석 처리된 코드

#### 발견 건수
- **레거시 관련 주석**: 92개 파일
- **TODO/FIXME**: 29개
- **일반 주석**: 7,770개 (일부는 유지 필요)

#### 제거 우선순위

**HIGH (즉시 제거)**:
```csharp
// ❌ 제거 대상
// 레거시: 사용 안함
// TODO: 제거 필요
// FIXME: 삭제 예정
// 주석 처리된 코드 블록 (/* ... */)
```

**MEDIUM (검토 후 제거)**:
```csharp
// ⚠️ 검토 필요
// TODO: 구현 필요 (미완성 기능)
// 주석 처리된 메서드 (사용 여부 확인)
```

---

### 6. 사용하지 않는 public 메서드

#### 발견 건수
- **Public 메서드**: 793개
- **사용 여부 미확인**: 다수

#### 검증 방법
1. 각 public 메서드에 대한 grep 검색
2. 0개 참조 = 사용하지 않음
3. 인터페이스 구현 메서드는 예외

---

## 🗂️ 시스템별 완전 재작성 계획

### Phase 1: CoreSystem (1주)

#### 1.1 SaveManager.cs 완전 재작성

**제거 대상**:
- FindObjectOfType 캐싱 (8개 매니저)
- 레거시 호환 메서드
- 주석 처리된 코드

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
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
    // 레거시 호환 코드 완전 제거
    // 새 유저룰에 맞게 완전 재작성
}
```

**작업량**: 8-12시간

---

#### 1.2 SceneTransitionManager.cs 완전 재작성

**제거 대상**:
- FindObjectOfType 사용
- 레거시 씬 전환 로직

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
public class SceneTransitionManager : MonoBehaviour, ISceneTransitionManager
{
    #region Dependency Injection
    
    [Inject] private IGameStateManager gameStateManager;
    [Inject] private IAudioManager audioManager;
    
    #endregion
    
    // FindObjectOfType 완전 제거
    // 이벤트 기반 씬 전환으로 재작성
}
```

**작업량**: 4-6시간

---

### Phase 2: CombatSystem (2주)

#### 2.1 TurnManager.cs 완전 재작성

**제거 대상**:
- 레거시 `TurnType` enum (420-431줄)
- 변환 메서드 2개 (400-417줄)
- 레거시 호환 메서드 전체

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
public class TurnManager : MonoBehaviour, ITurnManager
{
    // 레거시 TurnType enum 완전 제거
    // Interface.TurnType만 사용
    // 변환 메서드 불필요
    
    #region Dependency Injection
    
    [Inject] private ITurnController turnController;
    
    #endregion
    
    // 새 유저룰에 맞게 완전 재작성
    // 모든 메서드/변수명 새로 명명
}
```

**작업량**: 12-16시간

---

#### 2.2 TurnStartButtonHandler.cs 제거 또는 재작성

**옵션 1: 완전 제거**
- 상태 패턴에서 자동 처리되므로 불필요
- **작업량**: 1시간 (파일 삭제 + 참조 제거)

**옵션 2: 완전 재작성**
- 새로운 요구사항에 맞게 재작성
- **작업량**: 4-6시간

**권장**: 옵션 1 (제거)

---

#### 2.3 CombatStateMachine.cs 완전 재작성

**제거 대상**:
- 레거시 상태 관리 코드
- 주석 처리된 코드
- 디버그 코드

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
public class CombatStateMachine : MonoBehaviour
{
    #region Dependency Injection
    
    [Inject] private ICombatExecutionManager executionManager;
    [Inject] private ITurnController turnController;
    // ... 모든 의존성 DI로 주입
    
    #endregion
    
    // 레거시 코드 완전 제거
    // 새 유저룰에 맞게 완전 재작성
}
```

**작업량**: 16-20시간

---

### Phase 3: CharacterSystem (1.5주)

#### 3.1 PlayerManager.cs 완전 재작성

**제거 대상**:
- Resources.Load 사용
- 레거시 초기화 로직
- 중복 코드

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
public class PlayerManager : BaseCharacterManager, IPlayerManager
{
    #region Dependency Injection
    
    [Inject] private IPlayerCharacter playerCharacter;
    [Inject] private IPlayerResourceManager resourceManager;
    // Addressables로 리소스 로딩
    
    #endregion
    
    // Resources.Load 완전 제거
    // 새 유저룰에 맞게 완전 재작성
}
```

**작업량**: 10-14시간

---

### Phase 4: SkillCardSystem (2주)

#### 4.1 SkillCardFactory.cs 순환 의존성 해결

**문제**: 자기 자신 참조

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
public class SkillCardFactory : ISkillCardFactory
{
    // 순환 의존성 완전 제거
    // 인터페이스 기반으로 재설계
    // 새 유저룰에 맞게 완전 재작성
}
```

**작업량**: 8-12시간

---

### Phase 5: ItemSystem (1주)

#### 5.1 TestItemButton.cs 제거

**작업**:
- 파일 완전 삭제
- 참조 제거

**작업량**: 1시간

---

#### 5.2 ItemService.cs 완전 재작성

**제거 대상**:
- 레거시 아이템 처리 로직
- 주석 처리된 코드
- 중복 코드

**새 구조**:
```csharp
// ✅ 완전히 새로 작성
public class ItemService : MonoBehaviour, IItemService
{
    #region Dependency Injection
    
    [Inject] private IAudioManager audioManager;
    [Inject] private IItemTooltipManager tooltipManager;
    
    #endregion
    
    // 레거시 코드 완전 제거
    // 새 유저룰에 맞게 완전 재작성
}
```

**작업량**: 10-14시간

---

## 📋 제거 대상 파일 목록

### 즉시 삭제 대상

1. **`Assets/Script/ItemSystem/Runtime/TestItemButton.cs`**
   - 이유: 테스트 코드
   - 작업: 파일 삭제

2. **`Assets/Script/CombatSystem/Core/TurnStartButtonHandler.cs`** (옵션)
   - 이유: 레거시, 사용 안함
   - 작업: 파일 삭제 또는 완전 재작성

---

## 🔄 재작성 우선순위

### 우선순위 1: 핵심 시스템 (2주)

1. **SaveManager.cs** - FindObjectOfType 제거
2. **SceneTransitionManager.cs** - FindObjectOfType 제거
3. **TurnManager.cs** - 레거시 타입 제거

### 우선순위 2: 전투 시스템 (2주)

4. **CombatStateMachine.cs** - 레거시 코드 제거
5. **TurnStartButtonHandler.cs** - 제거 또는 재작성
6. **CombatExecutionManager.cs** - 완전 재작성

### 우선순위 3: 순환 의존성 해결 (1주)

7. **SlotMovementController.cs** - 순환 의존성 제거
8. **TurnController.cs** - 순환 의존성 제거
9. **SkillCardFactory.cs** - 순환 의존성 제거

### 우선순위 4: 캐릭터/카드 시스템 (2주)

10. **PlayerManager.cs** - Resources.Load 제거
11. **EnemyManager.cs** - 완전 재작성
12. **SkillCardFactory.cs** - 완전 재작성

### 우선순위 5: 정리 작업 (1주)

13. **TestItemButton.cs** - 삭제
14. **모든 TODO/FIXME** - 해결 또는 제거
15. **주석 처리된 코드** - 제거

---

## 📊 예상 작업량

| Phase | 시스템 | 파일 수 | 예상 시간 | 우선순위 |
|-------|--------|---------|----------|---------|
| 1 | CoreSystem | 2개 | 12-18시간 | 🔥 높음 |
| 2 | CombatSystem | 3개 | 28-36시간 | 🔥 높음 |
| 3 | 순환 의존성 | 3개 | 24-36시간 | ⚠️ 중간 |
| 4 | Character/SkillCard | 3개 | 28-40시간 | ⚠️ 중간 |
| 5 | 정리 작업 | 다수 | 8-12시간 | ⚠️ 낮음 |
| **총계** | **전체** | **11+** | **100-142시간** | - |

**예상 기간**: 4-6주 (주 20시간 기준)

---

## ✅ 재작성 체크리스트

### 각 파일 재작성 시 확인 사항

- [ ] 레거시 코드 완전 제거
- [ ] FindObjectOfType 제거 (DI로 대체)
- [ ] Resources.Load 제거 (Addressables로 대체)
- [ ] Update() 제거 (이벤트 기반으로 전환)
- [ ] 주석 처리된 코드 제거
- [ ] TODO/FIXME 해결 또는 제거
- [ ] 순환 의존성 제거
- [ ] 모든 변수/함수명 새 유저룰에 맞게 재명명
- [ ] 3-계층 예외 처리 적용
- [ ] XML 문서화 완료
- [ ] Inspector 한글화 완료
- [ ] DOTween 메모리 안전 적용
- [ ] 컴파일 오류 0개
- [ ] 경고 0개

---

## 🚨 주의사항

### 위험 관리

1. **백업 필수**: 각 Phase 시작 전 Git 커밋
2. **점진적 진행**: 한 번에 하나씩 재작성
3. **테스트**: 각 파일 재작성 후 즉시 테스트
4. **롤백 계획**: 문제 발생 시 즉시 롤백

### 호환성

- **레거시 호환성 유지 불필요**: 완전 재작성이므로 기존 코드와 호환 유지 불필요
- **데이터 호환성**: Save 데이터는 마이그레이션 필요할 수 있음

---

## 📝 변경 기록

| 날짜 | 담당 | 내용 |
|------|------|------|
| 2025-11-24 | Cursor AI | 완전 재작성 리팩토링 계획 초안 작성 |

---

## 🛠️ MCP 서버 활용 전략

### 리팩토링 전 검증 도구

각 Phase 시작 전 MCP 서버 도구를 활용하여 코드베이스 상태를 검증합니다.

#### Phase 1: CoreSystem 재작성 전

```bash
# 1. 금지된 API 검사
MCP: check_forbidden_apis
→ FindObjectOfType, Resources.Load 등 검사

# 2. 순환 의존성 감지
MCP: detect_circular_dependencies
→ SaveManager, SceneTransitionManager 의존성 확인

# 3. 품질 게이트 리포트
MCP: quality_gate_report
→ 전체 프로젝트 품질 상태 확인
```

#### Phase 2: CombatSystem 재작성 전

```bash
# 1. Update 루프 감지
MCP: detect_update_loops
→ CombatStateMachine 등 Update 사용 확인

# 2. DOTween 수명주기 검사
MCP: dotween_lifecycle_check
→ DOTween 메모리 안전 확인

# 3. 코드 중복 감지
MCP: detect_code_duplication
→ 중복 코드 블록 확인
```

#### Phase 3: 순환 의존성 해결 전

```bash
# 1. 순환 의존성 상세 분석
MCP: detect_circular_dependencies
→ SlotMovementController, TurnController, SkillCardFactory 분석

# 2. 순환 복잡도 계산
MCP: calculate_cyclomatic_complexity
→ 복잡한 메서드 식별
```

#### Phase 4: Character/SkillCard 재작성 전

```bash
# 1. Resources.Load 감사
MCP: addressables_audit
→ Resources.Load 사용 지점 확인

# 2. XML 문서화 검사
MCP: check_xml_documentation
→ 문서화 완성도 확인

# 3. Inspector 한글화 검사
MCP: inspector_korean_labels_check
→ 한글화 규칙 적용 확인
```

#### Phase 5: 정리 작업 전

```bash
# 1. 전체 품질 게이트 리포트
MCP: quality_gate_report
→ 최종 품질 상태 확인

# 2. GC 할당 분석
MCP: analyze_gc_allocations
→ 메모리 할당 패턴 확인
```

---

## 🎨 에셋 활용 전략

### ScriptableObject 에셋 (44개)

#### 재작성 시 활용 방안

**1. CharacterSystem 데이터 (11개)**
- `PlayerCharacterData`: 4개 에셋
  - 위치: `Assets/Resources/Data/Character/PlayerCharacters/`
  - 활용: 재작성 시 기존 에셋 재사용, 코드만 재작성
- `EnemyCharacterData`: 7개 에셋
  - 위치: `Assets/Resources/Data/Character/EnemyCharters/`
  - 활용: 기존 에셋 유지, 로딩 방식만 개선

**2. SkillCardSystem 데이터 (48+개)**
- `SkillCardDefinition`: 48개 에셋
  - 위치: `Assets/Resources/Data/SkillCard/Skill/`
  - 활용: 기존 에셋 재사용, Addressables로 전환

**3. ItemSystem 데이터 (59+개)**
- `ActiveItemDefinition`: 16개 에셋
- `PassiveItemDefinition`: 43개 에셋
  - 위치: `Assets/Resources/Data/Item/`
  - 활용: 기존 에셋 유지, 로딩 방식 개선

### Resources 폴더 구조 활용

#### 현재 구조
```
Assets/Resources/
├── Data/              # ScriptableObject (재사용)
├── Effect/            # 이펙트 프리팹 (재사용)
├── Font/              # 폰트 (재사용)
├── Image/             # 이미지 리소스 (재사용)
├── Prefab/            # 프리팹 (재사용)
└── Sounds/            # 오디오 파일 (재사용)
```

#### 리팩토링 전략

**1. 에셋은 유지, 로딩 방식만 개선**
- ✅ 기존 ScriptableObject 에셋 모두 재사용
- ✅ Resources.Load → Addressables 전환
- ✅ 에셋 경로는 유지 (마이그레이션 최소화)

**2. 프리팹 재사용**
- ✅ 기존 프리팹 모두 재사용
- ✅ 프리팹 내 스크립트만 재작성
- ✅ 프리팹 구조는 유지

**3. 리소스 재사용**
- ✅ 이미지, 사운드, 폰트 모두 재사용
- ✅ 로딩 방식만 개선

---

## 📋 에셋별 재작성 우선순위

### 우선순위 1: 데이터 로딩 개선

| 에셋 타입 | 개수 | 현재 방식 | 개선 방식 | 작업량 |
|----------|------|----------|----------|--------|
| PlayerCharacterData | 4개 | Resources.Load | Addressables | 2-3시간 |
| EnemyCharacterData | 7개 | Resources.Load | Addressables | 3-4시간 |
| SkillCardDefinition | 48개 | Resources.Load | Addressables | 4-6시간 |
| ActiveItemDefinition | 16개 | Resources.Load | Addressables | 2-3시간 |
| PassiveItemDefinition | 43개 | Resources.Load | Addressables | 3-4시간 |

**총 작업량**: 14-20시간

### 우선순위 2: 프리팹 스크립트 재작성

| 프리팹 | 스크립트 | 재작성 필요 | 작업량 |
|--------|---------|-----------|--------|
| SkillCard.prefab | SkillCardUI.cs | ✅ | 4-6시간 |
| PlayerCharacter.prefab | PlayerCharacter.cs | ✅ | 6-8시간 |
| EnemyCharacter.prefab | EnemyCharacter.cs | ✅ | 4-6시간 |
| RewardPanel.prefab | RewardPanelController.cs | ✅ | 6-8시간 |

**총 작업량**: 20-28시간

---

## 🔗 관련 문서

- [리팩토링 마스터 플랜](./RefactoringMasterPlan.md)
- [코드 품질 진단 리포트](./CodeQualityDiagnosisReport.md)
- [코드 로직 문서](./CodeLogicDocumentation.md)
- **[스크립트 상세 분석 및 재작성 계획](./DetailedScriptAnalysis.md)**: 모든 스크립트를 하나하나 체크하여 제거할 코드와 새로 작성할 코드를 정확하게 판단한 상세 분석 문서

