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
            Debug.Log("<color=orange>[CombatResultState] 상태 진입</color>");
            flowCoordinator.DisablePlayerInput();
            coroutineRunner.RunCoroutine(ExecuteResultPhase());
        }

        private IEnumerator ExecuteResultPhase()
        {
            // 1. 입력 차단 및 카드 복귀
            flowCoordinator.DisablePlayerInput();
            ReturnPlayerCardsToHand();
            yield return new WaitForEndOfFrame();

            // 2. UI 제거 및 효과 정리
            yield return flowCoordinator.PerformResultPhase();
            yield return new WaitForEndOfFrame();

            // 3. 적 또는 플레이어 사망 체크 후 다음 상태 결정
            if (flowCoordinator.IsEnemyDead())
            {
                Debug.Log("[CombatResultState] 적 사망 → 제거 및 다음 상태 결정");

                flowCoordinator.RemoveEnemyCharacter();
                yield return new WaitForSeconds(0.2f);

                if (flowCoordinator.CheckHasNextEnemy())
                {
                    Debug.Log("[CombatResultState] 다음 적 있음 → PrepareState 전환");

                    flowCoordinator.SpawnNextEnemy();
                    yield return new WaitForSeconds(0.2f);
                }
                else
                {
                    Debug.Log("[CombatResultState] 마지막 적 처치 → VictoryState 전환");

                    flowCoordinator.ClearEnemyHand();
                    yield return new WaitForEndOfFrame();

                    var victory = turnManager.GetStateFactory().CreateVictoryState();
                    turnManager.RequestStateChange(victory);
                    yield break;
                }
            }
            else if (flowCoordinator.IsPlayerDead())
            {
                Debug.Log("[CombatResultState] 플레이어 사망 → GameOverState 전환");

                yield return new WaitForSeconds(0.1f);
                var gameOver = turnManager.GetStateFactory().CreateGameOverState();
                turnManager.RequestStateChange(gameOver);
                yield break;
            }

            Debug.Log("[CombatResultState] 전투 지속 → PrepareState 전환");
            yield return new WaitForSeconds(0.1f);

            var next = turnManager.GetStateFactory().CreatePrepareState();
            turnManager.RequestStateChange(next);
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
                    slotRegistry.GetCombatSlot(pos)?.ClearAll();

                    Debug.Log($"[CombatResultState] 플레이어 카드 복귀 완료: {card.GetCardName()}");
                }
            }
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=grey>[CombatResultState] 상태 종료</color>");
        }
    }
}
