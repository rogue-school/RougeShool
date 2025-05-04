using Game.Interface;
using UnityEngine;

namespace Game.Battle
{
    public class DefaultBattleState : IBattleTurnState
    {
        public void ExecuteTurn()
        {
            Debug.Log("[BattleState] 일반 전투 턴 실행");
            // 기본 턴 처리
        }
    }
}
