using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 패배 상태입니다. 플레이어가 사망했을 경우 게임오버 UI를 출력합니다.
    /// </summary>
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly IGameOverManager gameOverManager;

        public CombatGameOverState(IGameOverManager gameOverManager)
        {
            this.gameOverManager = gameOverManager;
        }

        public void EnterState()
        {
            Debug.Log("[CombatGameOverState] 게임오버 상태 진입");

            // UI 출력, 재시작 버튼 등 외부 흐름 제어는 매니저에서 수행
            gameOverManager.ShowGameOverUI();
        }

        public void ExecuteState()
        {
            // 외부 UI 처리 상태. 로직 없음.
        }

        public void ExitState()
        {
            Debug.Log("[CombatGameOverState] 게임오버 상태 종료");
        }
    }
}
