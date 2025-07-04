namespace Game.IManager
{
    /// <summary>
    /// 전투 승리 시 UI 표시 및 후속 게임 흐름을 처리하는 매니저입니다.
    /// </summary>
    public interface IVictoryManager
    {
        /// <summary>
        /// 승리 화면(UI)을 표시합니다.
        /// 게임의 흐름은 정지되거나 승리 인터랙션이 활성화됩니다.
        /// </summary>
        void ShowVictoryUI();

        /// <summary>
        /// 전투 승리 시 후속 처리를 수행합니다.
        /// 예: 보상 지급, 다음 스테이지로 이동, 저장 등.
        /// </summary>
        void ProcessVictory();
    }
}
