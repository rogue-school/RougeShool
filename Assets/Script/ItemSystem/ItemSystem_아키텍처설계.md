# 아이템 시스템 아키텍처 설계

## 📋 시스템 개요

새로운 아이템 시스템은 기존 레거시 인벤토리를 완전히 대체하며, 액티브/패시브 아이템을 통합 관리하는 현대적인 아키텍처를 제공합니다.

## 🏗️ 전체 아키텍처

```
┌─────────────────────────────────────────────────────────────┐
│                    아이템 시스템 아키텍처                      │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  ┌─────────────────┐    ┌─────────────────┐                │
│  │   액티브 아이템   │    │   패시브 아이템   │                │
│  │                 │    │                 │                │
│  │ • 4슬롯 인벤토리  │    │ • 성급 시스템     │                │
│  │ • 즉시 사용      │    │ • 자동 합성      │                │
│  │ • 드래그앤드롭    │    │ • 데미지 보너스   │                │
│  │ • 교체/버리기     │    │ • 최대 ★3      │                │
│  └─────────────────┘    └─────────────────┘                │
│           │                       │                        │
│           └───────────┬───────────┘                        │
│                     │                                      │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │              통합 아이템 관리 시스템                      │ │
│  │                                                         │ │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │ │
│  │  │ ItemService │  │ ItemFactory │  │ ItemUI      │     │ │
│  │  │             │  │             │  │             │     │ │
│  │  │ • 사용/적용  │  │ • 생성/로드  │  │ • 표시/상호작용│     │ │
│  │  │ • 상태관리   │  │ • 효과생성   │  │ • 드래그앤드롭│     │ │
│  │  │ • 이벤트     │  │ • 검증      │  │ • 툴팁      │     │ │
│  │  └─────────────┘  └─────────────┘  └─────────────┘     │ │
│  └─────────────────────────────────────────────────────────┘ │
│                     │                                      │
│  ┌─────────────────────────────────────────────────────────┐ │
│  │                외부 시스템 연계                         │ │
│  │                                                         │ │
│  │  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐     │ │
│  │  │ 보상 시스템   │  │ 전투 시스템   │  │ 저장 시스템   │     │ │
│  │  │             │  │             │  │             │     │ │
│  │  │ • 적 처치    │  │ • 데미지계산 │  │ • 상태저장   │     │ │
│  │  │ • 보상지급   │  │ • 효과적용   │  │ • 세션복원   │     │ │
│  │  │ • 선택UI     │  │ • 버프/디버프│  │ • 진행상황   │     │ │
│  │  └─────────────┘  └─────────────┘  └─────────────┘     │ │
│  └─────────────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────┘
```

## 🎯 핵심 컴포넌트

### 1. 데이터 모델 계층

#### ItemDefinition (ScriptableObject)
```csharp
// 기본 아이템 정의
public abstract class ItemDefinition : ScriptableObject
{
    [Header("기본 정보")]
    public string itemId;
    public string displayName;
    public string description;
    public Sprite icon;
    public ItemRarity rarity;
}

// 액티브 아이템 정의
public class ActiveItemDefinition : ItemDefinition
{
    [Header("액티브 설정")]
    public ItemEffectSO effect;
    public int maxCharges = 1;
    public float cooldown = 0f;
    public bool consumable = true;
}

// 패시브 아이템 정의 (성급 시스템용)
public class PassiveItemDefinition : ItemDefinition
{
    [Header("패시브 설정")]
    public string targetSkillId; // 대상 스킬 ID
    public int damageBonusPerStar = 1; // 별당 데미지 보너스
    public int maxStars = 3; // 최대 성급
}
```

#### ItemEffectSO (효과 시스템)
```csharp
// 아이템 효과 추상 클래스
public abstract class ItemEffectSO : ScriptableObject
{
    public abstract void ApplyEffect(ItemUseContext context);
    public abstract bool CanUse(ItemUseContext context);
}

// 구체적 효과들
public class HealEffectSO : ItemEffectSO
public class AttackBuffEffectSO : ItemEffectSO
public class DrawCardEffectSO : ItemEffectSO
public class TimeStopEffectSO : ItemEffectSO
public class DiceEffectSO : ItemEffectSO
public class ShieldBreakerEffectSO : ItemEffectSO
public class ReviveEffectSO : ItemEffectSO
```

