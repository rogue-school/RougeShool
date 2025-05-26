using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.IManager;
using Game.SkillCardSystem.Slot;

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
        private readonly IEnemyCardSelector enemyCardSelector;
        private readonly ISlotSelector slotSelector;

        public CombatPreparationService(
            IPlayerManager playerManager,
            IEnemySpawnerManager spawnerManager,
            IEnemyManager enemyManager,
            IEnemyHandManager enemyHandManager,
            ISlotRegistry slotRegistry,
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService cardPlacementService,
            ICombatTurnManager combatTurnManager,
            IEnemyCardSelector enemyCardSelector,
            ISlotSelector slotSelector)
        {
            this.playerManager = playerManager;
            this.spawnerManager = spawnerManager;
            this.enemyManager = enemyManager;
            this.enemyHandManager = enemyHandManager;
            this.slotRegistry = slotRegistry;
            this.turnCardRegistry = turnCardRegistry;
            this.cardPlacementService = cardPlacementService;
            this.combatTurnManager = combatTurnManager;
            this.enemyCardSelector = enemyCardSelector;
            this.slotSelector = slotSelector;
        }

        public IEnumerator PrepareCombat(Action<bool> onComplete)
        {
            Debug.Log("[CombatPreparationService] 전투 준비 시작");

            playerManager.CreateAndRegisterPlayer();
            yield return null;

            var enemy = enemyManager.GetEnemy();
            if (enemy == null || enemy.IsDead())
            {
                spawnerManager.SpawnInitialEnemy();
                yield return null;

                enemy = enemyManager.GetEnemy();
                if (enemy == null || enemy.IsDead())
                {
                    Debug.LogError("[CombatPreparationService] 유효한 적이 없습니다.");
                    onComplete?.Invoke(false);
                    yield break;
                }
            }

            var enemyCard = enemyCardSelector.SelectCard(enemy);
            var cardUI = enemyHandManager.GetCardUIInHandSlot(SkillCardSlotPosition.ENEMY_SLOT_1);

            if (enemyCard == null || cardUI == null)
            {
                Debug.LogError("[CombatPreparationService] 적 카드 또는 UI가 null입니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            var (playerSlot, enemySlot) = slotSelector.SelectSlots();
            var enemyCombatSlot = slotRegistry.GetCombatSlot(enemySlot);

            if (enemyCombatSlot == null)
            {
                Debug.LogError("[CombatPreparationService] 적 전투 슬롯을 찾을 수 없습니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            cardPlacementService.PlaceCardInSlot(enemyCard, cardUI, enemyCombatSlot);
            turnCardRegistry.RegisterEnemyCard(enemyCard);
            turnCardRegistry.ReserveNextEnemySlot(enemySlot);
            combatTurnManager.RegisterEnemyCard(enemyCard);

            enemyHandManager.FillEmptySlots();

            Debug.Log($"[CombatPreparationService] 전투 준비 완료 - EnemyCard: {enemyCard.CardData?.Name ?? "Unknown"}");
            onComplete?.Invoke(true);
        }
    }
}
