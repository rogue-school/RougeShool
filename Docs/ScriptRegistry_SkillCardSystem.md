## SkillCardSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/SkillCardSystem/`  
**목적**: 스킬카드 정의/팩토리/런타임/이펙트, 덱/핸드/슬롯 관리, 카드 UI/툴팁, 검증/실행/드랍 등을 통합 관리  
**비고**: CombatSystem, CharacterSystem, ItemSystem, StageSystem과 긴밀히 연동되는 핵심 서브시스템

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **CardInstaller** | `Game.SkillCardSystem.Installer` | `Installer/CardInstaller.cs` | 스킬카드 시스템 전용 Zenject 설치자 (카드 관련 서비스/매니저 DI 구성) | `InstallBindings()` | 카드 관련 설정 필드 | `MonoInstaller`로 카드 시스템 관련 서비스/매니저를 AsSingle로 바인딩 (CombatInstaller와 협업) | Combat 씬, CoreSystem/StageSystem과 연동 | ✅ 사용 중 |
| **SkillCardDefinition** | `Game.SkillCardSystem.Data` | `Data/SkillCardDefinition.cs` | 스킬카드 ScriptableObject 정의 (표시/연출/구성/효과) | `CreateEffects()`, `GetCurrentAttackPowerStacks()` 등 | 카드 ID, 표시 이름, 설명, artwork, configuration, presentation 등 | 에셋, DI 없음 | `SkillCardFactory`, `SkillCard`, 에디터, 덱 데이터, 툴팁/미리보기 | ✅ 사용 중 |
| **OwnerPolicy / Owner** | `Game.SkillCardSystem.Data` | `Data/SkillCardDefinition.cs`, 기타 | 카드 소유자 정책/열거형 | - | enum 값 | DI 없음 | SkillCardFactory, SkillCard, 효과 전략 | ✅ 사용 중 |
| **SkillCardFactory** | `Game.SkillCardSystem.Factory` | `Factory/SkillCardFactory.cs` | 스킬카드 생성/검증/정의 로드를 담당하는 팩토리 | `CreateFromDefinition(...)`, `CreatePlayerCard(...)`, `CreateEnemyCard(...)`, `LoadDefinition(...)` 등 | `IAudioManager` 참조 | `CombatInstaller.BindFactories`에서 `BindInterfacesAndSelfTo<SkillCardFactory>().AsSingle()`로 바인딩 (`ISkillCardFactory`) | Stage/Combat에서 카드 생성, 덱 구성/보상 로직 | ✅ 사용 중 |
| **ISkillCardFactory** | `Game.SkillCardSystem.Interface` | `Interface/ISkillCardFactory.cs` | 카드 팩토리 인터페이스 | `CreateFromDefinition(...)` 등 | - | `SkillCardFactory` 구현체 바인딩 | StageSystem, CombatSystem, Item 보상 로직 | ✅ 사용 중 |
| **SkillCard** | `Game.SkillCardSystem.Runtime` | `Runtime/SkillCard.cs` | 스킬카드 런타임 인스턴스 (효과 명령/연출/실행 관리) | `Initialize(...)`, `GetCardName()`, `ExecuteAsync(...)` 등 | `SkillCardDefinition`, `Owner`, `effectCommands`, `audioManager` | `SkillCardFactory`에서 `new SkillCard(...)`로 생성, DI 컨테이너 바인딩 없음 | CardExecutor, CardCirculationSystem, UI/툴팁 | ✅ 사용 중 |
| **ISkillCard** | `Game.SkillCardSystem.Interface` | `Interface/ISkillCard.cs` | 스킬카드 런타임 인터페이스 | `GetCardName()`, `ExecuteAsync(...)` 등 | - | 런타임 인스턴스 타입 | CardCirculationSystem, UI, 실행기 | ✅ 사용 중 |
| **ICardCirculationSystem / CardCirculationSystem** | `Game.SkillCardSystem.Interface` / `Game.SkillCardSystem.Manager` | `Interface/ICardCirculationSystem.cs`, `Manager/CardCirculationSystem.cs` | 카드 순환/보상/턴 단위 카드 드로우 관리 | `Initialize(...)`, `DrawCardsForTurn()`, `StartTurn()`, `EndTurn()`, `UseCard(...)` 등 | `playerDeck`, `currentTurnCards`, 턴 상태 플래그 | `CombatInstaller.BindFactories`에서 `Bind<ICardCirculationSystem>().To<CardCirculationSystem>().AsSingle()` 바인딩 | Combat 턴 진행/카드 사용 흐름, 보상 지급 로직 | ✅ 사용 중 |
| **PlayerSkillDeck / EnemySkillDeck** | `Game.SkillCardSystem.Deck` | `Deck/PlayerSkillDeck.cs`, `Deck/EnemySkillDeck.cs` | 플레이어/적 덱 구성 ScriptableObject | - | 카드 리스트, 메타 정보 | 에셋, DI 없음 | Player/EnemyCharacterData, Deck 에디터, CardCirculationSystem | ✅ 사용 중 |
| **PlayerDeckManager / IPlayerDeckManager** | `Game.SkillCardSystem.Manager` / `Game.SkillCardSystem.Interface` | `Manager/PlayerDeckManager.cs`, `Interface/IPlayerDeckManager.cs` | 플레이어 덱 구성/수정/질의 관리 | `AddCardToDeck(...)`, `GetDeckCards()` 등 | 현재 덱 리스트/구성 정보 | `CombatInstaller.BindFactories`에서 `BindInterfacesAndSelfTo<PlayerDeckManager>().FromNewComponentOnNewGameObject().AsSingle()` | CardCirculationSystem, Stage/보상 시스템, 덱 에디터 | ✅ 사용 중 |
| **PlayerHandManager / IPlayerHandManager** | `Game.SkillCardSystem.Manager` / `Game.SkillCardSystem.Interface` | `Manager/PlayerHandManager.cs`, `Interface/IPlayerHandManager.cs` | 플레이어 핸드(손패) 카드 관리 | `AddCardToHand(...)`, `RemoveCardFromHand(...)` 등 | 핸드 카드 컬렉션 | `CombatInstaller.BindFactories`에서 `BindInterfacesAndSelfTo<PlayerHandManager>().FromNewComponentOnNewGameObject().AsSingle()` | CombatStateMachine, UI(핸드 슬롯) | ✅ 사용 중 |
| **CardExecutor** | `Game.SkillCardSystem.Executor` | `Executor/CardExecutor.cs` | 스킬카드 실행 로직 (EffectCommand 실행/컨텍스트 구성) | `ExecuteCardAsync(...)` 등 | 실행 컨텍스트, 참조 매니저들 | `CombatInstaller`에서 AsSingle 바인딩 | CombatExecutionManager, CombatStateMachine | ✅ 사용 중 |
| **SkillCardRegistry** | `Game.SkillCardSystem.Service` | `Service/SkillCardRegistry.cs` | 카드 정의 런타임 레지스트리 (id → SO 매핑) | `BuildIndex()`, `TryGet(...)`, `Add(...)` | `definitions`, 내부 딕셔너리 | `CardInstaller` 또는 CombatInstaller에서 AsSingle 바인딩 (정의 레지스트리) | SkillCardFactory, 덱/보상 구성 로직 | ✅ 사용 중 |
| **CardExecutionContextProvider / ICardExecutionContext** | `Game.SkillCardSystem.Service` / `Game.SkillCardSystem.Interface` | `Service/CardExecutionContextProvider.cs`, `Interface/ICardExecutionContext.cs` | 카드 실행 컨텍스트 제공/인터페이스 | `CreateContext(...)` 등 | Combat/Character/Slot 정보 | AsSingle 바인딩 | CardExecutor, SkillCard | ✅ 사용 중 |
| **ICardValidator / DefaultCardExecutionValidator / CardDefinitionValidator** | `Game.SkillCardSystem.Interface` / `Game.SkillCardSystem.Validator` | `Interface/ICardValidator.cs`, `Validator/*.cs` | 카드 실행/정의 유효성 검증 | `CanExecute(...)`, `Validate(...)` 등 | 규칙 설정 | `CardInstaller` 또는 CombatInstaller에서 AsSingle 바인딩 (검증기) | 카드 사용 전 조건 검사, 에디터/런타임 검증 | ✅ 사용 중 |
| **SkillCardUI / ISkillCardUI / BaseCardSlotUI / PlayerHandCardSlotUI / CombatExecutionSlotUI** | `Game.SkillCardSystem.UI` / `Game.SkillCardSystem.Slot` | `UI/*.cs`, `Slot/BaseCardSlotUI.cs`, `UI/CombatExecutionSlotUI.cs` | 스킬카드 UI 표시/슬롯 베이스/실행 슬롯 UI | `Initialize(...)`, `SetCard(...)` 등 | 카드 참조, 이미지/텍스트, 슬롯 인덱스 등 | CombatInstaller에서 `Bind<SkillCardUI>().FromInstance(cardUIPrefab).AsSingle()` (프리팹 인스턴스) | 전투 UI, 카드 선택/실행 인터페이스 | ✅ 사용 중 |
| **SkillCardUIFactory** | `Game.SkillCardSystem.UI` | `UI/SkillCardUIFactory.cs` | 스킬카드 UI 인스턴스 생성 팩토리 | `CreateUI(...)` 등 | UI 프리팹 참조 | AsSingle 또는 유틸로 사용 | PlayerHand/Execution 슬롯 UI 생성 | ✅ 사용 중 |
| **SkillCardTooltip / SkillCardTooltipManager / Tooltip* (Model/Builder/PlacementPolicy) / Tooltip Mappers** | `Game.SkillCardSystem.UI` | `UI/*.cs`, `UI/Mappers/*.cs` | 카드 툴팁 데이터/빌더/배치/매핑/표시 전반 | `ShowTooltip(...)`, `HideTooltip()` 등 | 툴팁 텍스트/아이콘/모델 | `SkillCardTooltipManager`는 `CoreSystemInstaller`에서 매니저로 AsSingle 바인딩, 나머지는 컴포넌트/헬퍼 | 카드 UI, Character/Stage/Combat 툴팁 표시 | ✅ 사용 중 |
| **Slot/Registry (SlotRegistry, HandSlotRegistry, CombatSlotRegistry, CharacterSlotRegistry)** | `Game.SkillCardSystem.Slot` | `Slot/*.cs` | 카드 슬롯 레지스트리/앵커/포지션 관리 | `RegisterSlot(...)`, `GetSlot(...)` 등 | 슬롯 컬렉션, 포지션 enum | CombatInstaller/카드 인스톨러에서 AsSingle 또는 씬 컴포넌트로 사용 | TurnManager, CardDropService, Drag&Drop, UI | ✅ 사용 중 |
| **Slot Positions (SkillCardSlotPosition, CombatSlotPosition, CombatFieldSlotPosition)** | `Game.SkillCardSystem.Slot` | `Slot/*.cs` | 카드/전투 슬롯 위치 열거형/구조체 | - | enum/구조체 값 | DI 없음 | CardSlotRegistry, SlotMovementController, SkillCard/Tooltips | ✅ 사용 중 |
| **Drag&Drop (CardDropService, CardDropToSlotHandler, CardDropToHandHandler, CardDragHandler)** | `Game.SkillCardSystem.DragDrop` | `DragDrop/*.cs` | 카드 드래그&드랍/슬롯 등록/입력 처리 | `RegisterHandlers(...)` 등 | 드랍 핸들러 참조, 카드 UI 참조 | `CardDropService`는 CombatInstaller에서 AsSingle (`SkillCardSystem`과 공유), 나머지는 씬 컴포넌트 | CombatSystem, SkillCard UI 상호작용 | ✅ 사용 중 |
| **EffectCommandFactory / ICardEffectCommandFactory / ICardEffectCommand / ICardEffect / SkillCardEffectSO** | `Game.SkillCardSystem.Effect` / `Game.SkillCardSystem.Interface` | `Effect/EffectCommandFactory.cs`, `Interface/*.cs`, `Effect/SkillCardEffectSO.cs` | 카드 효과(명령/전략/SO) 생성/인터페이스/기본 SO | `CreateCommand(...)` 등 | 효과 SO, 명령 매핑 | `SkillCard` 내부의 static `EffectCommandFactory`로 사용 (DI 바인딩 없음) | SkillCard, 개별 효과 전략들 | ✅ 사용 중 |
| **DamageEffectCommand/Strategy/SO, HealEffect*, GuardEffect*, BleedEffect*, CounterEffect*, ResourceGainEffect*, StunEffect*, CardUseStackEffect*, AttackPowerBuffSkillEffectSO/Strategy, ReplayPreviousTurnCardEffectSO/Strategy/Command, ResourceEffectStrategy** | `Game.SkillCardSystem.Effect` | `Effect/*.cs` | 개별 카드 효과(데미지/힐/가드/출혈/카운터/자원/스턴/사용스택/공격력버프/이전턴재실행 등) 구현 | `ExecuteAsync(...)`, `CreateEffectCommand(...)` 등 | 효과별 설정 필드 | 에셋(SO) + 런타임 명령, DI 없음 | SkillCard에서 효과 명령으로 사용, Combat/Character 상태 변화 | ✅ 사용 중 |
| **StunDebuff, GuardBuff, CounterBuff** | `Game.SkillCardSystem.Effect` | `Effect/*.cs` | 지속 디버프/버프 구현 | `Apply(...)`, `Remove(...)` 등 | 타겟/지속 턴 | 런타임 객체, DI 없음 | CharacterSystem, CombatStateMachine | ✅ 사용 중 |
| **IStatusEffectMarkers, IPerTurnEffect, IAttackPowerStackProvider** | `Game.SkillCardSystem.Interface` | `Interface/*.cs` | 상태 이펙트 마커/턴당 효과/공격력 스택 인터페이스 | - | - | 인터페이스 타입 | SkillCard, CharacterEffect, Combat/Character 연동 | ✅ 사용 중 |
| **DeckEditorUI / PlayerSkillDeckEditor / EnemySkillDeckEditor** | `Game.SkillCardSystem.UI` / `Game.SkillCardSystem.Editor` | `UI/DeckEditorUI.cs`, `Editor/*.cs` | 덱 에디터 런타임/에디터 UI | `RefreshDeck()`, 커스텀 인스펙터 메서드 등 | 덱 데이터 참조 | 에디터/씬용, DI 없음 | 디자이너용 덱 관리 | ✅ 사용 중 |

