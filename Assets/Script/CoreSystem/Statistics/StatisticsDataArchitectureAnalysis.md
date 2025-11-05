# 통계 시스템 SO 데이터 구조 분석 및 개선 방안

## 📊 프로젝트 SO 데이터 구조

### 1. 캐릭터 시스템

#### PlayerCharacterData
- **위치**: `Assets/Resources/Data/Character/PlayerCharacters/`
- **연결 관계**:
  ```
  PlayerCharacterData
    ├── SkillDeck (PlayerSkillDeck)
    │   └── CardEntries[] (List<CardEntry>)
    │       └── cardDefinition (SkillCardDefinition)
    │           ├── cardId (string)
    │           ├── displayName (string)
    │           └── displayNameKO (string)
    ├── MaxHP (int)
    ├── DisplayName (string)
    └── CharacterType (PlayerCharacterType)
  ```
- **실제 파일**: `Akein.asset`, `Serene.asset`

#### EnemyCharacterData
- **위치**: `Assets/Resources/Data/Character/EnemyCharters/Stage_1/`
- **연결 관계**:
  ```
  EnemyCharacterData
    ├── SkillDeck (EnemySkillDeck)
    │   └── cards[] (List<CardEntry>)
    │       └── definition (SkillCardDefinition)
    ├── MaxHP (int)
    ├── DisplayName (string)
    └── Prefab (GameObject)
  ```
- **실제 파일**: `턴컨.asset`, `비오네.asset`

### 2. 스킬카드 시스템

#### SkillCardDefinition
- **위치**: `Assets/Resources/Data/SkillCard/Skill/`
- **주요 필드**:
  - `cardId`: 고유 식별자
  - `displayName`: 표시 이름 (영문)
  - `displayNameKO`: 표시 이름 (한국어)
  - `configuration`: 카드 구성 (데미지, 리소스, 효과)
- **실제 파일**: 24개 이상 (예: `001_베기.asset`, `002_2연격.asset`, `503_화살 수급(대).asset`)

#### PlayerSkillDeck
- **위치**: `Assets/Resources/Data/SkillCard/Enemy/Player/PlayerSkillDeck/`
- **연결 관계**:
  ```
  PlayerSkillDeck
    └── CardEntries[] (List<CardEntry>)
        ├── cardDefinition (SkillCardDefinition)
        └── quantity (int)
  ```
- **실제 파일**: `Akein.asset`, `Serene.asset`

#### EnemySkillDeck
- **위치**: `Assets/Resources/Data/SkillCard/Enemy/EnemySkillDeck/`
- **연결 관계**:
  ```
  EnemySkillDeck
    └── cards[] (List<CardEntry>)
        ├── definition (SkillCardDefinition)
        ├── probability (float)
        └── damageOverride (int)
  ```
- **실제 파일**: `턴킨.asset`, `비오네_조교.asset`

### 3. 아이템 시스템

#### ActiveItemDefinition
- **위치**: `Assets/Resources/Data/Item/ActiveItem/`
- **연결 관계**:
  ```
  ActiveItemDefinition
    ├── ItemId (string)
    ├── DisplayName (string)
    ├── targetType (ItemTargetType)
    └── effectConfiguration (ItemEffectConfiguration)
        └── effects[] (List<ItemEffectConfig>)
            └── effectSO (ItemEffectSO)
  ```
- **실제 파일**: 8개
  - `001_회복 물약.asset`
  - `002_공격력 물약.asset`
  - `003_광대 물약.asset`
  - `004_타임 스톱 스크롤.asset`
  - `005_운명의 주사위.asset`
  - `006_역행의 모래시계.asset`
  - `007_실드 브레이커.asset`
  - `008_부활의 징표.asset`

#### PassiveItemDefinition
- **위치**: `Assets/Resources/Data/Item/PassiveItem/`
- **연결 관계**:
  ```
  PassiveItemDefinition
    ├── ItemId (string)
    ├── DisplayName (string)
    ├── targetSkill (SkillCardDefinition) ⭐ 중요: 스킬 참조
    ├── enhancementIncrements[] (int[])
    └── category (PassiveItemCategory)
  ```
- **실제 파일**: 14개
  - `101_베기.asset` → `001_베기.asset` (SkillCardDefinition) 참조
  - `102_2연격.asset` → `002_2연격.asset` 참조
  - `103_기습.asset` → `003_기습.asset` 참조
  - `104_열참.asset` → `004_열참.asset` 참조
  - `105_강공.asset` → `005_강공.asset` 참조
  - `106_일섬.asset` → `006_일섬.asset` 참조
  - `107_단검베기.asset` → `008_단검 베기.asset` 참조
  - `108_더블샷.asset` → `009_더블 샷.asset` 참조
  - `109_트리플샷.asset` → `010_트리플 샷.asset` 참조
  - `110_곡사.asset` → `011_곡사.asset` 참조
  - `116_망토.asset` (체력 보너스, 스킬 참조 없음)
  - `117_투구.asset` (체력 보너스, 스킬 참조 없음)
  - `118_갑옷.asset` (체력 보너스, 스킬 참조 없음)

