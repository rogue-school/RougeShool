# UtilitySystem 개발 문서

## 📋 시스템 개요
UtilitySystem은 게임의 유틸리티 기능들을 관리하는 시스템입니다. 게임 플로우, DontDestroyOnLoad 관리, 드롭 핸들러 주입 등 다양한 유틸리티 기능을 제공합니다. 싱글톤 패턴을 통한 전역 접근, 컨텍스트 패턴을 통한 상태 관리, 의존성 주입 패턴을 통한 느슨한 결합을 지원합니다.

### 최근 변경(요약)
- **AnimationSystem 의존성 완전 제거**: 모든 AnimationSystem 관련 코드 제거 완료
- **임시 애니메이션 비활성화**: 애니메이션 호출 부분을 Debug.Log로 대체하여 게임 로직 정상 동작
- **싱글톤 패턴 완료**: DontDestroyOnLoadContainer, DropHandlerInjector의 전역 접근 완료
- **컨텍스트 패턴 완료**: GameContext를 통한 플레이어 캐릭터 선택 상태 관리 완료
- **의존성 주입 패턴 완료**: DropHandlerInjector를 통한 드롭 핸들러 자동 주입 완료
- **게임 플로우 관리 완료**: 씬 전환 및 게임 상태 관리 완료
- **자동 관리 완료**: 오브젝트의 생성/소멸 자동 관리 완료
- **Zenject DI 통합 완료**: 모든 UtilitySystem 컴포넌트가 의존성 주입으로 전환 완료
- **컴파일 에러 해결**: 모든 UtilitySystem 관련 컴파일 에러 해결 완료

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

### 4. 디자인 패턴
- **싱글톤 패턴**: DontDestroyOnLoadContainer, DropHandlerInjector의 전역 접근
- **컨텍스트 패턴**: GameContext를 통한 플레이어 캐릭터 선택 상태 관리
- **의존성 주입 패턴**: DropHandlerInjector를 통한 드롭 핸들러 자동 주입
- **컨테이너 패턴**: DontDestroyOnLoadContainer를 통한 오브젝트 생명주기 관리

## 🔧 사용 방법

### 기본 사용법
```csharp
// GameContext를 통한 플레이어 캐릭터 관리
GameContext gameContext = new GameContext();
PlayerCharacterData characterData = Resources.Load<PlayerCharacterData>("Characters/SwordCharacter");
gameContext.SetSelectedCharacter(characterData);

// DontDestroyOnLoadContainer를 통한 오브젝트 관리
DontDestroyOnLoadContainer container = FindObjectOfType<DontDestroyOnLoadContainer>();
container.AddObject(gameObject);
container.RemoveObject(gameObject);
int objectCount = container.GetObjectCount();
bool isEmpty = container.IsEmpty();

// DropHandlerInjector를 통한 드롭 핸들러 주입
ICombatSlotRegistry slotRegistry = FindObjectOfType<CombatSlotRegistry>();
CardDropService dropService = FindObjectOfType<CardDropService>();
ICombatFlowCoordinator flowCoordinator = FindObjectOfType<CombatFlowCoordinator>();
DropHandlerInjector.InjectToAllCombatSlots(slotRegistry, dropService, flowCoordinator);
```

## 📊 주요 클래스 및 메서드

### GameContext 클래스
- **SetSelectedCharacter(PlayerCharacterData data)**: 선택된 플레이어 캐릭터 설정
- **SelectedCharacter**: 현재 선택된 플레이어 캐릭터 데이터 (프로퍼티)

### DontDestroyOnLoadContainer 클래스
- **AddObject(GameObject newObject)**: 새로운 오브젝트를 컨테이너에 추가
- **RemoveObject(GameObject objectToRemove)**: 특정 오브젝트를 컨테이너에서 제거
- **GetAllChildren()**: 컨테이너의 모든 하위 오브젝트 목록 반환
- **IsEmpty()**: 컨테이너가 비어있는지 확인
- **GetObjectCount()**: 컨테이너의 오브젝트 개수 반환
- **applyToSelf**: 컨테이너 자체에 DontDestroyOnLoad 적용 여부 (인스펙터 설정)
- **applyToChildren**: 모든 하위 오브젝트에 DontDestroyOnLoad 적용 여부 (인스펙터 설정)
- **applyToNewChildren**: 새로운 오브젝트에 DontDestroyOnLoad 적용 여부 (인스펙터 설정)
- **persistAcrossScenes**: 씬 전환 시 유지 여부 (인스펙터 설정)
- **enableDebugLogging**: 디버그 로깅 활성화 여부 (인스펙터 설정)

### DropHandlerInjector 클래스 (정적 클래스)
- **InjectToAllCombatSlots(ICombatSlotRegistry slotRegistry, CardDropService dropService, ICombatFlowCoordinator flowCoordinator)**: 모든 전투 슬롯에 드롭 핸들러 주입

### 인터페이스
- **IGameContext**: 게임 컨텍스트 인터페이스 (SelectedCharacter, SetSelectedCharacter)
- **ISceneLoader**: 씬 로딩 인터페이스 (LoadScene)

## 🏗️ 아키텍처 패턴

### 1. 컨텍스트 패턴 (Context Pattern)
- **GameContext**: 플레이어 캐릭터 선택 상태 관리
- **상태 전환**: 컨텍스트를 통한 캐릭터 상태 전환

### 2. 컨테이너 패턴 (Container Pattern)
- **DontDestroyOnLoadContainer**: 씬 전환 시에도 유지되는 오브젝트 관리
- **자동 관리**: 오브젝트의 생성/소멸 자동 관리

### 3. 의존성 주입 패턴 (Dependency Injection)
- **DropHandlerInjector**: 드롭 핸들러 자동 주입
- **인터페이스 기반**: 인터페이스를 통한 느슨한 결합

### 4. 인터페이스 분리 원칙 (Interface Segregation Principle)
- **IGameContext**: 게임 컨텍스트 인터페이스
- **ISceneLoader**: 씬 로딩 인터페이스

## 📝 변경 기록(Delta)
- 형식: `YYYY-MM-DD | 작성자 | 변경 요약 | 영향도(코드/씬/문서)`

- 2025-01-27 | Maintainer | UtilitySystem 개발 문서 초기 작성 | 문서
- 2025-01-27 | Maintainer | 실제 폴더 구조 반영 및 파일 수 정정 | 문서
- 2025-01-27 | Maintainer | AnimationSystem 의존성 완전 제거 및 컴파일 에러 해결 | 코드/문서
- 2025-01-27 | Maintainer | 실제 코드 분석 기반 주요 클래스 및 메서드 정보 추가 | 문서
