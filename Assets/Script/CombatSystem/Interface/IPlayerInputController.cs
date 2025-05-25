using System.Collections;

namespace Game.CombatSystem.Interface
{
    public interface IPlayerInputController
    {
        IEnumerator EnablePlayerInput();
        IEnumerator DisablePlayerInput();
    }
}