### 4. 스테이지 시스템

#### StageData
- **위치**: `Assets/Resources/Data/Character/EnemyCharters/Stage_1/`
- **연결 관계**:
  ```
  StageData
    ├── stageNumber (int)
    ├── stageName (string)
    └── enemies[] (List<EnemyCharacterData>)
  ```
- **실제 파일**: `Stage1.asset`

### 5. 보상 시스템

#### RewardPool
- **위치**: `Assets/Resources/Data/Reward/`
- **연결 관계**:
  ```
  RewardPool
    └── entries[] (List<WeightedEntry>)
        ├── item (ItemDefinition) → ActiveItemDefinition | PassiveItemDefinition
        ├── weight (int)
        ├── tags[] (string[])
        ├── minStage (int)
        ├── maxStage (int)
        └── uniquePerRun (bool)
  ```
- **실제 파일**: `RewardPool.asset`, `RewardPool 1.asset`

#### PlayerRewardProfile
- **위치**: `Assets/Resources/Data/Reward/`
- **연결 관계**:
  ```
  PlayerRewardProfile
    ├── allowedTags[] (string[])
    ├── bannedTags[] (string[])
    ├── bannedActiveItems[] (ActiveItemDefinition[])
    └── bannedPassiveItems[] (PassiveItemDefinition[])
  ```
- **실제 파일**: `AkeinPlayerRewardProfile.asset`, `SerenRewardProfile.asset`

## 🔗 SO 데이터 간 연결 관계도

```
Game Session
    │
    ├── PlayerCharacterData (Akein/Serene)
    │   └── PlayerSkillDeck
    │       └── SkillCardDefinition[] (덱 구성)
    │
    ├── StageData
    │   └── EnemyCharacterData[]
    │       └── EnemySkillDeck
    │           └── SkillCardDefinition[] (적 덱)
    │
    └── Reward System
        ├── RewardPool
        │   └── ItemDefinition[] (보상 풀)
        │       ├── ActiveItemDefinition[]
        │       └── PassiveItemDefinition[]
        │           └── SkillCardDefinition (targetSkill 참조)
        │
        └── PlayerRewardProfile
            └── ItemDefinition[] (필터링된 보상)
```

## 📈 현재 통계 수집 방식

### 구조
```
CombatStatsAggregator (전투별 통계)
    └── 이벤트 구독
        ├── CombatEvents.OnPlayerCardUsed
        ├── CombatEvents.OnPlayerCardSpawn
        ├── IItemService.OnActiveItemAdded
        ├── IItemService.OnActiveItemRemoved
        └── IItemService.OnPassiveItemAdded
    └── Dictionary<string, int> 저장
        ├── playerSkillUsageByCardId
        ├── playerSkillUsageByName
        ├── activeItemUsageByName
        └── ...

GameSessionStatistics (세션별 통계)
    └── CombatStatsSnapshot 수집
    └── SessionStatisticsData 저장
        ├── skillCardSpawnCountByCardId
        ├── skillCardUseCountByCardId
        ├── activeItemSpawnCountByItemId
        └── ...

StatisticsManager (저장/로드)
    └── JSON 직렬화
    └── SerializableKeyValuePair 사용
```

### 장점
✅ **실시간 수집**: 이벤트 기반으로 즉시 통계 수집
✅ **메모리 효율**: 실제 사용된 항목만 Dictionary에 저장
✅ **확장성**: 새로운 이벤트 추가 시 쉽게 확장 가능
✅ **성능**: Dictionary 조회 O(1) 시간 복잡도

### 단점
❌ **ID 기반 저장**: SO 참조 정보가 손실됨
❌ **메타데이터 부재**: 저장 시 카드/아이템 이름만 저장, 카테고리/타입 정보 없음
❌ **미획득 항목 계산**: 별도 로직 필요 (이미 구현됨: `ItemResourceCache` 사용)
❌ **덱 순서 정렬**: 저장 시점에 `PlayerSkillDeck` 필요

## 💡 개선된 통계 수집 방안

### 방안 1: SO 기반 인덱싱 시스템 (추천)

#### 개념
게임 시작 시 모든 SO 데이터를 인덱싱하여 통계 수집 시 SO 참조를 통해 메타데이터 접근

#### 구조
```csharp
public class StatisticsMetadataRegistry
{
    // 모든 가능한 스킬카드 인덱스
    private Dictionary<string, SkillCardMetadata> skillCardMetadata;
    
    // 모든 가능한 아이템 인덱스
    private Dictionary<string, ItemMetadata> itemMetadata;
    
    // 플레이어 덱 정보 (정렬용)
    private Dictionary<string, PlayerDeckInfo> playerDeckInfo;
}
```

#### 장점
✅ **메타데이터 보존**: SO 참조를 통해 카테고리, 타입, 연결 관계 정보 유지
✅ **확장성**: 새로운 통계 항목 추가 시 SO에서 메타데이터 추출 가능
✅ **정렬 효율**: 덱 정보를 미리 인덱싱하여 정렬 성능 향상
✅ **검증 용이**: 존재하지 않는 ID 발견 시 즉시 감지 가능

