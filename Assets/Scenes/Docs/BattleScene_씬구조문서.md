# BattleScene 씬 제작 가이드

## 🎯 목표
전투 씬(BattleScene)을 빠르게 제작/재현할 수 있도록 필수 오브젝트 배치, 인스펙터 연결, Zenject 바인딩, 검증 절차를 단계별로 안내합니다.

## 📦 준비물(사전 요구)
- Zenject: SceneContext, CombatInstaller
- 전투 매니저 스크립트: CombatFlowCoordinator, CombatStartupManager, CombatTurnManager, CombatSlotManager, PlayerManager, EnemyManager, GameOverManager
- 슬롯 시스템: SlotRegistry, SlotInitializer, PlayerHandCardSlotUI, EnemyHandCardSlotUI, CombatExecutionSlotUI, CombatSlotPositionHolder, CharacterSlotUI, (컴포넌트) HandSlotRegistry/CombatSlotRegistry/CharacterSlotRegistry
- 카드 UI 프리팹: SkillCardUI (CombatInstaller.cardUIPrefab)
- 스테이지 데이터: StageData (StageManager.currentStage)
- 카메라: Main Camera, UICamera
- UI: UICanvas(CanvasScaler 1920×1080 권장), EventSystem

## 🏗️ 제작 절차(Step-by-Step)
1) 루트 생성
- Main Camera, UICamera, UICanvas(CanvasScaler 1920×1080 권장), EventSystem 추가

2) CombatManager 컨테이너 생성
- 빈 오브젝트 `CombatManager` 생성 후 하위에 다음 오브젝트 추가:
  - CombatFlowCoordinator, CombatStartupManager, CombatTurnManager, CombatSlotManager, PlayerManager, EnemyManager, GameOverManager, AnimationFacade

3) Installer 구성(Zenject)
- `Installer` 컨테이너 📦 생성 후 하위에 `CombatInstaller`, `SceneContext` 배치
- SceneContext.MonoInstallers에 `CombatInstaller` 등록 상태 확인
- CombatInstaller 인스펙터:
  - cardUIPrefab = SkillCardUI 프리팹 참조(필수)
  - startButtonHandler = TurnStartButton 핸들러 참조(필수)

4) 슬롯 UI 구성(UICanvas 하위)
- `CombatArena/PlayerHandCardBackground` 하위에 `PlayerHandCardSlot_1..3` 생성
  - 각 오브젝트에 `PlayerHandCardSlotUI` 부착, position = PLAYER_SLOT_1..3
- `CombatArena/EnemyHandCardBackground` 하위에 `EnemyHandCardSlot_1..3` 생성
  - 각 오브젝트에 `EnemyHandCardSlotUI` 부착, position = ENEMY_SLOT_1..3
- `CombatCardSlotBackground` 하위에 `CombatCardSlot_1..2` 생성
  - 각 오브젝트에 `CombatExecutionSlotUI` 부착
  - `CombatSlotPositionHolder`로 전장 위치(FIELD_LEFT/RIGHT) 지정
- 캐릭터 슬롯: `EnemyCharacterSlot`, `PlayerCharaterSlot`에 `CharacterSlotUI` 부착(owner/slotPosition 설정)

5) 레지스트리/초기화 구성
- 씬에 `SlotRegistry`와 `SlotInitializer` 추가
- SlotRegistry 인스펙터에서 Hand/Combat/Character 레지스트리를 필드 연결

6) 스테이지/적 스폰
- 빈 오브젝트에 `StageManager` 추가, `currentStage`에 `StageData` 연결
- 필요 시 씬 시작 시 `StageManager.SpawnNextEnemy()`가 실행되도록 초기화 스텝 구성 확인(EnemyHandInitializer 등)

7) 버튼(옵션)
- `TurnStartButton`이 필요하면 버튼 생성 후 UI 스타일 설정(현 설계상 onClick 의존 없음)

## 📁 하이라키 예시
```
Main Camera
UICamera
CombatManager
├─ CombatFlowCoordinator ⭐
├─ CombatStartupManager
├─ CombatTurnManager ⭐
├─ CombatSlotManager
├─ PlayerManager
├─ EnemyManager
├─ GameOverManager
├─ Installer 📦
│  ├─ CombatInstaller
│  └─ SceneContext
├─ SlotRegistry
├─ SlotInitializer
└─ AnimationFacade
UICanvas
├─ Background
├─ CombatArena 📦
│  ├─ PlayerHandCard 📦
│  │  └─ PlayerHandCardBackground
│  │     ├─ PlayerHandCardSlot_1 (PlayerHandCardSlotUI)
│  │     ├─ PlayerHandCardSlot_2 (PlayerHandCardSlotUI)
│  │     └─ PlayerHandCardSlot_3 (PlayerHandCardSlotUI)
│  ├─ EnemyHandCard 📦
│  │  └─ EnemyHandCardBackground
│  │     ├─ EnemyHandCardSlot_1 (EnemyHandCardSlotUI)
│  │     ├─ EnemyHandCardSlot_2 (EnemyHandCardSlotUI)
│  │     └─ EnemyHandCardSlot_3 (EnemyHandCardSlotUI)
│  └─ (옵션) TurnStartButton
├─ CombatCardSlotBackground 📦
│  ├─ CombatCardSlot_1 (CombatExecutionSlotUI, CombatSlotPositionHolder)
│  └─ CombatCardSlot_2 (CombatExecutionSlotUI, CombatSlotPositionHolder)
├─ EnemyCharacterSlot (CharacterSlotUI)
└─ PlayerCharaterSlot (CharacterSlotUI)
EventSystem
```

