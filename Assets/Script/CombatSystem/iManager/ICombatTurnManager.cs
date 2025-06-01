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

        // 추가됨
        CombatSlotPosition? GetReservedEnemySlot();
        void ChangeState(ICombatTurnState newState); // 누락되었던 정의 추가
    }
}
