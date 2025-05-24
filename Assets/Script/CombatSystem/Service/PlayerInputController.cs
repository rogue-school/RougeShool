using System.Collections;
using Game.CombatSystem.Interface;
using Game.IManager;

namespace Game.CombatSystem.Service
{
    public class PlayerInputController : IPlayerInputController
    {
        private readonly IPlayerHandManager playerHandManager;

        public PlayerInputController(IPlayerHandManager playerHandManager)
        {
            this.playerHandManager = playerHandManager;
        }

        public IEnumerator EnablePlayerInput()
        {
            playerHandManager.EnableInput(true);
            yield return null;
        }

        public IEnumerator DisablePlayerInput()
        {
            playerHandManager.EnableInput(false);
            yield return null;
        }
    }
}