> **사용 여부 메모**: SkillCardSystem 폴더의 스크립트들은 Combat/Character/Stage/Item 시스템에서 광범위하게 참조되며, DI 바인딩(CardInstaller/CombatInstaller) 및 런타임 생성(SkillCardFactory) 기준으로 **전부 실행 경로가 확인**되었습니다.  
> 순수 에셋/에디터/헬퍼/인터페이스는 코드 참조 여부, UI/드랍 핸들러는 Combat/카드 UI 프리팹/씬 구성 전제를 기준으로 `✅ 사용 중`으로 표기했습니다.

---

## 스크립트 상세 분석 (레벨 3)

### SkillCardFactory

#### 클래스 구조

```csharp
public class SkillCardFactory : ISkillCardFactory
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_registry` | `SkillCardRegistry` | `private` | `null` | 카드 정의 레지스트리 | 카드 ID → `SkillCardDefinition` 매핑 |
| `_audioManager` | `IAudioManager` | `[Inject] private` | `null` | 오디오 매니저 | 카드 실행 시 효과음 재생에 사용 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `CreateFromDefinition(SkillCardDefinition def, Owner owner)` | `ISkillCard` | `SkillCardDefinition def, Owner owner` | `public` | 1. null/owner 유효성 검사<br>2. `new SkillCard(def, owner, _audioManager, ...)` 생성<br>3. 필요 시 초기 효과 명령 세팅 | 정의 SO 기반으로 런타임 스킬카드 인스턴스를 생성 |
| `CreatePlayerCard(string cardId)` | `ISkillCard` | `string cardId` | `public` | 1. `_registry.TryGet(cardId, out def)`로 정의 조회<br>2. 실패 시 경고 로그 후 null 반환<br>3. 성공 시 `CreateFromDefinition(def, Owner.Player)` 호출 | 플레이어 덱/보상에서 사용할 카드 생성 |
| `CreateEnemyCard(string cardId)` | `ISkillCard` | `string cardId` | `public` | PlayerCard와 동일 구조, Owner.Enemy로 생성 | 적 덱/행동용 카드 생성 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `CardInstaller` / `CombatInstaller` | DI 바인딩 | `ISkillCardFactory ← SkillCardFactory` | StageSystem/CombatSystem/ItemSystem에서 공통 카드 생성기 사용 |
| `CardCirculationSystem` | DI 주입 | 턴 시작/보상 → 카드 생성 | 플레이어 턴 진입 시 필요한 만큼 카드를 생성해 핸드에 공급 |
| `RewardGenerator` (ItemSystem) | ID/정의 기반 호출 | 보상 카드 ID → SkillCardDefinition | 카드 보상 선택 시 실제 카드 인스턴스 생성 |

