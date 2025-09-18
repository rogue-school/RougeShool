# CharacterSystem 개발 문서

## 📋 시스템 개요
CharacterSystem은 게임의 모든 캐릭터(플레이어, 적)를 관리하는 시스템입니다. 캐릭터의 기본 속성, 상태, 행동을 통합적으로 관리하며, 새로운 리그 오브 레전드 스타일의 플레이어 캐릭터 UI 시스템을 제공합니다.

### 최근 변경(요약)
- **새로운 플레이어 UI 시스템**: 리그 오브 레전드 스타일의 HP/MP 바 구현
- **통합 UI 컨트롤러**: PlayerCharacterUIController로 모든 플레이어 UI 통합 관리
- **버프/디버프 아이콘 시스템**: 개별 아이콘 관리 및 시각적 효과 제공
- **캐릭터별 리소스 시스템**: 검/활/지팡이 타입별 특수 리소스 관리
- **DOTween 애니메이션**: 부드러운 UI 전환 및 시각적 피드백

## 🏗️ 폴더 구조
```
CharacterSystem/
├── Core/             # 캐릭터 핵심 로직 (4개 파일)
├── Data/             # 캐릭터 데이터 (4개 파일)
├── Interface/        # 캐릭터 인터페이스 (8개 파일)
├── Manager/          # 캐릭터 매니저 (3개 파일)
├── Intialization/    # 캐릭터 초기화 (6개 파일) [주의: 폴더명 오타 - Initialization이어야 함]
├── Slot/             # 캐릭터 슬롯 (1개 파일)
├── UI/               # 캐릭터 UI (2개 파일)
└── Utility/          # 캐릭터 유틸리티 (4개 파일)
```

## 📁 주요 컴포넌트

### Core 폴더 (4개 파일)
- **CharacterBase.cs**: 모든 캐릭터의 기본 클래스
- **PlayerCharacter.cs**: 플레이어 캐릭터 구현
- **EnemyCharacter.cs**: 적 캐릭터 구현
- **CharacterState.cs**: 캐릭터 상태 관리

### Data 폴더 (4개 파일)
- **PlayerCharacterData.cs**: 플레이어 캐릭터 데이터 (ScriptableObject)
- **EnemyCharacterData.cs**: 적 캐릭터 데이터 (ScriptableObject)
- **PlayerCharacterType.cs**: 플레이어 캐릭터 타입 열거형
- **PlayerCharacterTypeHelper.cs**: 플레이어 캐릭터 타입 헬퍼

### Interface 폴더 (8개 파일)
- **ICharacter.cs**: 캐릭터 기본 인터페이스
- **ICharacterData.cs**: 캐릭터 데이터 인터페이스
- **ICharacterState.cs**: 캐릭터 상태 인터페이스
- **ICharacterAction.cs**: 캐릭터 행동 인터페이스
- **ICharacterEffect.cs**: 캐릭터 효과 인터페이스
- **ICharacterUI.cs**: 캐릭터 UI 인터페이스
- **IPlayerResourceManager.cs**: 플레이어 리소스 관리 인터페이스
- **ICharacterDeathListener.cs**: 캐릭터 사망 리스너 인터페이스

### Manager 폴더 (3개 파일)
- **PlayerManager.cs**: 플레이어 캐릭터 매니저
- **EnemyManager.cs**: 적 캐릭터 매니저
- **PlayerResourceManager.cs**: 플레이어 리소스 관리 매니저

### Intialization 폴더 (6개 파일) [폴더명 오타 주의]
- **EnemyCharacterInitializer.cs**: 적 캐릭터 초기화
- **EnemyHandInitializer.cs**: 적 핸드 초기화
- **EnemyInitializer.cs**: 적 초기화 통합 관리
- **HandInitializer.cs**: 핸드 초기화 기본 클래스
- **PlayerCharacterInitializer.cs**: 플레이어 캐릭터 초기화
- **PlayerSkillCardInitializer.cs**: 플레이어 스킬카드 초기화

