using Game.CombatSystem.Utility;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 플로우 관리자 인터페이스
    /// </summary>
    public interface ICombatFlowManager
    {
        /// <summary>
        /// 전투 시작
        /// </summary>
        void StartCombat();

        /// <summary>
        /// 전투 종료
        /// </summary>
        void EndCombat();

        /// <summary>
        /// 전투 단계 변경
        /// </summary>
        /// <param name="newPhase">새로운 전투 단계</param>
        void ChangeCombatPhase(CombatPhase newPhase);

        /// <summary>
        /// 다음 턴으로 진행
        /// </summary>
        void ProgressTurn();

        /// <summary>
        /// 전투 리셋
        /// </summary>
        void ResetCombat();

        /// <summary>
        /// 현재 전투 단계
        /// </summary>
        CombatPhase CurrentPhase { get; }

        /// <summary>
        /// 전투 활성 상태
        /// </summary>
        bool IsCombatActive { get; }

        /// <summary>
        /// 초기화 상태
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// 전투 단계 변경 이벤트
        /// </summary>
        System.Action<CombatPhase> OnCombatPhaseChanged { get; set; }

        /// <summary>
        /// 전투 시작 이벤트
        /// </summary>
        System.Action OnCombatStarted { get; set; }

        /// <summary>
        /// 전투 종료 이벤트
        /// </summary>
        System.Action OnCombatEnded { get; set; }
    }
}
