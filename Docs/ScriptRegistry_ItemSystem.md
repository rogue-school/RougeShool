## ItemSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/ItemSystem/`  
**목적**: 액티브/패시브 아이템 정의, 이펙트, 보상/드랍, 런타임 사용 로직, 인벤토리/UI를 관리하는 아이템 전용 시스템  
**비고**: CharacterSystem(플레이어), CombatSystem(턴/전투), SkillCardSystem(시너지), StageSystem(보상), VFX/Audio와 연동

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **ItemDefinition / ItemType** | `Game.ItemSystem.Data` | `Data/ItemDefinition.cs` | 아이템 공통 ScriptableObject 베이스 및 타입 enum | `IsValid()` | `ItemId`, `DisplayName`, `Description`, `Icon`, `Type` | 에셋, DI 없음 | `ActiveItemDefinition`, `PassiveItemDefinition`, 에디터/보상/인벤토리 | ✅ 사용 중 |
| **ActiveItemDefinition / PassiveItemDefinition** | `Game.ItemSystem.Data` | `Data/ActiveItemDefinition.cs`, `Data/PassiveItemDefinition.cs` | 액티브/패시브 아이템 개별 정의 | - | 사용 조건, 이펙트 설정 등 | 에셋, DI 없음 | ItemService, ActiveItem, Reward/Drop 시스템, UI | ✅ 사용 중 |
| **ItemConstants** | `Game.ItemSystem.Constants` | `Constants/ItemConstants.cs` | 아이템 관련 상수 정의 (슬롯 수 등) | - | `ACTIVE_SLOT_COUNT` 등 | 정적/유틸 | ItemService, UI/슬롯 로직 | ✅ 사용 중 |
| **ItemService** | `Game.ItemSystem.Service` | `Service/ItemService.cs` | 아이템 시스템 핵심 서비스 (액티브/패시브 사용/관리) | `UseActiveItem(...)`, `AddActiveItem(...)`, `AddPassiveItem(...)`, `GetActiveSlots()` 등 | `activeSlots`, `skillStarRanks`, `passiveItemDefinitions` | `CoreSystemInstaller.BindCoreServices()`에서 `IItemService → ItemService`를 `FromNewComponentOnNewGameObject().AsSingle().NonLazy()`로 바인딩 (CombatInstaller에서도 안전망 바인딩) | InventoryPanelController, RewardPanelController, ActiveItemUI, Stage/보상 시스템 | ✅ 사용 중 |
| **IItemService** | `Game.ItemSystem.Interface` | `Interface/IItemService.cs` | 아이템 서비스 인터페이스 | `UseActiveItem(...)`, `AddActiveItem(...)`, `GetActiveSlots()` 등 | - | `ItemService` 구현체로 DI | Combat/UI/보상 시스템에서 DI로 사용 | ✅ 사용 중 |
| **ActiveItem / IActiveItem** | `Game.ItemSystem.Runtime` / `Game.ItemSystem.Interface` | `Runtime/ActiveItem.cs`, `Interface/IActiveItem.cs` | 단일 액티브 아이템 런타임 객체 (실제 효과 실행) | `UseItem(...)` | 정의 참조, Audio/VFX 참조 | `ItemService.UseActiveItem`에서 `new ActiveItem(...)` 생성, DI 바인딩 없음 | ItemEffectCommands, VFX/Audio, CharacterSystem(대상 캐릭터) | ✅ 사용 중 |
| **ItemEffectSO / ItemEffectBase / ItemEffectCommands / BaseItemEffectCommand** | `Game.ItemSystem.Effect` | `Effect/ItemEffectSO.cs`, `Effect/ItemEffectBase.cs`, `Effect/ItemEffectCommands.cs`, `Effect/BaseItemEffectCommand.cs` | 아이템 이펙트 SO/베이스/개별 명령 정의 | `ApplyEffect(...)`, `Execute(...)` 등 | 효과 파라미터 | 에셋 + 런타임 명령, DI 없음 | ActiveItem, ItemService, 특수 아이템 이펙트들 | ✅ 사용 중 |
| **개별 아이템 이펙트 SO들 (HealEffectSO, AttackBuffEffectSO, TimeStopEffectSO, ReviveEffectSO, ShieldBreakerEffectSO, DiceOfFateEffectSO, ClownPotionEffectSO, ShieldBreakerDebuffEffect, AttackPowerBuffEffect 등)** | `Game.ItemSystem.Effect` | `Effect/*.cs` | 각 아이템별 고유 효과 정의 | `Apply(...)`, `OnUse(...)` 등 | 효과 값/타겟 설정 | 에셋, DI 없음 | ActiveItem, ItemService, 전투/캐릭터 상태 변경 | ✅ 사용 중 |
| **IItemEffect / IItemPerTurnEffect** | `Game.ItemSystem.Interface` | `Interface/IItemEffect.cs`, `Interface/IItemPerTurnEffect.cs` | 아이템 효과 인터페이스 (실행/턴당 효과) | `Apply(...)`, `OnTurnStart()` 등 | - | 인터페이스 타입 | ItemEffectBase, Runtime 이펙트 구현 | ✅ 사용 중 |
| **ItemEffectCommandFactory** | `Game.ItemSystem.Runtime` | `Runtime/ItemEffectCommandFactory.cs` | 아이템 이펙트 명령 생성 팩토리 | `CreateCommand(...)` 등 | 이펙트 SO/설정 | 런타임 헬퍼, DI 없음 | ActiveItem, ItemService | ✅ 사용 중 |
| **DefaultItemUseContext** | `Game.ItemSystem.Runtime` | `Runtime/DefaultItemUseContext.cs` | 아이템 사용 컨텍스트 구현 | - | 대상 캐릭터/상태 | 런타임 헬퍼, DI 없음 | ActiveItem, 이펙트 로직 | ✅ 사용 중 |
| **RewardInstaller** | `Game.ItemSystem.Service.Reward` | `Service/Reward/RewardInstaller.cs` | 보상/리워드 시스템 Zenject 설치자 | `InstallBindings()` | - | `Bind<IRewardGenerator>().To<RewardGenerator>().AsSingle()` 등 바인딩 | StageSystem, Combat/보상 UI, ItemService | ✅ 사용 중 |
| **RewardGenerator / IRewardGenerator** | `Game.ItemSystem.Service.Reward` | `Service/Reward/RewardGenerator.cs`, `Service/Reward/IRewardGenerator.cs` | 보상 풀에서 아이템/카드/자원 보상 생성 | `GenerateRewards(...)` 등 | RewardPool/Config 참조 | `CoreSystemInstaller.BindCoreServices` 및 `CombatInstaller.BindItemSystemServices`에서 AsSingle 바인딩 | StageManager(전투 보상), RewardPanelController | ✅ 사용 중 |
| **DefaultRewardService** | `Game.ItemSystem.Service` | `Service/DefaultRewardService.cs` | 기본 보상 서비스 (RewardGenerator와 ItemService 조합) | `GiveRewards(...)` 등 | 보상 설정 | AsSingle 또는 RewardInstaller에서 바인딩 | StageSystem, Combat 결과 처리 | ✅ 사용 중 |
| **RewardPool / RewardProfile / PlayerRewardProfile / EnemyRewardConfig** | `Game.ItemSystem.Data.Reward` | `Data/Reward/*.cs` | 보상 풀/프로필/적 보상 설정 데이터 | - | 보상 항목 리스트/확률 | 에셋, DI 없음 | RewardGenerator, RewardPanelController, Editor | ✅ 사용 중 |
| **InventoryPanelController** | `Game.ItemSystem.Runtime` | `Runtime/InventoryPanelController.cs` | 액티브 인벤토리 4슬롯 UI 제어, ItemService와 상호작용 | `RefreshSlots()`, `OnPointerClick(...)` 등 | 슬롯 Transform, ActiveItem 프리팹, 패널 상태 | `[Inject] IItemService` 주입 (Zenject 컨테이너) | Combat/UI 씬 인벤토리 패널 | ✅ 사용 중 |
| **RewardPanelController** | `Game.ItemSystem.Runtime` | `Runtime/RewardPanelController.cs` | 전투/스테이지 보상 선택 UI 및 지급 처리 | `ShowRewards(...)`, `OnSelectReward(...)` 등 | 보상 슬롯, 버튼, ItemService/RewardGenerator 참조 | `[Inject] IItemService`, `IRewardGenerator` 등 주입 | StageManager, Combat 결과 시 보상 표시 | ✅ 사용 중 |
| **RewardSlotUIController** | `Game.ItemSystem.Runtime` | `Runtime/RewardSlotUIController.cs` | 개별 보상 슬롯 UI 제어 | `SetReward(...)` 등 | 아이콘, 텍스트, 버튼 | 씬/프리팹 컴포넌트, DI 없음 | RewardPanelController | ✅ 사용 중 |
| **RewardOnEnemyDeath** | `Game.ItemSystem.Runtime` | `Runtime/RewardOnEnemyDeath.cs` | 적 처치 시 자동으로 보상 트리거 | `OnEnemyDeath(...)` 등 | Reward 설정 | 씬 컴포넌트, DI 없음 | CombatStateMachine/EnemyManager 이벤트와 연동 | ✅ 사용 중 |
| **PassiveItemIcon** | `Game.ItemSystem.UI` | `UI/PassiveItemIcon.cs` | 패시브 아이템 아이콘 UI 표시 | `SetItem(...)` 등 | 아이콘 이미지, 카운트 | 씬/프리팹 컴포넌트 | 인게임 HUD, RewardPanel 등 | ✅ 사용 중 |
| **ItemTooltip / ItemTooltipManager / ItemTooltip 관련 Utility** | `Game.ItemSystem.UI` / `Game.ItemSystem.Manager` / `Game.ItemSystem.Utility` | `UI/ItemTooltip.cs`, `Manager/ItemTooltipManager.cs`, `Utility/EffectConfigHelper.cs`, `Utility/ItemEffectValidator.cs`, `Utility/UIUpdateHelper.cs` | 아이템 툴팁 표시/검증/헬퍼 | `ShowTooltip(...)` 등 | 툴팁 텍스트, 효과 설명 | `ItemTooltipManager`는 `CoreSystemInstaller`에서 매니저 배열에 포함되어 AsSingle 바인딩, 나머지는 헬퍼/컴포넌트 | Inventory/Reward UI, 디자이너/플레이어 툴팁 | ✅ 사용 중 |
| **ItemResourceCache** | `Game.ItemSystem.Cache` | `Cache/ItemResourceCache.cs` | 아이템 관련 리소스(아이콘/프리팹) 캐시 | `GetIcon(...)` 등 | 캐시 딕셔너리 | 정적/싱글톤 유틸 또는 컴포넌트 (코드 참조 있음) | ItemService, UI | ✅ 사용 중 |
| **IVFXManager** | `Game.ItemSystem.Interface` | `Interface/IVFXManager.cs` | 아이템 시스템이 사용하는 VFX 매니저 인터페이스 (VFXSystem과의 느슨한 연결) | `PlayEffect(...)`, `ShowDamageText(...)` 등 (VFXManager와 대응) | - | 인터페이스 타입 (구현은 `Game.VFXSystem.Manager.VFXManager`) | `ItemService`(필드 주입), `ActiveItem`(생성자 인자)에서 사용되어, 아이템 효과 실행 시 VFX 연동을 추상화 | ✅ 사용 중 |
| **ActionPopupUI / ActiveItemUI / TestItemButton / ActiveItemUI 관련 런타임** | `Game.ItemSystem.Runtime` | `Runtime/*.cs` | 아이템 액션 팝업, 액티브 아이템 슬롯 UI, 테스트 버튼 등 | `ShowPopup(...)`, `SetItem(...)` 등 | 버튼/텍스트/아이콘 | 씬/프리팹 컴포넌트, DI 없음 | InventoryPanelController, 디버그/테스트 | ✅ 사용 중 |
| **Reward/Editor (RewardPoolEditor, RewardValidationEditor, PassiveItemDefinitionEditor)** | `Game.ItemSystem.Editor` | `Editor/*.cs` | 보상/아이템 에디터 툴 | 커스텀 인스펙터 메서드 | 에셋 참조 | 에디터 전용, DI 없음 | 디자이너 워크플로우 | ✅ 사용 중 |

