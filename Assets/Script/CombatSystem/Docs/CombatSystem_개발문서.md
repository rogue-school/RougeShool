# CombatSystem 개발 문서

## 📋 시스템 개요
전투 시스템은 4개의 전투 슬롯(SLOT_1~SLOT_4)을 중심으로 즉시 실행(즉발) 카드 흐름을 관리합니다. 적 핸드슬롯과 턴 시작 버튼은 존재하지 않으며, 플레이어가 1번 슬롯에 카드를 놓는 즉시 실행되고, 실행 종료 후 슬롯들이 앞으로 이동합니다.

## 🏗️ 폴더 구조(코드 기준)
- `CombatSystem/Core/`: 인스톨러 및 컨텍스트
- `CombatSystem/Manager/`: 턴/슬롯/플로우 매니저
- `CombatSystem/Service/`: 실행 서비스(즉시 실행/페이즈)
- `CombatSystem/Interface/`: 인터페이스 정의
- `CombatSystem/State/`: 상태 패턴 관련(호환 목적)
- `CombatSystem/Utility/`: 유틸리티
- `CombatSystem/Docs/`: 본 문서

## 📁 주요 컴포넌트
- `CombatTurnManager`: 턴 판정·진행·가드 효과 적용
- `CombatSlotManager`: 4개 슬롯 자동 바인딩/검증
- `CombatExecutorService`: 카드 즉시 실행, 슬롯 이동(Shift), 페이즈 실행
- `CombatInstaller`: Zenject 바인딩, 덱/보상 시스템 연계
- `CombatSlotPosition`: `SLOT_1..SLOT_4` 및 레거시 `FIRST/SECOND` 호환

## 🎯 전투 핵심 규칙
1. 슬롯 구성: 4개 고정(`SLOT_1..SLOT_4`).
2. 초기 상태: `1=플레이어(빈)`, `2=적(카드)`, `3=플레이어(빈)`, `4=적(카드)`.
3. 턴 판정: `SLOT_1`이 비어있으면 플레이어 턴, 적 카드가 있으면 적 턴.
4. 즉시 실행: 1번 슬롯에 카드가 존재하면 즉시 실행(플레이어든 적이든).
5. 실행 후 이동(Shift): 실행 완료 직후 `2→1, 3→2, 4→3`, `4`는 비워짐.
6. 적 카드 보충: 필요 시 적 카드는 `SLOT_4`에 예약 등록 후 다음 Shift로 전진.
7. 가드 효과: “다음 순서의 적 카드 1장 무효화”로 즉시 소모(수치/지속 없음).

## 🔧 사용 방법(핵심 API)
- 턴 확인
  - `CombatTurnManager.GetCurrentTurnType()` → `Player | Enemy`
  - `IsPlayerTurn()`, `IsEnemyTurn()`
- 즉시 실행
  - `CombatExecutorService.ExecuteImmediately(card, CombatSlotPosition.SLOT_1)`
- 슬롯 이동
  - `CombatExecutorService.MoveSlotsForward()` (내부 호출됨)
- 적 카드 등록
  - `CombatTurnManager.RegisterEnemyCardInSlot4(skillCardDefinition)`
- 가드 효과 적용
  - `CombatTurnManager.ApplyGuardEffect()` → 다음 적 카드 1장을 무효화

## 🏗️ 아키텍처 패턴/DI
- 상태/전략/옵저버 조합. Update 최소화, 이벤트/콜백 기반.
- Zenject 바인딩: `CombatInstaller`에서 매니저/서비스/팩토리 바인딩.
- 인터페이스 중심: `ICombatTurnManager`, `ICombatExecutorService`, `ICombatSlotRegistry` 등.

## 🔧 기술적 구현 세부사항
- 즉시 실행 경로 분리: 플레이어가 1번 슬롯에 카드를 놓는 순간 `ExecuteImmediately`.
- 슬롯 이동 최적화: 정적 이동 순서 배열 기반 O(1) 참조 전진.
- 다단 히트: 히트당 0.15초 간격, 히트마다 SFX 재생.
- 연출 단순화: 카드별 `sfxClip`/`visualEffectPrefab`만 사용(타이밍/지연/애니메이션 지정 제거).
- 용어 통일: `pierceable` → `ignoreGuard`(가드 무시), 효과 라벨 한글화.

## ▶️ 6턴 시뮬레이션(예시)
초기: `[1:P(빈)] [2:E(카드A)] [3:P(빈)] [4:E(카드B)]`

1) 턴1(플레이어): 플레이어 카드 X를 1번에 올림 → 즉시 실행 → Shift
   - 결과: `[1:E(A)] [2:P(빈)] [3:E(B)] [4:빈]`
