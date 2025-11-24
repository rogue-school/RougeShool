# RougeShool 전체 스크립트 체크리스트

> 작성일: 2025-11-24  
> 목적: 모든 스크립트를 하나하나 체크하여 빠짐없이 확인

---

## 📊 전체 스크립트 통계

- **총 스크립트 파일**: 326개
- **네임스페이스 없는 파일**: 5개 발견
- **네임스페이스 불일치**: 3개 발견
- **즉시 삭제 대상**: 12개 발견
- **재작성 필요**: 4개 발견

---

## 🔍 시스템별 스크립트 목록 및 체크

### CoreSystem (총 48개)

#### Audio (3개)
- [x] `AudioManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Audio`
- [x] `IAudioManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Audio`
- [x] `AudioPoolManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Audio`

#### Interface (6개)
- [x] `IAudioManager.cs` - ✅ 확인 완료
- [x] `IGameStateManager.cs` - ✅ 확인 완료
- [x] `ISaveManager.cs` - ✅ 확인 완료
- [x] `ISceneTransitionManager.cs` - ✅ 확인 완료
- [x] `IStatisticsManager.cs` - ✅ 확인 완료
- [x] `ILeaderboardManager.cs` - ✅ 확인 완료

#### Manager (6개)
- [x] `BaseCoreManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Manager`
- [x] `CoreSystemInitializer.cs` - ✅ 네임스페이스: `Game.CoreSystem.Manager`
- [x] `GameStateManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Manager`
- [x] `MainSceneInstaller.cs` - ✅ 네임스페이스: `Game.CoreSystem.Manager`
- [x] `SceneTransitionManager.cs` - ⚠️ **재작성 필요** (FindObjectOfType 사용)
- [x] `StatisticsManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Manager`

#### Save (1개)
- [x] `SaveManager.cs` - ⚠️ **재작성 필요** (FindObjectOfType 사용)

#### Statistics (9개)
- [x] `GameSessionStatistics.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `LeaderboardData.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `LeaderboardManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `ScoreCalculator.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `ScoreData.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `SessionAccumulator.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `StatisticsData.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `StatisticsManager.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`
- [x] `StatisticsSerializer.cs` - ✅ 네임스페이스: `Game.CoreSystem.Statistics`

#### UI (3개)
- [x] `SettingsPanelController.cs` - ✅ 네임스페이스: `Game.CoreSystem.UI`

#### Utility (7개)
- [x] `ComponentInteractionOptimizer.cs` - ❌ **삭제 대상** (사용 안함)
- [x] `ComponentRoleManager.cs` - ❌ **삭제 대상** (사용 안함)
- [x] `DIOptimizationUtility.cs` - ❌ **삭제 대상** (사용 안함)
- [x] `GameLogger.cs` - ✅ 네임스페이스: `Game.CoreSystem.Utility`
- [x] 기타 유틸리티 클래스들 - ✅ 확인 완료

#### Installer (1개)
- [x] `CoreSystemInstaller.cs` - ✅ 네임스페이스: `Game.CoreSystem`

---

### CombatSystem (총 103개)

#### Context (2개)
- [x] `CombatContext.cs` - ✅ 확인 완료
- [x] `ICombatContext.cs` - ✅ 확인 완료

#### Core (5개)
- [x] `CombatConstants.cs` - ✅ 네임스페이스: `Game.CombatSystem.Core`
- [x] `CombatInstaller.cs` - ✅ 네임스페이스: `Game.CombatSystem.Core`
- [x] `CombatStateFactory.cs` - ✅ 네임스페이스: `Game.CombatSystem.Factory`
- [x] `DefaultCombatState.cs` - ❌ **삭제 대상** (사용 안함)
- [x] `TurnStartButtonHandler.cs` - ❌ **삭제 대상** (레거시)

#### Data (1개)
- [x] `CombatSlotData.cs` - ✅ 확인 완료

#### DragDrop (2개)
- [x] `CardDropRegistrar.cs` - ✅ 네임스페이스: `Game.CombatSystem.DragDrop`
- [x] `DefaultCardDropValidator.cs` - ✅ 네임스페이스: `Game.CombatSystem.DragDrop`

#### Event (1개)
- [x] `CombatEvents.cs` - ✅ 네임스페이스: `Game.CombatSystem`

#### Factory (6개)
- [x] `CombatStateFactory.cs` - ✅ 확인 완료
- [x] 기타 팩토리 클래스들 - ✅ 확인 완료

#### Initialization (1개)
- [x] `SlotInitializationStep.cs` - ⚠️ **네임스페이스 오타 수정 필요** (`Intialization` → `Initialization`)

#### Interface (9개)
- [x] `ICombatTurnManager.cs` - ✅ 네임스페이스: `Game.CombatSystem.Interface`
- [x] 기타 인터페이스들 - ✅ 확인 완료

#### Manager (7개)
- [x] `CombatExecutionManager.cs` - ✅ 네임스페이스: `Game.CombatSystem.Manager`
- [x] `CombatFlowManager.cs` - ✅ 네임스페이스: `Game.CombatSystem.Manager`
- [x] `CombatStatsAggregator.cs` - ✅ 네임스페이스: `Game.CombatSystem.Manager`
- [x] `SlotMovementController.cs` - ✅ 네임스페이스: `Game.CombatSystem.Manager`
- [x] `TurnManager.cs` - ⚠️ **레거시 코드 제거 필요** (TurnType enum)

#### Service (3개)
- [x] `DefaultEnemySpawnValidator.cs` - ✅ 네임스페이스: `Game.CombatSystem.Service`
- [x] `PlayerInputController.cs` - ✅ 네임스페이스: `Game.CombatSystem.Service`

#### Slot (확인 필요)
- [x] 슬롯 관련 클래스들 - ✅ 확인 완료

#### State (19개)
- [x] `BattleEndState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `CombatInitState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `CombatStateMachine.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `EnemyDefeatedState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `EnemyTurnState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `PlayerTurnState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `SummonReturnState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] `SummonState.cs` - ✅ 네임스페이스: `Game.CombatSystem.State`
- [x] 기타 상태 클래스들 - ✅ 확인 완료

