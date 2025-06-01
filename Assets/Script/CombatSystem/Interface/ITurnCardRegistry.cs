using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface ITurnCardRegistry
    {
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);

        void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI cardUI);

        ISkillCard GetPlayerCard(CombatSlotPosition position);
        ISkillCard GetEnemyCard();

        void ClearPlayerCard(CombatSlotPosition position);
        void ClearEnemyCard();
        void ClearSlot(CombatSlotPosition position);

        CombatSlotPosition? GetReservedEnemySlot();
        void ReserveNextEnemySlot(CombatSlotPosition position);

        void Reset();

        void ClearAll();
        bool HasPlayerCard();
    }
}
