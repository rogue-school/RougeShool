using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ITurnStateController
    {
        void RequestStateChange(ICombatTurnState nextState);

        void ReserveNextEnemySlot(CombatSlotPosition slot);

        void RegisterPlayerGuard();

        // 플레이어 카드 등록을 위한 메서드 추가
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
    }
}
