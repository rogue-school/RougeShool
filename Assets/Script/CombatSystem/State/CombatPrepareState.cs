using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Slot;
using Game.Utility;
using Game.CombatSystem.Context;

namespace Game.CombatSystem.State
{
    public class CombatPrepareState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly IPlayerHandManager playerHandManager;
        private readonly IEnemyHandManager enemyHandManager;
        private readonly ISlotRegistry slotRegistry;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly TurnContext turnContext; 

        public CombatPrepareState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            IPlayerHandManager playerHandManager,
            IEnemyHandManager enemyHandManager,
            ISlotRegistry slotRegistry,
            ICoroutineRunner coroutineRunner,
            TurnContext turnContext)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.playerHandManager = playerHandManager;
            this.enemyHandManager = enemyHandManager;
            this.slotRegistry = slotRegistry;
            this.coroutineRunner = coroutineRunner;
            this.turnContext = turnContext;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[CombatPrepareState] 상태 진입</color>");
            turnContext.Reset();
            coroutineRunner.RunCoroutine(PrepareRoutine());
        }

        private IEnumerator PrepareRoutine()
        {
            ReturnPlayerCardToHand();

            yield return new WaitForEndOfFrame();

            if (!flowCoordinator.HasEnemy())
            {
                flowCoordinator.SpawnNextEnemy();
                yield return new WaitForSeconds(0.5f);
            }

            flowCoordinator.ClearEnemyCombatSlots();

            yield return new WaitForEndOfFrame();

            RegisterEnemyCardToSlot();

            yield return new WaitForEndOfFrame();

            enemyHandManager.FillEmptySlots();

            yield return new WaitForSeconds(0.1f);

            var next = turnManager.GetStateFactory().CreatePlayerInputState();
            turnManager.RequestStateChange(next);
        }


        private void ReturnPlayerCardToHand()
        {
            foreach (CombatSlotPosition pos in System.Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = flowCoordinator.GetCardInSlot(pos);
                if (card != null && card.IsFromPlayer())
                {
                    playerHandManager.RemoveCard(card);
                    playerHandManager.RestoreCardToHand(card);
                    slotRegistry.GetCombatSlot(pos)?.ClearAll();
                    Debug.Log($"[CombatPrepareState] 카드 복귀 완료: {card.GetCardName()}");
                }
            }
        }

        private void RegisterEnemyCardToSlot()
        {
            var (card, ui) = enemyHandManager.PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card == null)
            {
                Debug.LogWarning("[CombatPrepareState] 적 핸드 슬롯 1번에 카드 없음");
                return;
            }

            var slotPos = CombatSlotPosition.SECOND;
            flowCoordinator.RegisterCardToCombatSlot(slotPos, card, ui);
            flowCoordinator.GetTurnCardRegistry().RegisterCard(slotPos, card, ui, SlotOwner.ENEMY);

            Debug.Log($"[CombatPrepareState] 적 카드 등록 완료 → {card.GetCardName()}");
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatPrepareState] 상태 종료</color>");
        }
    }
}
