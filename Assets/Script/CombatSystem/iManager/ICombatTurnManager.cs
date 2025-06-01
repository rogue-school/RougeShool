using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

public interface ICombatTurnManager
{
    void Initialize();
    ICombatTurnState GetCurrentState();
    void Reset();
    void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
    void RequestStateChange(ICombatTurnState nextState);
}
