using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ITurnCardRegistry
    {
        void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);
        void ClearSlot(CombatSlotPosition slot);
        CombatSlotPosition? GetReservedEnemySlot();
        void ReserveNextEnemySlot(CombatSlotPosition slot);
        void Reset();
    }
}
