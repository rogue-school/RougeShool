namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 컴뱃(전투) 턴 상태를 나타내는 인터페이스입니다.
    /// 상태 패턴 기반으로 전투 흐름을 제어합니다.
    /// </summary>
    public interface ICombatTurnState
    {
        /// <summary>
        /// 이 상태에 진입할 때 호출됩니다.
        /// 초기화 로직 또는 로그 출력 등에 사용됩니다.
        /// </summary>
        void EnterState();

        /// <summary>
        /// 이 상태에서 나갈 때 호출됩니다.
        /// 정리 로직에 사용됩니다.
        /// </summary>
        void ExitState();

        /// <summary>
        /// 현재 상태에서 매 프레임 실행될 로직이 있을 경우 호출됩니다.
        /// 주로 조건 판단이나 상태 전환 트리거 등에 사용됩니다.
        /// </summary>
        void ExecuteState();
    }
}
