namespace Game.CombatSystem.Interface
{
    public interface ISlotRegistry
    {
        IHandSlotRegistry GetHandSlotRegistry();
        ICombatSlotRegistry GetCombatSlotRegistry();
        ICharacterSlotRegistry GetCharacterSlotRegistry();
        void MarkInitialized(); // 인터페이스에 명시적으로 선언
        bool IsInitialized { get; }
    }
}
