## CharacterSystem 스크립트 레지스트리

**루트 폴더**: `Assets/Script/CharacterSystem/`  
**목적**: 플레이어/적 캐릭터 데이터, 코어 로직, 매니저, UI, 초기화, 슬롯/이펙트 등 캐릭터 관련 전반을 관리  
**비고**: 전투/스테이지/아이템/스킬카드 시스템의 핵심 의존 대상 (ICharacter, 캐릭터 데이터, 매니저 등)

---

## 스크립트 목록

| 스크립트 이름 | 네임스페이스 | 상대 경로 | 역할 | 주요 공개 메서드(대표) | 주요 필드/프로퍼티(대표) | Zenject 바인딩(있으면) | 주요 참조자(사용처) | 상태 |
|--------------|--------------|-----------|------|------------------------|---------------------------|------------------------|----------------------|------|
| **CharacterBase** | `Game.CharacterSystem.Core` | `Core/CharacterBase.cs` | 플레이어/적 공통 캐릭터 베이스 클래스 | `SetMaxHP(...)`, `TakeDamage(...)` 등 | HP, 방어력, 상태 플래그 | 직접 DI 바인딩 없음 (상속 기반) | `PlayerCharacter`, `EnemyCharacter` 등 모든 캐릭터 구현 | ✅ 사용 중 |
| **ICharacter** | `Game.CharacterSystem.Interface` | `Interface/ICharacter.cs` | 캐릭터 공통 인터페이스 | `SetCharacterData(...)`, `GetCharacterName()` 등 | `CharacterData` 프로퍼티 | Combat/Item/SkillCard 시스템에서 인터페이스로 주입 | `StageManager`, `ItemService`, `CombatStatsAggregator` 등 | ✅ 사용 중 |
| **ICharacterData** | `Game.CharacterSystem.Interface` | `Interface/ICharacterData.cs` | 캐릭터 데이터 공통 인터페이스 | - | 공용 데이터 프로퍼티 | ScriptableObject 데이터 타입의 공통 인터페이스 | `PlayerCharacterData`, `EnemyCharacterData`, UI/툴팁 로직 | ✅ 사용 중 |
| **PlayerCharacterData** | `Game.CharacterSystem.Data` | `Data/PlayerCharacterData.cs` | 플레이어 캐릭터 ScriptableObject 정의 | - | HP, 자원, 덱 정보 등 | 에셋으로만 사용, DI 바인딩 없음 | `PlayerCharacter`, `MainMenuController`(선택 UI), Stage/Combat 진입 | ✅ 사용 중 |
| **EnemyCharacterData** | `Game.CharacterSystem.Data` | `Data/EnemyCharacterData.cs` | 적 캐릭터 ScriptableObject 정의 | `HasPhases` | HP, 덱, 등장 연출, `Phases` 리스트 등 | 에셋으로만 사용, DI 바인딩 없음 | `EnemyCharacter`, `StageManager`, `AudioManager`(적별 BGM) | ✅ 사용 중 |
| **EnemyPhaseData** | `Game.CharacterSystem.Data` | `Data/EnemyPhaseData.cs` | 적 캐릭터 페이즈 정보 데이터 | `IsThresholdReached(...)`, `IsValid()` | `phaseName`, `healthThreshold`, `phaseMaxHP`, `phaseDeck`, `phaseEffects`, `phaseDisplayName`, `phaseIndexIcon`, `phasePortraitPrefab`, `phaseTransitionVFX`, `phaseTransitionSFX` | 데이터 전용, DI 없음 | `EnemyCharacterData`, `EnemyCharacter`(페이즈 전환 로직) | ✅ 사용 중 |
| **PlayerCharacterType** | `Game.CharacterSystem.Data` | `Data/PlayerCharacterType.cs` | 플레이어 캐릭터 타입 enum 정의 | - | enum 값 | DI 바인딩 없음 | `PlayerCharacterTypeHelper`, 선택 UI, 통계/저장 로직 | ✅ 사용 중 |
| **PlayerCharacterTypeHelper** | `Game.CharacterSystem.Data` | `Data/PlayerCharacterTypeHelper.cs` | 캐릭터 타입 ↔ 데이터 매핑 헬퍼 | `GetCharacterData(...)` 등 | 타입/데이터 매핑 테이블 | 정적/헬퍼, DI 바인딩 없음 | 캐릭터 선택/로드 시 타입 기반 데이터 선택 | ✅ 사용 중 |
| **CharacterEffectEntry** | `Game.CharacterSystem.Data` | `Data/CharacterEffectEntry.cs` | 캐릭터 적용 효과 항목 데이터 | - | 효과 SO, 스택 수 등 | 데이터 전용 | `CharacterEffectSO`, 버프/디버프 관리 | ✅ 사용 중 |
| **CharacterEffectSO** | `Game.CharacterSystem.Effect` | `Effect/CharacterEffectSO.cs` | 캐릭터에 적용되는 이펙트 ScriptableObject 베이스 | `Apply(...)` 등 | 효과 타입, 지속 턴 등 | 에셋, DI 없음 | 버프/디버프 시스템, `ICharacterEffect` 구현체 | ✅ 사용 중 |
| **SummonEffectSO** | `Game.CharacterSystem.Effect` | `Effect/SummonEffectSO.cs` | 적 소환/교체 관련 이펙트 정의 SO | `Execute(...)` 등 | 소환 대상 데이터, 연출 정보 | 에셋, DI 없음 | `SummonEffect`, `SummonState`, `StageManager` | ✅ 사용 중 |
| **SummonEffect** | `Game.CharacterSystem.Effect` | `Effect/SummonEffect.cs` | 소환 이펙트 런타임 로직 | `Execute(...)` | SummonEffectSO 참조 | DI 바인딩 없음, 카드/이펙트 실행 경로에서 사용 | CombatStateMachine, SkillCardSystem 이펙트 실행 | ✅ 사용 중 |
| **EnemyCharacter** | `Game.CharacterSystem.Core` | `Core/EnemyCharacter.cs` | 적 캐릭터 구현 (HP, 덱, UI, 사망/소환/페이즈 전환 처리 등) | `Initialize(...)`, `GetCharacterName()`, `IsPlayerControlled()`, `CurrentPhaseIndex`, `CurrentPhaseName`, `ShouldTransitionPhase()`, `IsPhaseTransitionPending()` 등 | `EnemyCharacterData`, HPBar/UI 참조, 효과 리스트, `currentPhaseIndex`, `isPhaseTransitionPending`, 페이즈 캐시 필드 | Combat/Stage에서 ICharacter로 취급, DI 직접 바인딩 없음 (프리팹/씬 배치) | `StageManager`(적 생성/소환), `HPBarController`, Combat/VFX 시스템, `SlotMovementController`(페이즈 전환 후 덱 캐시 업데이트 및 슬롯 재채우기), `CombatStateMachine`(페이즈 전환 안전 상태 확인) | ✅ 사용 중 |
| **PlayerCharacter** | `Game.CharacterSystem.Core` | `Core/PlayerCharacter.cs` | 플레이어 캐릭터 구현 (HP, UI, 카드 핸들링) | `SetCharacterData(...)`, `SetCharacterData(object)`, `InitializeCharacter(...)` 등 | `PlayerCharacterData`, HPBar, `PlayerCharacterUIController` | Combat/Stage에서 ICharacter로 취급, 프리팹/씬 배치 기반 | `PlayerManager`, `HPBarController`, `PlayerCharacterUIController` | ✅ 사용 중 |
| **LobbyCharacterSelector** | `Game.CharacterSystem.Core` | `Core/LobbyCharacterSelector.cs` | 로비에서 캐릭터 선택/프리뷰를 담당하는 컴포넌트 | `SelectCharacter(...)` 등 | 선택 UI 참조, 캐릭터 데이터 | 씬에 부착, DI 없음 | 로비/메인 메뉴 캐릭터 선택 화면 | ✅ 사용 중 (씬 부착 전제) |
| **PlayerCharacterSelector** | `Game.CharacterSystem.Core` | `Core/PlayerCharacterSelector.cs` | 플레이어 캐릭터 선택 로직(런타임) | `SelectCharacter(...)` 등 | 선택된 데이터, UI 참조 | 씬 컴포넌트, DI 없음 | Stage/Combat 진입 전에 캐릭터 선택 | ✅ 사용 중 (씬 부착 전제) |
| **BaseCharacterManager** | `Game.CharacterSystem.Manager` | `Manager/BaseCharacterManager.cs` | 캐릭터 매니저 공통 베이스 클래스 | `GetCharacter()`, `RegisterCharacter(...)` 등 | 현재 캐릭터 참조, 이벤트 | 상속 기반, DI 없음 | `PlayerManager`, `EnemyManager` | ✅ 사용 중 |
| **PlayerManager** | `Game.CharacterSystem.Manager` | `Manager/PlayerManager.cs` | 플레이어 캐릭터 등록/제거/이벤트 관리 | `RegisterCharacter(...)`, `GetPlayer()` 등 | 현재 플레이어 캐릭터 참조, 이벤트 | `StageManager`에 `[Inject(Optional = true)]`, `ItemService` 등에 DI/Find로 사용 | StageSystem, ItemSystem, Combat 통계/슬롯 로직 | ✅ 사용 중 |
| **EnemyManager** | `Game.CharacterSystem.Manager` | `Manager/EnemyManager.cs` | 적 캐릭터 등록/제거/슬롯 관리 | `RegisterCharacter(...)`, `GetEnemy()`, `UnregisterCharacter()` 등 | 현재 적 캐릭터, 슬롯 참조 | `CombatInstaller`에서 `BindInterfacesAndSelfTo<EnemyManager>().AsSingle()` 바인딩 | StageManager, TurnController, CombatStateMachine | ✅ 사용 중 |
| **BuffDebuffTooltipManager** | `Game.CharacterSystem.Manager` | `Manager/BuffDebuffTooltipManager.cs` | 버프/디버프 툴팁 전역 관리 | `ShowTooltip(...)`, `HideTooltip()` 등 | 현재 표시 중인 툴팁, UI 프리팹 | `CoreSystemInstaller`에서 매니저 배열에 포함, AsSingle 바인딩 | `BuffDebuffTooltip`, 캐릭터 UI | ✅ 사용 중 |
| **PlayerResourceManager** | `Game.CharacterSystem.Manager` | `Manager/PlayerResourceManager.cs` | 플레이어 자원(마나 등) 관리 | `Consume(...)`, `Restore(...)` 등 | 현재 자원 값, 최대값 | Combat/SkillCard 쪽에서 ICharacter/이벤트 기반으로 사용 (코드 참조 있음) | Turn/Combat/카드 사용 로직 | ✅ 사용 중 |
| **EnemySpawnerManager** | `Game.CharacterSystem.Manager` | `Manager/EnemySpawnerManager.cs` | 적 스폰 전담 매니저 (StageManager/EnemyManager와 협업) | `SpawnEnemyAsync(...)` 등 | 스폰 포인트, 프리팹 참조 | 씬 컴포넌트, DI 없음 | StageSystem과 EnemyManager 연계 | ✅ 사용 중 (씬 부착 전제) |
| **PlayerCharacterUIController** | `Game.CharacterSystem.UI` | `UI/PlayerCharacterUIController.cs` | 플레이어 캐릭터 UI 통합 컨트롤러 (HP/자원/버프/디버프 아이콘 관리) | `Initialize(PlayerCharacter)`, `UpdateHPBar()`, `SubscribeCharacterEvents()`, `UnsubscribeCharacterEvents()` 등 | HP/자원/아이콘 UI 참조, `playerCharacter`, `isSubscribed` 플래그 | 씬/프리팹 컴포넌트, DI 없음 | `PlayerCharacter`에서 초기화 호출, `OnHPChanged`/`OnBuffsChanged` 이벤트 구독 | ✅ 사용 중 |
| **EnemyCharacterUIController** | `Game.CharacterSystem.UI` | `UI/EnemyCharacterUIController.cs` | 적 캐릭터 UI 컨트롤러 | `Initialize(EnemyCharacter)` 등 | HP/이름/아이콘 UI 참조 | 씬/프리팹 컴포넌트, DI 없음 | `EnemyCharacter`, Stage/Combat UI | ✅ 사용 중 |
| **HPBarController** | `Game.CharacterSystem.UI` | `UI/HPBarController.cs` | HP 바 UI 컨트롤러 (플레이어/적 공용) | `Initialize(PlayerCharacter)`, `Initialize(EnemyCharacter)` 등 | HP 슬라이더/텍스트 참조 | 씬 컴포넌트, DI 없음 | `PlayerCharacter`, `EnemyCharacter`에서 Initialize 호출 | ✅ 사용 중 |
| **EffectNotificationPanel** | `Game.CharacterSystem.UI` | `UI/EffectNotificationPanel.cs` | 버프/디버프/이펙트 알림 패널 | `ShowNotification(...)` 등 | UI 텍스트/아이콘 | 씬 컴포넌트, DI 없음 | 스킬/효과 발동 시 알림 표시 | ✅ 사용 중 |
| **BuffDebuffTooltip** | `Game.CharacterSystem.UI` | `UI/BuffDebuffTooltip.cs` | 버프/디버프 툴팁 UI | `Show(...)`, `Hide()` 등 | 설명 텍스트, 아이콘 | `BuffDebuffTooltipManager`에 의해 제어 | 캐릭터 UI에서 버프/디버프 툴팁 표시 | ✅ 사용 중 |
| **BuffDebuffSlotView** | `Game.CharacterSystem.UI` | `UI/BuffDebuffSlotView.cs` | 버프/디버프 슬롯 하나의 UI 뷰 | `SetData(...)` 등 | 아이콘, 카운트 텍스트 | 씬/프리팹 컴포넌트, DI 없음 | Player/Enemy UI의 버프/디버프 슬롯 표시 | ✅ 사용 중 |
| **BuffDebuffIcon** | `Game.CharacterSystem.UI` | `UI/BuffDebuffIcon.cs` | 버프/디버프 아이콘 표시/상태 관리 | - | 아이콘 이미지, 카운트 | 씬/프리팹 컴포넌트 | BuffDebuffSlotView, Tooltip | ✅ 사용 중 |
| **CharacterSlotUI** | `Game.CharacterSystem.UI` | `UI/CharacterSlotUI.cs` | 전투 화면에서 캐릭터를 배치하는 슬롯 UI | `SetCharacter(...)` 등 | 슬롯 위치, 소유자, 캐릭터 참조 | 씬/프리팹 컴포넌트, DI 없음 | Stage/Combat에서 캐릭터 슬롯 배치 | ✅ 사용 중 |
| **ICharacterEffect** | `Game.CharacterSystem.Interface` | `Interface/ICharacterEffect.cs` | 캐릭터에 적용되는 런타임 효과 인터페이스 | `Apply(...)`, `Remove(...)` 등 | - | DI 없음, 런타임 객체 인터페이스 | `EnemyCharacter`, `PlayerCharacter`, 효과 시스템 | ✅ 사용 중 |
| **ICharacterSlot** | `Game.CharacterSystem.Interface` | `Interface/ICharacterSlot.cs` | 캐릭터 슬롯 인터페이스 | `SetCharacter(...)` 등 | - | Slot/Stage 연동 | `CharacterSlotPosition`, Stage/전투 배치 로직 | ✅ 사용 중 |
| **CharacterSlotPosition** | `Game.CharacterSystem.Slot` | `Slot/CharacterSlotPosition.cs` | 캐릭터 슬롯 위치/타입 정의 | enum/구조체 | 슬롯 인덱스/타입 | DI 없음 | Stage/Combat에서 캐릭터 배치 | ✅ 사용 중 |
| **EnemyInitializer** | `Game.CharacterSystem.Initialization` | `Initialization/EnemyInitializer.cs` | 전투/스테이지에서 적 캐릭터 초기화 컴포넌트 | `Initialize(...)` 등 | EnemyCharacter 참조 | `CombatInstaller`에서 `BindInterfacesAndSelfTo<EnemyCharacterInitializer>()`로 바인딩 | CombatStateMachine, StageManager 연동 | ✅ 사용 중 |
| **EnemyCharacterInitializer** | `Game.CharacterSystem.Initialization` | `Initialization/EnemyCharacterInitializer.cs` | 적 캐릭터 초기화 전담 컴포넌트 | `InitializeEnemy(...)` 등 | EnemyCharacter 참조 | `CombatInstaller`에서 InterfacesAndSelfTo 바인딩 | Combat 진입 시 적 초기화 | ✅ 사용 중 |
| **PlayerSkillCardInitializer** | `Game.CharacterSystem.Initialization` | `Initialization/PlayerSkillCardInitializer.cs` | 플레이어 스킬카드 초기화 컴포넌트 | `Initialize(...)` 등 | PlayerCharacter, 덱 참조 | Combat/Stage 씬에서 컴포넌트로 사용 | SkillCardSystem과 연동 | ✅ 사용 중 |
| **HandInitializer** | `Game.CharacterSystem.Initialization` | `Initialization/HandInitializer.cs` | 시작 핸드(카드/캐릭터) 초기화 | `Initialize(...)` 등 | 핸드/슬롯 참조 | Combat 씬 컴포넌트 | Turn 시작 시 초기 패 구성 | ✅ 사용 중 |
| **CharacterDeathHandler** | `Game.CharacterSystem.Utility` | `Utility/CharacterDeathHandler.cs` | 캐릭터 사망 처리 유틸리티 | `HandleDeath(...)` 등 | 효과/연출 참조 | 씬 컴포넌트 또는 유틸 호출 | Enemy/Player 사망 처리 일관화 | ✅ 사용 중 |
| **CardRegistrar** | `Game.CharacterSystem.Utility` | `Utility/CardRegistrar.cs` | 캐릭터와 카드 슬롯/덱 연결을 도와주는 유틸리티 | `RegisterCards(...)` 등 | 캐릭터/슬롯 참조 | Combat/SkillCard 쪽에서 호출 | 카드-캐릭터 연결 관리 | ✅ 사용 중 |
| **EnemySpawnResult** | `Game.CharacterSystem.Utility` | `Utility/EnemySpawnResult.cs` | 적 스폰 결과 데이터 구조 | - | 스폰된 캐릭터, 위치 등 | 데이터 전용 | `EnemySpawnerManager`, Stage/Combat 로직 | ✅ 사용 중 |
| **CardValidator** | `Game.CharacterSystem.Utility` | `Utility/CardValidator.cs` | 캐릭터/상태 기반 카드 사용 가능 여부 검증 | `CanPlayCard(...)` 등 | 검증 규칙 | Combat/SkillCard에서 호출 | 카드 유효성 검사 | ✅ 사용 중 |

