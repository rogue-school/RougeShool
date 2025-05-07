using UnityEngine;
using Game.Interface;
using Game.Slots;
using Game.Managers;
using Game.Combat.Turn;

namespace Game.Managers
{
    /// <summary>
    /// 전투 턴 흐름을 관리하는 매니저입니다.
    /// 상태 패턴 기반으로 턴 상태를 전환하며, 카드 실행 시점을 제어합니다.
    /// </summary>
    public class CombatTurnManager : MonoBehaviour
    {
        private ICombatTurnState currentState;

        private ISkillCard enemyCard;
        private ISkillCard playerCard;

        /// <summary>
        /// 턴 상태를 설정하고 상태 전환을 수행합니다.
        /// </summary>
        public void SetState(ICombatTurnState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();

            Debug.Log($"[CombatTurnManager] 상태 전환됨 → {newState.GetType().Name}");
        }

        private void Update()
        {
            currentState?.ExecuteState();
        }

        /// <summary>
        /// 적이 사용하는 카드 등록
        /// </summary>
        public void ReserveEnemyCard(ISkillCard card)
        {
            enemyCard = card;
        }

        /// <summary>
        /// 플레이어가 핸드에서 제출한 카드 등록 → 두 카드가 모두 준비되면 전투 실행
        /// </summary>
        public void RegisterPlayerCard(ISkillCard card)
        {
            playerCard = card;

            if (enemyCard != null)
                ExecuteCombat();
            else
                Debug.LogWarning("[CombatTurnManager] 적 카드가 아직 준비되지 않았습니다.");
        }

        /// <summary>
        /// 전투 슬롯에 배치된 두 카드를 실행
        /// </summary>
        private void ExecuteCombat()
        {
            var slotFirst = SlotRegistry.Instance.GetCombatSlot(CombatSlotPosition.FIRST);
            var slotSecond = SlotRegistry.Instance.GetCombatSlot(CombatSlotPosition.SECOND);

            if (enemyCard.GetOwnerSlot() == CombatSlotPosition.FIRST)
            {
                slotFirst.SetCard(enemyCard);
                slotSecond.SetCard(playerCard);
            }
            else
            {
                slotFirst.SetCard(playerCard);
                slotSecond.SetCard(enemyCard);
            }

            slotFirst.ExecuteCardAutomatically();
            slotSecond.ExecuteCardAutomatically();

            Debug.Log("[CombatTurnManager] 전투 실행 완료");

            // 다음 턴 준비
            playerCard = null;
            enemyCard = null;
            EnemyHandManager.Instance.AdvanceSlots();
        }
    }
}
