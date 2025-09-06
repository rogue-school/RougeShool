# AudioSystem 개발 문서

## 📋 시스템 개요
AudioSystem은 게임의 모든 오디오를 관리하는 시스템입니다. 현재 폴더 구조는 존재하지만 실제 구현은 CoreSystem/Audio에 위치하고 있습니다.

## 🏗️ 현재 폴더 구조
```
AudioSystem/
└── Manager/           # 비어있음 (실제 구현은 CoreSystem/Audio에 위치)
```

## 📁 실제 구현 위치
```
CoreSystem/Audio/
└── AudioManager.cs   # 실제 오디오 매니저 구현
```

## 📊 AudioManager.cs 분석

### 주요 기능
- **싱글톤 패턴**: Instance 프로퍼티를 통한 전역 접근
- **오디오 소스 관리**: 여러 AudioSource를 통한 동시 재생 지원
- **볼륨 제어**: 마스터, BGM, SFX 볼륨 개별 제어
- **오디오 클립 관리**: Resources 폴더에서 오디오 클립 로드
- **페이드 효과**: BGM 페이드 인/아웃 지원

### 주요 메서드
- `PlayBGM(string clipName)`: BGM 재생
- `PlaySFX(string clipName)`: 효과음 재생
- `StopBGM()`: BGM 정지
- `SetMasterVolume(float volume)`: 마스터 볼륨 설정
- `SetBGMVolume(float volume)`: BGM 볼륨 설정
- `SetSFXVolume(float volume)`: SFX 볼륨 설정
- `FadeInBGM(string clipName, float duration)`: BGM 페이드 인
- `FadeOutBGM(float duration)`: BGM 페이드 아웃

## 🎯 시스템 특징

### 장점
1. **중앙화된 오디오 관리**: 모든 오디오를 한 곳에서 관리
2. **볼륨 제어**: 세분화된 볼륨 제어 지원
3. **페이드 효과**: 부드러운 BGM 전환
4. **Resources 기반**: 런타임에서 오디오 클립 동적 로드

### 단점
1. **폴더 구조 불일치**: AudioSystem 폴더는 비어있고 CoreSystem에 구현
2. **제한적인 기능**: 기본적인 재생/정지/볼륨 제어만 지원
3. **오디오 풀링 없음**: 동시 재생 제한
4. **설정 저장 없음**: 볼륨 설정이 게임 재시작 시 초기화

## 🔧 사용 방법

### 기본 사용법
```csharp
// BGM 재생
AudioManager.Instance.PlayBGM("MainTheme");

// 효과음 재생
AudioManager.Instance.PlaySFX("ButtonClick");

// 볼륨 설정
AudioManager.Instance.SetMasterVolume(0.8f);
AudioManager.Instance.SetBGMVolume(0.6f);
AudioManager.Instance.SetSFXVolume(1.0f);

// 페이드 효과
AudioManager.Instance.FadeInBGM("BattleTheme", 2.0f);
AudioManager.Instance.FadeOutBGM(1.5f);
```


## 📊 시스템 평가
- **아키텍처**: 6/10 (폴더 구조 불일치)
- **확장성**: 5/10 (제한적인 기능)
- **성능**: 6/10 (기본적인 구현)
- **유지보수성**: 7/10 (단순한 구조)
- **전체 점수**: 6.0/10