> **사용 여부 메모**: 위 테이블의 상태가 `✅ 사용 중`인 항목들은 grep/DI 바인딩/씬 컴포넌트 관점에서 **실제 실행 경로가 확인된 스크립트**입니다.  
> 코드 참조는 없지만 씬에만 부착된 컴포넌트로 보이는 항목은, 별도 표시 없이 씬 부착 전제로 간주했습니다.

---

## 스크립트 상세 분석 (레벨 3)

### EnemyCharacter

#### 클래스 구조

```csharp
CharacterBase
  └── EnemyCharacter : ICharacter
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_data` | `EnemyCharacterData` | `private` (SerializeField) | `null` | 적 데이터 SO | 체력/공격/덱/연출/페이즈 정보를 가진 ScriptableObject 참조 |
| `_hpBarController` | `HPBarController` | `private` (SerializeField) | `null` | HP UI 컨트롤러 | 적 HP를 UI에 표시/갱신하는 컴포넌트 |
| `_uiController` | `EnemyCharacterUIController` | `private` (SerializeField) | `null` | 적 UI 컨트롤러 | 이름/아이콘/버프 슬롯 등을 제어 |
| `_currentHP` | `int` | `private` | `0` | 현재 체력 | 피해/회복 시 갱신되는 실제 HP 값 |
| `_isDead` | `bool` | `private` | `false` | 사망 여부 | 중복 사망 처리 방지 및 후속 로직 제어 |
| `_activeEffects` | `List<ICharacterEffect>` | `private` | 빈 리스트 | 적용된 효과 목록 | 버프/디버프 등 런타임 효과 관리 |
| `currentPhaseIndex` | `int` | `private` | `-1` | 현재 페이즈 인덱스 | -1 = 기본 정보(1페이즈), 0 이상 = Phases 리스트 인덱스(2페이즈, 3페이즈, ...) |
| `isPhaseTransitionPending` | `bool` | `private` | `false` | 페이즈 전환 대기 중 여부 | 중복 전환 방지 플래그 |
| `cachedPhaseDisplayName` / `cachedPhaseIndexIcon` / `cachedPhasePortraitPrefab` | `string` / `Sprite` / `GameObject` | `private` | `null` | 페이즈별 기본 정보 캐시 | 페이즈별 DisplayName, IndexIcon, PortraitPrefab 캐시 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `Initialize(EnemyCharacterData data)` | `void` | `EnemyCharacterData data` | `public` | 1. `data` null 검사<br>2. `_data` 저장 및 기본 HP/방어 초기화<br>3. 페이즈 시스템 초기화 (`InitializePhases()`)<br>4. UI/HPBar 초기화<br>5. GameLogger로 초기화 로그 출력 | StageManager/EnemySpawner가 적을 생성할 때 호출되는 초기화 진입점 |
| `TakeDamage(int amount)` | `void` | `int amount` | `public override` | 1. 이미 사망 시 조기 리턴<br>2. `amount`를 방어/효과 고려해 조정<br>3. `_currentHP` 감소 후 0 이하일 경우 `Die()` 호출<br>4. CombatEvents/HPBar/UI 갱신<br>5. `NotifyHealthChanged()` 호출로 페이즈 전환 체크 트리거 | 전투 중 피해를 받을 때 공통 처리 |
| `ApplyEffect(ICharacterEffect effect)` | `void` | `ICharacterEffect effect` | `public` | 1. null 검사<br>2. `_activeEffects`에 추가<br>3. `effect.Apply(this)` 호출 | 버프/디버프/지속 효과를 적에게 적용 |
| `Die()` | `void` | 없음 | `private` | 1. `_isDead` 플래그 설정<br>2. EnemyManager/StageManager에 적 사망 알림<br>3. VFX/Audio 트리거<br>4. 오브젝트 비활성/파괴 | 적 사망 시 실제 정리/알림 로직 |
| `CheckPhaseTransition(int currentHP, int maxHP)` | `void` | `int currentHP, int maxHP` | `private` | 1. 페이즈 시스템 유효성 검사<br>2. 현재 페이즈 인덱스 기준으로 다음 페이즈 체크<br>3. 체력 임계값 도달 시 `StartPhaseTransition()` 호출 | 체력 변경 시 페이즈 전환 조건 확인 |
| `StartPhaseTransition(int phaseIndex)` | `void` | `int phaseIndex` | `private` | 1. 중복 전환 방지 플래그 설정<br>2. `TransitionToPhaseCoroutine()` 코루틴 시작 | 페이즈 전환 시작 |
| `TransitionToPhaseCoroutine(int phaseIndex)` | `IEnumerator` | `int phaseIndex` | `private` | 1. `CombatStateMachine.WaitForSafeStateForPhaseTransition()` 호출로 안전한 상태 대기<br>2. 슬롯 이동 완료 대기<br>3. 페이즈 전환 연출 재생 (VFX/SFX)<br>4. 페이즈 설정 적용 (버프/디버프 제거, 체력 회복, 덱 교체, Portrait 프리팹 교체)<br>5. `SlotMovementController.RefillAllCombatSlotsWithEnemyDeckCoroutine()` 호출로 슬롯 재채우기<br>6. 페이즈 전환 완료 후 적 턴으로 자동 전환 (필요 시) | 페이즈 전환 전체 프로세스 처리 |
| `ApplyPhaseSettings(EnemyPhaseData phase, bool isTransition, bool skipCardRegeneration)` | `void` | `EnemyPhaseData phase, bool isTransition, bool skipCardRegeneration` | `private` | 1. 페이즈별 기본 정보 캐시<br>2. 전환 시 버프/디버프 제거<br>3. 최대 체력 변경 및 회복<br>4. 덱 교체<br>5. 페이즈 이펙트 적용<br>6. Portrait 프리팹 교체<br>7. UI 갱신 | 페이즈별 설정 적용 |
| `ClearEnemyCardsAndRegenerateCoroutine()` | `IEnumerator` | 없음 | `private` | 1. 적 카드가 있는 슬롯 찾기<br>2. 적 카드 제거<br>3. `SlotMovementController`의 적 덱 캐시 업데이트 (`UpdateEnemyCache`)<br>4. `SlotMovementController.RefillAllCombatSlotsWithEnemyDeckCoroutine()` 호출로 슬롯 재채우기 | 페이즈 전환 후 슬롯 카드 정리 및 재생성 |
| `ShouldTransitionPhase()` | `bool` | 없음 | `public` | 1. 페이즈 전환 대기 중이거나 사망 시 false 반환<br>2. 현재 체력과 최대 체력을 기준으로 다음 페이즈 임계값 확인<br>3. 임계값 도달 시 true 반환 | 페이즈 전환이 필요한지 확인 |
| `IsPhaseTransitionPending()` | `bool` | 없음 | `public` | `isPhaseTransitionPending` 플래그 반환 | 페이즈 전환이 현재 진행 중인지 확인 |

