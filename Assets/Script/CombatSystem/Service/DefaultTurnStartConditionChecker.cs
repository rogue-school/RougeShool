using Game.CombatSystem.Interface;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 기본적인 턴 시작 조건을 검사하는 클래스입니다.
    /// 플레이어와 적 모두 카드가 등록되어 있어야 턴을 시작할 수 있습니다.
    /// </summary>
    public class DefaultTurnStartConditionChecker : ITurnStartConditionChecker
    {
        #region 필드

        private readonly ITurnCardRegistry cardRegistry;

        #endregion

        #region 생성자

        /// <summary>
        /// 생성자 - 카드 등록 정보를 검사할 레지스트리를 주입받습니다.
        /// </summary>
        /// <param name="cardRegistry">턴 카드 레지스트리</param>
        public DefaultTurnStartConditionChecker(ITurnCardRegistry cardRegistry)
        {
            this.cardRegistry = cardRegistry;
        }

        #endregion

        #region 턴 조건 검사

        /// <summary>
        /// 턴을 시작할 수 있는 조건인지 검사합니다.
        /// 플레이어와 적 모두 카드가 있어야 true를 반환합니다.
        /// </summary>
        /// <returns>턴 시작 가능 여부</returns>
        public bool CanStartTurn()
        {
            bool result = cardRegistry.HasPlayerCard() && cardRegistry.HasEnemyCard();
            Debug.Log($"[ConditionChecker] CanStartTurn() => {result}");
            return result;
        }

        #endregion
    }
}
