using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 기본적인 턴 시작 조건을 검사하는 클래스입니다.
    /// 플레이어와 적 모두 카드가 등록되어 있어야 턴을 시작할 수 있습니다.
    /// </summary>
    public class DefaultTurnStartConditionChecker
    {
        #region 생성자

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public DefaultTurnStartConditionChecker()
        {
        }

        #endregion

        #region 턴 조건 검사

        /// <summary>
        /// 턴을 시작할 수 있는 조건인지 검사합니다.
        /// 현재는 항상 true를 반환합니다.
        /// </summary>
        /// <returns>턴 시작 가능 여부</returns>
        public bool CanStartTurn()
        {
            // 임시로 항상 true 반환 (향후 실제 조건 검사 로직 구현 예정)
            return true;
        }

        #endregion
    }
}
