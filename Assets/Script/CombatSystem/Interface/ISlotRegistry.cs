using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ISlotRegistry
    {
        IHandSlotRegistry GetHandSlotRegistry();
        ICombatSlotRegistry GetCombatSlotRegistry();
        ICharacterSlotRegistry GetCharacterSlotRegistry();
    }
}
