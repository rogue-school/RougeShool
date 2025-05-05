using Game.Battle;
using Game.Managers;
using UnityEngine;


namespace Game.BattleStates
{
    /// <summary>
    /// 기본 전투 턴 처리 상태입니다.
    /// </summary>
    public class DefaultBattleState : IBattleTurnState
    {
        private readonly BattleTurnManager turnManager;

        public DefaultBattleState(BattleTurnManager manager)
        {
            turnManager = manager;
        }

        public void ExecuteTurn()
        {
            // 플레이어와 적의 턴 처리 로직
            Debug.Log("기본 전투 턴이 실행되었습니다.");

            // 조건에 따라 다음 상태로 전환 가능
            // ex) turnManager.SetState(new PlayerGuardedState(turnManager));
        }
    }
}
