namespace Game.IManager
{
    /// <summary>
    /// 게임 오버 상황에서 관련 UI 처리 및 전환 로직을 담당하는 매니저 인터페이스입니다.
    /// </summary>
    public interface IGameOverManager
    {
        /// <summary>
        /// 게임 오버 UI를 화면에 표시합니다.
        /// 예: 사망 메시지, 재시작 버튼, 메인 메뉴 이동 등
        /// </summary>
        void ShowGameOverUI();

        /// <summary>
        /// 게임 오버 처리를 수행합니다.
        /// 보통 플레이어 사망 등의 조건 충족 시 호출되며,
        /// UI 출력과 게임 흐름 정지를 포함합니다.
        /// </summary>
        void TriggerGameOver();
    }
}
