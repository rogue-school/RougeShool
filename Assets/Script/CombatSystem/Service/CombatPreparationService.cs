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

        public CombatPreparationService(
            IPlayerManager playerManager,
            IEnemySpawnerManager spawnerManager,
            IEnemyManager enemyManager,
            IEnemyHandManager enemyHandManager,
            ISlotRegistry slotRegistry,
            ITurnCardRegistry turnCardRegistry,
            ICardPlacementService cardPlacementService)
        {
            this.playerManager = playerManager;
            this.spawnerManager = spawnerManager;
            this.enemyManager = enemyManager;
            this.enemyHandManager = enemyHandManager;
            this.slotRegistry = slotRegistry;
            this.turnCardRegistry = turnCardRegistry;
            this.cardPlacementService = cardPlacementService;
        }

        public IEnumerator PrepareCombat(Action<bool> onComplete)
        {
            Debug.Log("[Preparation] 플레이어 생성 시작");
            playerManager.CreateAndRegisterPlayer();
            yield return null;

            Debug.Log("[Preparation] 적 생성 시작");
            spawnerManager.SpawnInitialEnemy();
            yield return null;

            var enemy = enemyManager.GetEnemy();
            if (enemy == null)
            {
                Debug.LogError("[Preparation] 적 생성 실패");
                onComplete?.Invoke(false);
                yield break;
            }

            var (enemyCard, enemyUI) = enemyHandManager.PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (enemyCard == null || enemyUI == null)
            {
                Debug.LogError("[Preparation] 적 카드 또는 UI 불러오기 실패");
                enemyHandManager.LogHandSlotStates();
                onComplete?.Invoke(false);
                yield break;
            }

            var player = playerManager.GetPlayer();
            var playerCard = player?.GetCardInHandSlot(SkillCardSlotPosition.PLAYER_SLOT_1);
            var playerUI = player?.GetCardUIInHandSlot(SkillCardSlotPosition.PLAYER_SLOT_1) as SkillCardUI;

            if (playerCard == null || playerUI == null)
            {
                Debug.LogError("[Preparation] 플레이어 카드 또는 UI 불러오기 실패");
                onComplete?.Invoke(false);
                yield break;
            }

            // 선공 / 후공 슬롯을 무작위로 결정
            bool playerGoesFirst = UnityEngine.Random.value < 0.5f;

            CombatFieldSlotPosition playerSlotPos = playerGoesFirst
                ? CombatFieldSlotPosition.FIELD_LEFT
                : CombatFieldSlotPosition.FIELD_RIGHT;

            CombatFieldSlotPosition enemySlotPos = playerGoesFirst
                ? CombatFieldSlotPosition.FIELD_RIGHT
                : CombatFieldSlotPosition.FIELD_LEFT;

            var playerCombatSlot = slotRegistry.GetCombatSlot(playerSlotPos);
            var enemyCombatSlot = slotRegistry.GetCombatSlot(enemySlotPos);

            if (playerCombatSlot == null || enemyCombatSlot == null)
            {
                Debug.LogError("[Preparation] 전투 슬롯이 유효하지 않음");
                onComplete?.Invoke(false);
                yield break;
            }

            // 슬롯에 카드 배치
            cardPlacementService.PlaceCardInSlot(playerCard, playerUI, playerCombatSlot);
            cardPlacementService.PlaceCardInSlot(enemyCard, enemyUI, enemyCombatSlot);

            // 등록
            turnCardRegistry.RegisterPlayerCard(playerSlotPos, playerCard);
            turnCardRegistry.ReserveNextEnemySlot(SlotPositionUtil.ToExecutionSlot(enemySlotPos));

            // 적 핸드 슬롯 보충
            enemyHandManager.FillEmptySlots();

            Debug.Log("[Preparation] 전투 준비 완료");
            onComplete?.Invoke(true);
        }
    }
}
