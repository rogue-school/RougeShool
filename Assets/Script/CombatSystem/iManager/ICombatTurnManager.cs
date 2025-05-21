using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ICombatTurnManager : ITurnStateController
    {
        bool IsPlayerGuarded();
        CombatSlotPosition GetReservedEnemySlot();
        void ResetGuardAndReservation();

        void RegisterPlayerCard(ISkillCard card);
        void RegisterEnemyCard(ISkillCard card);
        bool AreBothSlotsReady();
        void ExecuteCombat();
        bool CanStartTurn();
    }
}