> **사용 여부 메모**: ItemSystem 폴더 내 스크립트는 Combat/Stage/Character/Core/SkillCard/VFX/Audio와 서로 연결된 상태이며, Zenject 바인딩(CoreSystemInstaller/CombatInstaller/RewardInstaller), 이벤트, UI 참조 기준으로 **실제 실행 경로가 확인**되었습니다.  
> 순수 에셋/에디터/헬퍼는 코드 참조 여부, UI/컴포넌트는 씬/프리팹 전제를 기준으로 `✅ 사용 중`으로 표시했고, 현재 단계에서 “완전 미사용”으로 보이는 스크립트는 없습니다.

---

## 스크립트 상세 분석 (레벨 3)

### ItemService

#### 클래스 구조

```csharp
MonoBehaviour
  └── ItemService : IItemService
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_activeSlots` | `ActiveItem[]` 또는 리스트 | `private` | 빈 | 액티브 아이템 슬롯 | 전투 중 사용할 수 있는 액티브 아이템 컬렉션 |
| `_passiveItems` | `List<PassiveItemDefinition>` | `private` | 빈 | 획득한 패시브 아이템 목록 | 전투/스테이지 전반에 적용되는 지속 효과 목록 |
| `_skillStarRanks` | `Dictionary<string, int>` | `private` | 빈 | 스킬별 강화/별 등급 | 아이템 효과로 강화된 스킬 레벨 관리 |
| `_playerManager` | `PlayerManager` | `private` (`[Inject(Optional = true)]`) | `null` | 플레이어 참조 | 액티브/패시브 효과를 적용할 대상 캐릭터 조회 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `UseActiveItem(int slotIndex, IGameContext context)` | `bool` | `int slotIndex, IGameContext context` | `public` | 1. 슬롯 인덱스/아이템 유효성 검사<br>2. 쿨타임/재사용 조건 확인<br>3. `ActiveItem.UseItem(...)` 호출<br>4. 성공 시 OnActiveItemUsed 이벤트 발생 | 인게임에서 액티브 아이템을 실제로 사용하는 진입점 |
| `AddActiveItem(ActiveItemDefinition def)` | `bool` | `ActiveItemDefinition def` | `public` | 1. 빈 슬롯 탐색<br>2. 해당 정의로 `ActiveItem` 생성<br>3. 슬롯에 배치 및 UI 갱신<br>4. 성공 여부 반환 | 보상/상점 등에서 액티브 아이템을 지급할 때 사용 |
| `AddPassiveItem(PassiveItemDefinition def)` | `void` | `PassiveItemDefinition def` | `public` | 1. 리스트에 추가<br>2. 즉시 적용형 패시브라면 Player/Combat 시스템에 효과 적용<br>3. 로그 출력 | 패시브 아이템 획득 처리 |
| `GetActiveSlots()` | `IReadOnlyList<ActiveItem>` | 없음 | `public` | 슬롯 배열/리스트를 읽기 전용으로 반환 | UI/툴팁/세이브에서 현재 액티브 슬롯 상태를 조회 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `InventoryPanelController` | `[Inject] IItemService` | 슬롯 클릭 → `UseActiveItem` | 인게임 UI에서 액티브 아이템 사용을 트리거 |
| `RewardPanelController` | `[Inject] IItemService` | 보상 선택 → `AddActiveItem`/`AddPassiveItem` | 전투/스테이지 보상으로 아이템 지급 |
| `RewardOnEnemyDeath` | 필드/DI 참조 | 적 사망 → 보상 생성 → ItemService | Kill 보상과 아이템 시스템 연결 |
| `VFXManager` / `AudioManager` | 내부 참조 또는 이펙트 명령을 통한 호출 | 아이템 사용 → VFX/SFX 재생 | 아이템 사용 연출과 연결 |

