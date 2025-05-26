using Game.CombatSystem.Interface;
using Game.IManager;
using System;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 플레이어 입력을 제어하는 컨트롤러 클래스
    /// SRP: 핸드 UI 입력 제어만 수행
    /// </summary>
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
