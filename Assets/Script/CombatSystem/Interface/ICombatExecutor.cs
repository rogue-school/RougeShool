using System.Collections;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    public interface ICombatExecutor
    {
        IEnumerator PerformAttack(CombatSlotPosition slotPosition);
    }
}
