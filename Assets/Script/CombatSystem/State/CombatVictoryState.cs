using UnityEngine;
using System.Collections;
using Game.CombatSystem.Manager;
using Game.UtilitySystem;
using Game.CoreSystem.Utility;
using Game.CombatSystem;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState
    {
        private readonly TurnManager turnManager;
        private readonly CombatSlotManager slotManager;
        private readonly ICoroutineRunner coroutineRunner;

        public CombatVictoryState(
            TurnManager turnManager,
            CombatSlotManager slotManager,
            ICoroutineRunner coroutineRunner)
        {
            this.turnManager = turnManager;
            this.slotManager = slotManager;
            this.coroutineRunner = coroutineRunner;
        }

        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatVictoryState 진입</color>");
            CombatEvents.RaiseVictory();
            coroutineRunner.RunCoroutine(HandleVictory());
        }

        private IEnumerator HandleVictory()
        {
            Debug.Log("[CombatVictoryState] 승리 처리 중...");
            yield return new WaitForSeconds(1.0f);

            // Note: Victory handling is now simplified in the new architecture
            Debug.Log("<color=cyan>[STATE] CombatVictoryState → 전투 완료</color>");
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("<color=cyan>[STATE] CombatVictoryState 종료</color>");
        }
    }
}
