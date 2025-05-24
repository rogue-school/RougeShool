using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.Turn
{
    public class CombatTurnStateController : ITurnStateController
    {
        private ICombatTurnState currentState;

        private CombatSlotPosition? reservedEnemySlot;

        public void SetInitialState(ICombatTurnState state)
        {
            currentState = state;
        }

        public void RequestStateChange(ICombatTurnState newState)
        {
            currentState = newState;
        }

        public void ExecuteCombat()
        {
            currentState?.ExecuteState();
        }

        // 인터페이스에서 요구하는 정확한 시그니처의 구현
        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
        }

        // 선택적: 다른 시스템에서 접근 가능하게 하기 위한 getter
        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return reservedEnemySlot;
        }
        public void RegisterPlayerGuard()
        {
            Debug.Log("[CombatTurnStateController] RegisterPlayerGuard() 호출됨");
            // 이 부분은 필요에 따라 나중에 구현 가능
        }

    }
}