### UI 폴더 (4개 파일)
- **CharacterSlotUI.cs**: 캐릭터 슬롯 UI
- **CharacterUIController.cs**: 캐릭터 UI 컨트롤러
- **PlayerCharacterUIController.cs**: 플레이어 캐릭터 통합 UI 컨트롤러 (새로 추가)
- **BuffDebuffIcon.cs**: 버프/디버프 아이콘 관리 (새로 추가)

### Utility 폴더 (4개 파일)
- **CharacterDeathHandler.cs**: 캐릭터 사망 처리
- **CardRegistrar.cs**: 카드 등록기
- **CardValidator.cs**: 카드 검증기
- **EnemySpawnResult.cs**: 적 스폰 결과

## 🎯 주요 기능

### 1. 캐릭터 기본 속성
- **체력 (Health)**: 캐릭터의 생명력 (currentHP, maxHP)
- **가드 (Guard)**: 데미지 감소 방어력 (currentGuard, isGuarded)
- **리소스 (Resource)**: 캐릭터 타입별 리소스 (Bow: 화살, Staff: 마나, Sword: 0)
- **턴 효과 (PerTurnEffect)**: 턴마다 적용되는 효과들

### 2. 플레이어 캐릭터 타입
- **검 (Sword)**: 근접 전투 특화
- **활 (Bow)**: 원거리 전투 특화, 화살 리소스 관리
- **지팡이 (Staff)**: 마법 전투 특화, 마나 리소스 관리

### 3. 상태 관리
- **생존 상태**: 살아있음/죽음
- **효과 상태**: 버프/디버프 효과

### 4. 행동 시스템
- **기본 공격**: 일반적인 공격 행동
- **스킬 사용**: 특수 능력 사용

### 5. 리소스 관리
- **화살 (Arrows)**: 활 캐릭터 전용 리소스
- **마나 (Mana)**: 지팡이 캐릭터 전용 리소스
- **리소스 소모**: 스킬 사용 시 리소스 소모

### 6. 초기화 시스템
- **자동 초기화**: 캐릭터 생성 시 자동 설정
- **스킬카드 초기화**: 캐릭터별 스킬카드 덱 설정

### 7. 새로운 플레이어 UI 시스템
- **리그 오브 레전드 스타일**: HP/MP 바의 시각적 디자인
- **통합 UI 컨트롤러**: 모든 플레이어 UI 요소를 하나의 컨트롤러로 관리
- **캐릭터 정보 표시**: 초상화, 문양, 이름, HP/MP 바
- **버프/디버프 아이콘**: 개별 아이콘 관리 및 지속시간 표시
- **DOTween 애니메이션**: 부드러운 UI 전환 및 시각적 피드백
- **캐릭터별 리소스 표시**: 검(없음), 활(화살), 지팡이(마나) 타입별 표시

## 📊 주요 클래스 및 메서드

### EnemyManager 클래스
- **RegisterEnemy(IEnemyCharacter enemy)**: 적 캐릭터 등록
- **UnregisterEnemy()**: 적 캐릭터 등록 해제
- **GetCurrentEnemy()**: 현재 적 캐릭터 조회
- **HasEnemy()**: 적 캐릭터 등록 여부 확인
- **ClearEnemy()**: 등록된 적 캐릭터 초기화
- **Reset()**: 매니저 상태 초기화

### PlayerResourceManager 클래스
- **Initialize(PlayerCharacterData characterData)**: 캐릭터 데이터로 초기화
- **CanConsumeResource(int amount)**: 리소스 소모 가능 여부 확인
- **ConsumeResource(int amount)**: 리소스 소모
- **RestoreResource(int amount)**: 리소스 회복
- **CurrentResource**: 현재 리소스 양 (프로퍼티)
- **MaxResource**: 최대 리소스 양 (프로퍼티)
- **ResourceName**: 리소스 이름 (프로퍼티)

