using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Core;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 모든 적 처치 후 전투에서 승리했을 때 진입하는 상태입니다.
    /// 외부 시스템에서 보상 및 다음 진행을 담당합니다.
    /// </summary>
    public class CombatVictoryState : ICombatTurnState
    {
        private readonly ITurnStateController controller;

        public CombatVictoryState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatVictoryState] 전투 승리 상태 진입");

            // 전투 승리 UI 및 보상 처리 호출 (VictoryManager 등에서 처리)
            VictoryManager.Instance?.ShowVictory();
        }

        public void ExecuteState()
        {
            // 승리 상태는 정지 상태이므로 별도 로직 없음
        }

        public void ExitState()
        {
            Debug.Log("[CombatVictoryState] 상태 종료 (다음 스테이지 또는 로비 이동 등)");
        }
    }
}
