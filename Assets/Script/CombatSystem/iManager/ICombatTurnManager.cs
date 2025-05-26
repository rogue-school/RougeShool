using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICombatTurnManager
    {
        void Initialize();
        void InjectFactory(ICombatStateFactory factory);
        void Reset();
        ICombatTurnState GetCurrentState();
    }
}
