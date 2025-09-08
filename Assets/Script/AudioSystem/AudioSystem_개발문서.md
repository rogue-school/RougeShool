# AudioSystem 개발 문서

## 📋 시스템 개요
AudioSystem은 게임의 모든 오디오를 관리하는 시스템입니다. 오디오 풀링과 이벤트 기반 시스템을 제공합니다.

## 🏗️ 현재 폴더 구조
```
AudioSystem/
└── Manager/           # 비어있음 (실제 구현은 CoreSystem/Audio에 위치)
```

## 📁 실제 구현 위치
```
CoreSystem/Audio/
├── AudioManager.cs           # 오디오 매니저 (확장됨)
├── AudioPoolManager.cs       # 오디오 풀링 매니저 (신규)
└── AudioEventTrigger.cs      # 오디오 이벤트 트리거 (신규)
```

## 📊 AudioManager.cs 분석

### 주요 기능
- **싱글톤 패턴**: Instance 프로퍼티를 통한 전역 접근
- **오디오 소스 관리**: 여러 AudioSource를 통한 동시 재생 지원
- **볼륨 제어**: 마스터, BGM, SFX 볼륨 개별 제어
- **오디오 클립 관리**: Resources 폴더에서 오디오 클립 로드
- **페이드 효과**: BGM 페이드 인/아웃 지원
- **오디오 풀링**: AudioPoolManager를 통한 사운드 중복 방지
- **이벤트 기반**: AudioEventTrigger를 통한 게임 이벤트 연동

### 주요 메서드
- `PlayBGM(string clipName)`: BGM 재생
- `PlaySFX(string clipName)`: 효과음 재생
- `PlaySFXWithPool(string clipName)`: 풀링을 사용한 효과음 재생 (신규)
- `StopBGM()`: BGM 정지
- `SetMasterVolume(float volume)`: 마스터 볼륨 설정
- `SetBGMVolume(float volume)`: BGM 볼륨 설정
- `SetSFXVolume(float volume)`: SFX 볼륨 설정
- `FadeInBGM(string clipName, float duration)`: BGM 페이드 인
- `FadeOutBGM(float duration)`: BGM 페이드 아웃
- `PlayCardUseSound()`: 카드 사용 사운드 (신규)
- `PlayEnemyDefeatSound()`: 적 처치 사운드 (신규)
- `PlayButtonClickSound()`: 버튼 클릭 사운드 (신규)

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
// BGM 재생
AudioManager.Instance.PlayBGM("MainTheme");

// 효과음 재생 (기본)
AudioManager.Instance.PlaySFX("ButtonClick");

// 효과음 재생 (풀링 사용, 중복 방지)
AudioManager.Instance.PlaySFXWithPool("CardUse");

// 전용 사운드 메서드
AudioManager.Instance.PlayCardUseSound();
AudioManager.Instance.PlayEnemyDefeatSound();
AudioManager.Instance.PlayButtonClickSound();

// 볼륨 설정
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetBGMVolume(0.6f);
AudioManager.Instance.SetSFXVolume(1.0f);

// 페이드 효과
AudioManager.Instance.FadeInBGM("BattleTheme", 2.0f);
AudioManager.Instance.FadeOutBGM(1.5f);
```

### 이벤트 기반 사용법
```csharp
// AudioEventTrigger를 통한 자동 사운드 재생
audioEventTrigger.OnCardUsed();        // 카드 사용 사운드
audioEventTrigger.OnEnemyDefeated();   // 적 처치 사운드
audioEventTrigger.OnButtonClicked();   // 버튼 클릭 사운드
```



