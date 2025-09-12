# AnimationSystem 개발 문서

## 📋 시스템 개요
AnimationSystem은 Unity 2D 게임의 모든 애니메이션을 통합 관리하는 시스템입니다. 캐릭터, 스킬카드, UI 등 다양한 요소의 애니메이션을 중앙화된 방식으로 제어합니다.

## 🏗️ 폴더 구조
```
AnimationSystem/
├── Manager/           # 애니메이션 매니저 (2개 파일)
├── Interface/         # 애니메이션 인터페이스 (15개 파일)
├── Data/             # 애니메이션 데이터 (8개 파일)
├── Animator/         # 애니메이션 구현체 (20개 파일)
│   ├── CharacterAnimation/    # 캐릭터 애니메이션 (8개 파일)
│   │   ├── DeathAnimation/    # 사망 애니메이션 (2개 파일)
│   │   └── SpawnAnimation/    # 등장 애니메이션 (2개 파일)
│   └── SkillCardAnimation/    # 스킬카드 애니메이션 (12개 파일)
│       ├── DragAnimation/     # 드래그 애니메이션 (2개 파일)
│       ├── DropAnimation/     # 드롭 애니메이션 (2개 파일)
│       ├── MoveAnimation/     # 이동 애니메이션 (2개 파일)
│       ├── MoveToCombatSlotAnimation/ # 전투 슬롯 이동 (2개 파일)
│       ├── SpawnAnimation/    # 등장 애니메이션 (2개 파일)
│       ├── UseAnimation/      # 사용 애니메이션 (2개 파일)
│       └── VanishAnimation/    # 소멸 애니메이션 (2개 파일)
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

### Data 폴더 (8개 파일)
- **CharacterAnimationEntry.cs**: 캐릭터 애니메이션 엔트리 데이터
- **CharacterAnimationSettings.cs**: 캐릭터 애니메이션 설정
- **EnemyCharacterAnimationDatabase.cs**: 적 캐릭터 애니메이션 데이터베이스
- **EnemySkillCardAnimationDatabase.cs**: 적 스킬카드 애니메이션 데이터베이스
- **PlayerCharacterAnimationDatabase.cs**: 플레이어 캐릭터 애니메이션 데이터베이스
- **PlayerSkillCardAnimationDatabase.cs**: 플레이어 스킬카드 애니메이션 데이터베이스
- **SkillCardAnimationEntry.cs**: 스킬카드 애니메이션 엔트리 데이터
- **SkillCardAnimationSettings.cs**: 스킬카드 애니메이션 설정

### Animator 폴더 (20개 파일)

#### CharacterAnimation 하위 폴더 (8개 파일)
- **DeathAnimation/**:
  - **CharacterDeathAnimation001.cs**: 캐릭터 사망 애니메이션 구현체(디폴트)
- **SpawnAnimation/**:
  - **CharacterSpawnAnimation001.cs**: 캐릭터 등장 애니메이션 구현체(디폴트)

#### SkillCardAnimation 하위 폴더 (12개 파일)
- **DragAnimation/**:
  - **SkillCardDragAnimation001.cs**: 스킬카드 드래그 애니메이션 구현체(디폴트)
- **DropAnimation/**:
  - **SkillCardDropAnimation001.cs**: 스킬카드 드롭 애니메이션 구현체(디폴트)
- **MoveAnimation/**:
  - **SkillCardMoveAnimation001.cs**: 스킬카드 이동 애니메이션 구현체(디폴트)
- **MoveToCombatSlotAnimation/**:
  - **SkillCardCombatSlotMoveAnimation001.cs**: 스킬카드 전투 슬롯 이동 애니메이션 구현체(디폴트)
- **SpawnAnimation/**:
  - **SkillCardSpawnAnimation001.cs**: 스킬카드 등장 애니메이션 구현체(디폴트)
- **UseAnimation/**:
  - **SkillCardUseAnimation001.cs**: 스킬카드 사용 애니메이션 구현체(디폴트)
- **VanishAnimation/**:
  - **SkillCardVanishAnimation001.cs**: 스킬카드 소멸 애니메이션 구현체(디폴트)

### Helper 폴더
- **AnimationHelper.cs**: 애니메이션 유틸리티 함수

### Controllers 폴더
- **CharacterAnimationController.cs**: 캐릭터 애니메이션 컨트롤러
- **SkillCardAnimationController.cs**: 스킬카드 애니메이션 컨트롤러

### Editor 경로(통합)
- 커스텀 인스펙터/드로어: `Assets/Script/UtilitySystem/Editor/AnimationSystem/` (전역 통합 위치)
- **AnimationDatabaseProEditor.cs**: 4종 DB 커스텀 인스펙터(전문 UI, 드롭다운, ReorderableList)

## 🎯 주요 기능

### 1. 통합 애니메이션 관리
- 모든 애니메이션을 중앙화된 방식으로 관리
- Facade 패턴을 통한 단순화된 인터페이스 제공

### 2. 데이터 기반 애니메이션(인스펙터 구동)
- ScriptableObject 기반 DB: 카드/캐릭터 별 엔트리 보유
- 각 엔트리의 `AnimationSettings`는 “스크립트 타입 문자열”만 보유(파라미터 제거)
- 타입 미지정 시 슬롯별 `*Animation001` 고정 사용(전역/폴백 개념 제거)

### 3. 타입별 애니메이션 지원
- 캐릭터 애니메이션 (플레이어/적)
- 스킬카드 애니메이션 (플레이어/적)
- UI 애니메이션 (다양한 UI 요소)

### 4. 커스텀 에디터 지원(전문 UI)
- 단일 에디터 `AnimationDatabaseProEditor`로 4종 DB 지원
- 슬롯별 인터페이스 필터링 드롭다운 제공(예: 드래그=ISkillCardDragAnimationScript)
- 드래그 애니메이션은 `start/end`를 스크립트로 전달하여 내부 분기

## 🔧 사용 방법

### 기본 사용법
```csharp
// AnimationFacade를 통한 캐릭터 애니메이션 실행
AnimationFacade.Instance.PlayCharacterAnimation(characterId, "spawn", target, onComplete, isEnemy);
AnimationFacade.Instance.PlayCharacterDeathAnimation(characterId, target, onComplete, isEnemy);

