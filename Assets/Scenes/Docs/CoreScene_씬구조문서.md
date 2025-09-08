# CoreScene 씬 제작 가이드

## 🎯 목표
CoreScene을 전역 시스템 허브로 구축하여 모든 씬에서 공통으로 사용하는 매니저/로더/오디오/UI를 안정적으로 초기화합니다.

## 📦 준비물(사전 요구)
- 전역 매니저: GameStateManager, SceneTransitionManager, AudioManager, SaveManager, SettingsManager
- 유틸리티: CoroutineRunner, GameLogger
- 오디오 소스: BGMSource, SFXSource (AudioManager 참조 연결)
- 로딩 UI: LoadingScreenController(필수 UI 레퍼런스 연결)
- Canvas/Camera/EventSystem

## 🏗️ 제작 절차(Step-by-Step)
1) 루트 생성
- Main Camera, Canvas(CanvasScaler 1920×1080 권장), EventSystem 추가

2) CoreContainer 컨테이너 구성
- 빈 오브젝트 `CoreContainer` 생성 후 하위에 다음 컨테이너 생성: CoreSystem, AudioSources, CoreUtilities, CoreUI
- CoreSystem 하위에 전역 매니저 배치:
  - GameStateManager, SceneTransitionManager, AudioManager, SaveManager, AnimationManager, AnimationDatabaseManager, CoreSystemInitializer, PlayerCharacterSelectionManager, SettingsManager
- AudioSources 하위에 오디오 소스 배치: `BGMSource(AudioSource)`, `SFXSource(AudioSource)`, AudioManager 인스펙터에 참조 연결
- CoreUtilities 하위에 `CoroutineRunner`
- CoreUI 하위에 `LoadingScreenController`와 관련 UI 배치(LoadingPanel/ProgressBar/ProgressText/LoadingText)

3) 초기화 순서 확인(CoreSystemInitializer)
- 초기화 순서 권장: CoroutineRunner → GameStateManager → SceneTransitionManager → PlayerCharacterSelectionManager → AudioManager → SaveManager → AnimationDatabaseManager → AnimationManager → SettingsManager → LoadingScreenController

4) SceneTransitionManager 설정
- `transitionCanvas` = Canvas 또는 전용 Transition Canvas
- `transitionImage` = 페이드 이미지(UI Image)
- CoreScene 로드 후 자동으로 MainScene 전환이 필요하다면 Initializer에서 호출 설정

5) AudioManager 설정
- `bgmSource` = BGMSource, `sfxSource` = SFXSource 연결
- 볼륨 기본값(bgm=0.7, sfx=1.0), 페이드 시간 설정
- SaveSystem 연동 시 시작 시 `SaveManager.LoadAudioSettings()`로 볼륨 반영

## 📁 하이라키 예시
```
Main Camera
Canvas
  LoadingPanel
    ProgressBar
      Fill (Image)
      Handle (Image)
  ProgressText (TMP_Text)
  LoadingText (TMP_Text)
EventSystem
CoreContainer 📦
├─ CoreSystem 📦
│  ├─ GameStateManager
│  ├─ SceneTransitionManager ⭐
│  ├─ AudioManager ⭐
│  ├─ SaveManager
│  ├─ AnimationManager
│  ├─ AnimationDatabaseManager
│  ├─ CoreSystemInitializer
│  ├─ PlayerCharacterSelectionManager
│  └─ SettingsManager
├─ AudioSources 📦
│  ├─ BGMSource (AudioSource)
│  └─ SFXSource (AudioSource)
├─ CoreUtilities 📦
│  └─ CoroutineRunner
└─ CoreUI 📦
   └─ LoadingScreenController
```

## 🔗 인스펙터 필수 연결 표
| 오브젝트 | 컴포넌트 | 필드 | 값/참조 | [필수] |
|---|---|---|---|---|
| AudioManager | AudioManager | bgmSource | BGMSource | 필수 |
| AudioManager | AudioManager | sfxSource | SFXSource | 필수 |
| SceneTransitionManager | SceneTransitionManager | transitionCanvas | Canvas | 필수 |
| SceneTransitionManager | SceneTransitionManager | transitionImage | 페이드 대상 Image | 필수 |
| LoadingScreenController | LoadingScreenController | loadingPanel | LoadingPanel | 필수 |
| LoadingScreenController | LoadingScreenController | progressBar | ProgressBar | 필수 |
| LoadingScreenController | LoadingScreenController | progressText | ProgressText | 필수 |
| LoadingScreenController | LoadingScreenController | loadingText | LoadingText | 필수 |

## ✅ 검증 체크리스트
- [ ] 루트/컨테이너/중요 오브젝트 순서 일치(Main→Canvas→CoreContainer→EventSystem)
- [ ] AudioManager/SceneTransition/Loading 참조 연결 완료
- [ ] 초기화 순서 로그가 코드와 일치
- [ ] SaveManager/AudioManager 볼륨 연동 작동(시작 시 로드)
- [ ] 다른 씬 전환 시 페이드/로딩 UI가 정상 동작

## 🧩 자주 발생하는 오류와 해결
- AudioManager 참조 누락 → bgmSource/sfxSource 필드 연결
- SceneTransitionManager 이미지/캔버스 누락 → transitionCanvas/transitionImage 필수 연결
- 로딩 UI 참조 미설정 → LoadingScreenController의 4개 필드 모두 연결

## 📝 변경 기록(Delta)
- 2025-09-08: 씬 제작 가이드 형식으로 전환, 필수 연결 표/체크리스트 보강
