# 페이즈 전환 로직 정밀 검사 결과

## 현재 흐름 분석

### 1. 페이즈 전환 트리거
```
NotifyHealthChanged()
  ↓
[즉시 체크] 페이즈 전환 조건 확인
  ↓
isPhaseTransitionPending = true (플래그 설정)
  ↓
CheckPhaseTransitionDelayed() 코루틴 시작 (0.5초 지연)
  ↓
[지연 후] 조건 재확인
  ↓
TransitionToPhaseCoroutine() 시작
```

### 2. TransitionToPhaseCoroutine 흐름
```
1. 슬롯 즉시 제거 (ClearAllSlots)
2. 상태 머신 안전 상태 대기
3. 슬롯 이동 완료 대기
4. 페이즈 전환 연출 재생
5. 페이즈 설정 적용 (skipCardRegeneration: true)
6. RegenerateSlotsAfterPhaseTransitionCoroutine() 호출
7. 플레이어 턴으로 전환 시도
```

## 발견된 문제점

### 🔴 문제 1: 중복된 슬롯 제거/재생성 로직

**위치:**
- `TransitionToPhaseCoroutine`: 슬롯 즉시 제거 (1452-1460)
- `ClearEnemyCardsAndRegenerateCoroutine`: 슬롯 제거 로직 포함 (1807-1832)
- `RegenerateSlotsAfterPhaseTransitionCoroutine`: 슬롯 재생성만 수행 (1854-1894)

**문제:**
- `ClearEnemyCardsAndRegenerateCoroutine`는 더 이상 사용되지 않지만 여전히 존재
- `ApplyPhaseSettings`에서 `skipCardRegeneration=false`일 때 호출 가능 (1579)
- 두 메서드가 거의 동일한 역할을 하지만 분리되어 있음

**해결 방안:**
- `ClearEnemyCardsAndRegenerateCoroutine` 제거 또는 명확한 역할 분리
- `ApplyPhaseSettings`의 `skipCardRegeneration` 로직 검토

### 🔴 문제 2: 복잡한 플래그 관리

**위치:**
- `NotifyHealthChanged`: 플래그 설정 (735)
- `CheckPhaseTransitionDelayed`: 플래그 확인 및 해제 (1208, 1240)
- `TransitionToPhaseCoroutine`: 플래그 해제 (1492)
- `StartPhaseTransition`: 플래그 재설정 (1369)

**문제:**
- 플래그가 여러 곳에서 설정/해제되어 추적이 어려움
- `StartPhaseTransition`에서 이미 설정된 플래그를 재설정 (중복)

**해결 방안:**
- 플래그 설정/해제를 단일 책임으로 통합
- `StartPhaseTransition`의 중복 플래그 설정 제거

### 🔴 문제 3: 슬롯 제거 타이밍 불일치

**위치:**
- `TransitionToPhaseCoroutine` 시작 시 즉시 제거 (1452)
- `RefillAllCombatSlotsWithEnemyDeckCoroutine`에서도 슬롯 제거 시도 (제거됨)

**문제:**
- 슬롯이 너무 일찍 제거되어 상태 머신이나 다른 시스템에서 참조할 수 있음
- 상태 머신 안전 상태 대기 전에 슬롯을 제거하면 문제 발생 가능

**해결 방안:**
- 슬롯 제거를 상태 머신 안전 상태 도달 후로 이동
- 또는 슬롯 제거를 연출 재생 후로 이동

### 🔴 문제 4: 상태 전환 로직 중복

**위치:**
- `TransitionToPhaseCoroutine`: 페이즈 전환 완료 후 플레이어 턴으로 전환 (1494-1519)
- `SlotMovingState`: 슬롯 이동 완료 후 턴 전환 (182-284)

**문제:**
- 두 곳에서 턴 전환을 시도하여 충돌 가능
- 페이즈 전환 중 슬롯이 비어있어 `SlotMovingState`의 로직과 충돌

**해결 방안:**
- 페이즈 전환 중에는 `SlotMovingState`의 턴 전환 로직 스킵
- 또는 페이즈 전환 완료 후에만 턴 전환 수행

### 🟡 문제 5: 덱 캐시 업데이트 타이밍

**위치:**
- `RegenerateSlotsAfterPhaseTransitionCoroutine`: 덱 캐시 업데이트 (1879-1883)
- `ApplyPhaseSettings`: 덱 교체 (1554-1561)

**문제:**
- 덱이 교체된 후에 캐시가 업데이트됨
- 캐시 업데이트 전에 다른 시스템이 덱을 참조할 수 있음

**해결 방안:**
- 덱 교체 직후 즉시 캐시 업데이트
- 또는 `ApplyPhaseSettings`에서 덱 교체와 동시에 캐시 업데이트

## 권장 수정 사항

### 1. 메서드 통합 및 정리
```csharp
// 제거: ClearEnemyCardsAndRegenerateCoroutine (더 이상 사용 안 함)
// 유지: RegenerateSlotsAfterPhaseTransitionCoroutine (페이즈 전환 전용)
// 수정: ApplyPhaseSettings의 skipCardRegeneration 로직 명확화
```

### 2. 플래그 관리 단순화
```csharp
// 플래그 설정: NotifyHealthChanged에서만
// 플래그 해제: TransitionToPhaseCoroutine 완료 시에만
// StartPhaseTransition의 중복 플래그 설정 제거
```

### 3. 슬롯 제거 타이밍 조정
```csharp
// 옵션 1: 상태 머신 안전 상태 도달 후 제거
// 옵션 2: 연출 재생 후 제거 (현재보다 늦게)
```

### 4. 상태 전환 로직 정리
```csharp
// 페이즈 전환 중 플래그로 SlotMovingState의 턴 전환 스킵
// 또는 페이즈 전환 완료 후에만 턴 전환 수행
```

## 현재 코드 구조도

```
NotifyHealthChanged
  ├─ 즉시 플래그 설정
  └─ CheckPhaseTransitionDelayed 시작
  
CheckPhaseTransitionDelayed
  ├─ 0.5초 대기
  ├─ 조건 재확인
  └─ TransitionToPhaseCoroutine 시작
  
TransitionToPhaseCoroutine
  ├─ 슬롯 즉시 제거 ⚠️ (너무 일찍)
  ├─ 상태 머신 안전 상태 대기
  ├─ 슬롯 이동 완료 대기
  ├─ 페이즈 전환 연출 재생
  ├─ ApplyPhaseSettings (skipCardRegeneration: true)
  ├─ RegenerateSlotsAfterPhaseTransitionCoroutine
  └─ 플레이어 턴으로 전환 시도
  
ApplyPhaseSettings
  └─ skipCardRegeneration=false일 때 ClearEnemyCardsAndRegenerateCoroutine 호출 ⚠️ (사용 안 함)
```

## 결론

주요 문제는 **중복된 로직**, **복잡한 플래그 관리**, **타이밍 불일치**입니다. 
코드를 단순화하고 책임을 명확히 분리해야 합니다.

