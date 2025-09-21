namespace Game.StageSystem.Data
{
    /// <summary>
    /// 스테이지 단계 상태
    /// </summary>
    public enum StagePhaseState
    {
        /// <summary>
        /// 초기 상태
        /// </summary>
        None,

        /// <summary>
        /// 준보스 단계
        /// </summary>
        SubBoss,

        /// <summary>
        /// 보스 단계
        /// </summary>
        Boss,

        /// <summary>
        /// 스테이지 완료
        /// </summary>
        Completed
    }

    /// <summary>
    /// 스테이지 진행 상태
    /// </summary>
    public enum StageProgressState
    {
        /// <summary>
        /// 스테이지 시작 전
        /// </summary>
        NotStarted,

        /// <summary>
        /// 스테이지 진행 중
        /// </summary>
        InProgress,

        /// <summary>
        /// 준보스 전투 중
        /// </summary>
        SubBossBattle,

        /// <summary>
        /// 보스 전투 중
        /// </summary>
        BossBattle,

        /// <summary>
        /// 스테이지 완료
        /// </summary>
        Completed,

        /// <summary>
        /// 스테이지 실패
        /// </summary>
        Failed
    }
}
