namespace Game.Domain.Stage.ValueObjects
{
    /// <summary>
    /// 스테이지 진행 상태를 나타내는 값입니다.
    /// </summary>
    public enum StageProgressState
    {
        /// <summary>
        /// 아직 시작되지 않은 상태입니다.
        /// </summary>
        NotStarted,

        /// <summary>
        /// 진행 중 상태입니다.
        /// </summary>
        InProgress,

        /// <summary>
        /// 완료(승리) 상태입니다.
        /// </summary>
        Completed,

        /// <summary>
        /// 실패 상태입니다.
        /// </summary>
        Failed
    }
}