#### 로직 흐름도 (피해/사망/페이즈 전환)

```text
TakeDamage(amount)
  ↓
[이미 사망 여부 체크]
  ↓
피해량 보정(버프/디버프/방어)
  ↓
_currentHP 감소
  ↓
HPBar/UI 갱신
  ↓
_currentHP <= 0 ?
  ├─ 예 → Die() 호출
  └─ 아니오 → NotifyHealthChanged() 호출
              ↓
              [페이즈 시스템 체크]
              ↓
              CheckPhaseTransitionDelayed() 코루틴 시작
              ↓
              CheckPhaseTransition() 호출
              ↓
              [임계값 도달?]
              ├─ 예 → StartPhaseTransition() → TransitionToPhaseCoroutine()
              │         ↓
              │         [슬롯 이동 완료 대기]
              │         ↓
              │         [페이즈 전환 연출 (VFX/SFX)]
              │         ↓
              │         [페이즈 설정 적용: 버프/디버프 제거, 체력 회복, 덱 교체, Portrait 교체]
              │         ↓
              │         [슬롯 카드 제거 및 SlotMovementController 캐시 업데이트]
              │         ↓
              │         [다음 슬롯 이동 시 자동 보충 로직이 빈 슬롯 채움]
              └─ 아니오 → 종료
```

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `StageManager` | 생성/초기화 시 참조 | 스테이지 데이터 → EnemyCharacterData | 현재 스테이지의 적 데이터를 기반으로 적 인스턴스를 초기화 |
| `EnemyManager` | 등록/해제 메서드 호출 | EnemyCharacter → 전역 적 레지스트리 | 현재 전투에서 활성화된 적 컬렉션 관리 |
| `HPBarController` / `EnemyCharacterUIController` | SerializeField / Initialize | 캐릭터 상태 → UI | HP/이름/버프 등을 화면에 반영 |
| `CombatEvents` | 정적 이벤트 호출 | 피해/사망 → 이벤트 브로드캐스트 | VFX/Audio/통계 시스템이 적 피해/사망에 반응 |
| `SlotMovementController` | DI 주입 (`ISlotMovementController`) | 페이즈 전환 → 적 덱 캐시 업데이트 (`UpdateEnemyCache`) → 슬롯 재채우기 (`RefillAllCombatSlotsWithEnemyDeckCoroutine`) | 페이즈 전환 후 새로운 페이즈의 덱을 사용하도록 캐시 업데이트 및 슬롯 재채우기 |
| `CombatStateMachine` | DI 주입 (Optional) | 페이즈 전환 → 안전한 상태 확인 (`WaitForSafeStateForPhaseTransition`) | 페이즈 전환이 안전한 상태에서만 진행되도록 보장 |
| `VFXManager` / `AudioManager` | DI 주입 (Optional) | 페이즈 전환 연출 → VFX/SFX 재생 | 페이즈 전환 시 시각/청각 피드백 제공 |

