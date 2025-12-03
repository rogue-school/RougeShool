## VFXSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/VFXSystem/`  
**목적**: 전투/캐릭터/카드/아이템 등에서 사용하는 비주얼 이펙트(VFX)와 데미지 텍스트/버프 아이콘/카드 UI를 풀링·재사용하며, 카메라/레이어 설정을 포함한 시각 효과 전반을 관리  
**비고**: CombatSystem, SkillCardSystem, ItemSystem, CharacterSystem, UISystem과 연동되는 공용 VFX 계층

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **VFXManager** | `Game.VFXSystem.Manager` | `Manager/VFXManager.cs` | VFX 및 데미지 텍스트/버프 아이콘/스킬카드 UI 풀을 관리하는 메인 매니저 | `ShowDamageText(...)`, `PlayEffect(...)`, `PlayEffectAtCharacterCenter(...)` 등 | `damageTextPool`, `buffIconPool`, `skillCardUIPool`, `defaultEffectDuration` | `CoreSystemInstaller` 또는 `CombatInstaller.BindFactories`에서 `VFXManager`를 씬에서 찾거나 자동 생성 후 AsSingle로 바인딩 (코드 상 DI 사용 지점 존재) | EnemyCharacter/PlayerCharacter(피격/버프 연출), SkillCard 효과, Item 이펙트, Combat/Victory UI | ✅ 사용 중 |
| **VFXAnchorPoint** | `Game.VFXSystem.Component` | `Component/VFXAnchorPoint.cs` | 캐릭터/오브젝트에 붙는 VFX 기준점 컴포넌트 | - | 기준 위치 Transform | 씬/프리팹 컴포넌트, DI 없음 | VFXManager에서 이 anchor를 기준으로 이펙트 배치 | ✅ 사용 중 |
| **GenericUIPool** | `Game.VFXSystem.Pool` | `Pool/GenericUIPool.cs` | UI용 GameObject 풀링 베이스 클래스 | `Get(...)`, `Return(...)` 등 | 풀 리스트/프리팹 참조 | 씬 컴포넌트, DI 없음 | DamageTextPool, SkillCardUIPool, BuffIconPool의 베이스 | ✅ 사용 중 |
| **DamageTextPool** | `Game.VFXSystem.Pool` | `Pool/DamageTextPool.cs` | 데미지 텍스트 UI 전용 풀 | `Get(position, parent)`, `Return(obj)` 등 | 프리팹, 최대 개수, 풀 리스트 | `VFXManager.damageTextPool`로 SerializeField 연결 | VFXManager.ShowDamageText, Combat/Character 피격 연출 | ✅ 사용 중 |
| **BuffIconPool** | `Game.VFXSystem.Pool` | `Pool/BuffIconPool.cs` | 버프 아이콘 UI 전용 풀 | `Get(...)`, `Return(...)` 등 | 프리팹/풀 리스트 | `VFXManager.buffIconPool`로 SerializeField 연결 | CharacterSystem UI(버프 아이콘 재사용), 버프/디버프 연출 | ✅ 사용 중 |
| **SkillCardUIPool** | `Game.VFXSystem.Pool` | `Pool/SkillCardUIPool.cs` | 카드 UI(연출용) 전용 풀 | `Get(...)`, `Return(...)` 등 | 카드 UI 프리팹/풀 리스트 | `VFXManager.skillCardUIPool`로 SerializeField 연결 | SkillCardSystem 연출, 카드 사용 이펙트 | ✅ 사용 중 |
| **EffectDuration** | `Game.VFXSystem.Component` | `Component/EffectDuration.cs` | 개별 이펙트 오브젝트의 지속시간/자동 파괴를 관리하는 컴포넌트 | - | 지속 시간 값 | 프리팹 컴포넌트, DI 없음 | VFXManager의 `GetEffectDuration(...)`/`DestroyEffectAfterDelay(...)` 로직과 연동 | ✅ 사용 중 |

> **사용 여부 메모**: VFXSystem의 스크립트는 VFXManager와 그 풀/컴포넌트들로 매우 응집되어 있으며, 스테이지/전투/캐릭터/아이템/카드 연출 경로에서 실제 사용되고 있습니다.  
> 풀/컴포넌트 스크립트는 씬/프리팹 구성 전제를 바탕으로 `✅ 사용 중`으로 표기했습니다.

