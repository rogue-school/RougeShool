# RougeShool 코드 품질 진단 리포트

> 작성일: 2025-11-24  
> 목적: Cursor User Rule 기준 프로젝트 전반의 개선 필요 사항 진단

---

## 📊 종합 요약

| 항목 | 발견 건수 | 우선순위 | 상태 |
|------|----------|---------|------|
| **금지된 API 사용** | 2개 파일 | 🔥 높음 | FindObjectOfType 사용 |
| **Update 루프** | 9개 파일 | 🔥 높음 | 이벤트 기반 전환 필요 |
| **Resources.Load** | 17개 파일 | ⚠️ 중간 | Addressables 전환 검토 |
| **DOTween 사용** | 26개 파일 | ⚠️ 중간 | 메모리 안전 검증 필요 |
| **TODO/FIXME** | 29개 | ⚠️ 중간 | 정리 필요 |
| **Inspector 한글화** | 879개 | ✅ 양호 | 대부분 적용됨 |
| **XML 문서화** | 3,978개 | ✅ 양호 | 대부분 적용됨 |

---

## 🔥 높은 우선순위 개선 사항

### 1. FindObjectOfType 사용 (금지된 API)

**규칙 위반**: `FindObjectOfType` 사용은 금지되어 있으며, Zenject DI로 대체해야 합니다.

#### 발견된 파일 (2개)

1. **`Assets/Script/CoreSystem/Save/SaveManager.cs`**
   - 문제: 8개의 매니저를 `FindObjectOfType`으로 캐싱
   - 영향: 의존성 주입 패턴 위반, 테스트 어려움
   - 개선 방안:
     ```csharp
     // ❌ 현재 (FindObjectOfType 캐싱)
     private Game.StageSystem.Manager.StageManager cachedStageManager;
     private GameManager GetCachedStageManager() {
         if (cachedStageManager == null)
             cachedStageManager = FindObjectOfType<StageManager>();
     }
     
     // ✅ 개선 (Zenject DI)
     [Inject] private IStageManager stageManager;
     ```

2. **`Assets/Script/CoreSystem/Manager/SceneTransitionManager.cs`**
   - 문제: `FindObjectOfType` 사용
   - 개선 방안: Zenject DI로 전환

#### 개선 작업량
- **예상 시간**: 2-3시간
- **영향 범위**: SaveManager, SceneTransitionManager
- **위험도**: 중간 (기존 동작 유지하면서 DI 전환)

---

### 2. Update/FixedUpdate/LateUpdate 사용 (9개 파일)

**규칙 위반**: Update() 메서드는 이벤트 기반으로 전환해야 합니다. 단, Input 폴링, Physics 계산, 실시간 보간은 예외입니다.

#### 발견된 파일 (9개)

1. **`Assets/Script/ItemSystem/Runtime/InventoryPanelController.cs`**
   - 문제: `Update()`에서 `Input.GetMouseButtonDown(0)` 폴링
   - 평가: ✅ **허용** (Input 폴링은 예외)
   - 개선 제안: 이벤트 기반으로 전환 가능 (PointerClick 이벤트 활용)

2. **`Assets/Script/ItemSystem/Manager/ItemTooltipManager.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

3. **`Assets/Script/CharacterSystem/Manager/BuffDebuffTooltipManager.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

4. **`Assets/Script/SkillCardSystem/Manager/SkillCardTooltipManager.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

5. **`Assets/Script/CombatSystem/State/CombatStateMachine.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

6. **`Assets/Script/UISystem/ButtonHoverEffect.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

7. **`Assets/Script/StageSystem/UI/StageUIController.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

8. **`Assets/Script/CombatSystem/Utility/UnityMainThreadDispatcher.cs`**
   - 상태: 검토 필요 (메인 스레드 디스패처는 예외 가능성)

9. **`Assets/Script/UISystem/SettingsUIController.cs`**
   - 상태: 검토 필요 (Update 내용 확인 필요)

