using System;
using System.Collections;
using System.Linq;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.IManager;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 전투 준비를 담당하는 서비스.
    /// 적 소환, 플레이어 카드 배치, 턴 시스템 초기화를 수행합니다.
    /// </summary>
    public class CombatPreparationService : ICombatPreparationService
    {
        #region 필드

        private readonly IPlayerManager playerManager;
        private readonly IEnemySpawnerManager enemySpawnerManager;
        private readonly IEnemyManager enemyManager;
        // 적 핸드 시스템 제거
        private readonly ITurnCardRegistry turnCardRegistry;
        private readonly ICardPlacementService placementService;
        private readonly ICombatTurnManager turnManager;
        private readonly ISlotSelector slotSelector;
        private readonly ISlotRegistry slotRegistry;

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자 - 모든 전투 준비에 필요한 의존성 객체를 주입받습니다.
        /// </summary>
        public CombatPreparationService(
            IPlayerManager playerManager,
            IEnemySpawnerManager enemySpawnerManager,
            IEnemyManager enemyManager,
            
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService placementService,
            ICombatTurnManager turnManager,
            ISlotSelector slotSelector,
            ISlotRegistry slotRegistry)
        {
            this.playerManager = playerManager;
            this.enemySpawnerManager = enemySpawnerManager;
            this.enemyManager = enemyManager;
            this.turnCardRegistry = turnCardRegistry;
            this.placementService = placementService;
            this.turnManager = turnManager;
            this.slotSelector = slotSelector;
            this.slotRegistry = slotRegistry;
        }

        #endregion

        #region 전투 준비

        /// <summary>
        /// 전투를 준비합니다.
        /// 적 소환 → 플레이어 카드 배치 → 턴 시스템 초기화 순으로 진행됩니다.
        /// </summary>
        /// <param name="onComplete">전투 준비 완료 콜백</param>
        public IEnumerator PrepareCombat(Action<bool> onComplete)
        {
            // 1. 적 소환
            enemySpawnerManager.SpawnInitialEnemy();
            yield return null;

            // 2. 플레이어 카드 등록
            var player = playerManager.GetPlayer();
            var deck = player?.CharacterData?.SkillDeck;

            if (deck != null)
            {
                var cards = deck.GetAllCards();
                var handSlots = slotRegistry.GetHandSlotRegistry().GetPlayerHandSlots().ToList();

                int cardCount = cards.Count;
                int slotCount = handSlots.Count;

                for (int i = 0; i < cardCount && i < slotCount; i++)
                {
                    var cardDefinition = cards[i];
                    var slot = handSlots[i];

                    // 카드 인스턴스 생성
                    var factory = new SkillCardFactory();
                    var skillCard = factory.CreateFromDefinition(
                        cardDefinition,
                        Owner.Player,
                        "Player"
                    );

                    var combatSlotPosition = CombatSlotPosition.BATTLE_SLOT; // 기본값

                    if (slot is ICombatCardSlot combatSlot)
                    {
                        var cardUI = slot.GetCardUI() as SkillCardUI;
                        if (cardUI == null)
                        {
                            Debug.LogError($"[CombatPreparationService] SkillCardUI 캐스팅 실패 (슬롯: {combatSlotPosition})");
                            continue;
                        }

                        // 카드 배치 및 등록
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

            // 4. 완료 콜백 호출
            onComplete?.Invoke(true);
        }

        #endregion
    }
}
