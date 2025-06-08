namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 턴 상태를 나타내는 상태 패턴 인터페이스입니다.
    /// 상태 전환(Enter/Exit)과 프레임 기반 실행 로직을 정의합니다.
    /// </summary>
    public interface ICombatTurnState
    {
        /// <summary>
        /// 상태에 진입할 때 호출됩니다.
        /// 초기화 또는 시작 로직을 수행합니다.
        /// </summary>
        void EnterState();

        /// <summary>
        /// 상태에서 벗어날 때 호출됩니다.
        /// 리소스 해제나 정리 로직을 수행합니다.
        /// </summary>
        void ExitState();

        /// <summary>
        /// 상태가 활성화된 동안 매 프레임 호출됩니다.
        /// 실시간 처리 로직을 구현할 수 있습니다.
        /// </summary>
        void ExecuteState();
    }
}