// 스킬카드 애니메이션 실행
AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "spawn", target);
AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "move", target, onComplete);

// ISkillCard 기반 애니메이션 실행
AnimationFacade.Instance.PlaySkillCardAnimation(skillCard, "use", target, onComplete);
```

### 주요 클래스 및 메서드

#### AnimationFacade 클래스
- **Instance**: 싱글톤 인스턴스
- **PlayCharacterAnimation()**: 캐릭터 애니메이션 실행
- **PlayCharacterDeathAnimation()**: 캐릭터 사망 애니메이션 실행
- **PlaySkillCardAnimation()**: 스킬카드 애니메이션 실행 (다중 오버로드)
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

#### 데이터 클래스
- **PlayerCharacterAnimationEntry**: 플레이어 캐릭터 애니메이션 엔트리
- **EnemyCharacterAnimationEntry**: 적 캐릭터 애니메이션 엔트리
- **CharacterAnimationSettings**: 스크립트 타입 문자열만 보유
- **SkillCardAnimationSettings**: 스크립트 타입 문자열만 보유(드래그 start/end는 내부에서 drag로 매핑)

### 애니메이션 설정 구조(간소화)
```csharp
// SkillCardAnimationSettings / CharacterAnimationSettings
// 인스펙터에서 선택되는 스크립트 타입 문자열만 직렬화 보유
[Serializable]
public class SkillCardAnimationSettings { [SerializeField] string animationScriptType; }
public class CharacterAnimationSettings { [SerializeField] string animationScriptType; }
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
 - 2025-09-12 | Maintainer | 인스펙터 구동 구조로 문서 전면 개정(전역/폴백 제거, *001 디폴트, 에디터 경로 통합, 드래그 start/end 매핑 명시) | 문서
