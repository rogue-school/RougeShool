using Game.Battle;
using Game.Managers;
using UnityEngine;

namespace Game.BattleStates
{
    public class PlayerGuardedState : IBattleTurnState
    {
        private readonly BattleTurnManager turnManager;

        public PlayerGuardedState(BattleTurnManager manager)
        {
            turnManager = manager;
        }

        public void ExecuteTurn()
        {
            Debug.Log("플레이어가 방어 상태에서 턴이 실행되었습니다.");
            // 방어 처리 로직
            turnManager.SetState(new DefaultBattleState(turnManager));
        }
    }
}