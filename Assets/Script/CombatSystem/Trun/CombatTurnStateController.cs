using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
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

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
        }

        public CombatSlotPosition? GetReservedEnemySlot()
        {
            return reservedEnemySlot;
        }

        public void RegisterPlayerGuard()
        {
            Debug.Log("[CombatTurnStateController] RegisterPlayerGuard() 호출됨");
            // 가드 관련 처리 필요 시 구현
        }

        //  인터페이스 메서드 완성
        public void RegisterPlayerCard(CombatSlotPosition position, ISkillCard card)
        {
            Debug.Log($"[CombatTurnStateController] RegisterPlayerCard 호출됨: 위치={position}, 카드={card?.CardData?.Name ?? "null"}");
            // 실제 전투 카드 등록 로직이 필요한 경우, CombatTurnManager 같은 곳으로 위임하는 구조를 만들 수 있습니다.
        }
    }
}
