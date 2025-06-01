using System;
using System.Collections;

namespace Game.CombatSystem.Interface
{
    public interface ICombatPreparationService
    {
        IEnumerator PrepareCombat(Action<bool> onComplete);
    }
}
