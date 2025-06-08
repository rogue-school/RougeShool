using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using System;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 플레이어 입력을 제어하는 컨트롤러입니다.
    /// 핸드 카드 드래그 입력을 활성화 또는 비활성화합니다.
    /// </summary>
    public class PlayerInputController : IPlayerInputController
    {
        #region 필드

        private readonly IPlayerHandManager playerHandManager;

        #endregion

        #region 생성자

        /// <summary>
        /// PlayerInputController를 초기화합니다.
        /// </summary>
        /// <param name="playerHandManager">플레이어 핸드 매니저</param>
        /// <exception cref="ArgumentNullException">playerHandManager가 null일 경우</exception>
        public PlayerInputController(IPlayerHandManager playerHandManager)
        {
            this.playerHandManager = playerHandManager ?? throw new ArgumentNullException(nameof(playerHandManager));
        }

        #endregion

        #region 입력 제어

        /// <summary>
        /// 플레이어 입력을 활성화합니다.
        /// </summary>
        public void EnablePlayerInput()
        {
            playerHandManager.EnableInput(true);
        }

        /// <summary>
        /// 플레이어 입력을 비활성화합니다.
        /// </summary>
        public void DisablePlayerInput()
        {
            playerHandManager.EnableInput(false);
        }

        #endregion
    }
}
