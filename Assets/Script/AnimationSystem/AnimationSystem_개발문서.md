# AnimationSystem 개발 문서

## 📋 시스템 개요
AnimationSystem은 Unity 2D 게임의 모든 애니메이션을 통합 관리하는 시스템입니다. 캐릭터, 스킬카드, UI 등 다양한 요소의 애니메이션을 중앙화된 방식으로 제어합니다. 플레이어와 적 스킬카드의 애니메이션을 하나의 통합 데이터베이스에서 관리하며, 타입별 필터링된 드롭다운을 통해 정확한 애니메이션 설정을 제공합니다.

### 최근 변경(요약)
- **Zenject DI 통합**: AnimationFacade가 의존성 주입으로 전환 완료
- **매개변수 순서 통일**: 모든 PlaySkillCardAnimation 호출이 `(ISkillCard, GameObject, string, Action)` 순서로 통일 완료
- **싱글톤 제거**: AnimationFacade에서 싱글톤 패턴 제거하고 DI 패턴으로 전환 완료
- **이벤트 구독 정리**: 제거된 메서드들의 이벤트 구독 정리 완료
- **컴파일 에러 해결**: 모든 AnimationFacade 관련 컴파일 에러 해결 완료

## 🏗️ 폴더 구조
```
AnimationSystem/
├── Manager/           # 애니메이션 매니저 (2개 파일)
├── Interface/         # 애니메이션 인터페이스 (15개 파일)
├── Data/             # 애니메이션 데이터 (4개 파일)
├── Animator/         # 애니메이션 구현체 (14개 파일)
│   ├── CharacterAnimation/    # 캐릭터 애니메이션 (4개 파일)
│   │   ├── DeathAnimation/    # 사망 애니메이션 (1개 파일)
│   │   └── SpawnAnimation/    # 등장 애니메이션 (1개 파일)
│   └── SkillCardAnimation/    # 스킬카드 애니메이션 (7개 파일)
│       ├── DragAnimation/     # 드래그 애니메이션 (1개 파일)
│       ├── DropAnimation/     # 드롭 애니메이션 (1개 파일)
│       ├── MoveAnimation/     # 이동 애니메이션 (1개 파일)
│       ├── MoveToCombatSlotAnimation/ # 전투 슬롯 이동 (1개 파일)
│       ├── SpawnAnimation/    # 등장 애니메이션 (1개 파일)
│       ├── UseAnimation/      # 사용 애니메이션 (1개 파일)
│       └── VanishAnimation/    # 소멸 애니메이션 (1개 파일)
├── Editor/           # 에디터 도구 (3개 파일)
├── Helper/           # 애니메이션 헬퍼 (1개 파일)
└── Controllers/      # 애니메이션 컨트롤러 (2개 파일)
```

## 📁 주요 컴포넌트

### Manager 폴더
- **AnimationFacade.cs**: 애니메이션 시스템의 통합 인터페이스
- **AnimationSystemInitializer.cs**: 애니메이션 시스템 초기화

### Interface 폴더 (15개 파일)
- **IAnimationScript.cs**: 애니메이션 스크립트 기본 인터페이스
- **ICharacterCombatSlotMoveAnimationScript.cs**: 캐릭터 전투 슬롯 이동 애니메이션
- **ICharacterDeathAnimationScript.cs**: 캐릭터 사망 애니메이션 인터페이스
- **ICharacterMoveAnimationScript.cs**: 캐릭터 이동 애니메이션 인터페이스
- **ICharacterSpawnAnimationScript.cs**: 캐릭터 등장 애니메이션 인터페이스
- **ISkillCardCombatSlotMoveAnimationScript.cs**: 스킬카드 전투 슬롯 이동 애니메이션
- **ISkillCardDeathAnimationScript.cs**: 스킬카드 사망 애니메이션 인터페이스
- **ISkillCardDragAnimationScript.cs**: 스킬카드 드래그 애니메이션 인터페이스
- **ISkillCardDropAnimationScript.cs**: 스킬카드 드롭 애니메이션 인터페이스
- **ISkillCardMoveAnimationScript.cs**: 스킬카드 이동 애니메이션 인터페이스
- **ISkillCardSpawnAnimationScript.cs**: 스킬카드 등장 애니메이션 인터페이스
- **ISkillCardUseAnimationScript.cs**: 스킬카드 사용 애니메이션 인터페이스
- **ISkillCardVanishAnimationScript.cs**: 스킬카드 소멸 애니메이션 인터페이스

### Data 폴더 (4개 파일)
- **CharacterAnimationEntry.cs**: 캐릭터 애니메이션 엔트리 데이터
- **CharacterAnimationSettings.cs**: 캐릭터 애니메이션 설정
- **UnifiedSkillCardAnimationDatabase.cs**: 통합 스킬카드 애니메이션 데이터베이스 (플레이어/적 통합)
- **UnifiedSkillCardAnimationEntry.cs**: 통합 스킬카드 애니메이션 엔트리 데이터
- **SkillCardAnimationSettings.cs**: 스킬카드 애니메이션 설정 (타입별 필터링 지원)

