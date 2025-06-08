namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 상태(FSM)를 생성하는 팩토리 인터페이스입니다.
    /// 각 전투 단계(준비, 입력, 공격, 결과 등)에 해당하는 상태 객체를 생성합니다.
    /// 상태 전이는 CombatTurnManager 또는 CombatFlowCoordinator에서 제어합니다.
    /// </summary>
    public interface ICombatStateFactory
    {
        /// <summary>
        /// 전투 준비 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreatePrepareState();

        /// <summary>
        /// 플레이어 입력 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreatePlayerInputState();

        /// <summary>
        /// 선공 캐릭터의 공격 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreateFirstAttackState();

        /// <summary>
        /// 후공 캐릭터의 공격 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreateSecondAttackState();

        /// <summary>
        /// 공격 결과 정리 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreateResultState();

        /// <summary>
        /// 전투 승리 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreateVictoryState();

        /// <summary>
        /// 게임 오버 상태를 생성합니다.
        /// </summary>
        ICombatTurnState CreateGameOverState();
    }
}
