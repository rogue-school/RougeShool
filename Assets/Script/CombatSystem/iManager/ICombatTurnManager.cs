using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

public interface ICombatTurnManager
{
    void Inject(ICombatStateFactory factory, ITurnCardRegistry cardRegistry);
    void Initialize();
    ICombatTurnState GetCurrentState();
    void Reset();

    // 누락된 메서드 추가
    void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card);
    void RequestStateChange(ICombatTurnState nextState);
}
