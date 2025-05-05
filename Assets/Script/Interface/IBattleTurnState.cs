namespace Game.Battle
{
    /// <summary>
    /// 전투 턴 상태를 위한 상태 인터페이스입니다.
    /// </summary>
    public interface IBattleTurnState
    {
        /// <summary>
        /// 해당 턴 상태에서 실행될 로직입니다.
        /// </summary>
        void ExecuteTurn();
    }
}
