using Game.StageSystem.Data;

namespace Game.StageSystem.Interface
{
    /// <summary>
    /// 스테이지 단계별 관리 인터페이스
    /// </summary>
    public interface IStagePhaseManager
    {
        #region 현재 상태

        /// <summary>
        /// 현재 스테이지 단계
        /// </summary>
        StagePhaseState CurrentPhase { get; }

        /// <summary>
        /// 현재 스테이지 진행 상태
        /// </summary>
        StageProgressState ProgressState { get; }

        /// <summary>
        /// 준보스가 처치되었는지 확인
        /// </summary>
        bool IsSubBossDefeated { get; }

        /// <summary>
        /// 보스가 처치되었는지 확인
        /// </summary>
        bool IsBossDefeated { get; }

        #endregion

        #region 단계 관리

        /// <summary>
        /// 준보스 단계 시작
        /// </summary>
        void StartSubBossPhase();

        /// <summary>
        /// 보스 단계 시작
        /// </summary>
        void StartBossPhase();

        /// <summary>
        /// 스테이지 완료
        /// </summary>
        void CompleteStage();

        /// <summary>
        /// 스테이지 실패
        /// </summary>
        void FailStage();

        #endregion

        #region 단계 확인

        /// <summary>
        /// 준보스 단계인지 확인
        /// </summary>
        bool IsSubBossPhase();

        /// <summary>
        /// 보스 단계인지 확인
        /// </summary>
        bool IsBossPhase();

        /// <summary>
        /// 스테이지가 완료되었는지 확인
        /// </summary>
        bool IsStageCompleted();

        #endregion

        #region 이벤트

        /// <summary>
        /// 단계 변경 이벤트
        /// </summary>
        event System.Action<StagePhaseState> OnPhaseChanged;

        /// <summary>
        /// 진행 상태 변경 이벤트
        /// </summary>
        event System.Action<StageProgressState> OnProgressChanged;

        #endregion
    }
}
