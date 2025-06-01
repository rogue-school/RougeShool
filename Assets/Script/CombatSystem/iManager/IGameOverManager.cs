namespace Game.IManager
{
    /// <summary>
    /// 게임 오버 시 관련 처리를 담당하는 매니저입니다.
    /// </summary>
    public interface IGameOverManager
    {
        void ShowGameOverUI();

        /// <summary>
        /// 게임 오버 처리를 수행합니다. (플레이어 사망 시 호출됨)
        /// </summary>
        void TriggerGameOver(); // 추가됨
    }
}