### PlayerCharacterUIController 클래스 (새로 추가)
- **Initialize(PlayerCharacter character)**: 플레이어 캐릭터로 UI 초기화
- **UpdateHP(int currentHP, int maxHP)**: HP 바 업데이트
- **UpdateResource(int currentResource, int maxResource)**: 리소스 바 업데이트
- **OnTakeDamage(int damage)**: 데미지 받을 때 UI 효과
- **OnHeal(int healAmount)**: 힐 받을 때 UI 효과
- **AddBuffDebuffIcon(Sprite icon, string name, int duration, bool isDebuff)**: 버프/디버프 아이콘 추가
- **RemoveBuffDebuffIcon(string iconName)**: 버프/디버프 아이콘 제거
- **ClearAllBuffDebuffIcons()**: 모든 버프/디버프 아이콘 제거
- **SetCharacterInfo(PlayerCharacterData data)**: 캐릭터 정보 설정
- **UpdateResourceDisplay()**: 리소스 표시 업데이트

### BuffDebuffIcon 클래스 (새로 추가)
- **Initialize(Sprite icon, string name, int duration, bool isDebuff)**: 아이콘 초기화
- **UpdateDuration(int newDuration)**: 지속시간 업데이트
- **StartExpirationWarning()**: 만료 경고 시작
- **Expire()**: 아이콘 만료 처리
- **SetHoverEffect(bool isHovering)**: 호버 효과 설정
- **FadeIn()**: 페이드 인 애니메이션
- **FadeOut()**: 페이드 아웃 애니메이션

### EnemySpawnerManager 클래스
- **SpawnEnemy(EnemyCharacterData data)**: 적 데이터로 스폰
- **SpawnEnemyWithAnimation()**: 애니메이션과 함께 적 스폰 (코루틴)
- **GetAllEnemies()**: 스폰된 모든 적 캐릭터 조회
- **SpawnInitialEnemy()**: 초기 적 스폰 (Deprecated)

### ICharacterSlot 인터페이스
- **SetCharacter(ICharacter character)**: 슬롯에 캐릭터 설정
- **Clear()**: 슬롯에서 캐릭터 제거
- **GetCharacter()**: 현재 슬롯의 캐릭터 조회
- **GetTransform()**: 슬롯의 Transform 반환
- **GetSlotPosition()**: 슬롯 위치 정보 반환
- **GetOwner()**: 슬롯 소유자 정보 반환

### ICharacterSlotRegistry 인터페이스
- **RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots)**: 캐릭터 슬롯들 등록
- **GetCharacterSlot(SlotOwner owner)**: 소유자별 캐릭터 슬롯 조회
- **GetAllCharacterSlots()**: 모든 캐릭터 슬롯 조회

### PlayerCharacterData 클래스
- **DisplayName**: 캐릭터 표시 이름 (프로퍼티)
- **CharacterType**: 캐릭터 타입 (프로퍼티)
- **MaxHP**: 최대 체력 (프로퍼티)
- **Portrait**: 캐릭터 초상화 (프로퍼티)
- **MaxResource**: 최대 리소스 (프로퍼티)
- **ResourceName**: 리소스 이름 (프로퍼티)

### EnemyCharacter 클래스
- **Initialize(EnemyCharacterData data)**: 적 캐릭터 데이터로 초기화
- **CharacterData**: 적 캐릭터 데이터 (프로퍼티)
- **CharacterName**: 캐릭터 이름 (프로퍼티)
- **Data**: 적 캐릭터 데이터 (프로퍼티)

## 🔧 사용 방법

### 기본 사용법
```csharp
// 캐릭터 생성 및 초기화
PlayerCharacter player = Instantiate(playerPrefab);
player.Initialize(playerData);

EnemyCharacter enemy = Instantiate(enemyPrefab);
enemy.Initialize(enemyData);

// 캐릭터 상태 확인
if (player.IsAlive)
{
    // 공격 실행
    player.Attack(enemy);
}
```