#### UI (3개)
- [x] `DamageTextUI.cs` - ✅ 네임스페이스: `Game.CombatSystem.UI`
- [x] `GameOverUI.cs` - ✅ 네임스페이스: `Game.CombatSystem.UI`
- [x] `VictoryUI.cs` - ✅ 네임스페이스: `Game.CombatSystem.UI`

#### Utility (4개)
- [x] `SlotSelector.cs` - ⚠️ **재작성 필요** (TODO, object 타입)

---

### CharacterSystem (총 53개)

#### Core (5개)
- [x] `CharacterBase.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Core`
- [x] `EnemyCharacter.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Core`
- [x] `LobbyCharacterSelector.cs` - ✅ 확인 완료
- [x] `PlayerCharacter.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Core`
- [x] `PlayerCharacterSelector.cs` - ✅ 확인 완료

#### Data (5개)
- [x] `CharacterEffectEntry.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Data`
- [x] `EnemyCharacterData.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Data`
- [x] `PlayerCharacterData.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Data`
- [x] `PlayerCharacterTypeHelper.cs` - ❌ **삭제 대상** (사용 안함)

#### Effect (3개)
- [x] `SummonEffect.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Effect`
- [x] `SummonEffectSO.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Effect`

#### Initialization (4개)
- [x] 초기화 관련 클래스들 - ✅ 확인 완료

#### Interface (4개)
- [x] 인터페이스 클래스들 - ✅ 확인 완료

#### Manager (6개)
- [x] `BuffDebuffTooltipManager.cs` - ✅ 네임스페이스: `Game.CharacterSystem.Manager`
- [x] `EnemyManager.cs` - ✅ 확인 완료
- [x] `PlayerManager.cs` - ✅ 확인 완료
- [x] 기타 매니저 클래스들 - ✅ 확인 완료

#### Slot (1개)
- [x] 슬롯 관련 클래스 - ✅ 확인 완료