2) 턴2(적): 1번 적 카드 A 즉시 실행 → Shift
   - 결과: `[1:P(빈)] [2:E(B)] [3:P(빈)] [4:빈]`
   - 필요 시 적 카드 C를 `RegisterEnemyCardInSlot4`로 예약
3) 턴3(플레이어): 플레이어 카드 Y 실행 → Shift
   - 결과(적 C 예약 전진 시작): `[1:E(B)] [2:P(빈)] [3:E(C)] [4:빈]`
4) 턴4(적): 1번 적 카드 B 실행 → Shift
   - 결과: `[1:P(빈)] [2:E(C)] [3:P(빈)] [4:빈]`
5) 턴5(플레이어): 플레이어 카드 Z 실행 → Shift
   - 결과: `[1:E(C)] [2:P(빈)] [3:빈] [4:빈]`
6) 턴6(적): 1번 적 카드 C 실행 → Shift
   - 결과(초기 패턴 재형성): `[1:P(빈)] [2:E(…)] [3:P(빈)] [4:E(…)]`

가드 예외: 플레이어가 가드 카드를 사용해 `ApplyGuardEffect()`가 활성인 경우, 이후 “다음” 적 카드 1장이 무효 처리되고 가드는 소멸.

### ASCII 다이어그램(시각화)
```
전설: [슬롯번호:턴주체(내용)]  P=플레이어, E=적, 빈=비어있음

초기 상태
[1:P(빈)] [2:E(A)] [3:P(빈)] [4:E(B)]

턴1 (플레이어 카드 X 사용 → 즉시 실행 → Shift)
실행: [1:P(X)]
이동: 2→1, 3→2, 4→3
[1:E(A)] [2:P(빈)] [3:E(B)] [4:빈]

턴2 (적 카드 A 즉시 실행 → Shift)
실행: [1:E(A)]
이동: 2→1, 3→2, 4→3
[1:P(빈)] [2:E(B)] [3:P(빈)] [4:빈]

턴3 (플레이어 카드 Y 사용 → 즉시 실행 → Shift)
실행: [1:P(Y)]
적 C를 4번에 예약 등록
이동: 2→1, 3→2, 4→3
[1:E(B)] [2:P(빈)] [3:E(C)] [4:빈]

턴4 (적 카드 B 즉시 실행 → Shift)
실행: [1:E(B)]
이동: 2→1, 3→2, 4→3
[1:P(빈)] [2:E(C)] [3:P(빈)] [4:빈]

턴5 (플레이어 카드 Z 사용 → 즉시 실행 → Shift)
실행: [1:P(Z)]
이동: 2→1, 3→2, 4→3
[1:E(C)] [2:P(빈)] [3:빈] [4:빈]

턴6 (적 카드 C 즉시 실행 → Shift)
실행: [1:E(C)]
이동: 2→1, 3→2, 4→3
[1:P(빈)] [2:E(…)] [3:P(빈)] [4:E(…)]

가드 활성(예):
턴1에 가드 사용 → 이후 "다음" 적 카드(턴2의 A)가 무효 처리됨 → Shift만 수행
```

## 🧪 검증/테스트 지침
- 체크리스트
  - [ ] 1번 슬롯 즉시 실행 동작 확인(플레이어/적)
  - [ ] 실행 후 슬롯 이동 시 참조/UI 동기화 확인
  - [ ] 적 카드 `SLOT_4` 등록 → 자연스러운 전진 진입 확인
  - [ ] 가드 효과 시 다음 적 카드 1장 무효화 확인
  - [ ] 다단 히트 간격/히트당 SFX 재생 확인
  - [ ] 플레이/전환/로그 경고 0

## 성능·메모리 고려사항
- Update/GC 최소화, 배열/리스트 재사용, 캐싱 철저.
- DOTween SafeMode 유지, TweensCapacity 튜닝 권장.
- 이벤트 콜백 기반으로 상태 갱신, 불필요한 코루틴 금지.

## 📚 내부 참조
- `CombatTurnManager`, `CombatExecutorService`, `CombatSlotManager`
- `SkillCardSystem/*` (카드 정의/효과/런타임)
- `CoreSystem/Audio/AudioManager` (히트 SFX)

## 📝 변경 기록(Delta)
- 2025-09-15 | Maintainer | 4슬롯 즉시 실행 전투 흐름 문서 최초 작성 | 문서
- 2025-09-15 | Maintainer | 가드 효과 정의(다음 적 카드 1장 무효) 명시 | 문서
- 2025-09-15 | Maintainer | 다단 히트 0.15초/히트당 SFX 규칙 반영 | 문서