### 2. 서비스 계층

#### ItemService (핵심 비즈니스 로직)
```csharp
public class ItemService : IItemService
{
    // 액티브 아이템 관리
    public bool UseActiveItem(int slotIndex);
    public bool AddActiveItem(ActiveItemDefinition item);
    public bool RemoveActiveItem(int slotIndex);
    public ActiveItemSlot[] GetActiveSlots();
    
    // 패시브 아이템 관리 (성급 시스템)
    public void AddPassiveItem(PassiveItemDefinition item);
    public int GetSkillStarRank(string skillId);
    public int GetSkillDamageBonus(string skillId);
    
    // 이벤트
    public event Action<ActiveItemDefinition> OnActiveItemUsed;
    public event Action<string, int> OnSkillStarUpgraded;
}
```

#### ItemFactory (생성 및 검증)
```csharp
public class ItemFactory : IItemFactory
{
    public ItemDefinition CreateItem(string itemId);
    public ItemEffectSO CreateEffect(ItemEffectType type);
    public bool ValidateItem(ItemDefinition item);
}
```

### 3. UI 계층

#### ItemUI (사용자 인터페이스)
```csharp
public class ItemUI : MonoBehaviour
{
    // 액티브 아이템 UI
    public ActiveItemSlot[] activeSlots;
    public Button[] useButtons;
    public Image[] cooldownOverlays;
    
    // 패시브 아이템 UI (성급 표시)
    public SkillStarDisplay[] skillStarDisplays;
    
    // 드래그앤드롭 시스템
    public ItemDragHandler dragHandler;
    public ItemDropZone dropZone;
    public TrashBin trashBin;
    
    // 툴팁 시스템
    public ItemTooltip tooltip;
}
```

#### ActiveItemSlot (액티브 슬롯)
```csharp
public class ActiveItemSlot : MonoBehaviour, IDropHandler
{
    [Header("UI 컴포넌트")]
    public Image itemIcon;
    public TextMeshProUGUI chargeText;
    public Button useButton;
    public Image cooldownOverlay;
    
    private ActiveItemDefinition currentItem;
    private int currentCharges;
    private float cooldownTimer;
    
    public void SetItem(ActiveItemDefinition item);
    public void UseItem();
    public void RemoveItem();
    public void OnDrop(PointerEventData eventData);
}
```

### 4. 외부 시스템 연계

#### 보상 시스템 연계
```csharp
public class RewardItemManager : MonoBehaviour
{
    [Inject] private IItemService itemService;
    
    public void GiveActiveItemReward(ActiveItemDefinition item)
    {
        // 인벤토리 풀 체크
        if (itemService.IsActiveInventoryFull())
        {
            // 교체/버리기 UI 표시
            ShowItemReplacementUI(item);
        }
        else
        {
            // 자동 추가
            itemService.AddActiveItem(item);
        }
    }
    
    public void GivePassiveItemReward(PassiveItemDefinition item)
    {
        // 성급 시스템에 자동 적용
        itemService.AddPassiveItem(item);
    }
}
```

#### 전투 시스템 연계
```csharp
public class CombatItemIntegration : MonoBehaviour
{
    [Inject] private IItemService itemService;
    
    // 데미지 계산 시 성급 보너스 적용
    public int CalculateDamageWithStars(string skillId, int baseDamage)
    {
        int starBonus = itemService.GetSkillDamageBonus(skillId);
        return baseDamage + starBonus;
    }
    
    // 부활 효과 처리
    public void HandleReviveEffect()
    {
        // 최대 체력 회복 + 디버프 해제
        // ItemService에서 처리
    }
}
```

## 🔄 데이터 플로우

