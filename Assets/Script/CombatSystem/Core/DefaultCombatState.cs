using UnityEngine;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Core
{
    /// <summary>
    /// [DefaultCombatState]
    /// 기본 전투 턴 상태로, 아무런 게임 로직도 실행되지 않는 상태입니다.
    /// 주로 초기 상태나 디버깅, 상태 전이 흐름 확인을 위해 사용됩니다.
    /// </summary>
    public class DefaultCombatState : ICombatTurnState
    {
        #region Fields

        private readonly ICombatTurnManager turnManager;

        #endregion

        #region Constructor

        /// <summary>
        /// 기본 전투 상태를 초기화합니다.
        /// </summary>
        /// <param name="manager">턴 매니저 인스턴스</param>
        public DefaultCombatState(ICombatTurnManager manager)
        {
            turnManager = manager;
        }

        #endregion

        #region ICombatTurnState Implementation

        /// <summary>
        /// 상태에 진입할 때 호출됩니다.
        /// </summary>
        public void EnterState()
        {
            Debug.Log("[DefaultCombatState] 상태 진입");
        }

        /// <summary>
        /// 상태 실행 시 호출됩니다. 실제 게임 로직은 없습니다.
        /// </summary>
        public void ExecuteState()
        {
            Debug.Log("[DefaultCombatState] 상태 실행 중 (기본 상태)");
            // 필요 시 자동 상태 전이 가능
            // 예: turnManager.RequestStateChange(...);
        }

        /// <summary>
        /// 상태에서 나갈 때 호출됩니다.
        /// </summary>
        public void ExitState()
        {
            Debug.Log("[DefaultCombatState] 상태 종료");
        }

        #endregion
    }
}
