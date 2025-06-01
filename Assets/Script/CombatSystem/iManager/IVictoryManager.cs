namespace Game.IManager
{
    /// <summary>
    /// 전투 승리 시 승리 UI 또는 게임 흐름을 처리하는 매니저입니다.
    /// </summary>
    public interface IVictoryManager
    {
        void ShowVictoryUI();

        /// <summary>
        /// 전투 승리 처리를 수행합니다. (캐릭터 처치 후 호출됨)
        /// </summary>
        void ProcessVictory(); // 추가됨
    }
}
