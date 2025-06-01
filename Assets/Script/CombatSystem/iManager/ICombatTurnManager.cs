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

        // 추가
        CombatSlotPosition? GetReservedEnemySlot();

        //FSM 상태 클래스에서 다음 상태 생성 시 필요
        ICombatStateFactory GetStateFactory();
    }
}
