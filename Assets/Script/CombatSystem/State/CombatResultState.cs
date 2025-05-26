using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatResultState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ISlotRegistry slotRegistry;

        public CombatResultState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatStateFactory stateFactory,
            ISlotRegistry slotRegistry)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.stateFactory = stateFactory;
            this.slotRegistry = slotRegistry;
        }

        public void EnterState()
        {
            Debug.Log("[State] CombatResultState: 전투 결과 판단 시작");

            if (flowCoordinator is MonoBehaviour mono)
            {
                mono.StartCoroutine(ResultRoutine());
            }
            else
            {
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
            }
        }

        private IEnumerator ResultRoutine()
        {
            yield return flowCoordinator.PerformResultPhase();

            if (flowCoordinator.IsPlayerDead())
            {
                Debug.Log("[State] CombatResultState: 플레이어 사망 → GameOver");
                var nextState = stateFactory.CreateGameOverState();
                turnManager.RequestStateChange(nextState);
            }
            else if (!flowCoordinator.IsEnemyDead())
            {
                Debug.Log("[State] CombatResultState: 적이 아직 살아 있음 → 다음 턴 준비");
                var nextState = stateFactory.CreatePrepareState(); // 또는 PlayerInputState
                turnManager.RequestStateChange(nextState);
            }
            else if (flowCoordinator.CheckHasNextEnemy())
            {
                Debug.Log("[State] CombatResultState: 적 사망 + 다음 적 존재 → 다음 적 준비");
                var nextState = stateFactory.CreatePrepareState();
                turnManager.RequestStateChange(nextState);
            }
            else
            {
                Debug.Log("[State] CombatResultState: 적 사망 + 다음 적 없음 → Victory");
                var nextState = stateFactory.CreateVictoryState();
                turnManager.RequestStateChange(nextState);
            }
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