#### 개선 작업량
- **예상 시간**: 4-6시간 (파일별 검토 후 이벤트 전환)
- **영향 범위**: 9개 파일
- **위험도**: 낮음 (점진적 전환 가능)

---

## ⚠️ 중간 우선순위 개선 사항

### 3. Resources.Load 사용 (17개 파일)

**규칙 위반**: `Resources.Load`는 Addressables로 전환하는 것이 권장됩니다.

#### 발견된 파일 (17개)

주요 파일:
- `Assets/Script/CoreSystem/Audio/AudioManager.cs`
- `Assets/Script/CharacterSystem/Manager/PlayerManager.cs`
- `Assets/Script/SkillCardSystem/Factory/SkillCardFactory.cs`
- `Assets/Script/ItemSystem/Cache/ItemResourceCache.cs`
- 기타 13개 파일

#### 개선 방안

```csharp
// ❌ 현재 (Resources.Load)
AudioClip clip = Resources.Load<AudioClip>("Sounds/BGM/MainTheme");

// ✅ 개선 (Addressables)
var handle = Addressables.LoadAssetAsync<AudioClip>("Sounds/BGM/MainTheme");
await handle.Task;
AudioClip clip = handle.Result;
```

#### 개선 작업량
- **예상 시간**: 8-12시간 (Addressables 설정 포함)
- **영향 범위**: 17개 파일
- **위험도**: 중간 (리소스 로딩 경로 변경)

---

### 4. DOTween 메모리 안전 검증 (26개 파일)

**규칙 위반**: DOTween 사용 시 `SetAutoKill(true)`, `OnDisable/OnDestroy`에서 Kill 필수

#### 발견된 파일 (26개)

주요 파일:
- `Assets/Script/SkillCardSystem/DragDrop/CardDragHandler.cs`
- `Assets/Script/CharacterSystem/Core/CharacterBase.cs`
- `Assets/Script/SkillCardSystem/UI/SkillCardUI.cs`
- `Assets/Script/ItemSystem/Runtime/ActiveItemUI.cs`
- 기타 22개 파일

#### 검증 체크리스트

각 파일에서 다음을 확인해야 합니다:

```csharp
// ✅ 필수: SetAutoKill(true)
transform.DOMove(targetPos, 1f)
    .SetAutoKill(true)  // 필수
    .OnComplete(() => { /* ... */ });

// ✅ 필수: OnDisable에서 Kill
private void OnDisable()
{
    // 모든 활성 Tween Kill
    DOTween.Kill(this);
    // 또는 개별 추적
    activeTweens?.ForEach(t => t?.Kill());
}
```

#### 개선 작업량
- **예상 시간**: 6-8시간 (26개 파일 검토 및 수정)
- **영향 범위**: 26개 파일
- **위험도**: 중간 (메모리 누수 방지)

---

### 5. TODO/FIXME 주석 정리 (29개)

**규칙 위반**: 리팩토링 모드에서는 TODO 주석을 제거하고 완전한 구현이 필요합니다.

#### 발견된 위치

주요 파일:
- `Assets/Script/CombatSystem/Manager/CombatFlowManager.cs` (2개)
- `Assets/Script/CombatSystem/Utility/SlotSelector.cs` (2개)
- `Assets/Script/CombatSystem/Initialization/SlotInitializationStep.cs` (3개)
- 기타 20개 파일

#### 개선 작업량
- **예상 시간**: 4-6시간 (각 TODO 해결)
- **영향 범위**: 29개 위치
- **위험도**: 낮음 (기능 동작에 영향 없음)

---

## ✅ 양호한 영역

### 1. Inspector 한글화 (879개)

- **상태**: 대부분의 Inspector 필드에 `[Header]`, `[Tooltip]` 한글 적용됨
- **평가**: ✅ **양호**

### 2. XML 문서화 (3,978개)

- **상태**: 대부분의 public API에 XML 문서화 적용됨
- **평가**: ✅ **양호**

### 3. Singleton 패턴

- **발견 건수**: 0개
- **평가**: ✅ **완벽** (Zenject DI 사용)

