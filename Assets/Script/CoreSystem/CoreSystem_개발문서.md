# CoreSystem 개발 문서

## 📋 시스템 개요
CoreSystem은 게임의 핵심 시스템들을 관리하는 중앙 집중식 시스템입니다. 모든 씬에서 공유되는 전역 시스템들을 통합적으로 관리하며, 게임의 기본 기능을 제공합니다.

## 🏗️ 폴더 구조
```
CoreSystem/
├── Manager/          # 코어 매니저 (4개 파일)
├── Audio/           # 오디오 관리 (1개 파일)
├── Save/            # 저장 관리 (1개 파일)
├── Utility/         # 코어 유틸리티 (3개 파일)
├── UI/              # 코어 UI (4개 파일)
├── Interface/       # 코어 인터페이스 (2개 파일)
└── Animation/      # 애니메이션 관리 (2개 파일)
```

## 📁 주요 컴포넌트

### Manager 폴더 (4개 파일)
- **CoreSystemInitializer.cs**: 모든 코어 시스템 초기화
- **SceneTransitionManager.cs**: 씬 전환 관리
- **GameStateManager.cs**: 게임 상태 관리
- **SystemManager.cs**: 시스템 통합 관리

### Audio 폴더 (1개 파일)
- **AudioManager.cs**: 오디오 시스템 관리 (AudioSystem으로 이동 필요)

### Save 폴더 (1개 파일)
- **SaveManager.cs**: 저장 시스템 관리 (SaveSystem으로 이동 필요)

### Utility 폴더 (3개 파일)
- **CoroutineRunner.cs**: 코루틴 실행 관리
- **GameLogger.cs**: 게임 로깅 시스템
- **ICoroutineRunner.cs**: 코루틴 러너 인터페이스

### UI 폴더 (4개 파일)
- **LoadingScreenController.cs**: 로딩 화면 컨트롤러
- **SettingsManager.cs**: 설정 관리
- **SettingsPanelController.cs**: 설정 패널 컨트롤러
- **TransitionEffectController.cs**: 전환 효과 컨트롤러

### Interface 폴더 (2개 파일)
- **ICoreSystemInitializable.cs**: 코어 시스템 초기화 인터페이스
- **IPlayerCharacterSelectionManager.cs**: 플레이어 캐릭터 선택 관리 인터페이스

### Animation 폴더 (2개 파일)
- **AnimationDatabaseManager.cs**: 애니메이션 데이터베이스 관리
- **AnimationManager.cs**: 애니메이션 시스템 관리

## 🎯 주요 기능

### 1. 시스템 초기화
- **자동 초기화**: 모든 코어 시스템의 자동 초기화
- **초기화 순서**: 의존성을 고려한 초기화 순서 관리
- **초기화 상태**: 초기화 완료 상태 추적

### 2. 씬 전환 관리
- **씬 로딩**: 씬 로딩 및 전환 처리
- **로딩 화면**: 로딩 중 화면 표시
- **전환 효과**: 씬 전환 시 효과 처리

### 3. 게임 상태 관리
- **전역 상태**: 게임 전체 상태 관리
- **상태 전환**: 게임 상태 간 전환
- **상태 저장**: 게임 상태 저장/로드

### 4. 로깅 시스템
- **카테고리별 로깅**: 시스템별 로그 분류
- **로그 레벨**: 에러, 경고, 정보 등 레벨별 로깅
- **조건부 컴파일**: 릴리즈 빌드에서 로그 제거

### 5. 코루틴 관리
- **중앙화된 실행**: 모든 코루틴을 중앙에서 관리
- **생명주기 관리**: 코루틴의 생성/소멸 관리
- **에러 처리**: 코루틴 실행 중 에러 처리

## 🔧 사용 방법

### 기본 사용법
```csharp
// 코어 시스템 초기화
CoreSystemInitializer.Instance.InitializeAllSystems();

// 씬 전환
SceneTransitionManager.Instance.LoadScene("CombatScene");

// 로깅
GameLogger.Info("시스템 초기화 완료");
GameLogger.Warning("경고 메시지");
GameLogger.Error("에러 발생");

// 코루틴 실행
CoroutineRunner.Instance.StartCoroutine(MyCoroutine());
```

### 시스템 초기화
```csharp
// ICoreSystemInitializable 구현
public class MySystem : MonoBehaviour, ICoreSystemInitializable
{
    public void Initialize()
    {
        // 초기화 로직
    }
}
```

## 🏗️ 아키텍처 패턴

### 1. 싱글톤 패턴 (Singleton Pattern)
- **CoreSystemInitializer**: 시스템 초기화 싱글톤
- **SceneTransitionManager**: 씬 전환 관리 싱글톤
- **GameLogger**: 로깅 시스템 싱글톤

### 2. 팩토리 패턴 (Factory Pattern)
- **시스템 생성**: 각 시스템의 생성 및 초기화
- **씬 생성**: 씬 객체 생성 및 관리

### 3. 옵저버 패턴 (Observer Pattern)
- **이벤트 시스템**: 시스템 간 이벤트 통신
- **상태 변경**: 게임 상태 변경 알림
- **초기화 완료**: 초기화 완료 알림

### 4. 파사드 패턴 (Facade Pattern)
- **CoreSystemInitializer**: 복잡한 초기화 과정을 단순화
- **SceneTransitionManager**: 씬 전환 과정을 단순화


## 📊 시스템 평가
- **아키텍처**: 8/10 (잘 구조화된 중앙 집중식 설계)
- **확장성**: 7/10 (새로운 시스템 추가 가능)
- **성능**: 7/10 (최적화 여지 있음)
- **유지보수성**: 8/10 (명확한 책임 분리)
- **전체 점수**: 7.5/10

