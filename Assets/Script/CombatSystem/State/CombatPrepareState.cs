using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager controller;
        private readonly IEnemySpawnerManager spawnerManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ICombatStateFactory stateFactory;

        public CombatPrepareState(
            ICombatTurnManager controller,
            IEnemySpawnerManager spawnerManager,
            IEnemyHandManager enemyHandManager,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.spawnerManager = spawnerManager;
            this.enemyHandManager = enemyHandManager;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatPrepareState] 상태 진입 - 적 스폰 및 카드 배치");

            spawnerManager.SpawnInitialEnemy();
            enemyHandManager.GenerateInitialHand();

            var card = enemyHandManager.GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card != null)
            {
                var slot = Random.value < 0.5f ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;
                controller.ReserveEnemySlot(slot);

                var combatSlot = SlotRegistry.Instance?.GetCombatSlot(slot);
                combatSlot?.SetCard(card);
                Debug.Log($"[CombatPrepareState] 적 카드 '{card.GetCardName()}' → 전투 슬롯 {slot}에 배치");
            }

            var nextState = stateFactory.CreatePlayerInputState();
            controller.RequestStateChange(nextState);
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatPrepareState] 상태 종료");
        }
    }
}
