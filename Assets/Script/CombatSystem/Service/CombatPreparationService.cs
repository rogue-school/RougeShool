using System;
using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.IManager;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Service
{
    public class CombatPreparationService : ICombatPreparationService
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemySpawnerManager spawnerManager;
        private readonly IEnemyManager enemyManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ITurnCardRegistry turnCardRegistry;
        private readonly ICardPlacementService cardPlacementService;
        private readonly ICombatTurnManager combatTurnManager;
        private readonly ISlotSelector slotSelector;
        private readonly ISlotRegistry slotRegistry;

        public CombatPreparationService(
            IPlayerManager playerManager,
            IEnemySpawnerManager spawnerManager,
            IEnemyManager enemyManager,
            IEnemyHandManager enemyHandManager,
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService cardPlacementService,
            ICombatTurnManager combatTurnManager,
            ISlotSelector slotSelector,
            ISlotRegistry slotRegistry)
        {
            this.playerManager = playerManager;
            this.spawnerManager = spawnerManager;
            this.enemyManager = enemyManager;
            this.enemyHandManager = enemyHandManager;
            this.turnCardRegistry = turnCardRegistry;
            this.cardPlacementService = cardPlacementService;
            this.combatTurnManager = combatTurnManager;
            this.slotSelector = slotSelector;
            this.slotRegistry = slotRegistry;
        }

        public IEnumerator PrepareCombat(Action<bool> onComplete)
        {
            Debug.Log("[CombatPreparationService] 전투 준비 시작");

            yield return EnsurePlayerExists();
            yield return EnsureEnemyExists();

            var enemyCard = enemyHandManager.GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
            var cardUI = enemyHandManager.GetCardUI((int)SkillCardSlotPosition.ENEMY_SLOT_1);

            if (enemyCard == null || cardUI == null)
            {
                Debug.LogError("[CombatPreparationService] 핸드 슬롯 1번에 카드 또는 UI가 없습니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            // CombatSlotPosition 반환
            (CombatSlotPosition playerPos, CombatSlotPosition enemyPos) = slotSelector.SelectSlots();

            ICombatCardSlot enemySlot = slotRegistry.GetCombatSlot(enemyPos);
            if (enemySlot == null)
            {
                Debug.LogError("[CombatPreparationService] 적 전투 슬롯을 찾을 수 없습니다.");
                onComplete?.Invoke(false);
                yield break;
            }

            // SkillCardUI로 캐스팅
            cardPlacementService.PlaceCardInSlot(enemyCard, (SkillCardUI)cardUI, enemySlot);
            turnCardRegistry.RegisterEnemyCard(enemyCard);
            turnCardRegistry.ReserveNextEnemySlot(enemySlot.Position);

            enemyHandManager.FillEmptySlots();

            Debug.Log($"[CombatPreparationService] 전투 준비 완료 - EnemyCard: {enemyCard.CardData?.Name ?? "Unknown"}");
            onComplete?.Invoke(true);
        }

        private IEnumerator EnsurePlayerExists()
        {
            playerManager.CreateAndRegisterPlayer();
            yield return null;
        }

        private IEnumerator EnsureEnemyExists()
        {
            var enemy = enemyManager.GetEnemy();
            if (enemy == null || enemy.IsDead())
            {
                spawnerManager.SpawnInitialEnemy();
                yield return null;
            }
        }
    }
}