### 새로운 플레이어 UI 시스템 사용법
```csharp
// PlayerCharacterUIController를 통한 UI 관리
PlayerCharacterUIController uiController = FindObjectOfType<PlayerCharacterUIController>();

// 플레이어 캐릭터로 UI 초기화
uiController.Initialize(player);

// HP 업데이트
uiController.UpdateHP(player.CurrentHP, player.MaxHP);

// 리소스 업데이트 (활 캐릭터의 경우)
uiController.UpdateResource(resourceManager.CurrentResource, resourceManager.MaxResource);

// 데미지 받을 때 UI 효과
uiController.OnTakeDamage(10);

// 힐 받을 때 UI 효과
uiController.OnHeal(5);

// 버프 아이콘 추가
Sprite buffIcon = Resources.Load<Sprite>("Icons/StrengthBuff");
uiController.AddBuffDebuffIcon(buffIcon, "힘 강화", 3, false); // 3턴 지속

// 디버프 아이콘 추가
Sprite debuffIcon = Resources.Load<Sprite>("Icons/PoisonDebuff");
uiController.AddBuffDebuffIcon(debuffIcon, "독", 2, true); // 2턴 지속

// 버프/디버프 아이콘 제거
uiController.RemoveBuffDebuffIcon("힘 강화");

// 모든 버프/디버프 아이콘 제거
uiController.ClearAllBuffDebuffIcons();
```

### BuffDebuffIcon 개별 관리 사용법
```csharp
// BuffDebuffIcon 직접 생성 및 관리
BuffDebuffIcon buffIcon = Instantiate(buffIconPrefab);
buffIcon.Initialize(iconSprite, "힘 강화", 3, false);

// 지속시간 업데이트
buffIcon.UpdateDuration(2);

// 만료 경고 시작 (1턴 남았을 때)
buffIcon.StartExpirationWarning();

// 호버 효과 설정
buffIcon.SetHoverEffect(true);

// 페이드 인/아웃 애니메이션
buffIcon.FadeIn();
buffIcon.FadeOut();

// 아이콘 만료 처리
buffIcon.Expire();
```

### 매니저를 통한 캐릭터 관리
```csharp
// EnemyManager를 통한 적 캐릭터 관리
EnemyManager enemyManager = FindObjectOfType<EnemyManager>();

// 적 캐릭터 등록
enemyManager.RegisterEnemy(enemy);

// 현재 적 캐릭터 조회
IEnemyCharacter currentEnemy = enemyManager.GetCurrentEnemy();

// 적 캐릭터 등록 해제
enemyManager.UnregisterEnemy();

// 적 캐릭터 초기화
enemyManager.ClearEnemy();
```

### 리소스 관리
```csharp
// PlayerResourceManager를 통한 리소스 관리
PlayerResourceManager resourceManager = FindObjectOfType<PlayerResourceManager>();

// 캐릭터 데이터로 초기화
resourceManager.Initialize(playerData);

// 리소스 소모 가능 여부 확인
if (resourceManager.CanConsumeResource(5))
{
    resourceManager.ConsumeResource(5);
}

// 리소스 회복
resourceManager.RestoreResource(3);

// 리소스 상태 조회
int currentResource = resourceManager.CurrentResource;
int maxResource = resourceManager.MaxResource;
string resourceName = resourceManager.ResourceName;
```

### 적 스폰 관리
```csharp
// EnemySpawnerManager를 통한 적 스폰
EnemySpawnerManager spawnerManager = FindObjectOfType<EnemySpawnerManager>();

// 적 데이터로 스폰
EnemySpawnResult result = spawnerManager.SpawnEnemy(enemyData);

if (result.IsSuccess)
{
    EnemyCharacter spawnedEnemy = result.EnemyCharacter;
    // 스폰된 적 사용
}

// 스폰된 모든 적 조회
List<EnemyCharacter> allEnemies = spawnerManager.GetAllEnemies();
```

### 캐릭터 슬롯 관리
```csharp
// ICharacterSlot을 통한 슬롯 관리
ICharacterSlot playerSlot = slotRegistry.GetCharacterSlot(SlotOwner.PLAYER);
ICharacterSlot enemySlot = slotRegistry.GetCharacterSlot(SlotOwner.ENEMY);

// 슬롯에 캐릭터 설정
playerSlot.SetCharacter(player);
enemySlot.SetCharacter(enemy);

// 슬롯에서 캐릭터 조회
ICharacter slotCharacter = playerSlot.GetCharacter();

// 슬롯 초기화
playerSlot.Clear();
```

