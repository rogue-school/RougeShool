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

        /// <summary>
        /// 적이 선턴으로 무작위 슬롯에 카드 배치
        /// </summary>
        public void BeginEnemyTurn()
        {
            // 적 핸드에서 카드 가져오기
            var enemyCard = EnemyHandManager.Instance.GetCardForCombat();
            if (enemyCard == null)
            {
                Debug.LogWarning("[CombatTurnManager] 적 핸드 카드가 비어 있습니다.");
                return;
            }

            // 슬롯 위치 결정: 50% 확률로 FIRST 또는 SECOND
            CombatSlotPosition chosenSlot = (Random.value < 0.5f)
                ? CombatSlotPosition.FIRST
                : CombatSlotPosition.SECOND;

            enemyCard.SetCombatSlot(chosenSlot);
            ReserveEnemyCard(enemyCard);

            // 슬롯에 카드 등록
            var slot = SlotRegistry.Instance.GetCombatSlot(chosenSlot);
            slot.SetCard(enemyCard);

            // UI도 등록
            var enemyCardUI = EnemyHandManager.Instance.GetCardUI(0);
            if (enemyCardUI != null)
            {
                slot.SetCardUI(enemyCardUI);
                enemyCardUI.transform.SetParent(((MonoBehaviour)slot).transform);
                enemyCardUI.transform.localPosition = Vector3.zero;
                enemyCardUI.transform.localScale = Vector3.one;
            }

            Debug.Log($"[CombatTurnManager] 적 카드 전투 예약 완료 → {chosenSlot}");
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

            // 다음 적 턴 자동 호출 가능 시 여기에 추가
        }
    }
}