## 🔗 인스펙터 필수 연결 표
| 오브젝트 | 컴포넌트 | 필드 | 값/참조 | [필수] |
|---|---|---|---|---|
| CombatInstaller | CombatInstaller | cardUIPrefab | SkillCardUI 프리팹 | 필수 |
| CombatInstaller | CombatInstaller | startButtonHandler | TurnStartButton 핸들러 | 필수 |
| SceneContext | SceneContext | MonoInstallers | CombatInstaller | 필수 |
| UICanvas | Canvas | camera | UICamera | 필수 |
| PlayerHandCardSlot_1..3 | PlayerHandCardSlotUI | position | PLAYER_SLOT_1..3 | 필수 |
| EnemyHandCardSlot_1..3 | EnemyHandCardSlotUI | position | ENEMY_SLOT_1..3 | 필수 |
| CombatCardSlot_1..2 | CombatExecutionSlotUI | PositionHolder | FIRST/SECOND(+FIELD_LEFT/RIGHT) | 필수 |
| EnemyCharacterSlot | CharacterSlotUI | owner/slotPosition | owner=ENEMY, slot=1 | 필수 |
| PlayerCharaterSlot | CharacterSlotUI | owner/slotPosition | owner=PLAYER, slot=0 | 필수 |
| StageManager | StageManager | currentStage | StageData | 권장 |

## 🧰 컴포넌트별 인스펙터 설정 상세
- CombatInstaller
  - cardUIPrefab: `Assets/.../SkillCardUI.prefab`
  - startButtonHandler: `TurnStartButton` 오브젝트의 `TurnStartButtonHandler` 컴포넌트 참조
- SceneContext
  - MonoInstallers: `CombatInstaller`(씬의 해당 컴포넌트 드래그)
  - AutoRun: On(기본)
- SlotRegistry
  - HandSlotRegistry: 씬 내 Hand 슬롯 레지스트리 컴포넌트 참조
  - CombatSlotRegistry: 씬 내 전투 슬롯 레지스트리 컴포넌트 참조
  - CharacterSlotRegistry: 씬 내 캐릭터 슬롯 레지스트리 컴포넌트 참조
- SlotInitializer
  - (설정 없음) 플레이 시 자동 검색/등록
- PlayerManager
  - playerPrefab: 플레이어 캐릭터 프리팹
  - playerSlot: `PlayerCharaterSlot` RectTransform
  - defaultCharacterData: 기본 캐릭터 Data(optional)
- EnemyManager
  - (필수 필드 없음) StageManager가 RegisterEnemy 호출
- StageManager
  - currentStage: 전투에 사용할 `StageData`
- GameOverManager
  - gameOverUI: 게임오버 UI 루트(패널)
- PlayerHandCardSlotUI / EnemyHandCardSlotUI
  - position: PLAYER_SLOT_1..3 / ENEMY_SLOT_1..3 정확히 매핑
- CombatExecutionSlotUI
  - Position: FIRST/SECOND
  - PositionHolder(별도 컴포넌트): FIELD_LEFT/RIGHT 설정
- CharacterSlotUI
  - owner: PLAYER/ENEMY
  - slotPosition: 0(플레이어)/1(적 등, 프로젝트 규칙에 맞게)
- AnimationFacade
  - 루트 오브젝트로 배치하거나 DontDestroyOnLoad 사용 지양(경고 방지)

