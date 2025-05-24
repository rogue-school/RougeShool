using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.SkillCardSystem.Interface;

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

            yield return preparationService.PrepareCombat(success =>
            {
                onComplete?.Invoke(success);
            });
        }

        public IEnumerator EnablePlayerInput()
        {
            Debug.Log("[CombatFlowCoordinator] EnablePlayerInput 호출");
            if (inputController != null)
                yield return inputController.EnablePlayerInput();
            if (playerHandManager != null)
                playerHandManager.EnableInput(true);
        }

        public IEnumerator DisablePlayerInput()
        {
            Debug.Log("[CombatFlowCoordinator] DisablePlayerInput 호출");
            if (inputController != null)
                yield return inputController.DisablePlayerInput();
            if (playerHandManager != null)
                playerHandManager.EnableInput(false);
        }

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


        public IEnumerator PerformResultPhase()
        {
            Debug.Log("[CombatFlowCoordinator] PerformResultPhase 호출");
            yield return null;
        }

        public IEnumerator PerformVictoryPhase()
        {
            Debug.Log("[CombatFlowCoordinator] PerformVictoryPhase 호출");
            yield return null;
        }

        public IEnumerator PerformGameOverPhase()
        {
            Debug.Log("[CombatFlowCoordinator] PerformGameOverPhase 호출");
            yield return null;
        }

        public bool IsPlayerDead()
        {
            var player = playerManager?.GetPlayer();
            return player == null || player.IsDead();
        }

        public bool IsEnemyDead()
        {
            var enemy = enemyManager?.GetEnemy();
            return enemy == null || enemy.IsDead();
        }

        public bool CheckHasNextEnemy()
        {
            return stageManager?.HasNextEnemy() ?? false;
        }

        public void StartCombatFlow()
        {
            Debug.Log("[CombatFlowCoordinator] StartCombatFlow 호출됨");
            turnManager?.Initialize();
        }
    }
}
