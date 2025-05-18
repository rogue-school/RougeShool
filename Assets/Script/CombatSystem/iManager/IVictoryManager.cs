namespace Game.IManager
{
    /// <summary>
    /// 전투 승리 시 승리 UI 또는 게임 흐름을 처리하는 매니저입니다.
    /// </summary>
    public interface IVictoryManager
    {
        /// <summary>
        /// 승리 화면 또는 후속 처리 흐름을 시작합니다.
        /// </summary>
        void ShowVictoryUI();
    }
}