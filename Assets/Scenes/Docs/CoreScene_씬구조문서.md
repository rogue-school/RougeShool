# CoreScene 씬 구조 문서

## 목차
- [Quick-Scan 요약](#quick-scan-요약)
- [하이라키 트리](#하이라키-트리)
- [컨테이너/정렬 규칙](#컨테이너정렬-규칙)
- [필수 오브젝트](#필수-오브젝트)
- [핵심 설정값 표](#핵심-설정값-표)
- [인스펙터 연결 표](#인스펙터-연결-표)
- [시스템 연동 포인트](#시스템-연동-포인트)
- [변경 가이드](#변경-가이드)
- [검증 체크리스트](#검증-체크리스트)
- [변경 기록(Delta)](#변경-기록delta)

## Quick-Scan 요약
- 루트 순서: Main Camera → Canvas → CoreContainer (컨테이너) 📦 → EventSystem
- 컨테이너: CoreSystem / AudioSources / CoreUtilities / CoreUI
- 필수 전역 매니저: GameStateManager, SceneTransitionManager, AudioManager, SaveManager, SettingsManager
- 전환 필수 연결: Canvas, FadeImage
- 오디오 소스: BGMSource, SFXSource (AudioManager에 참조 연결)
- 초기화 순서: CoroutineRunner → GameStateManager → SceneTransitionManager → PlayerCharacterSelectionManager → AudioManager → SaveManager → AnimationDatabaseManager → AnimationManager → SettingsManager → LoadingScreenController

## 하이라키 트리
```
Main Camera (Camera, UniversalAdditionalCameraData, AudioListener)
Canvas (Canvas, CanvasScaler, GraphicRaycaster)
  LoadingPanel (Image)
    ProgressBar (Slider)
      Fill Area
        Fill (Image)
      Handle Slide Area
        Handle (Image)
  ProgressText (TMP_Text)
  LoadingText (TMP_Text)
EventSystem (EventSystem, InputSystemUIInputModule)
CoreContainer (컨테이너) 📦
├─ CoreSystem (컨테이너) 📦
│  ├─ GameStateManager (GameStateManager)
│  ├─ SceneTransitionManager (SceneTransitionManager) ⭐
│  ├─ AudioManager (AudioManager) ⭐
│  ├─ SaveManager (SaveManager)
│  ├─ AnimationManager (AnimationManager)
│  ├─ AnimationDatabaseManager (AnimationDatabaseManager)
│  ├─ CoreSystemInitializer (CoreSystemInitializer)
│  ├─ PlayerCharacterSelectionManager (PlayerCharacterSelectionManager)
│  └─ SettingsManager (SettingsManager)
├─ AudioSources (컨테이너) 📦
│  ├─ BGMSource (AudioSource)
│  └─ SFXSource (AudioSource)
├─ CoreUtilities (컨테이너) 📦
│  └─ CoroutineRunner (CoroutineRunner)
└─ CoreUI (컨테이너) 📦
   └─ LoadingScreenController (LoadingScreenController)
```

## 컨테이너/정렬 규칙
- 루트 정렬(위→아래): Main Camera → Canvas → CoreContainer (컨테이너) 📦 → EventSystem
- CoreContainer (컨테이너) 📦 내부 정렬: CoreSystem → AudioSources → CoreUtilities → CoreUI
- 오브젝트명은 역할 중심, 씬 내 유일성 유지.

## 필수 오브젝트
- Main Camera, Canvas, EventSystem, CoreContainer (컨테이너) 📦(하위 4 컨테이너 포함)
- 누락 시: 입력/전역 매니저/오디오/씬 전환 기능 동작 불가.

## 핵심 설정값 표
| 항목 | 값 | 비고 |
|---|---|---|
| CanvasScaler.ReferenceResolution | 800×600 | 현재 값(문서 기준). 1920×1080 권장 |
| CanvasScaler.UiScaleMode | ConstantPixelSize | |
| AudioSource(BGM/SFX).PlayOnAwake | true | 기본값 유지 |
| AudioSource(BGM/SFX).Volume | 1.0 | 프로젝트 설정과 동기화 권장 |
| AudioManager.bgmVolume | 0.7 | |
| AudioManager.sfxVolume | 1.0 | |
| AudioManager.fadeTime | 1.0 | |
| Transition.Duration | 1.0 | 커브 Linear(0→1) |
| Transition.Scenes | Core/Main/Battle | SceneTransitionManager 필드 |
| Initializer.DebugLogging | On | 개발 단계 권장 |
| Initializer.Order | CoroutineRunner → GameStateManager → SceneTransitionManager → PlayerCharacterSelectionManager → AudioManager → SaveManager → AnimationDatabaseManager → AnimationManager → SettingsManager → LoadingScreenController | 코드 기준 |

## 인스펙터 연결 표
| 오브젝트 | 컴포넌트 | 필드 | 값/참조 | [필수] |
|---|---|---|---|---|
| AudioManager | AudioManager | bgmSource | 누락 | 필수(연결 필요) |
| AudioManager | AudioManager | sfxSource | 누락 | 필수(연결 필요) |
| SceneTransitionManager | SceneTransitionManager | transitionCanvas | Canvas | 필수 |
| SceneTransitionManager | SceneTransitionManager | transitionImage | FadeImage (Image) | 필수 |
| LoadingScreenController | LoadingScreenController | loadingPanel | LoadingPanel | 필수 |
| LoadingScreenController | LoadingScreenController | progressBar | ProgressBar (Slider) | 필수 |
| LoadingScreenController | LoadingScreenController | progressText | ProgressText (Text) | 필수 |
| LoadingScreenController | LoadingScreenController | loadingText | LoadingText (Text) | 필수 |

## 시스템 연동 포인트
- 오디오: AudioManager 전역 BGM/SFX 제어(설정 연동 고려)
- 전환: SceneTransitionManager 씬 로딩/전환 애니메이션 제어
- 세이브: SaveManager 전역 세이브 파일 관리
- 애니메이션: AnimationManager/AnimationDatabaseManager 전역 애니메이션 데이터 관리

## 변경 가이드
- 컨테이너 순서/필수 컴포넌트 변경 금지.
- 오브젝트명 변경 전 전역 참조(스크립트/UnityEvent/프리팹) 영향도 확인.
- 전역 시스템 추가 시 CoreSystemInitializer 초기화 순서에 반영.

## 검증 체크리스트
- [ ] 루트/컨테이너/중요 오브젝트 순서 일치
- [ ] Canvas/오디오/전환 핵심 설정값 일치
- [ ] AudioManager/SceneTransition/Loading 참조 연결 완료
- [ ] Initializer 초기화 순서 최신 상태(코드와 일치)
- [ ] 플레이 시 경고/에러 없음

## 변경 기록(Delta)
- 2025-09-08: 문서 규칙 개선 반영(TOC, Quick-Scan, 표 기반 구성) 및 최신 CoreScene 값 동기화
