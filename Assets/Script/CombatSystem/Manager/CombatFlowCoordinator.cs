using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Executor;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Service;
using Game.CombatSystem.Context;
using Game.CombatSystem.Executor;

namespace Game.CombatSystem.Core
{
    public class CombatFlowCoordinator : MonoBehaviour, ICombatFlowCoordinator
    {
        private ISlotRegistry slotRegistry;
        private IEnemyHandManager enemyHandManager;
        private IPlayerHandManager playerHandManager;
        private IEnemySpawnerManager enemySpawner;
        private IPlayerManager playerManager;
        private IStageManager stageManager;
        private IEnemyManager enemyManager;
        private ICombatTurnManager turnManager;
        private ICombatStateFactory stateFactory;
        private ICombatPreparationService preparationService;
        private IPlayerInputController inputController;
        private ICombatExecutor executor;

        private SkillCardUI skillCardPrefab;

        private bool playerInputEnabled = false;

        public void Inject(
            ISlotRegistry slotRegistry,
            IEnemyHandManager enemyHandManager,
            IPlayerHandManager playerHandManager,
            IEnemySpawnerManager enemySpawner,
            IPlayerManager playerManager,
            IStageManager stageManager,
            IEnemyManager enemyManager,
            ICombatTurnManager turnManager,
            ICombatStateFactory stateFactory)
        {
            this.slotRegistry = slotRegistry;
            this.enemyHandManager = enemyHandManager;
            this.playerHandManager = playerHandManager;
            this.enemySpawner = enemySpawner;
            this.playerManager = playerManager;
            this.stageManager = stageManager;
            this.enemyManager = enemyManager;
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        public void InjectUI(SkillCardUI skillCardPrefab)
        {
            this.skillCardPrefab = skillCardPrefab;
        }

        public void InjectTurnStateDependencies(ICombatTurnManager turnManager, ICombatStateFactory stateFactory)
        {
            this.turnManager = turnManager;
            this.stateFactory = stateFactory;
        }

        public void InjectExternalServices(
            ICombatPreparationService preparationService,
            IPlayerInputController inputController,
            ICombatExecutor executor)
        {
            this.preparationService = preparationService;
            this.inputController = inputController;
            this.executor = executor;
        }

        public IEnumerator PerformCombatPreparation()
        {
            bool success = false;
            yield return PerformCombatPreparation(result => success = result);
        }

        public IEnumerator PerformCombatPreparation(Action<bool> onComplete)
        {
            if (preparationService == null)
            {
                Debug.LogError("[CombatFlowCoordinator] preparationService가 null입니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            // ✅ 이전 적 및 핸드 정리
            var prevEnemy = enemyManager?.GetEnemy();
            if (prevEnemy != null)
            {
                Debug.Log("[CombatFlowCoordinator] 이전 적 핸드 정리");
                enemyHandManager?.ClearHand();               // 카드 UI 제거
                enemyManager?.ClearEnemy();                  // 이전 적 캐릭터 제거
            }

            yield return preparationService.PrepareCombat(success =>
            {
                if (success)
                {
                    Debug.Log("[CombatFlowCoordinator] 전투 준비 완료. 컨텍스트/실행기 주입 시작.");

                    var player = playerManager.GetPlayer();
                    var enemy = enemyManager.GetEnemy();

                    if (player == null || enemy == null)
                    {
                        Debug.LogError("[CombatFlowCoordinator] 플레이어나 적 캐릭터가 null입니다. 주입 실패.");
                        onComplete?.Invoke(false);
                        return;
                    }

                    var contextProvider = new DefaultCardExecutionContextProvider(player, enemy);
                    var cardExecutor = new CardExecutor();

                    executor?.InjectExecutionDependencies(contextProvider, cardExecutor);
                    Debug.Log("[CombatFlowCoordinator] 카드 실행기 및 컨텍스트 프로바이더 주입 완료.");
                }

                onComplete?.Invoke(success);
            
            });
        }

        // ✅ 변경된 메서드: IEnumerator → void
        public void EnablePlayerInput()
        {
            playerInputEnabled = true;
            Debug.Log("[CombatFlowCoordinator] EnablePlayerInput 호출");

            inputController?.EnablePlayerInput();
            playerHandManager?.EnableInput(true);
        }

        // ✅ 변경된 메서드: IEnumerator → void
        public void DisablePlayerInput()
        {
            playerInputEnabled = false;
            Debug.Log("[CombatFlowCoordinator] DisablePlayerInput 호출");

            inputController?.DisablePlayerInput();
            playerHandManager?.EnableInput(false);
        }

        public bool IsPlayerInputEnabled() => playerInputEnabled;

        public IEnumerator PerformFirstAttack()
        {
            Debug.Log("[CombatFlowCoordinator] PerformFirstAttack 호출");
            if (executor != null)
                yield return executor.PerformAttack(CombatSlotPosition.FIRST);
            else
                Debug.LogError("[CombatFlowCoordinator] executor가 null입니다.");
        }

        public IEnumerator PerformSecondAttack()
        {
            Debug.Log("[CombatFlowCoordinator] PerformSecondAttack 호출");
            if (executor != null)
                yield return executor.PerformAttack(CombatSlotPosition.SECOND);
            else
                Debug.LogError("[CombatFlowCoordinator] executor가 null입니다.");
        }

        public IEnumerator PerformResultPhase() => CoroutineStub("PerformResultPhase");
        public IEnumerator PerformVictoryPhase() => CoroutineStub("PerformVictoryPhase");
        public IEnumerator PerformGameOverPhase() => CoroutineStub("PerformGameOverPhase");

        private IEnumerator CoroutineStub(string phaseName)
        {
            Debug.Log($"[CombatFlowCoordinator] {phaseName} 호출");
            yield return null;
        }

        public bool IsPlayerDead() => playerManager?.GetPlayer()?.IsDead() ?? true;
        public bool IsEnemyDead() => enemyManager?.GetEnemy()?.IsDead() ?? true;
        public bool CheckHasNextEnemy() => stageManager?.HasNextEnemy() ?? false;

        public void StartCombatFlow()
        {
            Debug.Log("[CombatFlowCoordinator] StartCombatFlow 호출됨");
            turnManager?.Initialize();
        }
    }
}
