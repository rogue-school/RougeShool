# UtilitySystem 개발 문서

## 📋 시스템 개요
UtilitySystem은 게임의 유틸리티 기능들을 관리하는 시스템입니다. 게임 플로우, DontDestroyOnLoad 관리, 드롭 핸들러 주입 등 다양한 유틸리티 기능을 제공합니다.

## 🏗️ 폴더 구조 (리팩토링 후)
```
UtilitySystem/
├── GameFlow/         # 게임 플로우 (3개 파일)
└── 루트/             # 루트 유틸리티 (2개 파일) - 간소화됨
```

## 📁 주요 컴포넌트 (리팩토링 후)

### GameFlow 폴더 (3개 파일)
- **GameContext.cs**: 게임 컨텍스트 관리
- **IGameContext.cs**: 게임 컨텍스트 인터페이스
- **ISceneLoader.cs**: 씬 로더 인터페이스

### 루트 폴더 (2개 파일) - 간소화됨
- **DontDestroyOnLoadContainer.cs**: DontDestroyOnLoad 오브젝트 관리
- **DropHandlerInjector.cs**: 드롭 핸들러 주입
- ~~**CameraResolutionFixer.cs**: 카메라 해상도 수정~~ (제거됨)

## 🎯 주요 기능

### 1. 게임 플로우 관리
- **게임 컨텍스트**: 게임의 전역 상태 관리
- **씬 로딩**: 씬 전환 및 로딩 관리
- **상태 전환**: 게임 상태 간 전환

### 2. DontDestroyOnLoad 관리
- **오브젝트 등록**: DontDestroyOnLoad로 설정할 오브젝트 등록
- **자동 관리**: 씬 전환 시에도 유지되는 오브젝트 관리
- **생명주기 관리**: 오브젝트의 생성/소멸 관리

### 3. 드롭 핸들러 주입
- **자동 주입**: 드롭 핸들러를 자동으로 주입
- **의존성 관리**: 드롭 핸들러 간 의존성 관리
- **초기화**: 드롭 핸들러의 자동 초기화

### 4. 카메라 해상도 수정 (제거됨)
- ~~**해상도 조정**: 다양한 해상도에 맞춰 카메라 설정 조정~~
- ~~**비율 유지**: 화면 비율 유지~~
- ~~**자동 적용**: 게임 시작 시 자동 적용~~
- **제거 사유**: 불필요한 기능으로 판단되어 제거됨

## 🔧 사용 방법

### 기본 사용법 (리팩토링 후)
```csharp
// 게임 컨텍스트 설정
GameContext.Instance.SetCurrentScene("MainMenu");
GameContext.Instance.SetGameState(GameState.MainMenu);

// DontDestroyOnLoad 오브젝트 등록
DontDestroyOnLoadContainer.Instance.RegisterObject(gameObject);

// 드롭 핸들러 주입
DropHandlerInjector.Instance.InjectHandlers();

// 카메라 해상도 수정 (제거됨)
// CameraResolutionFixer.Instance.FixResolution(); // 더 이상 사용하지 않음
```

### 게임 플로우 관리
```csharp
// 씬 로딩
ISceneLoader sceneLoader = GameContext.Instance.SceneLoader;
sceneLoader.LoadScene("CombatScene");

// 게임 상태 확인
if (GameContext.Instance.CurrentState == GameState.Combat)
{
    // 전투 상태 처리
}
```

## 🏗️ 아키텍처 패턴

### 1. 싱글톤 패턴 (Singleton Pattern)
- **GameContext**: 게임 컨텍스트 싱글톤
- **DontDestroyOnLoadContainer**: DontDestroyOnLoad 관리 싱글톤
- **DropHandlerInjector**: 드롭 핸들러 주입 싱글톤
- **CameraResolutionFixer**: 카메라 해상도 수정 싱글톤

### 2. 팩토리 패턴 (Factory Pattern)
- **씬 로더 생성**: 다양한 씬 로더 구현체 생성
- **드롭 핸들러 생성**: 다양한 드롭 핸들러 생성

### 3. 의존성 주입 패턴 (Dependency Injection)
- **DropHandlerInjector**: 드롭 핸들러 자동 주입
- **인터페이스 기반**: 인터페이스를 통한 느슨한 결합

### 4. 컨텍스트 패턴 (Context Pattern)
- **GameContext**: 게임의 전역 상태 관리
- **상태 전환**: 컨텍스트를 통한 상태 전환



