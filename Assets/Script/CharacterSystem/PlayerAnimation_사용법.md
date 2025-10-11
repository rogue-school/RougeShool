# Player Animation 사용법 가이드

## 🎯 개요
Player_Ainima 폴더의 `idle`과 `hit` 애니메이션을 게임에 적용하는 방법입니다.

## 📋 준비된 애니메이션
- **Idle**: 대기 상태 애니메이션 (20프레임)
- **Hit**: 피격 상태 애니메이션 (20프레임)

## 🚀 적용 단계

### 1단계: 프리팹 설정
1. **PlayerManager** 오브젝트를 선택
2. **Inspector**에서 `Character Prefab` 필드에 `Player_idle_Hit.prefab` 연결
3. **Character Slot** 필드에 플레이어가 배치될 위치 설정

### 2단계: Animator Controller 확인
`Player_idle_Hit` 폴더의 `Player.controller`가 다음을 포함하는지 확인:
- **Idle** 상태 (기본 상태)
- **Hit** 상태 (피격 상태)
- **Idle** 트리거 파라미터
- **Hit** 트리거 파라미터

### 3단계: 자동 동작 확인
시스템이 자동으로 다음을 수행합니다:
- 플레이어 생성 시 **Idle 애니메이션** 자동 재생
- 피격 시 **Hit 애니메이션** 자동 재생
- Hit 애니메이션 완료 후 **Idle 애니메이션**으로 복귀

## 💻 코드 사용법

### 기본 사용법
```csharp
// PlayerCharacter 참조 가져오기
var playerCharacter = playerManager.GetCharacter() as PlayerCharacter;

// 대기 애니메이션 재생
playerCharacter.PlayIdleAnimation();

// 피격 애니메이션 재생
playerCharacter.PlayHitAnimation();
```

### 애니메이션 상태 확인
```csharp
// 특정 애니메이션이 재생 중인지 확인
bool isIdlePlaying = playerCharacter.IsAnimationPlaying("Idle");
bool isHitPlaying = playerCharacter.IsAnimationPlaying("Hit");

// 현재 애니메이션 진행률 확인 (0.0 ~ 1.0)
float progress = playerCharacter.GetCurrentAnimationProgress();
```

### 이벤트 기반 사용법
```csharp
// 피격 이벤트 구독 (자동으로 Hit 애니메이션 재생됨)
playerCharacter.OnHPChanged += (current, max) => {
    // HP가 감소하면 자동으로 Hit 애니메이션 재생
    GameLogger.LogInfo($"플레이어 HP 변경: {current}/{max}", GameLogger.LogCategory.Character);
};
```

## 🎮 게임 시나리오별 사용

### 전투 시작
```csharp
// 플레이어 생성 시 자동으로 Idle 애니메이션 재생
playerManager.CreateAndRegisterPlayer();
```

### 피격 시
```csharp
// 데미지 적용 시 자동으로 Hit 애니메이션 재생
playerCharacter.TakeDamage(10);
```

### 턴 종료 후
```csharp
// 턴 종료 시 Idle 상태로 복귀
playerCharacter.PlayIdleAnimation();
```

## 🔧 문제 해결

### 애니메이션이 재생되지 않는 경우
1. **Animator Controller**가 프리팹에 연결되어 있는지 확인
2. **PlayerCharacter** 스크립트가 프리팹에 연결되어 있는지 확인
3. **PlayerManager**의 `Character Prefab` 필드가 올바르게 설정되어 있는지 확인

### 애니메이션이 끊기는 경우
1. **Animator Controller**의 Transition 설정 확인
2. **Has Exit Time** 옵션 확인
3. **Transition Duration** 설정 확인

### 로그 확인
```csharp
// 애니메이션 재생 로그 확인
GameLogger.LogInfo("플레이어 대기 애니메이션 재생", GameLogger.LogCategory.Character);
GameLogger.LogInfo("플레이어 피격 애니메이션 재생", GameLogger.LogCategory.Character);
```

## 📝 추가 개선 사항

### 애니메이션 이벤트 추가
Animator에서 특정 프레임에 이벤트를 추가하여:
- 피격 사운드 재생
- 화면 흔들림 효과
- 데미지 텍스트 표시

### 애니메이션 블렌딩
여러 애니메이션을 부드럽게 전환하려면:
- **Blend Tree** 사용
- **Transition Duration** 조정
- **Animation Curves** 활용

## ✅ 완료 체크리스트
- [ ] Player_idle_Hit.prefab을 PlayerManager에 연결
- [ ] Animator Controller 설정 확인
- [ ] 플레이어 생성 시 Idle 애니메이션 재생 확인
- [ ] 피격 시 Hit 애니메이션 재생 확인
- [ ] 애니메이션 전환 부드러움 확인
- [ ] 로그 메시지 정상 출력 확인

## 🎉 결과
이제 플레이어 캐릭터가 다음과 같이 동작합니다:
1. **게임 시작** → Idle 애니메이션 자동 재생
2. **피격 시** → Hit 애니메이션 자동 재생
3. **피격 완료** → Idle 애니메이션으로 자동 복귀

총 **30분 내에 완전히 적용 가능**하며, 기존 시스템과 완벽하게 통합됩니다!