#### 단점
⚠️ **초기화 오버헤드**: 게임 시작 시 모든 SO 로드 필요
⚠️ **메모리 사용**: 사용하지 않는 SO도 메모리에 로드

### 방안 2: 하이브리드 방식 (현재 + SO 참조)

#### 개념
현재 방식 유지하되, 저장 시점에 SO 참조를 통해 메타데이터 보강

#### 구조
```csharp
public class EnhancedStatisticsData
{
    // 현재 방식 유지 (ID 기반)
    public Dictionary<string, int> skillCardUseCountByCardId;
    
    // 저장 시점에 SO 로드하여 메타데이터 보강
    public void EnrichWithMetadata()
    {
        // SkillCardDefinition 로드하여 displayNameKO, category 등 추가
        // 저장 시점에만 SO 로드하므로 런타임 오버헤드 최소화
    }
}
```

#### 장점
✅ **기존 코드 유지**: 현재 구조 최소 변경
✅ **런타임 효율**: 통계 수집 시 Dictionary만 사용
✅ **메타데이터 보강**: 저장 시점에 필요한 정보만 추가

#### 단점
⚠️ **저장 오버헤드**: 저장 시마다 SO 로드 필요
⚠️ **일관성**: 통계 수집과 저장 로직 분리로 복잡도 증가

### 방안 3: 이벤트 확장 (SO 참조 전달)

#### 개념
이벤트에 SO 참조를 포함하여 통계 수집 시점에 메타데이터 접근

#### 구조
```csharp
// 이벤트 시그니처 변경
CombatEvents.OnPlayerCardUsed(SkillCardDefinition card, ...);
IItemService.OnActiveItemAdded(ActiveItemDefinition item, ...);

// 통계 수집
private void HandleCardUsed(SkillCardDefinition card, ...)
{
    // SO 참조를 통해 즉시 메타데이터 접근
    string cardId = card.cardId;
    string displayNameKO = card.displayNameKO;
    string category = card.configuration.category; // 예시
    
    // Dictionary에 저장
    _skillUsage[cardId]++;
    _skillUsageByName[displayNameKO]++;
}
```

#### 장점
✅ **즉시 메타데이터 접근**: 통계 수집 시점에 SO 정보 활용 가능
✅ **확장성**: 새로운 통계 항목 추가 시 SO에서 바로 추출
✅ **일관성**: 통계 수집과 메타데이터 추출을 한 곳에서 처리

#### 단점
⚠️ **이벤트 시그니처 변경**: 기존 이벤트 구독자 모두 수정 필요
⚠️ **의존성 증가**: 통계 수집 로직이 SO 구조에 의존

## 📊 비교표

| 항목 | 현재 방식 | 방안 1: SO 인덱싱 | 방안 2: 하이브리드 | 방안 3: 이벤트 확장 |
|------|----------|------------------|-------------------|-------------------|
| **런타임 성능** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ |
| **메타데이터 보존** | ⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **확장성** | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ |
| **코드 변경 범위** | - | ⭐⭐⭐ | ⭐⭐ | ⭐⭐⭐⭐ |
| **메모리 사용** | ⭐⭐⭐⭐⭐ | ⭐⭐⭐ | ⭐⭐⭐⭐⭐ | ⭐⭐⭐⭐⭐ |

## 🎯 권장 방안

### 단기: 현재 방식 유지 + 개선
- ✅ 현재 구조가 잘 작동하고 있음
- ✅ 미획득 아이템 계산은 이미 `ItemResourceCache` 활용
- ✅ 덱 순서 정렬도 이미 구현됨 (`PrepareForSerialization`)

### 중기: 방안 2 (하이브리드) 적용
- ✅ 저장 시점에 SO 메타데이터 보강
- ✅ 런타임 성능 영향 최소화
- ✅ JSON에 카테고리, 타입 등 추가 정보 포함 가능

### 장기: 방안 3 (이벤트 확장) 고려
- ✅ 새로운 통계 항목 추가 시 SO 메타데이터 즉시 활용
- ✅ 통계 수집 로직과 메타데이터 추출의 일관성 확보
- ⚠️ 이벤트 시그니처 변경으로 인한 리팩토링 필요

## 📝 결론

현재 통계 수집 방식은 **이벤트 기반 실시간 수집**으로 충분히 효율적입니다.

**현재 방식의 강점**:
1. ✅ 실제 사용된 항목만 메모리에 저장 (효율적)
2. ✅ Dictionary 기반 빠른 조회
3. ✅ 미획득 아이템 계산 로직 이미 구현 (`ItemResourceCache`)
4. ✅ 덱 순서 정렬 로직 이미 구현 (`PrepareForSerialization`)

**개선 가능한 부분**:
1. 📌 저장 시점에 SO 메타데이터 보강 (방안 2)
2. 📌 통계 항목 확장 시 SO 참조 활용 고려 (방안 3)

**결론**: 현재 방식이 적절하며, 필요 시 하이브리드 방식으로 점진적 개선 가능합니다.

