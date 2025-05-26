namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 플레이어 입력 활성화/비활성화를 제어하는 인터페이스
    /// </summary>
    public interface IPlayerInputController
    {
        void EnablePlayerInput();
        void DisablePlayerInput();
    }
}
