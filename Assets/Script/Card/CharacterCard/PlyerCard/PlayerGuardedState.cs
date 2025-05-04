using Game.Interface;
using UnityEngine;

namespace Game.Battle
{
    public class PlayerGuardedState : IBattleTurnState
    {
        public void ExecuteTurn()
        {
            Debug.Log("[BattleState] 플레이어가 방어 상태입니다. 적의 다음 공격은 무효화됩니다.");
            // 방어 상태 효과 유지 처리 등
        }
    }
}
