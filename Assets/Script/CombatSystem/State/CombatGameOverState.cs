using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어가 사망하여 게임 오버가 되었을 때 진입하는 상태입니다.
    /// 외부 UI 시스템에 게임 오버를 요청합니다.
    /// </summary>
    public class CombatGameOverState : ICombatTurnState
    {
        private readonly ITurnStateController controller;

        public CombatGameOverState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatGameOverState] 게임 오버 상태 진입");

            // 게임 오버 UI 호출 (GameOverManager 또는 UIController에서 처리 예정)
            GameOverManager.Instance?.ShowGameOver();
        }

        public void ExecuteState()
        {
            // 게임 오버 상태는 정지 상태이므로 별도 로직 없음
        }

        public void ExitState()
        {
            Debug.Log("[CombatGameOverState] 상태 종료 (재시작 또는 메인 메뉴 이동 등)");
        }
    }
}