---

## 스크립트 상세 분석 (레벨 3)

### VFXManager

#### 클래스 구조

```csharp
MonoBehaviour
  └── VFXManager
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `damageTextPool` | `DamageTextPool` | `private` (SerializeField) | `null` | 데미지 텍스트 풀 | 전투 중 떠오르는 데미지 숫자 UI 풀링 |
| `buffIconPool` | `BuffIconPool` | `private` (SerializeField) | `null` | 버프 아이콘 풀 | 캐릭터 머리 위/HP바 근처 버프 아이콘 재사용 |
| `skillCardUIPool` | `SkillCardUIPool` | `private` (SerializeField) | `null` | 카드 UI 풀 | 연출용 카드 UI 인스턴스 풀링 |
| `_defaultEffectDuration` | `float` | `private` (SerializeField) | 프로젝트 기본값 | 이펙트 기본 지속 시간 | 이펙트 프리팹에 별도 설정이 없을 때 사용할 시간 |
| `_camera` | `Camera` | `private` | `null` | VFX 카메라 | 특정 레이어(예: Effects)만 렌더링하는 카메라 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `ShowDamageText(int amount, Vector3 worldPosition, bool isCritical)` | `void` | `int amount, Vector3 worldPosition, bool isCritical` | `public` | 1. `damageTextPool.Get(...)`으로 UI 인스턴스 가져오기<br>2. 월드 좌표를 스크린/UI 좌표로 변환<br>3. 크리티컬 여부에 따라 색/크기 조정<br>4. 자동 반환/숨김 처리 예약 | 캐릭터 피격 시 데미지 숫자 연출 |
| `PlayEffect(GameObject effectPrefab, Vector3 position)` | `void` | `GameObject effectPrefab, Vector3 position` | `public` | 1. null 검사<br>2. `Instantiate`로 이펙트 생성<br>3. `EffectDuration` 컴포넌트 또는 `_defaultEffectDuration`으로 파괴 예약 | 단발성 VFX를 지정 위치에 재생 |
| `PlayEffectAtCharacterCenter(VFXAnchorPoint anchor, GameObject effectPrefab)` | `void` | `VFXAnchorPoint anchor, GameObject effectPrefab` | `public` | 1. anchor/null 검사<br>2. anchor 위치를 기준으로 `PlayEffect` 호출 | 캐릭터 중앙/머리 위 등 지정된 앵커에서 이펙트 재생 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `EnemyCharacter` / `PlayerCharacter` | 필드/DI 참조 | 피해/버프/디버프 → VFX 호출 | 캐릭터 이벤트에 따라 데미지 텍스트/버프 아이콘 등 재생 |
| `SkillCardSystem` 효과 명령 | 직접 참조 또는 이벤트 | 카드 효과 → 이펙트 프리팹 재생 | 카드 사용 시 공격/버프 이펙트 재생 |
| `ItemService` | 효과 실행 경로 | 아이템 사용 → VFX 호출 | 아이템 연출과 시각 효과 연결 |
| `VFXAnchorPoint` / `EffectDuration` | 컴포넌트 조합 | 위치/지속 시간 관리 | 이펙트 위치와 생명주기를 안전하게 관리 |

---

## 레거시/미사용 코드 정리

현재 VFXSystem 폴더 내에서는 VFXManager/풀/컴포넌트 간 결합 구조 및 Combat/SkillCard/Item/Character 연계 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
특히 `GenericUIPool`은 DamageText/SkillCardUI/BuffIcon 풀의 베이스로 적극 사용 중이며, 별도의 죽은 VFX 스크립트는 보이지 않습니다.

---

## 폴더 구조

```text
Assets/Script/VFXSystem/
├── Component/
│   ├── EffectDuration.cs
│   └── VFXAnchorPoint.cs
├── Manager/
│   └── VFXManager.cs
└── Pool/
    ├── BuffIconPool.cs
    ├── DamageTextPool.cs
    ├── GenericUIPool.cs
    └── SkillCardUIPool.cs
```


