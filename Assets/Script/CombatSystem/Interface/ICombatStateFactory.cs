using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투 상태(FSM)를 생성하는 팩토리 인터페이스입니다.
    /// 각 상태를 생성하며, 상태 전이는 CombatTurnManager에서 수행됩니다.
    /// </summary>
    public interface ICombatStateFactory
    {
        ICombatTurnState CreatePrepareState();
        ICombatTurnState CreatePlayerInputState();
        ICombatTurnState CreateFirstAttackState();
        ICombatTurnState CreateSecondAttackState();
        ICombatTurnState CreateResultState();
        ICombatTurnState CreateVictoryState();
        ICombatTurnState CreateGameOverState();
    }
}