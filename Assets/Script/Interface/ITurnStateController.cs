using Game.Battle;

namespace Game.Interface
{
    /// <summary>
    /// 턴 상태 제어 인터페이스
    /// </summary>
    public interface ITurnStateController
    {
        void RegisterPlayerGuard();

        void ReserveEnemySlot(BattleSlotPosition slot);
    }
}
