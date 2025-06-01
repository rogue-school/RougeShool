using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ISlotRegistry
    {
        IHandSlotRegistry GetHandSlotRegistry();
        ICombatSlotRegistry GetCombatSlotRegistry();
        ICharacterSlotRegistry GetCharacterSlotRegistry();

        // 직접 슬롯 접근 메서드 추가
        ICombatCardSlot GetCombatSlot(CombatSlotPosition position);
        ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition);

        void MarkInitialized();
        bool IsInitialized { get; }
    }
}