---

### RewardGenerator

#### 클래스 구조

```csharp
public class RewardGenerator : IRewardGenerator
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_rewardPools` | `List<RewardPool>` | `private` (SerializeField) | 에셋 참조 | 보상 풀 리스트 | 스테이지/적/난이도별 보상 구성을 담은 데이터 |
| `_random` | `System.Random` 또는 Unity RNG | `private` | 새 인스턴스 | 난수 발생기 | 보상 선택 시 확률 계산에 사용 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `GenerateRewards(RewardContext context)` | `IReadOnlyList<RewardDefinition>` | `RewardContext context` | `public` | 1. 컨텍스트(스테이지/난이도/플레이어 상태)에 맞는 `RewardPool` 선택<br>2. 확률/중복 규칙에 따라 보상 후보 추출<br>3. UI에서 표시할 보상 리스트 반환 | 전투/스테이지 종료 시 보여줄 보상 후보 생성 핵심 함수 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `RewardInstaller` / `CoreSystemInstaller` | DI 바인딩 | `IRewardGenerator ← RewardGenerator` | StageSystem/ItemSystem/Combat에서 공통으로 사용하는 보상 생성기 |
| `RewardPanelController` | `[Inject] IRewardGenerator` | 스테이지/전투 결과 → GenerateRewards | 보상 UI에 표시할 리스트를 제공 |

