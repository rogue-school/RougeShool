using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using System;

namespace Game.CombatSystem.Interface
{
    public interface ITurnCardRegistry
    {
        event Action OnCardStateChanged;

        void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);

        void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner);

        ISkillCard GetPlayerCard(CombatSlotPosition slot);
        ISkillCard GetEnemyCard();

        void ClearPlayerCard(CombatSlotPosition slot);
        void ClearEnemyCard();
        void ClearSlot(CombatSlotPosition slot);

        void ReserveNextEnemySlot(CombatSlotPosition slot);
        CombatSlotPosition? GetReservedEnemySlot();

        void Reset();
        void ClearAll();

        bool HasPlayerCard();
        bool HasEnemyCard();
    }
}
