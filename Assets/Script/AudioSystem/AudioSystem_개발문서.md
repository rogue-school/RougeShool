# AudioSystem 개발 문서

## 📋 시스템 개요
AudioSystem은 게임의 모든 오디오를 관리하는 시스템입니다. 오디오 풀링과 이벤트 기반 시스템을 제공합니다.

## 🏗️ 폴더 구조
```
AudioSystem/
└── (빈 폴더)          # 실제 구현은 CoreSystem/Audio에 위치
```

## 📁 실제 구현 위치
```
CoreSystem/Audio/
├── AudioManager.cs           # 오디오 매니저 (싱글톤)
├── AudioPoolManager.cs       # 오디오 풀링 매니저
└── AudioEventTrigger.cs      # 오디오 이벤트 트리거
```

## 📊 실제 구현 클래스 분석

### AudioManager.cs 주요 기능
- **싱글톤 패턴**: Instance 프로퍼티를 통한 전역 접근
- **ICoreSystemInitializable**: CoreSystem 초기화 인터페이스 구현
- **오디오 소스 관리**: BGM용, SFX용 AudioSource 분리 관리
- **페이드 효과**: BGM 전환 시 부드러운 페이드 인/아웃
- **오디오 풀링**: AudioPoolManager를 통한 사운드 중복 방지
- **볼륨 제어**: BGM, SFX 볼륨 개별 제어

### AudioManager.cs 주요 메서드
- **PlayBGM(AudioClip bgmClip, bool fadeIn)**: BGM 재생 (페이드 옵션)
- **PlaySFX(AudioClip sfxClip)**: 효과음 재생 (기본 방식)
- **PlaySFXWithPool(AudioClip sfxClip, float volume, int priority)**: 풀링을 사용한 효과음 재생
- **StopBGM()**: BGM 정지
- **SetBGMVolume(float volume)**: BGM 볼륨 설정
- **SetSFXVolume(float volume)**: SFX 볼륨 설정
- **FadeToNewBGM(AudioClip newBGM)**: BGM 페이드 전환 (내부 코루틴)
- **Initialize()**: ICoreSystemInitializable 구현
- **OnInitializationFailed()**: 초기화 실패 처리

### AudioPoolManager.cs 주요 기능
- **AudioSource 풀링**: 미리 생성된 AudioSource 풀 관리
- **사운드 중복 방지**: 쿨다운 시스템으로 동일 사운드 중복 재생 방지
- **우선순위 시스템**: 사운드별 우선순위 설정으로 중요한 사운드 우선 재생
- **자동 풀 반환**: 재생 완료 후 AudioSource 자동 풀 반환

### AudioPoolManager.cs 주요 메서드
- **PlaySound(AudioClip clip, float volume, int priority)**: 우선순위 지정 사운드 재생
- **PlaySound(AudioClip clip, float volume)**: 자동 우선순위 사운드 재생
- **GetAudioSourceFromPool()**: 풀에서 AudioSource 가져오기
- **ReturnToPoolAfterPlay()**: 재생 완료 후 풀에 반환
- **IsInCooldown(string clipName)**: 쿨다운 상태 확인
- **CanPlayWithPriority(string clipName, int priority)**: 우선순위 체크
- **GetSoundPriority(string clipName)**: 사운드 우선순위 조회
- **InitializeSoundPriority()**: 사운드 우선순위 초기화

### 사운드 우선순위 설정
```csharp
// 전투 사운드 (높은 우선순위)
"enemy_defeat" = 10
"skill_activation" = 9
"card_use" = 8
"turn_start" = 7
"turn_complete" = 6

// UI 사운드 (중간 우선순위)
"button_click" = 5
"card_drag" = 4
"card_drop" = 4
"menu_open" = 3
"menu_close" = 3

// 기타 사운드 (낮은 우선순위)
"default" = 1
```

## 🎯 시스템 특징