### 캐릭터 타입별 특수 기능
```csharp
// 플레이어 캐릭터 타입별 특수 기능
if (player.CharacterType == PlayerCharacterType.Sword)
{
    // 검 캐릭터 특수 기능
    player.SwordAttack();
}
else if (player.CharacterType == PlayerCharacterType.Bow)
{
    // 활 캐릭터 특수 기능 (화살 리소스 사용)
    if (resourceManager.CanConsumeResource(1))
    {
        resourceManager.ConsumeResource(1);
        player.BowAttack();
    }
}
else if (player.CharacterType == PlayerCharacterType.Staff)
{
    // 지팡이 캐릭터 특수 기능 (마나 리소스 사용)
    if (resourceManager.CanConsumeResource(2))
    {
        resourceManager.ConsumeResource(2);
        player.StaffAttack();
    }
}
```

### 초기화 시스템 연동
```csharp
// ICombatInitializerStep을 통한 초기화
PlayerCharacterInitializer playerInitializer = FindObjectOfType<PlayerCharacterInitializer>();

// 초기화 순서 확인
int order = playerInitializer.Order; // 낮을수록 먼저 실행

// 초기화 실행 (CombatInitializer에서 자동 호출됨)
playerInitializer.ExecuteInitialization();
```

## 🏗️ 아키텍처 패턴

### 1. 상속 구조
- **CharacterBase**: 모든 캐릭터의 공통 기능
- **PlayerCharacter**: 플레이어 전용 기능
- **EnemyCharacter**: 적 전용 기능

### 2. 인터페이스 분리
- **ICharacter**: 기본 캐릭터 기능
- **ICharacterData**: 데이터 관련 기능
- **ICharacterState**: 상태 관리 기능

### 3. 매니저 패턴
- **PlayerManager**: 플레이어 캐릭터 관리
- **EnemyManager**: 적 캐릭터 관리
- **PlayerResourceManager**: 플레이어 리소스 관리

## 🔧 기술적 구현 세부사항

### 성능 최적화
- **메모리 관리**: 캐릭터 객체 풀링을 통한 GC 압박 최소화
- **프레임 최적화**: 캐릭터 상태 업데이트 최적화
- **렌더링 최적화**: 캐릭터 UI 업데이트 빈도 최적화
- **로딩 최적화**: 캐릭터 데이터 사전 로딩 및 캐싱

### 스레드 안전성
- **동시성 제어**: 캐릭터 상태 변경 시 락을 통한 동시성 제어
- **비동기 처리**: 캐릭터 초기화 시 비동기 처리
- **이벤트 처리**: 스레드 안전한 캐릭터 이벤트 시스템
- **데이터 동기화**: 캐릭터 상태 데이터 동기화

### 메모리 관리
- **생명주기 관리**: 캐릭터 객체의 생성/소멸 관리
- **리소스 해제**: 캐릭터 제거 시 리소스 정리
- **메모리 누수 방지**: 이벤트 구독 해제, 캐릭터 참조 해제
- **프로파일링**: 캐릭터 시스템 메모리 사용량 모니터링

## 🏗️ 시스템 아키텍처

### 의존성 다이어그램
```mermaid
graph TD
    A[PlayerManager] --> B[PlayerCharacter]
    A --> C[PlayerResourceManager]
    
    D[EnemyManager] --> E[EnemyCharacter]
    D --> F[EnemyInitializer]
    
    G[CharacterBase] --> H[PlayerCharacter]
    G --> I[EnemyCharacter]
    
    J[PlayerCharacterData] --> K[PlayerCharacter]
    L[EnemyCharacterData] --> M[EnemyCharacter]
    
    style A fill:#ff9999
    style D fill:#ffcc99
    style G fill:#99ccff
    style J fill:#ccffcc
    style L fill:#ccffcc
```

