# 🎮 스킬 카드 툴팁 시스템 가이드

> **목적**: 전문적인 계층적 툴팁 시스템 구현 및 사용 가이드

## 🎯 시스템 개요

### **계층적 툴팁 구조**
- ✅ **메인 툴팁**: 카드 기본 정보 (아이콘, 이름, 설명, 통계, 효과 목록)
- ✅ **서브 툴팁**: 개별 효과에 대한 상세 설명 (호버 시 표시)
- ✅ **호버 이벤트**: 효과 아이템에 마우스 오버 시 서브 툴팁 표시
- ✅ **자동 정리**: 메인 툴팁 숨김 시 서브 툴팁도 함께 숨김

### **최적화된 핵심 기능**
- ✅ **실제 게임 효과 시스템**: 현재 게임의 실제 효과들을 반영
- ✅ **동적 효과 표시**: 카드에 실제로 있는 효과만 표시
- ✅ **아이콘 + 텍스트 시스템**: 시각적으로 직관적인 정보 표시
- ✅ **색상 구분**: 효과 타입별 색상으로 구분

### **시각적 효과**
- ✅ **볼드 처리**: 효과 이름이 `<b>` 태그로 강조 표시
- ✅ **색상 강조**: 호버 시 효과 이름 색상 변경 (기본: 흰색 → 강조: 노란색)
- ✅ **부드러운 애니메이션**: DOTween을 사용한 페이드 인/아웃
- ✅ **스마트 위치**: 화면 경계 내 자동 배치

## 🔧 최적화된 툴팁 구조

### **새로운 구조**
```
┌─────────────────────────────────────┐
│ [아이콘] 스킬 이름                    │ ← 헤더 (아이콘 + 이름)
├─────────────────────────────────────┤
│ 스킬 설명 텍스트...                  │ ← 설명
├─────────────────────────────────────┤
│ [데미지아이콘] 데미지: 100            │ ← 통계 (아이콘 + 텍스트)
│ [자원아이콘] 소모 자원: 2            │
├─────────────────────────────────────┤
│ [출혈아이콘] <b>출혈</b>              │ ← 실제 게임 효과들 (볼드 처리)
│ 출혈량: 5, 지속: 3턴                │
│ [반격아이콘] <b>반격</b>              │
│ 반격 지속: 2턴                      │
└─────────────────────────────────────┘
```

### **실제 게임 효과 시스템**
```csharp
// 실제 게임의 효과들을 반영한 효과 생성
private System.Collections.Generic.List<EffectData> GetCardEffects(SkillCardDefinition definition)
{
    var effects = new System.Collections.Generic.List<EffectData>();
    var config = definition.configuration;

    // 출혈 효과
    if (config.hasEffects && config.effects != null)
    {
        foreach (var effectConfig in config.effects)
        {
            if (effectConfig.useCustomSettings && effectConfig.customSettings != null)
            {
                var customSettings = effectConfig.customSettings;

                if (customSettings.bleedAmount > 0) 
                {
                    effects.Add(new EffectData 
                    { 
                        name = "출혈", 
                        description = $"출혈량: {customSettings.bleedAmount}, 지속: {customSettings.bleedDuration}턴", 
                        iconColor = Color.red, 
                        effectType = EffectType.Debuff 
                    });
                }
                // ... 기타 효과들
            }
        }
    }
    return effects;
}
```

## 🔧 구현된 컴포넌트

### **1. 확장된 EffectItemComponent**
```csharp
public class EffectItemComponent : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    // 호버 이벤트 처리
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 효과 이름 강조 + 서브 툴팁 표시
        parentTooltip.ShowSubTooltip(currentEffectData, eventData.position);
    }
    
    public void OnPointerExit(PointerEventData eventData)
    {
        // 효과 이름 원래 색상 + 서브 툴팁 숨김
        parentTooltip.HideSubTooltip();
    }
}
```

### **2. 새로운 SubTooltipComponent**
```csharp
public class SubTooltipComponent : MonoBehaviour
{
    // 효과 타입별 상세 설명 제공
    private string GetDetailedDescription(EffectData effectData)
    {
        switch (effectData.effectType)
        {
            case EffectType.Damage:
                return $"{baseDescription}\n\n공격력에 따라 데미지가 증가합니다.";
            case EffectType.Buff:
                return $"{baseDescription}\n\n버프 효과로 전투에 유리한 상태를 제공합니다.";
            // ... 기타 효과 타입들
        }
    }
}
```

### **3. 확장된 SkillCardTooltip**
```csharp
public class SkillCardTooltip : MonoBehaviour
{
    // 서브 툴팁 관리 메서드들
    public void ShowSubTooltip(EffectData effectData, Vector2 triggerPosition)
    public void HideSubTooltip()
    private void CreateSubTooltip(EffectData effectData, Vector2 triggerPosition)
}
```

## 📋 설정 방법

### **1. Inspector 설정**

#### **SkillCardTooltip 컴포넌트**
```
[Header("서브 툴팁 설정")]
- Sub Tooltip Prefab: 서브 툴팁 프리팹 할당
- Sub Tooltip Delay: 서브 툴팁 표시 지연 시간 (기본: 0.2초)
- Sub Tooltip Hide Delay: 서브 툴팁 숨김 지연 시간 (기본: 0.1초)
```

#### **EffectItemComponent 컴포넌트**
```
[Header("호버 효과")]
- Default Name Color: 기본 효과 이름 색상 (기본: 흰색)
- Highlight Name Color: 호버 시 효과 이름 색상 (기본: 노란색)
```

