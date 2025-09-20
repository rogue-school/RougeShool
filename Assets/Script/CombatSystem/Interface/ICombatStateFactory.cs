using Game.CombatSystem.State;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 상태(FSM)를 생성하는 팩토리 인터페이스입니다.
    /// 각 전투 단계(준비, 입력, 공격, 결과 등)에 해당하는 상태 객체를 생성합니다.
    /// 새로운 단순화된 아키텍처에서는 구체적인 상태 클래스를 반환합니다.
    /// </summary>
    public interface ICombatStateFactory
    {
        /// <summary>
        /// 전투 준비 상태를 생성합니다.
        /// </summary>
        CombatPrepareState CreatePrepareState();

        /// <summary>
        /// 플레이어 입력 상태를 생성합니다.
        /// </summary>
        CombatPlayerInputState CreatePlayerInputState();

        /// <summary>
        /// 공격 상태를 생성합니다.
        /// </summary>
        CombatAttackState CreateAttackState();

        /// <summary>
        /// 공격 결과 정리 상태를 생성합니다.
        /// </summary>
        CombatResultState CreateResultState();

        /// <summary>
        /// 전투 승리 상태를 생성합니다.
        /// </summary>
        CombatVictoryState CreateVictoryState();

        /// <summary>
        /// 게임 오버 상태를 생성합니다.
        /// </summary>
        CombatGameOverState CreateGameOverState();
    }
}