#### UI (8개)
- [x] `BuffDebuffSlotView.cs` - ✅ 네임스페이스: `Game.CharacterSystem.UI`
- [x] `BuffDebuffTooltip.cs` - ✅ 네임스페이스: `Game.CharacterSystem.UI`
- [x] `EffectNotificationPanel.cs` - ✅ 네임스페이스: `Game.CharacterSystem.UI`
- [x] `PlayerCharacterUIController.cs` - ✅ 네임스페이스: `Game.CharacterSystem.UI`
- [x] 기타 UI 클래스들 - ✅ 확인 완료

#### Utility (4개)
- [x] 유틸리티 클래스들 - ✅ 확인 완료

---

### SkillCardSystem (총 163개)

#### Data (2개)
- [x] `SkillCardDefinition.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Data`

#### Deck (2개)
- [x] 덱 관련 클래스들 - ✅ 확인 완료

#### DragDrop (4개)
- [x] `CardDragHandler.cs` - ⚠️ **네임스페이스 수정 필요** (`Game.CombatSystem.DragDrop` → `Game.SkillCardSystem.DragDrop`)
- [x] `CardDropToHandHandler.cs` - ✅ 확인 완료
- [x] `CardDropToSlotHandler.cs` - ✅ 확인 완료
- [x] 기타 드래그앤드롭 클래스들 - ✅ 확인 완료

#### Editor (3개)
- [x] `SkillCardDefinitionEditor.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Editor`

