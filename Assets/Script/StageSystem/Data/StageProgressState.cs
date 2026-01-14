namespace Game.StageSystem.Data
{
    /// <summary>
    /// 스테이지 진행 상태(단순 버전). 단계 구분 없이 진행/완료/실패만 관리합니다.
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
        /// 스테이지 완료
        /// </summary>
        Completed,

        /// <summary>
        /// 스테이지 실패
        /// </summary>
        Failed
    }
}


