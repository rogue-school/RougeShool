using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICombatTurnManager
    {
        void Initialize();
        ICombatTurnState GetCurrentState();
        void Reset();
        void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
        void RequestStateChange(ICombatTurnState nextState);
        void ChangeState(ICombatTurnState newState);

        CombatSlotPosition? GetReservedEnemySlot();

        ICombatStateFactory GetStateFactory();

        bool IsPlayerInputTurn();
    }
}
