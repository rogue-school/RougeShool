using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.State
{
    /// <summary>
    /// 선공 상태를 나타내며, 선공 처리 후 후공 상태로 전환합니다.
    /// </summary>
    public class CombatFirstAttackState : ICombatTurnState
    {
        #region 필드

        private readonly ICombatTurnManager turnManager;
        private readonly ICombatFlowCoordinator flowCoordinator;
        private readonly ICombatSlotRegistry slotRegistry;

        #endregion

        #region 생성자

        /// <summary>
        /// 선공 상태를 초기화합니다.
        /// </summary>
        public CombatFirstAttackState(
            ICombatTurnManager turnManager,
            ICombatFlowCoordinator flowCoordinator,
            ICombatSlotRegistry slotRegistry)
        {
            this.turnManager = turnManager;
            this.flowCoordinator = flowCoordinator;
            this.slotRegistry = slotRegistry;
        }

        #endregion

        #region 상태 메서드

        /// <summary>
        /// 선공 상태 진입 시 호출되며, 선공 처리를 수행한 후 후공 상태로 전이합니다.
        /// </summary>
        public void EnterState()
        {
            Debug.Log("<color=cyan>[STATE] CombatFirstAttackState 진입</color>");
            flowCoordinator.RequestFirstAttack(() =>
            {
                Debug.Log("<color=cyan>[STATE] CombatFirstAttackState → CombatSecondAttackState 전이</color>");
                var next = turnManager.GetStateFactory().CreateSecondAttackState();
                turnManager.RequestStateChange(next);
            });
        }

        /// <summary>
        /// 선공 상태 실행 중 반복적으로 호출됩니다. (현재는 비어 있음)
        /// </summary>
        public void ExecuteState() { }

        /// <summary>
        /// 상태 종료 시 호출됩니다. (현재는 비어 있음)
        /// </summary>
        public void ExitState() 
        { 
            Debug.Log("<color=cyan>[STATE] CombatFirstAttackState 종료</color>"); 
        }

        #endregion
    }
}
