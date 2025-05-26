using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ITurnCardRegistry
    {
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);

        ISkillCard GetPlayerCard(CombatSlotPosition position);
        ISkillCard GetEnemyCard();

        void ClearPlayerCard(CombatSlotPosition position);
        void ClearEnemyCard();

        CombatSlotPosition? GetReservedEnemySlot();
        void ReserveNextEnemySlot(CombatSlotPosition position);

        void Reset();
    }
}
