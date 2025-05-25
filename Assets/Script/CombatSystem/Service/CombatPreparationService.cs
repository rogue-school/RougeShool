using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Service
{
    public class CombatPreparationService : ICombatPreparationService
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemySpawnerManager spawnerManager;
        private readonly IEnemyManager enemyManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ISlotRegistry slotRegistry;
        private readonly ITurnCardRegistry turnCardRegistry;
        private readonly ICardPlacementService cardPlacementService;
        private readonly ICombatTurnManager combatTurnManager;

        public CombatPreparationService(
            IPlayerManager playerManager,
            IEnemySpawnerManager spawnerManager,
            IEnemyManager enemyManager,
            IEnemyHandManager enemyHandManager,
            ISlotRegistry slotRegistry,
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService cardPlacementService,
            ICombatTurnManager combatTurnManager)
        {
            this.playerManager = playerManager;
            this.spawnerManager = spawnerManager;
            this.enemyManager = enemyManager;
            this.enemyHandManager = enemyHandManager;
            this.slotRegistry = slotRegistry;
            this.turnCardRegistry = turnCardRegistry;
            this.cardPlacementService = cardPlacementService;
            this.combatTurnManager = combatTurnManager;
        }

        public IEnumerator PrepareCombat(Action<bool> onComplete)
        {
            Debug.Log("[CombatPreparationService] 전투 준비 시작");

            playerManager.CreateAndRegisterPlayer();
            yield return null;

            spawnerManager.SpawnInitialEnemy();
            yield return null;

            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogError("[CombatPreparationService] 적 생성 실패");
                onComplete?.Invoke(false);
                yield break;
            }

            var (enemyCard, enemyUI) = enemyHandManager.PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (enemyCard == null || enemyUI == null)
            {
                Debug.LogError("[CombatPreparationService] 적 카드 슬롯 비어 있음");
                onComplete?.Invoke(false);
                yield break;
            }

            var slotOrder = UnityEngine.Random.value < 0.5f
                ? new[] { CombatSlotPosition.FIRST, CombatSlotPosition.SECOND }
                : new[] { CombatSlotPosition.SECOND, CombatSlotPosition.FIRST };

            CombatSlotPosition playerSlot = slotOrder[0];
            CombatSlotPosition enemySlot = slotOrder[1];

            var enemyCombatSlot = slotRegistry.GetCombatSlot(enemySlot);
            if (enemyCombatSlot == null)
            {
                Debug.LogError("[CombatPreparationService] 전투 슬롯 가져오기 실패 (Enemy)");
                onComplete?.Invoke(false);
                yield break;
            }

            cardPlacementService.PlaceCardInSlot(enemyCard, enemyUI, enemyCombatSlot);

            turnCardRegistry.RegisterEnemyCard(enemyCard);
            turnCardRegistry.ReserveNextEnemySlot(enemySlot);
            combatTurnManager.RegisterEnemyCard(enemyCard);

            enemyHandManager.FillEmptySlots();

            Debug.Log($"[CombatPreparationService] 전투 준비 완료 - EnemyCard: {enemyCard.CardData.Name}");
            onComplete?.Invoke(true);
        }
    }
}