#### **SubTooltipComponent 컴포넌트**
```
[Header("서브 툴팁 배경")]
- Background Image: 서브 툴팁 배경 이미지
- Border Image: 서브 툴팁 테두리 이미지

[Header("효과 상세 정보")]
- Effect Name Text: 효과 이름 텍스트
- Effect Description Text: 효과 상세 설명 텍스트

[Header("위치 설정")]
- Offset X: 메인 툴팁 오프셋 X (기본: 20)
- Offset Y: 메인 툴팁 오프셋 Y (기본: 0)
```

### **2. 프리팹 구성**

#### **메인 툴팁 프리팹**
```
SkillCardTooltip (GameObject)
├── Background (Image)
├── Border (Image)
├── CardIcon (Image)
├── CardName (TextMeshProUGUI)
├── CardType (TextMeshProUGUI)
├── Description (TextMeshProUGUI)
├── DamageIcon (Image)
├── DamageText (TextMeshProUGUI)
├── ResourceIcon (Image)
├── ResourceCostText (TextMeshProUGUI)
└── EffectsContainer (Transform)
    └── EffectItemPrefab (GameObject)
        ├── Background (Image)
        ├── Icon (Image)
        ├── NameText (TextMeshProUGUI)
        └── DescriptionText (TextMeshProUGUI)
```

#### **서브 툴팁 프리팹**
```
SubTooltipPrefab (GameObject)
├── Background (Image)
├── Border (Image)
├── EffectNameText (TextMeshProUGUI)
└── EffectDescriptionText (TextMeshProUGUI)
```

## 🎮 사용법

### **1. 기본 사용**
```csharp
// 기존과 동일한 방식으로 사용
var tooltipManager = FindObjectOfType<SkillCardTooltipManager>();
tooltipManager.ShowTooltip(card, mousePosition);
```

### **2. 계층적 툴팁 동작**
1. **카드 호버**: 메인 툴팁 표시 (카드 기본 정보 + 효과 목록)
2. **효과 호버**: 효과 이름 강조 + 서브 툴팁 표시 (상세 설명)
3. **효과 이탈**: 효과 이름 원래 색상 + 서브 툴팁 숨김
4. **카드 이탈**: 메인 툴팁 + 서브 툴팁 모두 숨김

## 🔧 커스터마이징

### **1. 효과 타입별 설명 추가**
```csharp
// SubTooltipComponent.GetDetailedDescription() 메서드 수정
case EffectType.Damage:
    return $"{baseDescription}\n\n공격력에 따라 데미지가 증가합니다.";
```

### **2. 호버 색상 변경**
```csharp
// EffectItemComponent Inspector에서 설정
[SerializeField] private Color highlightNameColor = Color.yellow;
```

### **3. 서브 툴팁 위치 조정**
```csharp
// SubTooltipComponent Inspector에서 설정
[SerializeField] private float offsetX = 20f; // 오른쪽 오프셋
[SerializeField] private float offsetY = 0f;  // 위아래 오프셋
```

## ⚡ 성능 최적화

### **1. 메모리 관리**
- ✅ **자동 정리**: 서브 툴팁은 자동으로 생성/삭제
- ✅ **코루틴 관리**: 지연 표시/숨김 코루틴 자동 정리
- ✅ **DOTween 정리**: 애니메이션 완료 시 자동 정리

### **2. 이벤트 최적화**
- ✅ **지연 표시**: 서브 툴팁은 0.2초 지연 후 표시
- ✅ **중복 방지**: 같은 효과에 호버 중일 때만 표시
- ✅ **자동 숨김**: 메인 툴팁 숨김 시 서브 툴팁도 함께 숨김

## 🐛 문제 해결

### **1. 서브 툴팁이 표시되지 않는 경우**
- `subTooltipPrefab`이 할당되었는지 확인
- `EffectItemComponent`에 `parentTooltip` 참조가 설정되었는지 확인
- `IPointerEnterHandler`, `IPointerExitHandler` 인터페이스가 구현되었는지 확인

### **2. 서브 툴팁 위치가 이상한 경우**
- `SubTooltipComponent`의 `offsetX`, `offsetY` 값 조정
- `ClampToScreenBounds` 메서드가 올바르게 작동하는지 확인

### **3. 호버 효과가 작동하지 않는 경우**
- `EffectItemComponent`의 `defaultNameColor`, `highlightNameColor` 설정 확인
- `nameText` 컴포넌트가 할당되었는지 확인

## 📝 변경 사항

### **기존 코드 확장**
- ✅ `EffectItemComponent`에 호버 이벤트 처리 추가
- ✅ `SkillCardTooltip`에 서브 툴팁 관리 기능 추가
- ✅ `SubTooltipComponent` 새로 추가
- ✅ 기존 매니저는 그대로 유지 (호환성 보장)

### **규칙 준수**
- ✅ 기존 코드 우선 원칙 준수
- ✅ 특정 게임 참조 주석 제거
- ✅ 중복 기능 생성 방지
- ✅ 프로젝트 패턴 일치

## 🎯 다음 단계

1. **프리팹 설정**: 서브 툴팁 프리팹 생성 및 할당
2. **UI 디자인**: 서브 툴팁의 시각적 디자인 완성
3. **테스트**: 다양한 카드와 효과로 테스트
4. **최적화**: 성능 및 사용성 개선

---

이제 기존 툴팁 시스템이 계층적 구조를 지원하여 사용자가 원하는 기능을 제공합니다!
