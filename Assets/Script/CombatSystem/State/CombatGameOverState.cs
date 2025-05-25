using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;
using System.Collections;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatStateFactory stateFactory;
        private readonly ISlotRegistry slotRegistry;

        public CombatGameOverState(
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
            Debug.Log("[State] CombatGameOverState: 게임 오버 처리 시작");

            if (flowCoordinator is MonoBehaviour mono)
            {
                mono.StartCoroutine(GameOverRoutine());
            }
            else
            {
                Debug.LogError("flowCoordinator가 MonoBehaviour가 아닙니다. Coroutine 실행 불가.");
            }
        }

        private IEnumerator GameOverRoutine()
        {
            yield return flowCoordinator.PerformGameOverPhase();

            // 게임 오버 이후 씬 전환 또는 UI 처리 예정
            Debug.Log("[State] CombatGameOverState: 게임 오버 처리 완료");
        }

        public void ExecuteState() { }

        public void ExitState() { }
    }
}