---

### PlayerCharacter

#### 클래스 구조

```csharp
CharacterBase
  └── PlayerCharacter : ICharacter
```

#### 변수 상세 (대표)

| 변수 이름 | 타입 | 접근성 | 초기값 | 용도 | 설명 |
|----------|------|--------|--------|------|------|
| `_data` | `PlayerCharacterData` | `private` (SerializeField) | `null` | 플레이어 데이터 SO | HP/자원/덱/초기 스킬 구성을 가진 ScriptableObject |
| `_uiController` | `PlayerCharacterUIController` | `private` (SerializeField) | `null` | 플레이어 UI 컨트롤러 | HP/자원/아이콘 등 플레이어 UI 통합 제어 |
| `_hpBarController` | `HPBarController` | `private` (SerializeField) | `null` | HP UI 컨트롤러 | EnemyCharacter와 공유하는 HP 바 로직 |
| `_currentResource` | `int` | `private` | `0` | 현재 자원(마나 등) | 카드 사용/아이템 사용 시 소비되는 자원 |
| `_playerManager` | `PlayerManager` | `private` | `null` | 플레이어 매니저 | 등록/조회/이벤트 연동용 참조 |

#### 함수 상세 (대표)

| 함수 이름 | 반환 타입 | 매개변수 | 접근성 | 로직 흐름 | 설명 |
|----------|----------|---------|--------|----------|------|
| `SetCharacterData(PlayerCharacterData data)` | `void` | `PlayerCharacterData data` | `public` | 1. null 검사<br>2. `_data` 저장<br>3. HP/자원 등 초기화<br>4. UI/HPBar 초기 세팅 | 메인 메뉴/스테이지에서 선택된 캐릭터 데이터를 주입 |
| `InitializeCharacter()` | `void` | 없음 | `public` | 1. PlayerManager에 자신 등록<br>2. UIController/HPBar와 연결<br>3. 초기 핸드/덱 초기화 트리거 | 전투 진입 시 플레이어 객체 초기 준비 |
| `ConsumeResource(int amount)` | `bool` | `int amount` | `public` | 1. 현재 자원 검사<br>2. 부족하면 false 반환 및 경고 로그<br>3. 충분하면 자원 감소 후 true 반환 | 카드/아이템 사용 전 자원 체크 및 소비 |
| `TakeDamage(int amount)` | `void` | `int amount` | `public override` | EnemyCharacter와 유사하게 HP 감소/사망 처리, 대신 게임오버/스테이지 실패 트리거와 연동 | 플레이어 피해 처리 |

