namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 플레이어의 입력 가능 여부를 제어하는 인터페이스입니다.
    /// 드래그 등 카드 입력을 활성화하거나 비활성화할 수 있습니다.
    /// </summary>
    public interface IPlayerInputController
    {
        /// <summary>
        /// 플레이어 입력을 활성화합니다.
        /// </summary>
        void EnablePlayerInput();

        /// <summary>
        /// 플레이어 입력을 비활성화합니다.
        /// </summary>
        void DisablePlayerInput();
    }
}
