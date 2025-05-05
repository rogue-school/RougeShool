namespace Game.Battle
{
    /// <summary>
    /// 전투 턴 상태 제어를 위한 인터페이스입니다.
    /// 상태 패턴 의존성을 약화하고 테스트 가능성을 높입니다.
    /// </summary>
    public interface ITurnStateController
    {
        void SetState(IBattleTurnState newState);
        void RegisterPlayerGuard();
        void ReserveEnemySlot(SlotPosition slot);
    }
}
