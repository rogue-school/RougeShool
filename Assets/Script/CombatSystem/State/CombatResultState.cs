using UnityEngine;
using System.Collections;
using Game.CombatSystem.Interface;
using Game.Utility;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

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
            Debug.Log("<color=cyan>[STATE] CombatResultState 진입</color>");
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
                Debug.Log("<color=cyan>[STATE] CombatResultState → CombatVictoryState 전이 (적 사망)</color>");
                // 승리 이벤트는 VictoryState에서 처리

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
                Debug.Log("<color=cyan>[STATE] CombatResultState → CombatGameOverState 전이 (플레이어 사망)</color>");
                // 패배 이벤트는 GameOverState에서 처리
                yield return new WaitForSeconds(0.1f);

                turnManager.RequestStateChange(turnManager.GetStateFactory().CreateGameOverState());
                yield break;
            }

            // 4. 전투 지속 → 다음 준비 상태로 전이
            Debug.Log("<color=cyan>[STATE] CombatResultState → CombatPrepareState 전이 (전투 지속)</color>");
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
            Debug.Log("<color=cyan>[STATE] CombatResultState 종료</color>");
        }
    }
}
