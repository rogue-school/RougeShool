namespace Game.IManager
{
    /// <summary>
    /// 게임 오버 시 관련 처리를 담당하는 매니저입니다.
    /// </summary>
    public interface IGameOverManager
    {
        /// <summary>
        /// 게임 오버 UI 및 종료 처리 흐름을 실행합니다.
        /// </summary>
        void ShowGameOverUI();
    }
}