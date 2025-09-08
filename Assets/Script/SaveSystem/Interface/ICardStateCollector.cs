using Game.SaveSystem.Data;

namespace Game.SaveSystem.Interface
{
    /// <summary>
    /// 카드 상태 수집 인터페이스
    /// 슬레이 더 스파이어 방식: 모든 카드 상태를 수집하는 인터페이스
    /// </summary>
    public interface ICardStateCollector
    {
        #region 카드 상태 수집

        /// <summary>
        /// 완전한 카드 상태를 수집합니다.
        /// </summary>
        /// <param name="saveTrigger">저장 트리거</param>
        /// <returns>수집된 카드 상태 데이터</returns>
        CompleteCardStateData CollectCompleteCardState(string saveTrigger);

        /// <summary>
        /// 플레이어 핸드카드 상태를 수집합니다.
        /// </summary>
        /// <returns>플레이어 핸드카드 데이터</returns>
        CompleteCardStateData CollectPlayerHandState();

        /// <summary>
        /// 적 핸드카드 상태를 수집합니다.
        /// </summary>
        /// <returns>적 핸드카드 데이터</returns>
        CompleteCardStateData CollectEnemyHandState();

        /// <summary>
        /// 전투 슬롯 카드 상태를 수집합니다.
        /// </summary>
        /// <returns>전투 슬롯 카드 데이터</returns>
        CompleteCardStateData CollectCombatSlotState();

        /// <summary>
        /// 카드 순환 시스템 상태를 수집합니다.
        /// </summary>
        /// <returns>카드 순환 상태 데이터</returns>
        CompleteCardStateData CollectCardCirculationState();

        #endregion

        #region 턴 상태 수집

        /// <summary>
        /// 현재 턴 상태를 수집합니다.
        /// </summary>
        /// <returns>턴 상태 데이터</returns>
        CompleteCardStateData CollectTurnState();

        /// <summary>
        /// 현재 턴 단계를 가져옵니다.
        /// </summary>
        /// <returns>턴 단계 문자열</returns>
        string GetCurrentTurnPhase();

        #endregion

        #region 유틸리티

        /// <summary>
        /// 카드 상태 수집이 가능한지 확인합니다.
        /// </summary>
        /// <returns>수집 가능 여부</returns>
        bool CanCollectCardState();

        /// <summary>
        /// 수집된 카드 상태의 유효성을 검증합니다.
        /// </summary>
        /// <param name="cardState">검증할 카드 상태</param>
        /// <returns>유효성 여부</returns>
        bool ValidateCardState(CompleteCardStateData cardState);

        #endregion
    }
}
