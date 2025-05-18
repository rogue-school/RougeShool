using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatVictoryState : ICombatTurnState
    {
        private readonly IVictoryManager victoryManager;

        public CombatVictoryState(IVictoryManager victoryManager)
        {
            this.victoryManager = victoryManager;
        }

        public void EnterState()
        {
            Debug.Log("[CombatVictoryState] 전투 승리 상태 진입");
            victoryManager.ShowVictoryUI();
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatVictoryState] 전투 승리 상태 종료");
        }
    }
}