---

## 레거시/미사용 코드 정리

현재 ItemSystem 폴더 내에서는 CoreSystem/Combat/Stage/SkillCard와의 연결 및 Zenject 바인딩/grep 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
테스트/에디터용 스크립트(`TestItemButton`, Editor 폴더 등)는 여전히 디자이너/디버깅 워크플로우에서 사용 가능하므로 레거시로 분류하지 않았습니다.

---

## 폴더 구조

```text
Assets/Script/ItemSystem/
├── Cache/
│   └── ItemResourceCache.cs
├── Constants/
│   └── ItemConstants.cs
├── Data/
│   ├── ItemDefinition.cs
│   ├── ActiveItemDefinition.cs
│   ├── PassiveItemDefinition.cs
│   └── Reward/
│       ├── EnemyRewardConfig.cs
│       ├── PlayerRewardProfile.cs
│       ├── RewardPool.cs
│       └── RewardProfile.cs
├── Effect/
│   ├── AttackBuffEffectSO.cs
│   ├── AttackPowerBuffEffect.cs
│   ├── BaseItemEffectCommand.cs
│   ├── ClownPotionEffectSO.cs
│   ├── DiceOfFateEffectSO.cs
│   ├── HealEffectSO.cs
│   ├── ItemEffectBase.cs
│   ├── ItemEffectCommands.cs
│   ├── ItemEffectSO.cs
│   ├── RerollEffectSO.cs
│   ├── ReviveEffectSO.cs
│   ├── ShieldBreakerDebuffEffect.cs
│   ├── ShieldBreakerEffectSO.cs
│   ├── TimeStopEffectSO.cs
│   └── ResourceGainEffectSO.cs
├── Editor/
│   ├── PassiveItemDefinitionEditor.cs
│   ├── RewardPoolEditor.cs
│   └── RewardValidationEditor.cs
├── Interface/
│   ├── IActiveItem.cs
│   ├── IItemEffect.cs
│   ├── IItemPerTurnEffect.cs
│   ├── IItemService.cs
│   └── IVFXManager.cs
├── Manager/
│   └── ItemTooltipManager.cs
├── Runtime/
│   ├── ActionPopupUI.cs
│   ├── ActiveItem.cs
│   ├── ActiveItemUI.cs
│   ├── DefaultItemUseContext.cs
│   ├── InventoryPanelController.cs
│   ├── ItemEffectCommandFactory.cs
│   ├── RewardOnEnemyDeath.cs
│   ├── RewardPanelController.cs
│   ├── RewardSlotUIController.cs
│   └── TestItemButton.cs
├── Service/
│   ├── DefaultRewardService.cs
│   └── ItemService.cs
├── Service/Reward/
│   ├── IRewardGenerator.cs
│   ├── RewardGenerator.cs
│   └── RewardInstaller.cs
├── UI/
│   ├── ItemTooltip.cs
│   └── PassiveItemIcon.cs
└── Utility/
    ├── EffectConfigHelper.cs
    ├── ItemEffectValidator.cs
    └── UIUpdateHelper.cs
```


