using System.Collections;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICombatExecutor
    {
        IEnumerator PerformAttack(CombatSlotPosition slotPosition);
        void InjectExecutionDependencies(ICardExecutionContextProvider provider, ICardExecutor executor);
    }
}
