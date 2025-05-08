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
        public static CombatTurnManager Instance { get; private set; }

        private ICombatTurnState currentState;
        private ISkillCard enemyCard;
        private ISkillCard playerCard;

        private void Awake()
        {
            // 싱글톤 초기화
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

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

        public void ReserveEnemyCard(ISkillCard card)
        {
            enemyCard = card;
        }

        public void RegisterPlayerCard(ISkillCard card)
        {
            playerCard = card;

            if (enemyCard != null)
                ExecuteCombat();
            else
                Debug.LogWarning("[CombatTurnManager] 적 카드가 아직 준비되지 않았습니다.");
        }

        private void ExecuteCombat()
        {
            var slotFirst = SlotRegistry.Instance.GetCombatSlot(CombatSlotPosition.FIRST);
            var slotSecond = SlotRegistry.Instance.GetCombatSlot(CombatSlotPosition.SECOND);

            if (enemyCard.GetCombatSlot() == CombatSlotPosition.FIRST)
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

            playerCard = null;
            enemyCard = null;
            EnemyHandManager.Instance.AdvanceSlots();
        }
    }
}
