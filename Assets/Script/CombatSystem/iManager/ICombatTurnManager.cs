using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Interface
{
    public interface ICombatTurnManager
    {
        void Initialize();
        void Reset();

        void RequestStateChange(ICombatTurnState nextState);
        void ChangeState(ICombatTurnState newState);
        ICombatTurnState GetCurrentState();
        ICombatStateFactory GetStateFactory();

        void ReserveNextEnemySlot(CombatSlotPosition slot);
        CombatSlotPosition? GetReservedEnemySlot();

        bool IsPlayerInputTurn();
        void RegisterCard(CombatSlotPosition slot, ISkillCard card, SkillCardUI ui, SlotOwner owner);
    }
}
