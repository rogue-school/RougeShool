using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using System;

namespace Game.CombatSystem.Service
{
    public class PlayerInputController : IPlayerInputController
    {
        private readonly IPlayerHandManager playerHandManager;

        public PlayerInputController(IPlayerHandManager playerHandManager)
        {
            this.playerHandManager = playerHandManager ?? throw new ArgumentNullException(nameof(playerHandManager));
        }

        public void EnablePlayerInput()
        {
            playerHandManager.EnableInput(true);
        }

        public void DisablePlayerInput()
        {
            playerHandManager.EnableInput(false);
        }
    }
}
