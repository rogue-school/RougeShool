using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 전투 승리 상태입니다. UI 연출, 보상, 다음 스테이지 흐름을 처리합니다.
    /// </summary>
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

            // 전투 종료 UI 연출 및 보상 처리 등
            victoryManager.ShowVictoryUI();

            // 추후 보상 선택 → 다음 스테이지 등 추가 가능
        }

        public void ExecuteState()
        {
            // 이 상태는 수동 전이만 발생 (UI/버튼 등 외부 입력 기반)
        }

        public void ExitState()
        {
            Debug.Log("[CombatVictoryState] 전투 승리 상태 종료");
        }
    }
}
