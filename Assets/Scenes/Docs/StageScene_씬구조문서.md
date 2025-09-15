# StageScene 씬 제작 문서

## TOC
- [Quick-Scan 요약](#quick-scan-요약)
- [하이라키 트리](#하이라키-트리)
- [컨테이너/정렬 불변식](#컨테이너정렬-불변식)
- [인스펙터 연결 표](#인스펙터-연결-표)
- [핵심 설정값 표](#핵심-설정값-표)
- [제작 워크플로](#제작-워크플로)
- [시스템 연동 포인트](#시스템-연동-포인트)
- [검증/품질 게이트](#검증품질-게이트)
- [성능·메모리 고려사항](#성능메모리-고려사항)
- [트러블슈팅](#트러블슈팅)
- [변경 기록(Delta)](#변경-기록delta)

## Quick-Scan 요약
- 목적: StageScene에서 플레이어/적 캐릭터를 스폰하고 4-슬롯 즉발 전투를 수행한다.
- 규칙: 1번 슬롯에 카드가 놓이는 즉시 실행 → 실행 직후 슬롯 이동(2→1, 3→2, 4→3).
- 제약: 플레이어 카드 드랍은 `SLOT_1`만 허용. 적 카드는 4번에서 예약/전진.
- 전제: CoreScene에서 `GameLogger`, `AudioManager`, `AnimationDatabaseManager`가 초기화 완료, 통합 스킬카드 애니메이션 DB 인스펙터 연결.

## 하이라키 트리
```
Main Camera (Camera, AudioListener)
EventSystem (EventSystem, InputSystemUIInputModule)
Stage (Canvas, CanvasScaler, GraphicRaycaster)  // UICanvas
├─ CombatArena (컨테이너) 📦
│  ├─ CharacterStage (컨테이너) 📦
│  │  ├─ PlayerSpawnPoint (RectTransform) ⭐
│  │  └─ EnemySpawnPoint (RectTransform) ⭐
│  └─ CombatExecutionArea (RectTransform, HorizontalLayoutGroup) 📦
│     ├─ CombatSlot_1 (Image, CombatExecutionSlotUI) ⭐
│     ├─ CombatSlot_2 (Image, CombatExecutionSlotUI) ⭐
│     ├─ CombatSlot_3 (Image, CombatExecutionSlotUI) ⭐
│     └─ CombatSlot_4 (Image, CombatExecutionSlotUI) ⭐
├─ PlayerHandsoltContainer (RectTransform) 📦
│  ├─ PlayerHandslot1 (RectTransform, Image)
│  ├─ PlayerHandslot2 (RectTransform, Image)
│  └─ PlayerHandslot3 (RectTransform, Image)
├─ Systems (컨테이너) 📦
│  ├─ SlotRegistry (SlotRegistry) ⭐
│  ├─ PlayerHandManager (PlayerHandManager) ⭐
│  └─ StageManager (StageManager) ⭐
└─ Installer (컨테이너) 📦
   ├─ SceneContext (SceneContext) ⭐
   └─ CombatInstaller (MonoInstaller) ⭐
```

참고: 현재 리포지토리의 `StageScene.unity`에는 `CombatslotContainer`와 `PlayerHandsoltContainer`가 존재합니다. 상단 트리에 맞춰 `CombatArena/CharacterStage/SpawnPoint`와 `CombatExecutionArea`(슬롯 4개)를 구성하면 됩니다.

## 컨테이너/정렬 불변식
1) 루트 정렬: Main Camera → EventSystem → UICanvas(Stage) → 기타 컨테이너.
2) `CombatExecutionArea`는 `HorizontalLayoutGroup`으로 균등 간격 유지(Spacing 32 권장).
3) 슬롯 오브젝트(`CombatSlot_*`)는 동일 크기(예: 280×380)와 중앙 정렬.
4) 스폰 포인트는 화면 좌우 중앙(좌=플레이어, 우=적), Z=0, 정렬/SortingLayer 충돌 없음.

## 인스펙터 연결 표
| 오브젝트 | 컴포넌트 | 필드 | 값/참조 | [필수] |
|---|---|---|---|---|
| Stage | CanvasScaler | ReferenceResolution | 1920×1080 권장 | 필수 |
| CombatExecutionArea | HorizontalLayoutGroup | Spacing | 32 | 필수 |
| CombatExecutionArea | HorizontalLayoutGroup | Child Control/Expand | Off/Off | 필수 |
| CombatSlot_1 | CombatExecutionSlotUI | Position | SLOT_1 | 필수 |
| CombatSlot_2 | CombatExecutionSlotUI | Position | SLOT_2 | 필수 |
| CombatSlot_3 | CombatExecutionSlotUI | Position | SLOT_3 | 필수 |
| CombatSlot_4 | CombatExecutionSlotUI | Position | SLOT_4 | 필수 |
| PlayerHandslot1..3 | PlayerHandCardSlotUI | (기본 설정) | 드래그 시작 지점 | 권장 |
| CombatArena | CombatSlotManager | (자동) | 씬 로드시 자동 바인딩 | 필수 |
| Systems/SlotRegistry | SlotRegistry | (자체) | 씬에 1개 존재 | 필수 |
| Systems/PlayerHandManager | PlayerHandManager | (자체) | 핸드 슬롯 자동 바인딩 | 필수 |
| Systems/StageManager | StageManager | (자체) | 스테이지 진행 관리 | 필수 |
| Installer/SceneContext | SceneContext | MonoInstallers | CombatInstaller 등록 | 필수 |
| Installer/CombatInstaller | CombatInstaller | cardUIPrefab | 카드 UI 프리팹(선택) | 권장 |
| Installer/CombatInstaller | CombatInstaller | startButtonHandler | 비움 허용(즉발 규칙) | 선택 |
| CoreScene | AnimationDatabaseManager | Unified SkillCard Database | 통합 DB 에셋 연결 | 필수 |

## 핵심 설정값 표
| 항목 | 값 | 비고 |
|---|---|---|
| 카드 드랍 허용 슬롯 | SLOT_1 | 검증기로 강제됨 |
| 슬롯 이동 규칙 | 2→1, 3→2, 4→3 | 실행 직후 즉시 |
| 스폰 포인트 위치 | 좌/우 중앙 | Player/Enemy |
| Tween 설정 | SafeMode ON | DOTween Settings |

## 제작 워크플로
1. 레이아웃 컨테이너 준비
   - `Stage`(Canvas) 하위에 `CombatArena` 생성.
   - `CombatArena` 하위에 `CharacterStage` 생성 후 `PlayerSpawnPoint`(좌측 중앙) / `EnemySpawnPoint`(우측 중앙) 추가.
   - `CombatArena` 하위에 `CombatExecutionArea` 생성. 현재 씬의 `CombatslotContainer`를 사용해도 됩니다(이름 유지 허용).
2. 슬롯 배치
   - `CombatExecutionArea`(또는 `CombatslotContainer`)에 `HorizontalLayoutGroup` 추가
     - Spacing 32, Child Control/Expand = Off/Off, Alignment = Middle Center
   - 자식으로 `CombatSlot_1..4` 생성(또는 기존 슬롯 사용) 후 각 오브젝트에 `CombatExecutionSlotUI` 부착
     - `Position` = `SLOT_1`, `SLOT_2`, `SLOT_3`, `SLOT_4`
   - 중요: `CombatExecutionArea`(또는 `CombatslotContainer`) 오브젝트에 `CombatSlotManager`를 부착합니다. 이 오브젝트는 4개 슬롯의 "부모"여야 합니다.
3. 핸드 UI
   - `PlayerHandsoltContainer` 아래 `PlayerHandslot1..3`에 `PlayerHandCardSlotUI` 부착(드래그 시작 지점)
4. 필수 시스템 배치
   - `Systems` 컨테이너에 다음 오브젝트를 생성
     - `SlotRegistry`(컴포넌트: `SlotRegistry`)
     - `PlayerHandManager`(컴포넌트: `PlayerHandManager`)
     - `StageManager`(컴포넌트: `StageManager`)
5. Zenject DI 설정
   - `Installer` 컨테이너에 `SceneContext` 추가
   - 같은 오브젝트(또는 자식)에 `CombatInstaller`(MonoInstaller) 추가
   - `SceneContext` 인스펙터의 `Mono Installers` 리스트에 `CombatInstaller` 등록(Size=1, Element0=CombatInstaller)
   - `CombatInstaller` 인스펙터
     - `cardUIPrefab`(선택), `startButtonHandler`는 비워도 됨(즉발 규칙)
6. 실행 및 검증
   - 플레이 → 콘솔에서 `CombatSlotManager` 자동 바인딩 경고가 없는지 확인
   - "SlotRegistry를 찾을 수 없습니다"/"StageManager를 찾을 수 없습니다"가 나오면 `Systems` 컨테이너 구성을 재확인
   - 카드 드랍: `SLOT_1`만 성공, 즉시 실행 후 2→1, 3→2, 4→3 이동 확인

## 시스템 연동 포인트
- 전투 슬롯: `CombatSlotManager`가 `CombatExecutionSlotUI`를 자동 바인딩.
- 드래그/드랍: 플레이어 카드는 `SLOT_1`만 드랍 허용(기본 검증기 적용).
- 애니메이션: CoreScene의 `AnimationDatabaseManager`가 통합 스킬카드 애니메이션 DB를 제공.
- 적 카드: 4번 슬롯 예약 → 시프트로 전진하여 1번 진입.
 - DI: `CombatExecutorService` 등 서비스들은 Zenject로 바인딩되며 인스펙터 컴포넌트가 아닙니다.

## 실제 제작용 완전 하이라키(정확한 이름/컴포넌트/필드)
아래 트리를 그대로 구성하면 StageScene이 BattleScene 수준의 시스템을 갖춘 상태로 즉시 동작합니다.
```
Main Camera (Camera, UniversalAdditionalCameraData, AudioListener)
EventSystem (EventSystem, InputSystemUIInputModule)

Stage (Canvas, CanvasScaler, GraphicRaycaster)
├─ CombatArena (Empty)
│  ├─ CharacterStage (Empty)
│  │  ├─ PlayerSpawnPoint (RectTransform)
│  │  └─ EnemySpawnPoint (RectTransform)
│  └─ CombatExecutionArea (HorizontalLayoutGroup, CombatSlotManager)
│     ├─ CombatSlot_1 (Image, CombatExecutionSlotUI)
│     │   └─ [Inspector] CombatExecutionSlotUI.Position = SLOT_1
│     ├─ CombatSlot_2 (Image, CombatExecutionSlotUI)
│     │   └─ [Inspector] CombatExecutionSlotUI.Position = SLOT_2
│     ├─ CombatSlot_3 (Image, CombatExecutionSlotUI)
│     │   └─ [Inspector] CombatExecutionSlotUI.Position = SLOT_3
│     └─ CombatSlot_4 (Image, CombatExecutionSlotUI)
│         └─ [Inspector] CombatExecutionSlotUI.Position = SLOT_4
├─ PlayerHandsoltContainer (Empty)
│  ├─ PlayerHandslot1 (Image, PlayerHandCardSlotUI)
│  ├─ PlayerHandslot2 (Image, PlayerHandCardSlotUI)
│  └─ PlayerHandslot3 (Image, PlayerHandCardSlotUI)
├─ Systems (Empty)
│  ├─ SlotRegistry (SlotRegistry, HandSlotRegistry, CombatSlotRegistry, CharacterSlotRegistry)
│  │   └─ [Inspector: SlotRegistry]
│  │      handSlotRegistry = (this) HandSlotRegistry
│  │      combatSlotRegistry = (this) CombatSlotRegistry
│  │      characterSlotRegistry = (this) CharacterSlotRegistry
│  ├─ SlotInitializer (SlotInitializer)
│  ├─ PlayerHandManager (PlayerHandManager)
│  ├─ EnemyHandManager (EnemyHandManager)
│  ├─ PlayerManager (PlayerManager)
│  ├─ EnemyManager (EnemyManager)
│  ├─ EnemySpawnerManager (EnemySpawnerManager)
│  ├─ CardCirculationSystem (CardCirculationSystem)
│  ├─ TurnBasedCardManager (TurnBasedCardManager)
│  ├─ PlayerDeckManager (PlayerDeckManager)
│  ├─ CardRewardManager (CardRewardManager)
│  └─ StageManager (StageManager)
└─ Installer (Empty)
   ├─ CombatInstaller (CombatInstaller)
   │   └─ [Inspector]
   │      cardUIPrefab = Assets/.../SkillCardUI.prefab (선택)
   │      startButtonHandler = (비워도 됨)
   └─ SceneContext (SceneContext)
       └─ [Inspector] Mono Installers = (CombatInstaller)
```

## 인스펙터 필수/권장 연결 상세
| 오브젝트 | 컴포넌트 | 필드 | 값/참조 | [필수] |
|---|---|---|---|---|
| Stage | CanvasScaler | ReferenceResolution | 1920×1080 권장 | 필수 |
| CombatExecutionArea | HorizontalLayoutGroup | Spacing/Align/Control | 32 / MiddleCenter / Off/Off | 필수 |
| CombatExecutionArea | CombatSlotManager | (자동) | 자식의 4개 슬롯 자동 바인딩 | 필수 |
| CombatSlot_1..4 | CombatExecutionSlotUI | Position | SLOT_1..4 | 필수 |
| PlayerHandslot1..3 | PlayerHandCardSlotUI | (기본) | 드래그 시작 지점 | 권장 |
| Systems/SlotRegistry | SlotRegistry | hand/combat/character | (self) 각 레지스트리 | 필수 |
| Systems/PlayerHandManager | PlayerHandManager | (자동) | 핸드 슬롯 바인딩 | 필수 |
| Systems/EnemyHandManager | EnemyHandManager | (자동) | 사용 시 자동 | 선택 |
| Systems/StageManager | StageManager | currentStage | StageData | 권장 |
| Systems/PlayerDeckManager | PlayerDeckManager | (자동) | 카드 덱 관리 | 선택 |
| Systems/CardCirculationSystem | CardCirculationSystem | (자동) | 카드 순환 | 선택 |
| Installer/SceneContext | SceneContext | Mono Installers | CombatInstaller 등록 | 필수 |
| Installer/CombatInstaller | CombatInstaller | cardUIPrefab | SkillCardUI 프리팹 | 권장 |
| Installer/CombatInstaller | CombatInstaller | startButtonHandler | 비워도 됨(즉발) | 선택 |

## ✅ 검증 체크리스트(실행 전/후)
- [ ] Stage → CombatExecutionArea의 자식에 `CombatSlot_1..4` 존재, Position 정확
- [ ] CombatSlotManager가 슬롯 부모에 부착됨(누락 로그 없음)
- [ ] Systems 컨테이너에 SlotRegistry/PlayerHandManager/StageManager 존재
- [ ] SceneContext.MonoInstallers에 CombatInstaller 등록
- [ ] SLOT_1 드랍 제한, 드랍 즉시 실행, 실행 후 2→1/3→2/4→3 이동
- [ ] 적 카드 4번 예약 후 자연 전진
- [ ] 콘솔에 SlotRegistry/StageManager/slotInitializer 관련 에러 0

## 검증/품질 게이트
- [ ] CanvasScaler 1920×1080
- [ ] `CombatExecutionSlotUI.Position`이 `SLOT_1..4`로 정확히 지정
- [ ] 플레이어 카드 `SLOT_1` 외 드랍 불가 확인
- [ ] 실행 후 슬롯 이동 규칙 정상(2→1, 3→2, 4→3)
- [ ] 적 카드 4번 예약 후 자연 전진 확인
- [ ] 콘솔 경고/에러 0
- [ ] 통합 스킬카드 애니메이션 DB 연결 경고 없음

## 성능·메모리 고려사항
- DOTween SafeMode ON, 필요 시 `DOTween.SetTweensCapacity()`로 풀 용량 튜닝.
- Update 사용 최소화, 연출 콜백으로 후속 로직 처리.
- UI 스프라이트는 Sprite Atlas로 배치 줄이기.

## 트러블슈팅
- 카드 드랍이 모든 슬롯에서 되는 경우: `DefaultCardDropValidator` 적용 여부 확인, `CombatExecutionSlotUI.Position` 재검.
- 슬롯이 바인딩되지 않는 경우: `CombatExecutionSlotUI` 활성화 상태/씬 내 중복 확인.
- "통합 스킬카드 애니메이션 데이터베이스를 찾을 수 없습니다": CoreScene의 `AnimationDatabaseManager` 인스펙터에 DB 에셋 직접 연결 또는 `Assets/Resources/Data/Animation/Unified/UnifiedSkillCardAnimationDatabase.asset` 경로 확인.
- "SlotRegistry를 찾을 수 없습니다": `Systems/SlotRegistry` 생성 및 컴포넌트 부착.
- "StageManager를 찾을 수 없습니다": `Systems/StageManager` 생성 및 컴포넌트 부착.
- `slotInitializer가 null입니다`: `CombatInstaller`가 자동 생성한 `SlotInitializer`가 주입되지 않은 경우로, `SceneContext`의 `MonoInstallers`에 `CombatInstaller` 등록 여부 확인.
- "필수 슬롯 누락: SLOT_X": `CombatSlotManager`가 슬롯을 자식에서만 탐색합니다. `CombatSlotManager`가 붙은 오브젝트의 하위에 `CombatSlot_1..4`가 모두 있어야 합니다.

## 변경 기록(Delta)
- 2025-09-15 | Maintainer | StageScene 씬 구조 문서 초안 작성(4-슬롯 즉발 전투 반영) | 문서
- 2025-09-15 | Maintainer | Zenject/Installer 구조와 필수 시스템(SlotRegistry/PlayerHandManager/StageManager) 명시 | 문서


