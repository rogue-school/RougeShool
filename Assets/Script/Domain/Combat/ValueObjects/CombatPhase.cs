namespace Game.Domain.Combat.ValueObjects
{
    /// <summary>
    /// 전투의 진행 단계를 나타내는 값입니다.
    /// </summary>
    public enum CombatPhase
    {
        /// <summary>
        /// 아직 전투가 시작되지 않았습니다.
        /// </summary>
        None,

        /// <summary>
        /// 전투 준비 단계입니다.
        /// </summary>
        Preparation,

        /// <summary>
        /// 플레이어 턴입니다.
        /// </summary>
        PlayerTurn,

        /// <summary>
        /// 적 턴입니다.
        /// </summary>
        EnemyTurn,

        /// <summary>
        /// 카드/효과 해결 단계입니다.
        /// </summary>
        Resolution,

        /// <summary>
        /// 승리 상태입니다.
        /// </summary>
        Victory,

        /// <summary>
        /// 패배 상태입니다.
        /// </summary>
        Defeat,

        /// <summary>
        /// 전투가 종료된 상태입니다.
        /// </summary>
        Ended,

        /// <summary>
        /// 일시정지 상태입니다.
        /// </summary>
        Paused
    }
}