### 장점
1. **중앙화된 오디오 관리**: 모든 오디오를 한 곳에서 관리
2. **볼륨 제어**: 세분화된 볼륨 제어 지원
3. **페이드 효과**: 부드러운 BGM 전환
4. **Resources 기반**: 런타임에서 오디오 클립 동적 로드
5. **오디오 풀링**: 사운드 중복 방지 및 성능 최적화 (신규)
6. **이벤트 기반**: 게임 이벤트와 자동 연동 (신규)
7. **CoreSystem 통합**: 전역 시스템으로 완전 통합 (신규)

### 개선사항
1. **폴더 구조 정리**: AudioSystem 폴더는 문서용, 실제 구현은 CoreSystem/Audio
2. **기능 확장**: 오디오 풀링과 이벤트 기반 시스템 추가
3. **성능 최적화**: AudioSource 풀링으로 동시 재생 제한 해결
4. **설정 저장**: 향후 SaveSystem과 연동 예정

## 🔧 사용 방법

### 기본 사용법
```csharp
// BGM 재생 (AudioClip 직접 전달)
AudioClip mainTheme = Resources.Load<AudioClip>("Sounds/BGM/MainTheme");
AudioManager.Instance.PlayBGM(mainTheme, true); // 페이드 인 옵션

// 효과음 재생 (기본 방식)
AudioClip buttonClick = Resources.Load<AudioClip>("Sounds/SFX/ButtonClick");
AudioManager.Instance.PlaySFX(buttonClick);

// 효과음 재생 (풀링 사용, 중복 방지)
AudioClip cardUse = Resources.Load<AudioClip>("Sounds/SFX/CardUse");
AudioManager.Instance.PlaySFXWithPool(cardUse, 1.0f, 8); // 볼륨, 우선순위

// 볼륨 설정
AudioManager.Instance.SetBGMVolume(0.6f);
AudioManager.Instance.SetSFXVolume(1.0f);

// BGM 정지
AudioManager.Instance.StopBGM();
```

### AudioPoolManager 직접 사용법
```csharp
// AudioPoolManager를 통한 고급 사운드 제어
AudioPoolManager poolManager = AudioManager.Instance.GetComponent<AudioPoolManager>();

// 우선순위 지정 사운드 재생
AudioClip enemyDefeat = Resources.Load<AudioClip>("Sounds/SFX/EnemyDefeat");
poolManager.PlaySound(enemyDefeat, 1.0f, 10); // 높은 우선순위

// 자동 우선순위 사운드 재생
AudioClip skillActivation = Resources.Load<AudioClip>("Sounds/SFX/SkillActivation");
poolManager.PlaySound(skillActivation, 0.8f); // 우선순위 자동 설정
```

### CoreSystem 초기화 연동
```csharp
// ICoreSystemInitializable 구현으로 자동 초기화
// CoreSystemInitializer에서 자동으로 Initialize() 호출됨

// 초기화 상태 확인
if (AudioManager.Instance.IsInitialized)
{
    // 오디오 시스템 사용 가능
    AudioManager.Instance.PlayBGM(bgmClip);
}
```

### 사운드 우선순위 활용
```csharp
// 전투 사운드 (높은 우선순위) - 다른 사운드보다 우선 재생
poolManager.PlaySound(enemyDefeatClip, 1.0f, 10);
poolManager.PlaySound(skillActivationClip, 1.0f, 9);

// UI 사운드 (중간 우선순위)
poolManager.PlaySound(buttonClickClip, 0.7f, 5);
poolManager.PlaySound(cardDragClip, 0.5f, 4);

// 기타 사운드 (낮은 우선순위)
poolManager.PlaySound(ambientClip, 0.3f, 1);
```

## 📝 변경 기록(Delta)
- 형식: `YYYY-MM-DD | 작성자 | 변경 요약 | 영향도(코드/씬/문서)`

- 2025-01-27 | Maintainer | AudioSystem 개발 문서 초기 작성 | 문서
- 2025-01-27 | Maintainer | 실제 구현 위치 명시 및 폴더 구조 정정 | 문서
- 2025-01-27 | Maintainer | 실제 코드 분석 기반 구체적 클래스/메서드/우선순위 정보 추가 | 문서