### Animator 폴더 (14개 파일)

#### CharacterAnimation 하위 폴더 (4개 파일)
- **DeathAnimation/**:
  - **DefaultCharacterDeathAnimation.cs**: 기본 캐릭터 사망 애니메이션
- **SpawnAnimation/**:
  - **DefaultCharacterSpawnAnimation.cs**: 기본 캐릭터 등장 애니메이션

#### SkillCardAnimation 하위 폴더 (7개 파일)
- **DragAnimation/**:
  - **DefaultSkillCardDragAnimation.cs**: 기본 스킬카드 드래그 애니메이션
- **DropAnimation/**:
  - **DefaultSkillCardDropAnimation.cs**: 기본 스킬카드 드롭 애니메이션
- **MoveAnimation/**:
  - **DefaultSkillCardMoveAnimation.cs**: 기본 스킬카드 이동 애니메이션
- **MoveToCombatSlotAnimation/**:
  - **DefaultSkillCardCombatSlotMoveAnimation.cs**: 기본 스킬카드 전투 슬롯 이동 애니메이션
- **SpawnAnimation/**:
  - **DefaultSkillCardSpawnAnimation.cs**: 기본 스킬카드 등장 애니메이션
- **UseAnimation/**:
  - **DefaultSkillCardUseAnimation.cs**: 기본 스킬카드 사용 애니메이션
- **VanishAnimation/**:
  - **DefaultSkillCardVanishAnimation.cs**: 기본 스킬카드 소멸 애니메이션

### Editor 폴더 (3개 파일)
- **UnifiedSkillCardAnimationDatabaseEditor.cs**: 통합 스킬카드 애니메이션 데이터베이스 커스텀 에디터
- **UnifiedSkillCardAnimationEntryEditor.cs**: 통합 스킬카드 애니메이션 엔트리 PropertyDrawer
- **SkillCardAnimationSettingsDrawer.cs**: 스킬카드 애니메이션 설정 PropertyDrawer (타입별 필터링)
- **AnimationDatabaseMigrator.cs**: 애니메이션 데이터베이스 마이그레이션 도구

### Helper 폴더
- **AnimationHelper.cs**: 애니메이션 유틸리티 함수

### Controllers 폴더
- **CharacterAnimationController.cs**: 캐릭터 애니메이션 컨트롤러
- **SkillCardAnimationController.cs**: 스킬카드 애니메이션 컨트롤러


## 🎯 주요 기능

### 1. 통합 애니메이션 관리
- 모든 애니메이션을 중앙화된 방식으로 관리
- Facade 패턴을 통한 단순화된 인터페이스 제공
- 플레이어와 적 스킬카드 애니메이션을 하나의 데이터베이스에서 통합 관리

### 2. 데이터 기반 애니메이션
- ScriptableObject를 활용한 데이터 기반 설계
- 런타임에서 애니메이션 설정 변경 가능
- 통합된 데이터베이스로 중복 제거 및 관리 효율성 향상

### 3. 타입별 필터링 시스템
- 각 애니메이션 타입별로 해당하는 스크립트만 표시
- 실수 방지를 위한 정확한 드롭다운 옵션 제공
- None 옵션 제거로 명확한 애니메이션 설정 보장

### 4. 타입별 애니메이션 지원
- 캐릭터 애니메이션 (플레이어/적)
- 스킬카드 애니메이션 (플레이어/적 통합)
- UI 애니메이션 (다양한 UI 요소)

### 5. 커스텀 에디터 지원
- 통합 데이터베이스 전용 에디터 제공
- 그룹화된 UI로 공간 효율성 향상
- 타입별 필터링된 드롭다운으로 개발자 편의성 증대

## 🔧 사용 방법

### 기본 사용법
```csharp
// 통합된 스킬카드 애니메이션 실행 (ISkillCard 기반)
AnimationFacade.Instance.PlaySkillCardAnimation(skillCard, target, "spawn", onComplete);
AnimationFacade.Instance.PlaySkillCardAnimation(skillCard, target, "use", onComplete);

// 드래그/드롭 애니메이션 (플레이어 전용)
AnimationFacade.Instance.PlaySkillCardDragStartAnimation(skillCard, target, onComplete);
AnimationFacade.Instance.PlaySkillCardDropAnimation(skillCard, target, onComplete);

// 캐릭터 애니메이션 실행
AnimationFacade.Instance.PlayCharacterAnimation(characterId, "spawn", target, onComplete, isEnemy);
AnimationFacade.Instance.PlayCharacterDeathAnimation(characterId, target, onComplete, isEnemy);
```

### 주요 클래스 및 메서드

#### AnimationFacade 클래스
- **Instance**: 싱글톤 인스턴스
- **PlayCharacterAnimation()**: 캐릭터 애니메이션 실행
- **PlayCharacterDeathAnimation()**: 캐릭터 사망 애니메이션 실행
- **PlaySkillCardAnimation()**: 통합 스킬카드 애니메이션 실행 (ISkillCard 기반)
- **PlaySkillCardDragStartAnimation()**: 스킬카드 드래그 시작 애니메이션 (플레이어 전용)
- **PlaySkillCardDropAnimation()**: 스킬카드 드롭 애니메이션 (플레이어 전용)
- **LoadAllData()**: 모든 애니메이션 데이터 로드

#### CharacterAnimationController 클래스
- **PlayAnimation()**: 애니메이션 타입별 실행 (spawn, death, damage, heal)
- **GetSettings()**: 현재 애니메이션 설정 반환
- **UpdateSettings()**: 애니메이션 설정 업데이트
- **UpdateSpawnSettings()**: 등장 애니메이션 설정 업데이트
- **UpdateDeathSettings()**: 사망 애니메이션 설정 업데이트
- **UpdateDamageSettings()**: 피해 애니메이션 설정 업데이트

#### SkillCardAnimationController 클래스
- **PlaySpawnAnimation()**: 스킬카드 등장 애니메이션
- **PlayMoveAnimation()**: 스킬카드 이동 애니메이션
- **PlayUseAnimation()**: 스킬카드 사용 애니메이션

#### UnifiedSkillCardAnimationDatabase 클래스
- **SkillCardAnimations**: 통합 스킬카드 애니메이션 목록
- **FindEntryByCardId()**: 카드 ID로 애니메이션 엔트리 검색
- **FindEntryByCardName()**: 카드 이름으로 애니메이션 엔트리 검색

#### UnifiedSkillCardAnimationEntry 클래스
- **SkillCardDefinition**: 스킬카드 정의 참조
- **OwnerPolicy**: 소유자 정책 (Shared, Player, Enemy)
- **CanUseAnimation()**: 소유자 정책에 따른 애니메이션 사용 가능 여부 확인
- **GetSettingsByType()**: 애니메이션 타입별 설정 반환

#### 데이터 클래스
- **PlayerCharacterAnimationEntry**: 플레이어 캐릭터 애니메이션 엔트리
- **EnemyCharacterAnimationEntry**: 적 캐릭터 애니메이션 엔트리
- **CharacterAnimationSettings**: 캐릭터 애니메이션 설정 (spawn, death, damage, heal)
- **SkillCardAnimationSettings**: 스킬카드 애니메이션 설정 (spawn, move, use)

### 애니메이션 설정 구조
```csharp
// CharacterAnimationSettings 주요 속성
public class AnimationSettings
{
    // 등장 애니메이션
    public float spawnDuration = 1.0f;
    public Vector3 spawnStartScale = Vector3.zero;
    public Vector3 spawnEndScale = Vector3.one;
    public Ease spawnEase = Ease.OutBack;
    public bool useSpawnGlow = true;
    public Color spawnGlowColor = Color.blue;
    
    // 사망 애니메이션
    public float deathDuration = 1.5f;
    public Ease deathEase = Ease.InBack;
    public bool useDeathFade = true;
    public Vector3 deathEndScale = Vector3.zero;
    
    // 피해 애니메이션
    public float damageDuration = 0.3f;
    public bool useDamageShake = true;
    public float damageShakeStrength = 0.1f;
    
    // 치유 애니메이션
    public float healDuration = 0.8f;
    public Color healGlowColor = Color.green;
}
```

### 커스텀 애니메이션 추가
1. 해당 타입의 Animator 클래스 상속
2. AnimationDatabase에 애니메이션 데이터 등록
3. AnimationSettings에서 설정 구성

## 📝 변경 기록(Delta)
- 형식: `YYYY-MM-DD | 작성자 | 변경 요약 | 영향도(코드/씬/문서)`

- 2025-01-27 | Maintainer | AnimationSystem 개발 문서 초기 작성 | 문서
- 2025-01-27 | Maintainer | 실제 폴더 구조 반영 및 파일 수 정정 | 문서
- 2025-01-27 | Maintainer | 실제 코드 분석 기반 구체적 함수/변수/클래스 정보 추가 | 문서
- 2025-01-27 | Maintainer | 통합 애니메이션 시스템으로 마이그레이션 완료 | 코드/문서
- 2025-01-27 | Maintainer | 플레이어/적 스킬카드 애니메이션 통합 데이터베이스 구현 | 코드/문서
- 2025-01-27 | Maintainer | 타입별 필터링된 드롭다운 시스템 구현 | 코드/문서
- 2025-01-27 | Maintainer | 그룹화된 커스텀 에디터 UI 구현 | 코드/문서
- 2025-01-27 | Maintainer | 001 시리즈 제거 및 None 옵션 제거 | 코드/문서
