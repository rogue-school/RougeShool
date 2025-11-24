using Game.Domain.Combat.Entities;
using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Interfaces
{
    /// <summary>
    /// 전투 턴 진행을 관리하는 도메인 인터페이스입니다.
    /// </summary>
    public interface ITurnManager
    {
        /// <summary>
        /// 현재 턴 번호입니다. (0이면 아직 턴이 시작되지 않음)
        /// </summary>
        int CurrentTurnNumber { get; }

        /// <summary>
        /// 현재 턴 주체입니다.
        /// </summary>
        TurnType CurrentTurnType { get; }

        /// <summary>
        /// 현재 전투 페이즈입니다.
        /// </summary>
        CombatPhase Phase { get; }

        /// <summary>
        /// 새로운 턴을 시작합니다.
        /// </summary>
        /// <param name="turnType">턴 주체</param>
        /// <returns>시작된 턴 정보</returns>
        Turn StartNextTurn(TurnType turnType);

        /// <summary>
        /// 현재 턴을 완료 상태로 표시합니다.
        /// </summary>
        void CompleteCurrentTurn();

        /// <summary>
        /// 전투 페이즈를 변경합니다.
        /// </summary>
        /// <param name="nextPhase">다음 페이즈</param>
        void ChangePhase(CombatPhase nextPhase);
    }
}