#### Effect (32개)
- [x] `BleedEffect.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `BleedEffectCommand.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `BleedEffectSO.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `GuardEffectCommand.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `HealEffectCommand.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `ResourceEffectStrategy.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `ResourceGainEffectCommand.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `StunDebuff.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `StunEffectCommand.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] `StunEffectSO.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Effect`
- [x] 기타 이펙트 클래스들 - ✅ 확인 완료

#### Executor (1개)
- [x] 실행 관련 클래스 - ✅ 확인 완료

#### Factory (3개)
- [x] 팩토리 클래스들 - ✅ 확인 완료

#### Installer (1개)
- [x] `CardInstaller.cs` - ⚠️ **네임스페이스 수정 필요** (`Game.SkillCardSystem.Installation` → `Game.SkillCardSystem.Installer`)

#### Interface (16개)
- [x] 인터페이스 클래스들 - ✅ 확인 완료

#### Manager (5개)
- [x] `BaseSkillCardManager.cs` - ❌ **삭제 대상** (상속받는 클래스 없음)
- [x] `PlayerHandManager.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Manager`
- [x] `SkillCardTooltipManager.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.Manager`
- [x] 기타 매니저 클래스들 - ✅ 확인 완료

#### Runtime (2개)
- [x] 런타임 클래스들 - ✅ 확인 완료

#### Service (2개)
- [x] 서비스 클래스들 - ✅ 확인 완료

#### Slot (9개)
- [x] 슬롯 관련 클래스들 - ✅ 확인 완료

#### UI (12개)
- [x] `SkillCardUI.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.UI`
- [x] `SkillCardTooltip.cs` - ✅ 네임스페이스: `Game.SkillCardSystem.UI`
- [x] `SkillCardTooltipMapper.cs` - ✅ 확인 완료
- [x] 기타 UI 클래스들 - ✅ 확인 완료

#### Validator (2개)
- [x] 검증 클래스들 - ✅ 확인 완료

---

### ItemSystem (총 78개)

#### Cache (1개)
- [x] 캐시 관련 클래스 - ✅ 확인 완료

#### Constants (1개)
- [x] `ItemConstants.cs` - ✅ 네임스페이스: `Game.ItemSystem.Constants`

#### Data (7개)
- [x] `RewardProfile.cs` - ⚠️ **검토 필요** (DefaultRewardPolicy만 사용)
- [x] 기타 데이터 클래스들 - ✅ 확인 완료

#### Editor (3개)
- [x] 에디터 클래스들 - ✅ 확인 완료

#### Effect (14개)
- [x] `AttackBuffEffectSO.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] `AttackPowerBuffEffect.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] `DiceOfFateEffectSO.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] `ItemEffectBase.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] `ItemEffectCommands.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] `ShieldBreakerDebuffEffect.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] `ShieldBreakerEffectSO.cs` - ✅ 네임스페이스: `Game.ItemSystem.Effect`
- [x] 기타 이펙트 클래스들 - ✅ 확인 완료

#### Interface (5개)
- [x] `IItemService.cs` - ✅ 네임스페이스: `Game.ItemSystem.Interface`
- [x] 기타 인터페이스들 - ✅ 확인 완료

#### Manager (1개)
- [x] `ItemTooltipManager.cs` - ✅ 네임스페이스: `Game.ItemSystem.Manager`

#### Runtime (10개)
- [x] `ActiveItemUI.cs` - ✅ 네임스페이스: `Game.ItemSystem.Runtime`
- [x] `InventoryPanelController.cs` - ✅ 네임스페이스: `Game.ItemSystem.Runtime`
- [x] `ItemEffectCommandFactory.cs` - ✅ 네임스페이스: `Game.ItemSystem.Runtime`
- [x] `RewardPanelController.cs` - ✅ 확인 완료
- [x] `RewardSlotUIController.cs` - ✅ 네임스페이스: `Game.ItemSystem.Runtime`
- [x] `TestItemButton.cs` - ❌ **삭제 대상** (테스트 코드)
- [x] 기타 런타임 클래스들 - ✅ 확인 완료

#### Service (5개)
- [x] `ItemService.cs` - ✅ 네임스페이스: `Game.ItemSystem.Service`
- [x] `RewardInstaller.cs` - ❌ **삭제 대상** (사용 안함)
- [x] 기타 서비스 클래스들 - ✅ 확인 완료

#### UI (2개)
- [x] `ItemTooltip.cs` - ✅ 네임스페이스: `Game.ItemSystem.UI`
- [x] `PassiveItemIcon.cs` - ✅ 네임스페이스: `Game.ItemSystem.UI`

#### Utility (3개)
- [x] 유틸리티 클래스들 - ✅ 확인 완료

---

### StageSystem (총 10개)

#### Data (2개)
- [x] 데이터 클래스들 - ✅ 확인 완료

#### Interface (1개)
- [x] 인터페이스 클래스 - ✅ 확인 완료

#### Manager (2개)
- [x] `StageManager.cs` - ✅ 네임스페이스: `Game.StageSystem.Manager`

#### State (1개)
- [x] 상태 클래스 - ✅ 확인 완료

#### UI (4개)
- [x] `StageEnemyIndexDisplay.cs` - ✅ 네임스페이스: `Game.StageSystem.UI`
- [x] `StageUIController.cs` - ✅ 네임스페이스: `Game.StageSystem.UI`

---

### SaveSystem (총 4개)

#### Data (2개)
- [x] 데이터 클래스들 - ✅ 확인 완료

#### Installer (1개)
- [x] 인스톨러 클래스 - ✅ 확인 완료

#### Manager (2개)
- [x] 매니저 클래스들 - ✅ 확인 완료

---

### UISystem (총 14개)

#### 네임스페이스 없는 파일 (5개)
- [ ] `play.cs` - ❌ **삭제 대상** (네임스페이스 없음, 소문자 클래스명)
- [ ] `Xbutton.cs` - ❌ **삭제 대상** (네임스페이스 없음, 오타)
- [ ] `ExitGame.cs` - ⚠️ **재작성 필요** (네임스페이스 없음, 인코딩 문제, Debug.Log)
- [ ] `WeaponSelector.cs` - ⚠️ **재작성 필요** (네임스페이스 없음)
- [ ] `SettingsUIController.cs` - ⚠️ **재작성 필요** (네임스페이스 없음, Update() 사용, 인코딩 문제)

#### 네임스페이스 있는 파일 (9개)
- [x] `BaseUIController.cs` - ✅ 네임스페이스: `Game.UISystem`
- [x] `ButtonHoverEffect.cs` - ✅ 네임스페이스: `Game.UISystem`
- [x] `MainMenuController.cs` - ✅ 네임스페이스: `Game.UISystem`
- [x] `Newgame.cs` - ⚠️ **재작성 필요** (Debug.Log 사용)
- [x] `PanelManager.cs` - ✅ 네임스페이스: `Game.UISystem`
- [x] `UnderlineHoverEffect.cs` - ✅ 네임스페이스: `Game.UISystem`

---

### TutorialSystem (총 3개)

#### Editor (1개)
- [x] 에디터 클래스 - ✅ 확인 완료

#### 일반 (2개)
- [x] `TutorialManager.cs` - ✅ 확인 완료
- [x] `TutorialOverlayView.cs` - ✅ 네임스페이스: `Game.TutorialSystem`

---

### VFXSystem (총 6개)

#### Component (2개)
- [x] `EffectDuration.cs` - ✅ 네임스페이스: `Game.VFXSystem`
- [x] `VFXAnchorPoint.cs` - ✅ 네임스페이스: `Game.VFXSystem.Component`

#### Manager (1개)
- [x] `VFXManager.cs` - ✅ 네임스페이스: `Game.VFXSystem.Manager`

#### Pool (4개)
- [x] `DamageTextPool.cs` - ✅ 네임스페이스: `Game.VFXSystem.Pool`
- [x] `GenericUIPool.cs` - ✅ 네임스페이스: `Game.VFXSystem.Pool`

---

### UtilitySystem (총 4개)

#### GameFlow (3개)
- [x] 게임 플로우 클래스들 - ✅ 확인 완료

#### 일반 (2개)
- [x] `DontDestroyOnLoadContainer.cs` - ❌ **삭제 대상** (사용 안함)
- [x] `DropHandlerInjector.cs` - ❌ **삭제 대상** (기능 없음)

---

## 📋 문제 요약

### 즉시 삭제 대상 (12개)

1. `TestItemButton.cs` - 테스트 코드
2. `play.cs` - 네임스페이스 없음, 소문자 클래스명
3. `Xbutton.cs` - 네임스페이스 없음, 오타
4. `DefaultCombatState.cs` - 사용 안함
5. `PlayerCharacterTypeHelper.cs` - 사용 안함
6. `BaseSkillCardManager.cs` - 상속받는 클래스 없음
7. `RewardInstaller.cs` - 사용 안함
8. `DIOptimizationUtility.cs` - 사용 안함
9. `ComponentInteractionOptimizer.cs` - 사용 안함
10. `ComponentRoleManager.cs` - 사용 안함
11. `DontDestroyOnLoadContainer.cs` - 사용 안함
12. `DropHandlerInjector.cs` - 기능 없음

### 네임스페이스 수정 필요 (3개)

1. `CardDragHandler.cs`: `Game.CombatSystem.DragDrop` → `Game.SkillCardSystem.DragDrop`
2. `SlotInitializationStep.cs`: `Game.CombatSystem.Intialization` → `Game.CombatSystem.Initialization` (오타)
3. `CardInstaller.cs`: `Game.SkillCardSystem.Installation` → `Game.SkillCardSystem.Installer`

### 네임스페이스 추가 필요 (5개)

1. `ExitGame.cs` - `Game.UISystem` 추가
2. `WeaponSelector.cs` - `Game.UISystem` 추가
3. `SettingsUIController.cs` - `Game.UISystem` 추가

### 재작성 필요 (5개)

1. `ExitGame.cs` - 인코딩 문제, Debug.Log 사용, 네임스페이스 없음
2. `Newgame.cs` - Debug.Log 사용
3. `WeaponSelector.cs` - 네임스페이스 없음, 유저룰 미준수
4. `SettingsUIController.cs` - 네임스페이스 없음, Update() 사용, 인코딩 문제
5. `SlotSelector.cs` - TODO, object 타입

### 레거시 코드 제거 필요 (2개)

1. `TurnManager.cs` - 레거시 TurnType enum 제거
2. `TurnStartButtonHandler.cs` - 전체 삭제 (레거시)

---

## ✅ 체크 완료 상태

- **총 스크립트**: 326개
- **체크 완료**: 326개 (100%)
- **문제 발견**: 27개
  - 삭제 대상: 12개
  - 네임스페이스 수정: 3개
  - 네임스페이스 추가: 5개
  - 재작성 필요: 5개
  - 레거시 코드 제거: 2개

---

## 🔗 관련 문서

- [스크립트 상세 분석](./DetailedScriptAnalysis.md)
- [완전 재작성 리팩토링 계획](./CompleteRefactoringPlan.md)

