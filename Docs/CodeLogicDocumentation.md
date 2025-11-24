# RougeShool 코드 로직 문서

> 작성일: 2025-11-24  
> 목적: 프로젝트의 주요 함수, 변수, 코드 로직을 설명하는 참조 문서

---

## 📋 목차

1. [시스템 아키텍처 개요](#시스템-아키텍처-개요)
2. [핵심 인터페이스](#핵심-인터페이스)
3. [주요 클래스 및 메서드](#주요-클래스-및-메서드)
4. [데이터 구조 (ScriptableObject)](#데이터-구조-scriptableobject)
5. [코드 플로우](#코드-플로우)
6. [에셋 활용 가이드](#에셋-활용-가이드)

---

## 🏗️ 시스템 아키텍처 개요

### 전체 시스템 구조

```
RougeShool 프로젝트
├── CoreSystem          # 핵심 시스템 (오디오, 저장, 씬 전환)
├── CombatSystem        # 전투 시스템 (턴, 슬롯, 실행)
├── CharacterSystem     # 캐릭터 시스템 (플레이어, 적)
├── SkillCardSystem     # 스킬 카드 시스템 (카드, 덱, 효과)
├── ItemSystem          # 아이템 시스템 (액티브, 패시브, 보상)
├── StageSystem         # 스테이지 시스템 (스테이지 진행)
├── SaveSystem          # 저장 시스템 (자동 저장, 진행 상황)
├── UISystem            # UI 시스템 (메뉴, 설정)
├── VFXSystem           # VFX 시스템 (이펙트, 풀링)
└── UtilitySystem       # 유틸리티 시스템 (게임 플로우)
```

### 의존성 주입 (Zenject DI)

모든 시스템은 Zenject DI를 통해 의존성을 주입받습니다.

```csharp
// ✅ 올바른 패턴
[Inject] private IPlayerManager playerManager;
[Inject] private IAudioManager audioManager;

// ❌ 금지된 패턴
private PlayerManager playerManager = FindObjectOfType<PlayerManager>();
```

---

## 🔌 핵심 인터페이스

### CoreSystem 인터페이스

#### `IAudioManager`
오디오 재생을 담당하는 인터페이스

**주요 메서드**:
- `void PlayBGM(AudioClip clip, bool fadeIn = false)`: BGM 재생
- `void PlaySFX(AudioClip clip)`: SFX 재생
- `void PlaySFXWithPool(AudioClip clip, float volume, int poolSize)`: 풀링을 사용한 SFX 재생
- `void SetBGMVolume(float volume)`: BGM 볼륨 설정
- `void SetSFXVolume(float volume)`: SFX 볼륨 설정

**사용 위치**: 모든 시스템에서 사운드 재생 시 사용

---

#### `ISaveManager`
게임 저장/로드를 담당하는 인터페이스

**주요 메서드**:
- `Task SaveCurrentScene()`: 현재 씬 상태 저장
- `Task LoadGame()`: 게임 로드
- `void SaveAudioSettings(float bgmVolume, float sfxVolume)`: 오디오 설정 저장
- `(float bgm, float sfx) LoadAudioSettings(float defaultBGM, float defaultSFX)`: 오디오 설정 로드

**사용 위치**: `SaveManager.cs`, `SettingsManager.cs`

---

#### `ISceneTransitionManager`
씬 전환을 담당하는 인터페이스

**주요 메서드**:
- `Task TransitionToBattleScene()`: 전투 씬으로 전환
- `Task TransitionToMainScene()`: 메인 씬으로 전환
- `bool IsTransitioning { get; }`: 전환 중 여부

**사용 위치**: `MainMenuController.cs`, `Newgame.cs`

---

### CharacterSystem 인터페이스

#### `ICharacter`
캐릭터의 공통 인터페이스 (플레이어/적 공통)

**주요 메서드**:
- `string GetCharacterName()`: 캐릭터 이름 반환
- `int GetHP()`: 현재 체력 반환
- `int GetMaxHP()`: 최대 체력 반환
- `void TakeDamage(int damage)`: 데미지 받기
- `void Heal(int amount)`: 체력 회복
- `IReadOnlyList<IPerTurnEffect> GetBuffs()`: 버프/디버프 목록 반환

**이벤트**:
- `event Action<int, int> OnHPChanged`: 체력 변경 이벤트
- `event Action<bool> OnGuardStateChanged`: 가드 상태 변경 이벤트
- `event Action<IReadOnlyList<IPerTurnEffect>> OnBuffsChanged`: 버프 목록 변경 이벤트

**구현 클래스**: `PlayerCharacter.cs`, `EnemyCharacter.cs`

---

#### `IPlayerCharacter`
플레이어 캐릭터 전용 인터페이스

**주요 메서드**:
- `void Initialize(PlayerCharacterData data)`: 캐릭터 초기화
- `PlayerCharacterData PlayerCharacterData { get; }`: 플레이어 데이터

**구현 클래스**: `PlayerCharacter.cs`

---

#### `IEnemyCharacter`
적 캐릭터 전용 인터페이스

**주요 메서드**:
- `void Initialize(EnemyCharacterData data)`: 적 캐릭터 초기화
- `EnemyCharacterData EnemyCharacterData { get; }`: 적 데이터

**구현 클래스**: `EnemyCharacter.cs`

---

### CombatSystem 인터페이스

#### `ITurnManager`
턴 관리를 담당하는 인터페이스

**주요 메서드**:
- `TurnType GetCurrentTurnType()`: 현재 턴 타입 반환 (Player/Enemy)
- `void StartSetupPhase()`: 셋업 페이즈 시작
- `void CompleteSetup()`: 셋업 완료
- `void CompleteTurn()`: 턴 완료

**구현 클래스**: `TurnManager.cs`

---

#### `ICombatExecutionManager`
전투 실행을 담당하는 인터페이스

**주요 메서드**:
- `void ExecuteCardInBattleSlot()`: 전투 슬롯의 카드 실행
- `void MoveSlotsForwardNew()`: 슬롯 전진 (새 시스템)
- `void ExecuteImmediately(ISkillCard card, CombatSlotPosition position)`: 즉시 실행 (레거시)

**구현 클래스**: `CombatExecutionManager.cs`

---

#### `ICardSlotRegistry`
카드 슬롯 레지스트리 인터페이스

**주요 메서드**:
- `void RegisterSlot(CombatSlotPosition position, ICombatCardSlot slot)`: 슬롯 등록
- `ICombatCardSlot GetSlot(CombatSlotPosition position)`: 슬롯 조회
- `bool IsSlotEmpty(CombatSlotPosition position)`: 슬롯 비어있음 여부

**구현 클래스**: `CardSlotRegistry.cs`

---

### SkillCardSystem 인터페이스

#### `ISkillCard`
스킬 카드 인터페이스

**주요 메서드**:
- `string GetCardId()`: 카드 ID 반환
- `string GetDisplayName()`: 카드 표시 이름 반환
- `Owner GetOwner()`: 카드 소유자 반환 (Player/Enemy)
- `bool Execute(ICardExecutionContext context)`: 카드 실행
- `bool CanExecute(ICardExecutionContext context)`: 실행 가능 여부

**구현 클래스**: `SkillCard.cs`, `EnemySkillCardRuntime.cs`

---

#### `ISkillCardFactory`
스킬 카드 팩토리 인터페이스

**주요 메서드**:
- `ISkillCard CreateFromDefinition(SkillCardDefinition definition, Owner owner, string ownerName)`: 정의로부터 카드 생성
- `ISkillCard CreateEnemyCard(SkillCardDefinition definition, string enemyName)`: 적 카드 생성

**구현 클래스**: `SkillCardFactory.cs`

---

#### `IPlayerHandManager`
플레이어 핸드 관리 인터페이스

**주요 메서드**:
- `void AddCardToHand(ISkillCard card)`: 핸드에 카드 추가
- `void RemoveCardFromHand(ISkillCard card)`: 핸드에서 카드 제거
- `IReadOnlyList<ISkillCard> GetHandCards()`: 핸드 카드 목록 반환

**구현 클래스**: `PlayerHandManager.cs`

---

### ItemSystem 인터페이스

#### `IItemService`
아이템 서비스 인터페이스

**주요 메서드**:
- `bool UseActiveItem(int slotIndex)`: 액티브 아이템 사용
- `bool AddActiveItem(ActiveItemDefinition item)`: 액티브 아이템 추가
- `bool RemoveActiveItem(int slotIndex)`: 액티브 아이템 제거
- `ActiveItemSlotData[] GetActiveSlots()`: 액티브 슬롯 정보 조회
- `bool IsActiveInventoryFull()`: 인벤토리 가득 참 여부

**이벤트**:
- `event Action<ActiveItemDefinition> OnActiveItemAdded`: 액티브 아이템 추가 이벤트
- `event Action<int> OnActiveItemRemoved`: 액티브 아이템 제거 이벤트
- `event Action<int> OnActiveItemUsed`: 액티브 아이템 사용 이벤트

**구현 클래스**: `ItemService.cs`

---

## 📦 주요 클래스 및 메서드

### CoreSystem

#### `SaveManager`
게임 저장/로드를 담당하는 매니저

**주요 변수**:
- `string saveFileName`: 저장 파일 이름
- `string stageProgressFileName`: 스테이지 진행 파일 이름

**주요 메서드**:
- `Task SaveCurrentScene()`: 현재 씬 상태 저장
- `Task LoadGame()`: 게임 로드
- `void SaveAudioSettings(float bgmVolume, float sfxVolume)`: 오디오 설정 저장

**의존성**:
- `[Inject] private IGameStateManager gameStateManager`

**레거시 코드**:
- ❌ `FindObjectOfType` 캐싱 (8개 매니저) → DI로 전환 필요

---

#### `AudioManager`
오디오 재생을 담당하는 매니저

**주요 변수**:
- `AudioSource bgmSource`: BGM 오디오 소스
- `AudioPoolManager audioPoolManager`: SFX 풀링 매니저

**주요 메서드**:
- `void PlayBGM(AudioClip clip, bool fadeIn = false)`: BGM 재생
- `void PlaySFX(AudioClip clip)`: SFX 재생
- `void PlaySFXWithPool(AudioClip clip, float volume, int poolSize)`: 풀링을 사용한 SFX 재생

**Resources.Load 사용**:
- ⚠️ `Resources.Load<AudioClip>()` 사용 → Addressables 전환 검토

---

### CharacterSystem

#### `PlayerCharacter`
플레이어 캐릭터 클래스

**주요 변수**:
- `PlayerCharacterData PlayerCharacterData`: 플레이어 데이터 (ScriptableObject)
- `PlayerCharacterUIController playerCharacterUIController`: UI 컨트롤러

**주요 메서드**:
- `void Initialize(PlayerCharacterData data)`: 캐릭터 초기화
- `void TakeDamage(int damage)`: 데미지 받기
- `void Heal(int amount)`: 체력 회복

**상속**: `CharacterBase` → `ICharacter`

---

#### `EnemyCharacter`
적 캐릭터 클래스

**주요 변수**:
- `EnemyCharacterData EnemyCharacterData`: 적 데이터 (ScriptableObject)
- `EnemySkillDeck enemySkillDeck`: 적 스킬 덱

**주요 메서드**:
- `void Initialize(EnemyCharacterData data)`: 적 캐릭터 초기화
- `void TakeDamage(int damage)`: 데미지 받기

**상속**: `CharacterBase` → `ICharacter`

---

### CombatSystem

#### `TurnManager`
턴 관리를 담당하는 매니저

**주요 변수**:
- `ITurnController _turnController`: 턴 컨트롤러
- `CombatPhase _currentPhase`: 현재 페이즈 (Setup/Battle/End)

**주요 메서드**:
- `TurnType GetCurrentTurnType()`: 현재 턴 타입 반환
- `void StartSetupPhase()`: 셋업 페이즈 시작
- `void CompleteTurn()`: 턴 완료

**레거시 코드**:
- ❌ `public enum TurnType { Player, Enemy }` (420-431줄) → 제거 필요
- ❌ `ConvertToLegacyTurnType()` (400-417줄) → 제거 필요
- ❌ `ConvertToNewTurnType()` (400-417줄) → 제거 필요

**새 구조**: `Interface.TurnType`만 사용

---

#### `CombatStateMachine`
전투 상태 머신

**주요 변수**:
- `ICombatState _currentState`: 현재 상태
- `CombatStateContext _context`: 상태 컨텍스트

**주요 메서드**:
- `void TransitionTo<T>() where T : ICombatState`: 상태 전환
- `void OnCardExecuted()`: 카드 실행 완료 처리

**레거시 코드**:
- ⚠️ 디버그/부활/턴 로직 혼재 → 서비스로 분리 필요

---

### SkillCardSystem

#### `SkillCardFactory`
스킬 카드 팩토리

**주요 메서드**:
- `ISkillCard CreateFromDefinition(SkillCardDefinition definition, Owner owner, string ownerName)`: 정의로부터 카드 생성
- `ISkillCard CreateEnemyCard(SkillCardDefinition definition, string enemyName)`: 적 카드 생성

**순환 의존성**:
- ⚠️ 자기 자신 참조 → 인터페이스 기반으로 재설계 필요

---

#### `PlayerHandManager`
플레이어 핸드 관리 매니저

**주요 변수**:
- `List<ISkillCard> _handCards`: 핸드 카드 목록
- `IHandSlotRegistry _handSlotRegistry`: 핸드 슬롯 레지스트리

**주요 메서드**:
- `void AddCardToHand(ISkillCard card)`: 핸드에 카드 추가
- `void RemoveCardFromHand(ISkillCard card)`: 핸드에서 카드 제거
- `IReadOnlyList<ISkillCard> GetHandCards()`: 핸드 카드 목록 반환

---

### ItemSystem

#### `ItemService`
아이템 서비스

**주요 변수**:
- `ActiveItemSlotData[] _activeSlots`: 액티브 슬롯 데이터 (4개)
- `Dictionary<string, int> _skillStarRanks`: 스킬 성급 딕셔너리

**주요 메서드**:
- `bool UseActiveItem(int slotIndex)`: 액티브 아이템 사용
- `bool AddActiveItem(ActiveItemDefinition item)`: 액티브 아이템 추가
- `bool UpgradeSkillStarRank(string skillId)`: 스킬 성급 증가

**Resources.Load 사용**:
- ⚠️ `Resources.Load<ActiveItemDefinition>()` 사용 → Addressables 전환 검토

---

## 📊 데이터 구조 (ScriptableObject)

### CharacterSystem 데이터

#### `PlayerCharacterData`
플레이어 캐릭터 데이터

**위치**: `Assets/Script/CharacterSystem/Data/PlayerCharacterData.cs`  
**에셋 위치**: `Assets/Resources/Data/Character/PlayerCharacters/`

**주요 필드**:
- `string DisplayName`: 캐릭터 표시 이름
- `PlayerCharacterType CharacterType`: 캐릭터 타입 (Sword/Bow/Staff)
- `int MaxHP`: 최대 체력
- `Sprite Portrait`: 초상화 이미지
- `GameObject PortraitPrefab`: 초상화 프리팹
- `PlayerSkillDeck skillDeck`: 스킬 덱

**사용 위치**: `PlayerCharacter.cs`, `PlayerCharacterSelector.cs`

---

#### `EnemyCharacterData`
적 캐릭터 데이터

**위치**: `Assets/Script/CharacterSystem/Data/EnemyCharacterData.cs`  
**에셋 위치**: `Assets/Resources/Data/Character/EnemyCharters/`

**주요 필드**:
- `string DisplayName`: 적 캐릭터 이름
- `int MaxHP`: 최대 체력
- `Sprite Portrait`: 초상화 이미지
- `EnemySkillDeck enemySkillDeck`: 적 스킬 덱

**사용 위치**: `EnemyCharacter.cs`, `EnemySpawnerManager.cs`

---

### SkillCardSystem 데이터

#### `SkillCardDefinition`
스킬 카드 정의

**위치**: `Assets/Script/SkillCardSystem/Data/SkillCardDefinition.cs`  
**에셋 위치**: `Assets/Resources/Data/SkillCard/Skill/`

**주요 필드**:
- `string cardId`: 카드 고유 ID
- `string displayName`: 카드 표시 이름
- `string displayNameKO`: 카드 표시 이름 (한국어)
- `string description`: 카드 설명
- `Sprite artwork`: 카드 아트워크
- `CardPresentation presentation`: 연출 구성
- `CardConfiguration configuration`: 카드 구성

**사용 위치**: `SkillCardFactory.cs`, `SkillCard.cs`

---

### ItemSystem 데이터

#### `ActiveItemDefinition`
액티브 아이템 정의

**위치**: `Assets/Script/ItemSystem/Data/ActiveItemDefinition.cs`  
**에셋 위치**: `Assets/Resources/Data/Item/ActiveItem/`

**주요 필드**:
- `string itemId`: 아이템 고유 ID
- `string displayName`: 아이템 표시 이름
- `string description`: 아이템 설명
- `Sprite icon`: 아이템 아이콘
- `ItemEffectSO[] effects`: 아이템 효과 배열

**사용 위치**: `ItemService.cs`, `ActiveItem.cs`

---

#### `PassiveItemDefinition`
패시브 아이템 정의

**위치**: `Assets/Script/ItemSystem/Data/PassiveItemDefinition.cs`  
**에셋 위치**: `Assets/Resources/Data/Item/PassiveItem/`

**주요 필드**:
- `string itemId`: 아이템 고유 ID
- `string displayName`: 아이템 표시 이름
- `ItemEffectSO effect`: 아이템 효과

**사용 위치**: `ItemService.cs`

---

## 🔄 코드 플로우

### 전투 시작 플로우

```
1. StageManager.StartStage()
   ↓
2. CombatFlowManager.InitializeCombat()
   ↓
3. TurnManager.StartSetupPhase()
   ↓
4. PlayerHandManager.DrawCards()
   ↓
5. CombatStateMachine.TransitionTo<CombatPrepareState>()
   ↓
6. CombatStateMachine.TransitionTo<CombatPlayerInputState>()
```

---

### 카드 실행 플로우

```
1. PlayerHandManager에서 카드 드래그
   ↓
2. CardDropService.OnCardDropped()
   ↓
3. CombatExecutionManager.ExecuteCardInBattleSlot()
   ↓
4. CardExecutor.Execute()
   ↓
5. EffectCommandFactory.CreateCommand()
   ↓
6. 각 EffectCommand.Execute()
   ↓
7. CombatExecutionManager.MoveSlotsForwardNew()
   ↓
8. TurnManager.CompleteTurn()
```

---

### 아이템 사용 플로우

```
1. InventoryPanelController.OnSlotClicked()
   ↓
2. ItemService.UseActiveItem(slotIndex)
   ↓
3. ActiveItem.Use()
   ↓
4. ItemEffectCommandFactory.CreateCommand()
   ↓
5. 각 ItemEffectCommand.Execute()
   ↓
6. ItemService.OnActiveItemUsed 이벤트 발생
   ↓
7. InventoryPanelController.RefreshSlots()
```

---

## 🎨 에셋 활용 가이드

### Resources 폴더 구조

```
Assets/Resources/
├── Data/                    # ScriptableObject 데이터
│   ├── Character/          # 캐릭터 데이터
│   ├── SkillCard/          # 스킬 카드 데이터
│   ├── Item/               # 아이템 데이터
│   └── Reward/             # 보상 데이터
├── Effect/                 # 이펙트 프리팹/머티리얼
├── Font/                   # 폰트 파일
├── Image/                  # 이미지 리소스
├── Prefab/                 # 프리팹
└── Sounds/                 # 오디오 파일
```

### ScriptableObject 로딩

#### 현재 방식 (Resources.Load)
```csharp
// ❌ 레거시 방식
var playerData = Resources.Load<PlayerCharacterData>("Data/Character/PlayerCharacters/Akein");
```

#### 개선 방식 (Addressables)
```csharp
// ✅ 개선 방식 (리팩토링 시 적용)
var handle = Addressables.LoadAssetAsync<PlayerCharacterData>("Data/Character/PlayerCharacters/Akein");
await handle.Task;
var playerData = handle.Result;
```

### 프리팹 로딩

#### 현재 방식
```csharp
// Resources 폴더에서 로드
var prefab = Resources.Load<GameObject>("Prefab/SkillCard");
var instance = Instantiate(prefab);
```

#### 개선 방식
```csharp
// Addressables로 전환 (리팩토링 시 적용)
var handle = Addressables.LoadAssetAsync<GameObject>("Prefab/SkillCard");
await handle.Task;
var instance = Instantiate(handle.Result);
```

---

## 🔧 MCP 서버 활용

### 리팩토링 전 검증 도구

#### 1. 금지된 API 검사
```bash
# MCP 도구: check_forbidden_apis
# FindObjectOfType, Resources.Load 등 금지된 API 검사
```

#### 2. Update 루프 감지
```bash
# MCP 도구: detect_update_loops
# Update/FixedUpdate/LateUpdate 사용 감지 및 대체 패턴 제안
```

#### 3. 순환 의존성 감지
```bash
# MCP 도구: detect_circular_dependencies
# 순환 의존성 감지 및 의존성 그래프 생성
```

#### 4. 코드 중복 감지
```bash
# MCP 도구: detect_code_duplication
# 중복 코드 블록 감지
```

#### 5. 품질 게이트 리포트
```bash
# MCP 도구: quality_gate_report
# 프로젝트 품질 게이트 요약 리포트 생성
```

---

## 📝 변경 기록

| 날짜 | 담당 | 내용 |
|------|------|------|
| 2025-11-24 | Cursor AI | 코드 로직 문서 초안 작성 |

---

## 🔗 관련 문서

- [완전 재작성 리팩토링 계획](./CompleteRefactoringPlan.md)
- [리팩토링 마스터 플랜](./RefactoringMasterPlan.md)
- [코드 품질 진단 리포트](./CodeQualityDiagnosisReport.md)

