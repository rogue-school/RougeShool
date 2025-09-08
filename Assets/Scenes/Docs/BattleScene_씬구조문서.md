# BattleScene 씬 구조 문서

## 목차
- [Quick-Scan 요약](#quick-scan-요약)
- [하이라키 트리](#하이라키-트리)
- [컨테이너/정렬 규칙](#컨테이너정렬-규칙)
- [필수 오브젝트](#필수-오브젝트)
- [핵심 설정값 표](#핵심-설정값-표)
- [씬 제작 절차](#씬-제작-절차)
- [인스펙터 연결 표](#인스펙터-연결-표)
- [시스템 연동 포인트](#시스템-연동-포인트)
- [변경 가이드](#변경-가이드)
- [검증 체크리스트](#검증-체크리스트)
- [변경 기록(Delta)](#변경-기록delta)

## Quick-Scan 요약
- 루트 순서: Main Camera → UICamera → CombatManager → UICanvas → EventSystem
- 전투 핵심: CombatFlowCoordinator ⭐, CombatTurnManager ⭐, CombatSlotManager, PlayerManager/EnemyManager
- Installer: SceneContext + CombatInstaller (cardUIPrefab DI로 주입)
- UI 핵심: CombatArena(플레이어/적 핸드 슬롯), CombatCardSlotBackground(전투 슬롯 1/2)
- 슬롯 컴포넌트: PlayerHandCardSlotUI/EnemyHandCardSlotUI, CombatExecutionSlotUI(+CombatSlotPositionHolder)
- Zenject: SceneContext 활성, CombatInstaller 자동 실행

## 하이라키 트리
```
Main Camera (Camera, UniversalAdditionalCameraData, AudioListener)
UICamera (Camera, UniversalAdditionalCameraData)
CombatManager (Empty)
├─ CombatFlowCoordinator (CombatFlowCoordinator) ⭐
├─ CombatStartupManager (CombatStartupManager)
├─ CombatTurnManager (CombatTurnManager) ⭐
├─ CombatSlotManager (CombatSlotManager)
├─ PlayerManager (PlayerManager)
├─ EnemyManager (EnemyManager)
├─ GameOverManager (GameOverManager)
├─ Installer (컨테이너) 📦
│  ├─ CombatInstaller (CombatInstaller)
│  └─ SceneContext (SceneContext)
├─ SlotRegistry (SlotRegistry)
├─ SlotInitializer (SlotInitializer)
└─ AnimationFacade (AnimationFacade)
UICanvas (Canvas, CanvasScaler, GraphicRaycaster)
├─ Background (Image)
├─ CombatArena (컨테이너) 📦
│  ├─ PlayerHandCard (컨테이너) 📦
│  │  └─ PlayerHandCardBackground (Image)
│  │     ├─ PlayerHandCardSlot_1 (Image, PlayerHandCardSlotUI)
│  │     ├─ PlayerHandCardSlot_2 (Image, PlayerHandCardSlotUI)
│  │     └─ PlayerHandCardSlot_3 (Image, PlayerHandCardSlotUI)
│  ├─ EnemyHandCard (컨테이너) 📦
│  │  └─ EnemyHandCardBackground (Image)
│  │     ├─ EnemyHandCardSlot_1 (Image, EnemyHandCardSlotUI)
│  │     ├─ EnemyHandCardSlot_2 (Image, EnemyHandCardSlotUI)
│  │     └─ EnemyHandCardSlot_3 (Image, EnemyHandCardSlotUI)
│  └─ (옵션) TurnStartButton (Button, Image, TMP_Text)
├─ CombatCardSlotBackground (컨테이너) 📦
│  ├─ CombatCardSlot_1 (Image, CombatExecutionSlotUI, CombatSlotPositionHolder)
│  └─ CombatCardSlot_2 (Image, CombatExecutionSlotUI, CombatSlotPositionHolder)
├─ EnemyCharacterSlot (CharacterSlotUI, Image)
└─ PlayerCharaterSlot (CharacterSlotUI, Image)
EventSystem (EventSystem, InputSystemUIInputModule)
```

## 컨테이너/정렬 규칙
- 루트 정렬(위→아래): Main Camera → UICamera → CombatManager → UICanvas → EventSystem
- CombatManager 내부: Flow/Startup/Turn → Slot → Player/Enemy → GameOver → Installer → Animation 순
- UICanvas 내부: Background → CombatArena → CombatCardSlotBackground → EnemyCharacterSlot → PlayerCharaterSlot → TurnStartButton(CombatArena 하위)

## 필수 오브젝트
- Main Camera, UICamera, CombatManager(하위 구성 포함), UICanvas, EventSystem
- 누락 시: 입력/카드 배치/턴 진행/버튼 트리거 등 전투 진행 불가

## 핵심 설정값 표
| 항목 | 값 | 비고 |
|---|---|---|
| CanvasScaler.ReferenceResolution | 1920×1080 권장 | 현재 UICanvas 값 확인 필요 |
| (슬롯) PlayerHand.position | PLAYER_SLOT_1..3 | Enum: SkillCardSlotPosition |
| (슬롯) EnemyHand.position | ENEMY_SLOT_1..3 | Enum: SkillCardSlotPosition |
| (전투 슬롯) CombatSlot.Position | FIRST/SECOND | Enum: CombatSlotPosition |
| (전장 위치) FieldSlot.Position | FIELD_LEFT/RIGHT | Enum: CombatFieldSlotPosition |
| SceneContext.AutoRun | On | Installer 실행 |

## 씬 제작 절차
1) 루트 생성
- Main Camera, UICamera, CombatManager(Empty), UICanvas(CanvasScaler 1920×1080 권장), EventSystem 생성

2) CombatManager 구성
- 하위에 다음 컴포넌트 오브젝트 생성: `CombatFlowCoordinator`, `CombatStartupManager`, `CombatTurnManager`, `CombatSlotManager`, `PlayerManager`, `EnemyManager`, `GameOverManager`, `AnimationFacade`

3) Installer 컨테이너 구성
- `Installer` 컨테이너 📦 생성 후 `SceneContext`, `CombatInstaller` 배치
- `SceneContext.MonoInstallers`에 `CombatInstaller` 등록

4) UICanvas 구성(슬롯/버튼)
- `CombatArena/PlayerHandCardBackground` 하위에 `PlayerHandCardSlot_1..3` 생성 후 각 오브젝트에 `PlayerHandCardSlotUI` 부착, `position=PLAYER_SLOT_1..3`
- `CombatArena/EnemyHandCardBackground` 하위에 `EnemyHandCardSlot_1..3` 생성 후 각 오브젝트에 `EnemyHandCardSlotUI` 부착, `position=ENEMY_SLOT_1..3`
- `CombatCardSlotBackground` 하위의 `CombatCardSlot_1..2`에 `CombatExecutionSlotUI`와 `CombatSlotPositionHolder` 부착
  - CombatExecutionSlotUI.Position = FIRST / SECOND
  - CombatSlotPositionHolder.FieldPosition = FIELD_LEFT / FIELD_RIGHT
- (옵션) `TurnStartButton`이 필요하면 버튼 오브젝트를 생성하고 UI 스타일만 지정(현재 onClick 의존 없음)

5) 슬롯 레지스트리/초기화
- 씬에 `SlotRegistry` 배치 후 인스펙터 필드에 `HandSlotRegistry/CombatSlotRegistry/CharacterSlotRegistry` 연결
- `SlotInitializer`를 씬에 추가(씬 내 슬롯들을 자동 검색/등록)

6) DI/프리팹
- `CombatInstaller`에서 카드 UI 프리팹(`SkillCardUI`)을 바인딩(또는 인스펙터 연결)하여 `PlayerHandCardSlotUI`가 DI로 프리팹을 주입받도록 구성

7) 재생 전 검증(간단)
- 플레이 시 콘솔에서 슬롯 등록/상태 전이 로그 확인, 카드 드로우/등록/실행 경로에 에러가 없는지 점검

## 인스펙터 연결 표
| 오브젝트 | 컴포넌트 | 필드 | 값/참조 | [필수] |
|---|---|---|---|---|
| CombatInstaller | CombatInstaller | (cardUIPrefab) | DI로 바인딩됨 | 필수(DI) |
| SceneContext | SceneContext | MonoInstallers | CombatInstaller | 필수(연결됨) |
| PlayerHandCardSlot_1..3 | PlayerHandCardSlotUI | position | PLAYER_SLOT_1..3 | 필수 |
| EnemyHandCardSlot_1..3 | EnemyHandCardSlotUI | position | ENEMY_SLOT_1..3 | 필수 |
| CombatCardSlot_1..2 | CombatExecutionSlotUI | PositionHolder | FIRST/SECOND(+FIELD_LEFT/RIGHT) | 필수 |
| EnemyCharacterSlot | CharacterSlotUI | owner/slotPosition | owner=ENEMY, slotPosition=1 | 필수(연결/설정) |
| PlayerCharaterSlot | CharacterSlotUI | owner/slotPosition | owner=PLAYER, slotPosition=0 | 필수(연결/설정) |
| UICanvas | Canvas | camera | UICamera | 필수(연결됨) |

## 시스템 연동 포인트
- 턴/상태: CombatFlowCoordinator ↔ CombatTurnManager ↔ StateFactory
- 슬롯/레지스트리: SlotRegistry(Hand/Combat/Character) ↔ SlotInitializer ↔ SkillCardSystem
- 초기화: CombatStartupManager → 초기화 스텝들(SlotInitializationStep 등) 순차 실행
- 애니메이션: AnimationFacade 통해 카드/슬롯/캐릭터 애니메이션 호출

## 변경 가이드
- 슬롯 수/포지션 변경 시 모든 position/owner 값 일관성 유지
- Installer 필드(cardUIPrefab 등) 변경 시 PR 검증 표 반영

## 검증 체크리스트
- [x] 루트 순서와 컨테이너 정렬이 문서와 동일
- [ ] PlayerHandCardSlotUI/EnemyHandCardSlotUI에 올바른 position 설정
- [ ] CombatExecutionSlotUI + CombatSlotPositionHolder로 전투 슬롯 위치 설정
- [ ] SceneContext/CombatInstaller/SlotRegistry/SlotInitializer 존재 및 동작 확인
- [ ] 플레이 시 턴 진행/카드 실행 경로 에러 없음

## 변경 기록(Delta)
- 2025-09-08: 최초 작성(룰 적용, 실제 하이라키 기반 문서화)