---

## 📋 시스템별 상세 진단

### CoreSystem

| 항목 | 발견 | 상태 |
|------|------|------|
| FindObjectOfType | 2개 파일 | 🔥 개선 필요 |
| Update 루프 | 1개 파일 | ⚠️ 검토 필요 |
| Resources.Load | 3개 파일 | ⚠️ Addressables 전환 검토 |

**주요 개선 대상**:
- `SaveManager.cs`: FindObjectOfType → DI 전환
- `SceneTransitionManager.cs`: FindObjectOfType → DI 전환

### CombatSystem

| 항목 | 발견 | 상태 |
|------|------|------|
| Update 루프 | 2개 파일 | ⚠️ 검토 필요 |
| DOTween | 다수 | ⚠️ 메모리 안전 검증 |
| TODO | 7개 | ⚠️ 정리 필요 |

**주요 개선 대상**:
- `CombatStateMachine.cs`: Update 내용 검토
- DOTween 사용 파일들: 메모리 안전 검증

### CharacterSystem

| 항목 | 발견 | 상태 |
|------|------|------|
| Update 루프 | 1개 파일 | ⚠️ 검토 필요 |
| Resources.Load | 2개 파일 | ⚠️ Addressables 전환 검토 |
| DOTween | 다수 | ⚠️ 메모리 안전 검증 |

### SkillCardSystem

| 항목 | 발견 | 상태 |
|------|------|------|
| Update 루프 | 1개 파일 | ⚠️ 검토 필요 |
| Resources.Load | 3개 파일 | ⚠️ Addressables 전환 검토 |
| DOTween | 다수 | ⚠️ 메모리 안전 검증 |

### ItemSystem

| 항목 | 발견 | 상태 |
|------|------|------|
| Update 루프 | 2개 파일 | ⚠️ 검토 필요 (1개는 Input 폴링으로 허용 가능) |
| Resources.Load | 2개 파일 | ⚠️ Addressables 전환 검토 |
| DOTween | 다수 | ⚠️ 메모리 안전 검증 |

### UISystem

| 항목 | 발견 | 상태 |
|------|------|------|
| Update 루프 | 2개 파일 | ⚠️ 검토 필요 |
| DOTween | 다수 | ⚠️ 메모리 안전 검증 |

---

## 🎯 개선 우선순위 로드맵

### Phase 1: 긴급 수정 (1주)

1. **FindObjectOfType 제거** (2-3시간)
   - SaveManager DI 전환
   - SceneTransitionManager DI 전환

2. **Update 루프 검토** (4-6시간)
   - 9개 파일 검토 및 이벤트 전환

### Phase 2: 중요 개선 (2주)

3. **DOTween 메모리 안전** (6-8시간)
   - 26개 파일 검토 및 수정

4. **TODO 정리** (4-6시간)
   - 29개 TODO 해결

### Phase 3: 장기 개선 (선택)

5. **Resources.Load → Addressables** (8-12시간)
   - 17개 파일 전환 (선택적)

---

## 📊 품질 점수

| 영역 | 점수 | 평가 |
|------|------|------|
| **아키텍처 준수** | 85/100 | FindObjectOfType 2건 제외하면 양호 |
| **성능 최적화** | 75/100 | Update 루프, DOTween 검증 필요 |
| **코드 품질** | 90/100 | 문서화, 한글화 양호 |
| **메모리 안전** | 70/100 | DOTween 검증 필요 |
| **종합 점수** | **80/100** | ⭐⭐⭐⭐ |

---

## 🔍 다음 단계

1. **즉시 조치**: FindObjectOfType 2건 제거
2. **단기 조치**: Update 루프 검토 및 DOTween 메모리 안전 검증
3. **중기 조치**: TODO 정리
4. **장기 조치**: Addressables 전환 (선택)

---

## 📝 변경 기록

| 날짜 | 담당 | 내용 |
|------|------|------|
| 2025-11-24 | Cursor AI | 초기 진단 리포트 작성 |