---

### CardCirculationSystem

#### 클래스 구조

```csharp
public class CardCirculationSystem : ICardCirculationSystem
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_playerDeckManager` | `IPlayerDeckManager` | `private` | `null` | 덱 매니저 | 현재 덱 구성/남은 카드 정보 제공 |
| `_playerHandManager` | `IPlayerHandManager` | `private` | `null` | 핸드 매니저 | 현재 손패 상태 관리 |
| `_skillCardFactory` | `ISkillCardFactory` | `private` | `null` | 카드 팩토리 | 덱/보상에서 카드 인스턴스 생성 |
| `_currentTurnCards` | `List<ISkillCard>` | `private` | 빈 | 이번 턴에 드로우된 카드들 | 턴 단위 카드 순환 관리 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Initialize(PlayerSkillDeck deck)` | `void` | `PlayerSkillDeck deck` | `public` | 1. 덱 매니저에 덱 정보 전달<br>2. 초기 패/초기 카드 구성을 셋업 | 전투 시작 시 카드 순환 시스템을 초기화 |
| `StartTurn()` | `void` | 없음 | `public` | 1. 현재 턴 번호/상태 확인<br>2. 필요 시 `DrawCardsForTurn()` 호출<br>3. 턴 시작 이벤트 발생 | 턴 진입 시 카드 드로우를 포함한 카드 순환 시작 |
| `DrawCardsForTurn()` | `void` | 없음 | `public` | 1. 덱 매니저에서 남은 카드 목록 조회<br>2. 규칙에 따라 카드 ID 선택<br>3. `SkillCardFactory`로 인스턴스 생성<br>4. `PlayerHandManager.AddCardToHand(...)` 호출 | 한 턴에 필요한 만큼 카드를 드로우하여 손패에 추가 |
| `UseCard(ISkillCard card)` | `void` | `ISkillCard card` | `public` | 1. 핸드에서 카드 제거<br>2. `CombatExecutionManager`/`CardExecutor` 쪽으로 실행 요청 | 카드 사용 후 순환 상태를 갱신 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `PlayerDeckManager` / `PlayerHandManager` | DI 주입 | 덱/손패 구성 ↔ 순환 시스템 | 카드의 저장/표시/사용 상태를 실제로 관리 |
| `CombatStateMachine` / `TurnManager` | 호출/이벤트 | 턴 시작/종료 ↔ 카드 드로우/소각 | 턴 시스템과 카드 순환 시스템을 동기화 |
| `RewardGenerator` / Stage 보상 | 카드 ID 전달 | 보상으로 덱에 추가된 카드 → 다음 턴 이후 순환 | 장기적인 덱 성장과 전투 카드 순환 연결 |

---

## 레거시/미사용 코드 정리

현재 SkillCardSystem 폴더 내에서는 CardInstaller/CombatInstaller 바인딩, SkillCardFactory/Executor 경로 및 grep 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
Editor/DeckEditor 관련 스크립트는 여전히 덱 편집/디자이너 워크플로우에 사용되고 있어 레거시로 보지 않았습니다.

---

## 폴더 구조

```text
Assets/Script/SkillCardSystem/
├── Data/
│   ├── SkillCardDefinition.cs
│   ├── SlotRole.cs
│   └── 기타 카드 관련 데이터 파일
├── Deck/
│   ├── PlayerSkillDeck.cs
│   └── EnemySkillDeck.cs
├── DragDrop/
│   ├── CardDragHandler.cs
│   ├── CardDropService.cs
│   ├── CardDropToHandHandler.cs
│   └── CardDropToSlotHandler.cs
├── Effect/
│   ├── AttackPowerBuffSkillCommand.cs
│   ├── AttackPowerBuffSkillEffectSO.cs
│   ├── AttackPowerBuffSkillEffectStrategy.cs
│   ├── BleedEffect*.cs
│   ├── CardUseStack*.cs
│   ├── CounterEffect*.cs
│   ├── DamageEffect*.cs
│   ├── GuardEffect*.cs
│   ├── HealEffect*.cs
│   ├── OwnTurnEffectBase.cs
│   ├── ReplayPreviousTurnCardCommand.cs
│   ├── ReplayPreviousTurnCardEffectSO.cs
│   ├── ReplayPreviousTurnCardEffectStrategy.cs
│   ├── ResourceEffectStrategy.cs
│   ├── ResourceGainEffect*.cs
│   ├── SkillCardEffectSO.cs
│   ├── StunEffect*.cs
│   └── 관련 Strategy/Command/Buff/Debuff 스크립트들
├── Executor/
│   └── CardExecutor.cs
├── Factory/
│   ├── CardEffectCommandFactory.cs
│   ├── SkillCardEntry.cs
│   └── SkillCardFactory.cs
├── Installer/
│   └── CardInstaller.cs
├── Interface/
│   ├── IAttackPowerStackProvider.cs
│   ├── ICardCirculationSystem.cs
│   ├── ICardEffect.cs
│   ├── ICardEffectCommand.cs
│   ├── ICardEffectCommandFactory.cs
│   ├── ICardExecutionContext.cs
│   ├── ICardValidator.cs
│   ├── ICombatCardSlot.cs
│   ├── IHandCardSlot.cs
│   ├── IPerTurnEffect.cs
│   ├── IPlayerDeckManager.cs
│   ├── IPlayerHandManager.cs
│   ├── ISkillCard.cs
│   ├── ISkillCardFactory.cs
│   ├── ISkillCardUI.cs
│   └── IStatusEffectMarkers.cs
├── Manager/
│   ├── BaseSkillCardManager.cs
│   ├── CardCirculationSystem.cs
│   ├── PlayerDeckManager.cs
│   ├── PlayerHandManager.cs
│   └── SkillCardTooltipManager.cs
├── Runtime/
│   ├── EnemySkillCardRuntime.cs
│   └── SkillCard.cs
├── Service/
│   ├── CardExecutionContextProvider.cs
│   └── SkillCardRegistry.cs
├── Slot/
│   ├── BaseCardSlotUI.cs
│   ├── CombatFieldSlotPosition.cs
│   ├── CombatSlotPosition.cs
│   ├── CombatSlotRegistry.cs
│   ├── HandSlotRegistry.cs
│   ├── SlotAnchor.cs
│   ├── SlotRegistry.cs
│   ├── SkillCardSlotPosition.cs
│   └── CharacterSlotRegistry.cs
├── UI/
│   ├── CombatExecutionSlotUI.cs
│   ├── DeckEditorUI.cs
│   ├── PlayerHandCardSlotUI.cs
│   ├── SkillCardTooltip.cs
│   ├── SkillCardUI.cs
│   ├── SkillCardUIFactory.cs
│   ├── TooltipBuilder.cs
│   ├── TooltipModel.cs
│   ├── TooltipPlacementPolicy.cs
│   └── Mappers/
│       ├── EffectTooltipMapper.cs
│       ├── PerTurnEffectTooltipMapper.cs
│       └── SkillCardTooltipMapper.cs
├── Validator/
│   ├── CardDefinitionValidator.cs
│   └── DefaultCardExecutionValidator.cs
└── Editor/
    ├── EnemySkillDeckEditor.cs
    ├── PlayerSkillDeckEditor.cs
    └── SkillCardDefinitionEditor.cs
```


