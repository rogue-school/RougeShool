using System;
using System.Collections;
using System.Linq;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.UI;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 전투 준비를 담당하는 서비스.
    /// 적 소환, 카드 배치, 턴 시스템 초기화를 수행한다.
    /// </summary>
    public class CombatPreparationService : ICombatPreparationService
    {
        private readonly IPlayerManager playerManager;
        private readonly IEnemySpawnerManager enemySpawnerManager;
        private readonly IEnemyManager enemyManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ITurnCardRegistry turnCardRegistry;
        private readonly ICardPlacementService placementService;
        private readonly ICombatTurnManager turnManager;
        private readonly ISlotSelector slotSelector;
        private readonly ISlotRegistry slotRegistry;

        public CombatPreparationService(
            IPlayerManager playerManager,
            IEnemySpawnerManager enemySpawnerManager,
            IEnemyManager enemyManager,
            IEnemyHandManager enemyHandManager,
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService placementService,
            ICombatTurnManager turnManager,
            ISlotSelector slotSelector,
            ISlotRegistry slotRegistry)
        {
            this.playerManager = playerManager;
            this.enemySpawnerManager = enemySpawnerManager;
            this.enemyManager = enemyManager;
            this.enemyHandManager = enemyHandManager;
            this.turnCardRegistry = turnCardRegistry;
            this.placementService = placementService;
            this.turnManager = turnManager;
            this.slotSelector = slotSelector;
            this.slotRegistry = slotRegistry;
        }

        public IEnumerator PrepareCombat(Action<bool> onComplete)
        {
            // 1. 적 소환
            enemySpawnerManager.SpawnInitialEnemy();
            yield return null;

            // 2. 플레이어 카드 등록
            var player = playerManager.GetPlayer();
            var deck = player?.Data?.SkillDeck;

            if (deck != null)
            {
                var cards = deck.GetCards();
                var handSlots = slotRegistry.GetHandSlotRegistry().GetPlayerHandSlots().ToList();

                int cardCount = cards.Count;
                int slotCount = handSlots.Count;

                for (int i = 0; i < cardCount && i < slotCount; i++)
                {
                    var cardEntry = cards[i];
                    var slot = handSlots[i];

                    // 카드 인스턴스 생성
                    var skillCard = new PlayerSkillCardInstance(
                        cardEntry.Card.CardData,
                        cardEntry.CreateEffects(),
                        SlotOwner.PLAYER
                    );

                    var combatSlotPosition = SlotPositionUtil.ToCombatSlot(cardEntry.Slot);

                    if (slot is ICombatCardSlot combatSlot)
                    {
                        var cardUI = slot.GetCardUI() as SkillCardUI;
                        if (cardUI == null)
                        {
                            Debug.LogError($"[CombatPreparationService] SkillCardUI 캐스팅 실패 (슬롯: {combatSlotPosition})");
                            continue;
                        }

                        placementService.PlaceCardInSlot(skillCard, cardUI, combatSlot);

                        turnCardRegistry.RegisterCard(
                            combatSlotPosition,
                            skillCard,
                            cardUI,
                            SlotOwner.PLAYER
                        );
                    }

                }
            }

            // 3. 턴 시스템 초기화
            turnManager.Initialize();

            // 4. 완료 콜백
            onComplete?.Invoke(true);
        }
    }
}