#### 사용/연결 관계

| 연결 대상 | 연결 방식 | 데이터 흐름 | 설명 |
|----------|----------|------------|------|
| `PlayerManager` | 등록/조회 | PlayerCharacter → 전역 플레이어 참조 | 다른 시스템이 현재 플레이어를 안전하게 조회 |
| `StageManager` | 캐릭터 선택/스폰 | PlayerCharacterData → PlayerCharacter | 스테이지 시작 시 선택된 캐릭터를 전장에 배치 |
| `PlayerCharacterUIController` / `HPBarController` | SerializeField / Initialize | 상태 → UI | HP/자원/버프 등을 HUD에 표시 |
| `CardCirculationSystem` / `ItemService` | 런타임 참조 | 캐릭터 상태 → 카드/아이템 사용 조건 | 자원/상태에 따라 카드/아이템 사용 가능 여부를 결정 |

---

## 레거시/미사용 코드 정리

현재 CharacterSystem 폴더 내에서는 grep/Installer/씬 컴포넌트 전제를 기준으로 **레거시/완전 미사용으로 분류된 스크립트가 없습니다.**  
모든 스크립트가 전투/스테이지/카드/아이템과의 실행 경로나 씬/프리팹 구성에 연결되어 있습니다.

---

## 폴더 구조

