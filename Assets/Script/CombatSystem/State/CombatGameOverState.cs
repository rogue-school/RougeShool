using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly IGameOverManager gameOverManager;

        public CombatGameOverState(IGameOverManager gameOverManager)
        {
            this.gameOverManager = gameOverManager;
        }

        public void EnterState()
        {
            Debug.Log("[CombatGameOverState] 게임 오버 상태 진입");
            gameOverManager.ShowGameOverUI();
        }

        public void ExecuteState() { }

        public void ExitState()
        {
            Debug.Log("[CombatGameOverState] 게임 오버 상태 종료");
        }
    }
}