### 1. 아이템 획득 플로우
```
적 처치 → StageRewardData → RewardItemManager 
→ ItemService.AddActiveItem() → ActiveItemSlot 업데이트
→ UI 갱신
```

### 2. 아이템 사용 플로우
```
사용자 클릭 → ActiveItemSlot.UseItem() → ItemService.UseActiveItem()
→ ItemEffectSO.ApplyEffect() → 전투 시스템에 효과 적용
→ 슬롯 상태 업데이트 → UI 갱신
```

### 3. 성급 시스템 플로우
```
카드 획득 → CardCirculationSystem → ItemService.AddPassiveItem()
→ 성급 계산 및 업데이트 → 데미지 보너스 적용
→ UI 갱신 (성급 표시)
```

## 💾 저장 구조

### SaveData 구조
```csharp
[System.Serializable]
public class ItemSystemSaveData
{
    // 액티브 아이템 슬롯
    public ActiveItemSlotData[] activeSlots;
    
    // 패시브 아이템 성급
    public Dictionary<string, int> skillStarRanks;
    
    // 쿨다운 상태
    public Dictionary<string, float> cooldownTimers;
}

[System.Serializable]
public class ActiveItemSlotData
{
    public string itemId;
    public int charges;
    public float cooldownTimer;
}
```

## 🎨 UI/UX 플로우

### 1. 보상 선택 시나리오
```
보상 UI 표시 → 아이템 선택 → 인벤토리 상태 확인
├─ 빈 슬롯 있음: 자동 추가
├─ 인벤토리 풀: 교체/버리기 선택 UI
│  ├─ 교체 선택: 기존 아이템 휴지통으로 이동
│  └─ 버리기 선택: 새 아이템 휴지통으로 이동
└─ 완료 후 UI 닫기
```

### 2. 드래그앤드롭 시나리오
```
아이템 드래그 시작 → 드롭 존 하이라이트
├─ 유효한 슬롯: 교체 가능 표시
├─ 휴지통: 버리기 가능 표시
└─ 무효한 영역: 드롭 불가 표시
```

## ⚡ 성능 최적화

### 1. 메모리 관리
- Object Pooling: 아이템 UI 요소 재사용
- Lazy Loading: 필요 시에만 아이템 데이터 로드
- 캐싱: 자주 사용되는 아이템 정의 캐시

### 2. 업데이트 최적화
- 이벤트 기반: UI 업데이트는 상태 변경 시에만
- 배치 처리: 여러 아이템 변경 시 일괄 처리
- 프레임 분산: 무거운 연산은 여러 프레임에 분산

## 🔧 확장성 고려사항

### 1. 새로운 아이템 타입 추가
- `ItemDefinition` 상속으로 새로운 타입 정의
- `ItemEffectSO` 상속으로 새로운 효과 구현
- UI 컴포넌트 확장으로 새로운 인터페이스 제공

### 2. 새로운 효과 시스템
- 플러그인 방식으로 효과 추가 가능
- 런타임 효과 조합 지원
- 조건부 효과 적용 시스템

### 3. 다국어 지원
- 아이템 이름/설명 다국어 지원
- 지역별 밸런스 조정 가능
- 문화적 차이 고려한 UI/UX

## 📋 구현 우선순위

### Phase 1: 핵심 시스템
1. `ItemDefinition` 및 `ItemEffectSO` 기본 구조
2. `ItemService` 핵심 로직
3. 기본 액티브 아이템 효과 (회복, 공격력 버프)

### Phase 2: UI 시스템
1. `ActiveItemSlot` 기본 구현
2. 드래그앤드롭 시스템
3. 툴팁 및 상태 표시

### Phase 3: 통합 및 고도화
1. 보상 시스템 연계
2. 성급 시스템 구현
3. 저장/로드 시스템

### Phase 4: 최적화 및 확장
1. 성능 최적화
2. 추가 효과 구현
3. UI/UX 개선

이 아키텍처는 기존 시스템과의 호환성을 유지하면서도 확장 가능하고 유지보수가 용이한 구조를 제공합니다.
