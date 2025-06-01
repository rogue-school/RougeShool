using System.Collections;

namespace Game.CombatSystem.Interface
{
    public interface ICombatInitializerStep
    {
        int Order { get; }
        IEnumerator Initialize();
    }
}
