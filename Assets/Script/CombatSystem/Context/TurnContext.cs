using UnityEngine;

namespace Game.CombatSystem.Context
{
    /// <summary>
    /// 전투 턴 중 발생한 특정 이벤트를 추적하는 컨텍스트 클래스입니다.
    /// 적 처치와 핸드카드 소멸 애니메이션의 중복 실행을 방지합니다.
    /// </summary>
    public class TurnContext
    {
        #region 필드

        /// <summary>
        /// 이번 턴 동안 적이 처치되었는지 여부
        /// </summary>
        public bool WasEnemyDefeated { get; private set; }

        /// <summary>
        /// 이번 턴에 핸드카드 소멸 애니메이션이 실행되었는지 여부
        /// </summary>
        public bool WasHandCardsVanishedThisTurn { get; private set; }

        #endregion

        #region 생성자

        /// <summary>
        /// TurnContext를 초기화합니다.
        /// </summary>
        public TurnContext()
        {
            Reset();
        }

        #endregion

        #region 공개 메서드

        /// <summary>
        /// 적이 처치되었음을 표시합니다.
        /// </summary>
        public void MarkEnemyDefeated()
        {
            WasEnemyDefeated = true;
            Debug.Log("<color=yellow>[TurnContext] 적 처치 표시됨</color>");
        }

        /// <summary>
        /// 핸드카드 소멸 애니메이션이 실행되었음을 표시합니다.
        /// </summary>
        public void MarkHandCardsVanished()
        {
            WasHandCardsVanishedThisTurn = true;
            Debug.Log("<color=yellow>[TurnContext] 핸드카드 소멸 애니메이션 실행 표시됨</color>");
        }

        /// <summary>
        /// 턴 상태를 초기화합니다.
        /// 새로운 턴 시작 전에 호출되어야 합니다.
        /// </summary>
        public void Reset()
        {
            WasEnemyDefeated = false;
            WasHandCardsVanishedThisTurn = false;
            Debug.Log("<color=yellow>[TurnContext] 턴 상태 초기화됨</color>");
        }

        /// <summary>
        /// 현재 턴 상태를 문자열로 반환합니다.
        /// </summary>
        /// <returns>턴 상태 정보</returns>
        public override string ToString()
        {
            return $"TurnContext(EnemyDefeated: {WasEnemyDefeated}, HandCardsVanished: {WasHandCardsVanishedThisTurn})";
        }

        #endregion
    }
}
