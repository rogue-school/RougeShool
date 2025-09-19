using Game.SaveSystem.Data;

namespace Game.SaveSystem.Interface
{
    /// <summary>
    /// 카드 상태 복원 인터페이스
    /// 슬레이 더 스파이어 방식: 저장된 카드 상태를 복원하는 인터페이스
    /// </summary>
    public interface ICardStateRestorer
    {
        #region 카드 상태 복원

        /// <summary>
        /// 완전한 카드 상태를 복원합니다.
        /// </summary>
        /// <param name="cardState">복원할 카드 상태 데이터</param>
        /// <returns>복원 성공 여부</returns>
        bool RestoreCompleteCardState(CompleteCardStateData cardState);

        /// <summary>
        /// 플레이어 핸드카드 상태를 복원합니다.
        /// </summary>
        /// <param name="cardState">복원할 카드 상태 데이터</param>
        /// <returns>복원 성공 여부</returns>
        bool RestorePlayerHandState(CompleteCardStateData cardState);

        // 적 핸드카드 상태 복원 메서드 제거됨 - 적 카드는 대기 슬롯에서 직접 관리

        /// <summary>
        /// 전투 슬롯 카드 상태를 복원합니다.
        /// </summary>
        /// <param name="cardState">복원할 카드 상태 데이터</param>
        /// <returns>복원 성공 여부</returns>
        bool RestoreCombatSlotState(CompleteCardStateData cardState);

        /// <summary>
        /// 카드 순환 시스템 상태를 복원합니다.
        /// </summary>
        /// <param name="cardState">복원할 카드 상태 데이터</param>
        /// <returns>복원 성공 여부</returns>
        bool RestoreCardCirculationState(CompleteCardStateData cardState);

        #endregion

        #region 턴 상태 복원

        /// <summary>
        /// 턴 상태를 복원합니다.
        /// </summary>
        /// <param name="cardState">복원할 카드 상태 데이터</param>
        /// <returns>복원 성공 여부</returns>
        bool RestoreTurnState(CompleteCardStateData cardState);

        /// <summary>
        /// 현재 턴 단계를 설정합니다.
        /// </summary>
        /// <param name="turnPhase">턴 단계</param>
        /// <returns>설정 성공 여부</returns>
        bool SetCurrentTurnPhase(string turnPhase);

        #endregion

        #region 유틸리티

        /// <summary>
        /// 카드 상태 복원이 가능한지 확인합니다.
        /// </summary>
        /// <returns>복원 가능 여부</returns>
        bool CanRestoreCardState();

        /// <summary>
        /// 복원할 카드 상태의 유효성을 검증합니다.
        /// </summary>
        /// <param name="cardState">검증할 카드 상태</param>
        /// <returns>유효성 여부</returns>
        bool ValidateCardStateForRestore(CompleteCardStateData cardState);

        /// <summary>
        /// 현재 게임 상태를 초기화합니다.
        /// </summary>
        void ClearCurrentGameState();

        #endregion
    }
}
