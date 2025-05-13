using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.State;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어가 카드를 슬롯에 올리고, 턴 시작 버튼을 기다리는 상태입니다.
    /// </summary>
    public class CombatPlayerInputState : ICombatTurnState
    {
        private readonly ITurnStateController controller;
        private bool turnStarted = false;

        public CombatPlayerInputState(ITurnStateController controller)
        {
            this.controller = controller;
        }

        public void EnterState()
        {
            Debug.Log("[CombatPlayerInputState] 상태 진입 - 플레이어 입력 대기");
            turnStarted = false;

            // 버튼 활성화는 외부에서 관리 (UIHandler 등)
        }

        public void ExecuteState()
        {
            if (turnStarted)
            {
                Debug.Log("[CombatPlayerInputState] 턴 시작 감지 - 선공 상태로 전환");
                controller.RequestStateChange(new CombatFirstAttackState(controller));
            }
        }

        public void ExitState()
        {
            Debug.Log("[CombatPlayerInputState] 상태 종료 - 입력 비활성화");
        }

        /// <summary>
        /// TurnStartButtonHandler에서 호출됨
        /// </summary>
        public void TriggerTurnStart()
        {
            Debug.Log("[CombatPlayerInputState] TriggerTurnStart 호출됨");
            turnStarted = true;
        }
    }
}
