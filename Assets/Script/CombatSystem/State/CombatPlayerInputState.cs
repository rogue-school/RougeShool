using UnityEngine;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 플레이어가 스킬 카드를 전투 슬롯에 배치하고, 턴 시작 버튼을 기다리는 상태입니다.
    /// </summary>
    public class CombatPlayerInputState : ICombatTurnState
    {
        private readonly ITurnStateController controller;
        private readonly IPlayerHandManager playerHandManager;
        private readonly ICombatStateFactory stateFactory;

        private bool turnStarted = false;

        public CombatPlayerInputState(
            ITurnStateController controller,
            IPlayerHandManager playerHandManager,
            ICombatStateFactory stateFactory)
        {
            this.controller = controller;
            this.playerHandManager = playerHandManager;
            this.stateFactory = stateFactory;
        }

        public void EnterState()
        {
            Debug.Log("[CombatPlayerInputState] 상태 진입 - 플레이어 입력 대기 시작");

            turnStarted = false;

            // 핸드 UI 상호작용 활성화
            playerHandManager.EnableCardInteraction(true);

            // 턴 시작 버튼은 외부 UI에서 활성화되며, TriggerTurnStart()를 호출해야 함
        }

        public void ExecuteState()
        {
            if (!turnStarted)
                return;

            Debug.Log("[CombatPlayerInputState] 턴 시작 감지 - 선공 턴으로 전환");
            controller.RequestStateChange(stateFactory.CreateFirstAttackState());
        }

        public void ExitState()
        {
            Debug.Log("[CombatPlayerInputState] 상태 종료 - 플레이어 입력 종료");

            // 핸드 UI 상호작용 비활성화
            playerHandManager.EnableCardInteraction(false);
        }

        /// <summary>
        /// TurnStartButtonHandler 또는 외부 UI에서 호출하는 메서드입니다.
        /// </summary>
        public void TriggerTurnStart()
        {
            turnStarted = true;
        }
    }
}
