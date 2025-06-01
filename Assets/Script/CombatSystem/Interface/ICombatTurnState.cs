namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 컴뱃 턴 상태를 나타내는 인터페이스입니다.
    /// 상태 패턴 기반으로 전투 흐름을 제어합니다.
    /// </summary>
    public interface ICombatTurnState
    {
        /// <summary> 상태 진입 시 호출 </summary>
        void EnterState();

        /// <summary> 상태 종료 시 호출 </summary>
        void ExitState();

        /// <summary> 매 프레임 실행되는 상태 로직 </summary>
        void ExecuteState();
    }
}