## 🧱 실제 제작용 완전 하이라키(정확한 이름/컴포넌트/필드)
아래 트리를 그대로 만들면 SlotRegistry/StageManager/Installer 바인딩까지 포함해 곧바로 전투가 동작합니다.
```
Main Camera (Camera, UniversalAdditionalCameraData, AudioListener)
UICamera (Camera, UniversalAdditionalCameraData)
CombatManager (Empty)
├─ CombatFlowCoordinator (CombatFlowCoordinator)
├─ CombatStartupManager (CombatStartupManager)
├─ CombatTurnManager (CombatTurnManager)
├─ CombatSlotManager (CombatSlotManager)
├─ PlayerManager (PlayerManager)
│   ├─ [Inspector]
│   │   - playerPrefab = (Player Character Prefab)
│   │   - playerSlot = UICanvas/PlayerCharaterSlot (RectTransform)
│   │   - defaultCharacterData = (옵션)
├─ EnemyManager (EnemyManager)
├─ GameOverManager (GameOverManager)
│   ├─ [Inspector]
│   │   - gameOverUI = (GameOver UI Root)
├─ Installer (Empty)
│   ├─ CombatInstaller (CombatInstaller)
│   │   ├─ [Inspector]
│   │   │   - cardUIPrefab = Assets/.../SkillCardUI.prefab
│   │   │   - startButtonHandler = UICanvas/CombatArena/TurnStartButton/TurnStartButtonHandler
│   └─ SceneContext (SceneContext)
│       ├─ [Inspector]
│       │   - MonoInstallers = (CombatInstaller)
├─ SlotRegistry (SlotRegistry, HandSlotRegistry, CombatSlotRegistry, CharacterSlotRegistry)
│   ├─ [Inspector: SlotRegistry]
│   │   - handSlotRegistry = (this) HandSlotRegistry
│   │   - combatSlotRegistry = (this) CombatSlotRegistry
│   │   - characterSlotRegistry = (this) CharacterSlotRegistry
├─ SlotInitializer (SlotInitializer)
└─ AnimationFacade (AnimationFacade)

UICanvas (Canvas, CanvasScaler, GraphicRaycaster)
├─ Background (Image)
├─ CombatArena (Empty)
│   ├─ PlayerHandCard (Empty)
│   │   └─ PlayerHandCardBackground (Image)
│   │       ├─ PlayerHandCardSlot_1 (Image, PlayerHandCardSlotUI)
│   │       │   └─ [Inspector] position = PLAYER_SLOT_1
│   │       ├─ PlayerHandCardSlot_2 (Image, PlayerHandCardSlotUI)
│   │       │   └─ [Inspector] position = PLAYER_SLOT_2
│   │       └─ PlayerHandCardSlot_3 (Image, PlayerHandCardSlotUI)
│   │           └─ [Inspector] position = PLAYER_SLOT_3
│   ├─ EnemyHandCard (Empty)
│   │   └─ EnemyHandCardBackground (Image)
│   │       ├─ EnemyHandCardSlot_1 (Image, EnemyHandCardSlotUI)
│   │       │   └─ [Inspector] position = ENEMY_SLOT_1
│   │       ├─ EnemyHandCardSlot_2 (Image, EnemyHandCardSlotUI)
│   │       │   └─ [Inspector] position = ENEMY_SLOT_2
│   │       └─ EnemyHandCardSlot_3 (Image, EnemyHandCardSlotUI)
│   │           └─ [Inspector] position = ENEMY_SLOT_3
│   └─ TurnStartButton (Button, Image, TMP_Text, TurnStartButtonHandler)
├─ CombatCardSlotBackground (Empty)
│   ├─ CombatCardSlot_1 (Image, CombatExecutionSlotUI, CombatSlotPositionHolder)
│   │   ├─ [Inspector]
│   │   │   - CombatExecutionSlotUI.Position = FIRST
│   │   │   - CombatSlotPositionHolder.FieldPosition = FIELD_LEFT
│   └─ CombatCardSlot_2 (Image, CombatExecutionSlotUI, CombatSlotPositionHolder)
│       ├─ [Inspector]
│       │   - CombatExecutionSlotUI.Position = SECOND
│       │   - CombatSlotPositionHolder.FieldPosition = FIELD_RIGHT
├─ EnemyCharacterSlot (Image, CharacterSlotUI)
│   └─ [Inspector] owner = ENEMY, slotPosition = 1
└─ PlayerCharaterSlot (Image, CharacterSlotUI)
    └─ [Inspector] owner = PLAYER, slotPosition = 0

EventSystem (EventSystem, InputSystemUIInputModule)

StageManager (StageManager)
└─ [Inspector] currentStage = Assets/.../StageData.asset
```

## ✅ 검증 체크리스트
- [ ] 콘솔에 SlotRegistry/StageManager/Installer 관련 에러 없음
- [ ] CombatInstaller 인스펙터 2개 필드(cardUIPrefab/startButtonHandler) 채워짐
- [ ] 초기화 스텝 로그가 순서대로 출력됨(StartupManager)
- [ ] 적 스폰/핸드 등록/전투 슬롯 등록/턴 전환이 정상 동작
- [ ] `AnimationFacade`는 루트 오브젝트이거나 DontDestroyOnLoad 경고 없음

## 🧩 자주 발생하는 오류와 해결
- SlotRegistry를 찾을 수 없습니다 → 씬에 SlotRegistry 배치, 인스펙터 레지스트리 연결
- StageManager 바인딩 실패 → 씬에 StageManager 배치, currentStage 연결
- cardUIPrefab 미할당 → CombatInstaller에 SkillCardUI 프리팹 연결
- startButtonHandler 미할당 → TurnStartButton 핸들러 컴포넌트 연결
- DontDestroyOnLoad 경고 → AnimationFacade를 루트로 이동하거나 호출 제거

## 📝 변경 기록(Delta)
- 2025-09-08: 문서를 씬 제작 가이드 형식으로 전환(구성/체크리스트/오류 해결 추가)
- 2025-09-08: 컴포넌트별 인스펙터 상세 설정 추가
- 2025-09-08: "실제 제작용 완전 하이라키" 섹션 추가(정확한 이름/컴포넌트/필드 명시)