### 클래스 다이어그램
```mermaid
classDiagram
    class ICharacter {
        <<interface>>
        +Health: int
        +IsAlive: bool
        +Attack(target) void
        +TakeDamage(amount) void
    }
    
    class CharacterBase {
        -health: int
        -maxHealth: int
        -isAlive: bool
        +Health: int
        +IsAlive: bool
        +Attack(target) void
        +TakeDamage(amount) void
    }
    
    class PlayerCharacter {
        -characterType: PlayerCharacterType
        -resourceManager: PlayerResourceManager
        +CharacterType: PlayerCharacterType
        +ConsumeResource(amount) bool
    }
    
    class EnemyCharacter {
        -enemyData: EnemyCharacterData
        -aiController: EnemyAI
        +EnemyData: EnemyCharacterData
        +ExecuteAI() void
    }
    
    class PlayerResourceManager {
        -arrowCount: int
        -manaCount: int
        +ConsumeResource(type, amount) bool
        +RestoreResource(type, amount) void
    }
    
    ICharacter <|.. CharacterBase
    CharacterBase <|-- PlayerCharacter
    CharacterBase <|-- EnemyCharacter
    PlayerCharacter --> PlayerResourceManager
```

### 시퀀스 다이어그램
```mermaid
sequenceDiagram
    participant PM as PlayerManager
    participant PC as PlayerCharacter
    participant PRM as PlayerResourceManager
    participant EM as EnemyManager
    participant EC as EnemyCharacter
    
    PM->>PC: CreatePlayer(data, type)
    PC->>PRM: InitializeResources()
    PRM-->>PC: Resources initialized
    PC-->>PM: Player created
    
    PM->>PC: Attack(target)
    PC->>PRM: ConsumeResource(type, amount)
    PRM-->>PC: Resource consumed
    PC->>EC: TakeDamage(amount)
    EC-->>PC: Damage taken
    PC-->>PM: Attack complete
```

## 📚 참고 자료

### 관련 문서
- [Unity MonoBehaviour](https://docs.unity3d.com/Manual/class-MonoBehaviour.html)
- [ScriptableObject](https://docs.unity3d.com/Manual/class-ScriptableObject.html)
- [상속 구조](https://docs.microsoft.com/ko-kr/dotnet/csharp/programming-guide/classes-and-structs/inheritance)

## 📝 변경 기록(Delta)
- 형식: `YYYY-MM-DD | 작성자 | 변경 요약 | 영향도(코드/씬/문서)`

- 2025-01-27 | Maintainer | CharacterSystem 개발 문서 초기 작성 | 문서
- 2025-01-27 | Maintainer | 실제 폴더 구조 반영 및 Intialization 폴더명 오타 주의 표시 | 문서
- 2025-01-27 | Maintainer | 실제 코드 분석 기반 구체적 클래스/메서드/인터페이스 정보 추가 | 문서
- 2025-01-27 | Maintainer | 새로운 플레이어 UI 시스템 구현 완료 | 코드/문서
- 2025-01-27 | Maintainer | PlayerCharacterUIController 클래스 구현 - 리그 오브 레전드 스타일 UI | 코드/문서
- 2025-01-27 | Maintainer | BuffDebuffIcon 클래스 구현 - 개별 버프/디버프 아이콘 관리 | 코드/문서
- 2025-01-27 | Maintainer | PlayerCharacter 클래스에 새로운 UI 시스템 통합 | 코드/문서
- 2025-01-27 | Maintainer | 캐릭터별 리소스 시스템 구현 - 검/활/지팡이 타입별 표시 | 코드/문서
- 2025-01-27 | Maintainer | DOTween 애니메이션 시스템 통합 - 부드러운 UI 전환 | 코드/문서
- 2025-01-27 | Maintainer | 개발 문서 업데이트 - 새로운 UI 시스템 반영 | 문서
- 2025-01-27 | Maintainer | 실제 코드 기반 캐릭터 기본 속성 수정 (속도/공격력 제거, 가드/리소스/턴효과 추가) | 문서
