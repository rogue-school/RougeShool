# AnimationSystem 개발 문서

## 📋 시스템 개요
AnimationSystem은 Unity 2D 게임의 모든 애니메이션을 통합 관리하는 시스템입니다. 캐릭터, 스킬카드, UI 등 다양한 요소의 애니메이션을 중앙화된 방식으로 제어합니다.

## 🏗️ 폴더 구조
```
AnimationSystem/
├── Manager/           # 애니메이션 매니저 (2개 파일)
├── Interface/         # 애니메이션 인터페이스 (12개 파일)
├── Data/             # 애니메이션 데이터 (8개 파일)
├── Animator/         # 애니메이션 구현체 (15개 파일)
├── Helper/           # 애니메이션 헬퍼 (1개 파일)
├── Controllers/      # 애니메이션 컨트롤러 (2개 파일)
└── Editor/           # 커스텀 에디터 (5개 파일)
```

## 📁 주요 컴포넌트

### Manager 폴더
- **AnimationFacade.cs**: 애니메이션 시스템의 통합 인터페이스
- **AnimationSystemInitializer.cs**: 애니메이션 시스템 초기화

### Interface 폴더 (12개 파일)
- **IAnimationDatabase.cs**: 애니메이션 데이터베이스 인터페이스
- **IAnimationManager.cs**: 애니메이션 매니저 인터페이스
- **IAnimationScript.cs**: 애니메이션 스크립트 인터페이스
- **IAnimationSettings.cs**: 애니메이션 설정 인터페이스
- **IAnimationTrigger.cs**: 애니메이션 트리거 인터페이스
- **ICharacterAnimationScript.cs**: 캐릭터 애니메이션 스크립트 인터페이스
- **ISkillCardAnimationScript.cs**: 스킬카드 애니메이션 스크립트 인터페이스
- **IUIAnimationScript.cs**: UI 애니메이션 스크립트 인터페이스
- **IAnimationEvent.cs**: 애니메이션 이벤트 인터페이스
- **IAnimationCallback.cs**: 애니메이션 콜백 인터페이스
- **IAnimationState.cs**: 애니메이션 상태 인터페이스
- **IAnimationTransition.cs**: 애니메이션 전환 인터페이스

### Data 폴더 (8개 파일)
- **AnimationDatabase.cs**: 애니메이션 데이터베이스 기본 클래스
- **CharacterAnimationDatabase.cs**: 캐릭터 애니메이션 데이터베이스
- **SkillCardAnimationDatabase.cs**: 스킬카드 애니메이션 데이터베이스
- **UIAnimationDatabase.cs**: UI 애니메이션 데이터베이스
- **AnimationSettings.cs**: 애니메이션 설정
- **SkillCardAnimationSettings.cs**: 스킬카드 애니메이션 설정
- **CharacterAnimationSettings.cs**: 캐릭터 애니메이션 설정
- **UIAnimationSettings.cs**: UI 애니메이션 설정

### Animator 폴더 (15개 파일)
- **PlayerCharacterAnimator.cs**: 플레이어 캐릭터 애니메이터
- **EnemyCharacterAnimator.cs**: 적 캐릭터 애니메이터
- **PlayerSkillCardAnimator.cs**: 플레이어 스킬카드 애니메이터
- **EnemySkillCardAnimator.cs**: 적 스킬카드 애니메이터
- **UIAnimator.cs**: UI 애니메이터
- **ButtonAnimator.cs**: 버튼 애니메이터
- **PanelAnimator.cs**: 패널 애니메이터
- **TextAnimator.cs**: 텍스트 애니메이터
- **ImageAnimator.cs**: 이미지 애니메이터
- **SliderAnimator.cs**: 슬라이더 애니메이터
- **ScrollViewAnimator.cs**: 스크롤뷰 애니메이터
- **ToggleAnimator.cs**: 토글 애니메이터
- **DropdownAnimator.cs**: 드롭다운 애니메이터
- **InputFieldAnimator.cs**: 입력필드 애니메이터
- **CanvasGroupAnimator.cs**: 캔버스그룹 애니메이터

### Helper 폴더
- **AnimationHelper.cs**: 애니메이션 유틸리티 함수

### Controllers 폴더
- **CharacterAnimationController.cs**: 캐릭터 애니메이션 컨트롤러
- **SkillCardAnimationController.cs**: 스킬카드 애니메이션 컨트롤러

### Editor 폴더 (5개 파일)
- **AnimationDatabaseAssetFixer.cs**: 애니메이션 데이터베이스 에셋 수정기
- **EnemyCharacterAnimationDatabaseEditor.cs**: 적 캐릭터 애니메이션 데이터베이스 에디터
- **EnemySkillCardAnimationDatabaseEditor.cs**: 적 스킬카드 애니메이션 데이터베이스 에디터
- **PlayerCharacterAnimationDatabaseEditor.cs**: 플레이어 캐릭터 애니메이션 데이터베이스 에디터
- **PlayerSkillCardAnimationDatabaseEditor.cs**: 플레이어 스킬카드 애니메이션 데이터베이스 에디터

## 🎯 주요 기능

### 1. 통합 애니메이션 관리
- 모든 애니메이션을 중앙화된 방식으로 관리
- Facade 패턴을 통한 단순화된 인터페이스 제공

### 2. 데이터 기반 애니메이션
- ScriptableObject를 활용한 데이터 기반 설계
- 런타임에서 애니메이션 설정 변경 가능

### 3. 타입별 애니메이션 지원
- 캐릭터 애니메이션 (플레이어/적)
- 스킬카드 애니메이션 (플레이어/적)
- UI 애니메이션 (다양한 UI 요소)

### 4. 커스텀 에디터 지원
- 각 데이터베이스별 전용 에디터 제공
- 개발자 친화적인 인스펙터 인터페이스

## 🔧 사용 방법

### 기본 사용법
```csharp
// AnimationFacade를 통한 애니메이션 실행
AnimationFacade.Instance.PlayCharacterAnimation(characterId, "Attack");
AnimationFacade.Instance.PlaySkillCardAnimation(cardId, "Draw");
AnimationFacade.Instance.PlayUIAnimation(uiElement, "FadeIn");
```

### 커스텀 애니메이션 추가
1. 해당 타입의 Animator 클래스 상속
2. AnimationDatabase에 애니메이션 데이터 등록
3. AnimationSettings에서 설정 구성


