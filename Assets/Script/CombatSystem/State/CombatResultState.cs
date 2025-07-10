using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICoroutineRunner coroutineRunner;
        private readonly ISlotRegistry slotRegistry;
        private readonly IPlayerHandManager playerHandManager;

        public CombatResultState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICoroutineRunner coroutineRunner,
            ISlotRegistry slotRegistry,
            IPlayerHandManager playerHandManager)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.coroutineRunner = coroutineRunner;
            this.slotRegistry = slotRegistry;
            this.playerHandManager = playerHandManager;
        }

        public void EnterState()
        {
            flowCoordinator.DisablePlayerInput();
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            // 1. 플레이어 카드 복귀
            ReturnPlayerCardsToHand();
            yield return new WaitForEndOfFrame();

            // 2. 전투 시각 효과 및 UI 정리
            yield return flowCoordinator.PerformResultPhase();
            yield return new WaitForEndOfFrame();

            // 3. 사망 판정
            if (flowCoordinator.IsEnemyDead())
            {
                CombatEvents.RaiseCombatEnded(true); // 승리

                flowCoordinator.RemoveEnemyCharacter();
                yield return flowCoordinator.ClearEnemyHandSafely();
                yield return new WaitForSeconds(0.2f);

                if (!flowCoordinator.CheckHasNextEnemy())
                {
                    yield return new WaitForEndOfFrame();

                    turnManager.RequestStateChange(turnManager.GetStateFactory().CreateVictoryState());
                    yield break;
                }
            }
            else if (flowCoordinator.IsPlayerDead())
            {
                CombatEvents.RaiseCombatEnded(false); // 패배
                yield return new WaitForSeconds(0.1f);

                turnManager.RequestStateChange(turnManager.GetStateFactory().CreateGameOverState());
                yield break;
            }

            // 4. 전투 지속 → 다음 준비 상태로 전이
            yield return new WaitForSeconds(0.1f);
            turnManager.RequestStateChange(turnManager.GetStateFactory().CreatePrepareState());
        }

        private void ReturnPlayerCardsToHand()
        {
            foreach (CombatSlotPosition pos in System.Enum.GetValues(typeof(CombatSlotPosition)))
            {
                var card = flowCoordinator.GetCardInSlot(pos);
                if (card != null && card.IsFromPlayer())
                {
                    playerHandManager.RemoveCard(card);
                    playerHandManager.RestoreCardToHand(card);

                    var slot = slotRegistry.GetCombatSlot(pos);
                    slot?.ClearAll();
                }
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
        }
    }
}