```text
Assets/Script/CharacterSystem/
├── Core/
│   ├── CharacterBase.cs
│   ├── EnemyCharacter.cs
│   ├── LobbyCharacterSelector.cs
│   ├── PlayerCharacter.cs
│   └── PlayerCharacterSelector.cs
├── Data/
│   ├── CharacterEffectEntry.cs
│   ├── EnemyCharacterData.cs
│   ├── EnemyPhaseData.cs
│   ├── PlayerCharacterData.cs
│   ├── PlayerCharacterType.cs
│   └── PlayerCharacterTypeHelper.cs
├── Effect/
│   ├── CharacterEffectSO.cs
│   ├── SummonEffect.cs
│   └── SummonEffectSO.cs
├── Initialization/
│   ├── EnemyCharacterInitializer.cs
│   ├── EnemyInitializer.cs
│   ├── HandInitializer.cs
│   └── PlayerSkillCardInitializer.cs
├── Interface/
│   ├── ICharacter.cs
│   ├── ICharacterData.cs
│   ├── ICharacterEffect.cs
│   └── ICharacterSlot.cs
├── Manager/
│   ├── BaseCharacterManager.cs
│   ├── BuffDebuffTooltipManager.cs
│   ├── EnemyManager.cs
│   ├── EnemySpawnerManager.cs
│   ├── PlayerManager.cs
│   └── PlayerResourceManager.cs
├── Slot/
│   └── CharacterSlotPosition.cs
├── UI/
│   ├── BuffDebuffIcon.cs
│   ├── BuffDebuffSlotView.cs
│   ├── BuffDebuffTooltip.cs
│   ├── EffectNotificationPanel.cs
│   ├── EnemyCharacterUIController.cs
│   ├── HPBarController.cs
│   ├── PlayerCharacterUIController.cs
│   └── CharacterSlotUI.cs
└── Utility/
    ├── CardRegistrar.cs
    ├── CardValidator.cs
    ├── CharacterDeathHandler.cs
    └── EnemySpawnResult.cs
```


