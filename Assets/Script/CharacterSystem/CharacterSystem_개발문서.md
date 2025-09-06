# CharacterSystem 개발 문서

## 📋 시스템 개요
CharacterSystem은 게임의 모든 캐릭터(플레이어, 적)를 관리하는 시스템입니다. 캐릭터의 기본 속성, 상태, 행동을 통합적으로 관리합니다.

## 🏗️ 폴더 구조
```
CharacterSystem/
├── Core/             # 캐릭터 핵심 로직 (4개 파일)
├── Data/             # 캐릭터 데이터 (2개 파일)
├── Interface/        # 캐릭터 인터페이스 (6개 파일)
├── Manager/          # 캐릭터 매니저 (2개 파일)
├── Slot/             # 캐릭터 슬롯 (1개 파일)
└── UI/               # 캐릭터 UI (2개 파일)
```

## 📁 주요 컴포넌트

### Core 폴더 (4개 파일)
- **CharacterBase.cs**: 모든 캐릭터의 기본 클래스
- **PlayerCharacter.cs**: 플레이어 캐릭터 구현
- **EnemyCharacter.cs**: 적 캐릭터 구현
- **CharacterState.cs**: 캐릭터 상태 관리

### Data 폴더 (2개 파일)
- **PlayerCharacterData.cs**: 플레이어 캐릭터 데이터 (ScriptableObject)
- **EnemyCharacterData.cs**: 적 캐릭터 데이터 (ScriptableObject)

### Interface 폴더 (6개 파일)
- **ICharacter.cs**: 캐릭터 기본 인터페이스
- **ICharacterData.cs**: 캐릭터 데이터 인터페이스
- **ICharacterState.cs**: 캐릭터 상태 인터페이스
- **ICharacterAction.cs**: 캐릭터 행동 인터페이스
- **ICharacterEffect.cs**: 캐릭터 효과 인터페이스
- **ICharacterUI.cs**: 캐릭터 UI 인터페이스

### Manager 폴더 (2개 파일)
- **PlayerManager.cs**: 플레이어 캐릭터 매니저
- **EnemyManager.cs**: 적 캐릭터 매니저

### Slot 폴더 (1개 파일)
- **CharacterSlotPosition.cs**: 캐릭터 슬롯 위치 관리

### UI 폴더 (2개 파일)
- **CharacterSlotUI.cs**: 캐릭터 슬롯 UI
- **CharacterUIController.cs**: 캐릭터 UI 컨트롤러

## 🎯 주요 기능

### 1. 캐릭터 기본 속성
- **체력 (Health)**: 캐릭터의 생명력
- **방어력 (Guard)**: 데미지 감소
- **공격력 (Attack)**: 기본 공격력
- **속도 (Speed)**: 행동 순서 결정

### 2. 상태 관리
- **생존 상태**: 살아있음/죽음
- **효과 상태**: 버프/디버프 효과
- **턴별 효과**: 매 턴마다 적용되는 효과

### 3. 행동 시스템
- **기본 공격**: 일반적인 공격 행동
- **스킬 사용**: 특수 능력 사용
- **방어**: 데미지 감소 행동

### 4. 데이터 기반 설계
- **ScriptableObject**: 캐릭터 데이터를 에셋으로 관리
- **런타임 인스턴스**: 게임 중 동적 생성/수정

## 🔧 사용 방법

### 기본 사용법
```csharp
// 플레이어 캐릭터 생성
PlayerCharacter player = new PlayerCharacter(playerData);

// 적 캐릭터 생성
EnemyCharacter enemy = new EnemyCharacter(enemyData);

// 캐릭터 상태 확인
if (player.IsAlive)
{
    // 공격 실행
    player.Attack(enemy);
}

// 효과 적용
player.ApplyEffect(new DamageEffect(10));
```

### 매니저 사용법
```csharp
// 플레이어 매니저
PlayerManager.Instance.RegisterPlayer(player);
PlayerManager.Instance.GetPlayerById(playerId);

// 적 매니저
EnemyManager.Instance.SpawnEnemy(enemyData);
EnemyManager.Instance.GetAllEnemies();
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
- **ICharacterAction**: 행동 관련 기능
- **ICharacterEffect**: 효과 관련 기능
- **ICharacterUI**: UI 관련 기능

### 3. 매니저 패턴
- **PlayerManager**: 플레이어 캐릭터 관리
- **EnemyManager**: 적 캐릭터 관리


## 📊 시스템 평가
- **아키텍처**: 8/10 (잘 구조화된 상속 계층)
- **확장성**: 8/10 (인터페이스 기반 확장 가능)
- **성능**: 7/10 (최적화 여지 있음)
- **유지보수성**: 8/10 (명확한 책임 분리)
- **전체 점수**: 7.8/10

