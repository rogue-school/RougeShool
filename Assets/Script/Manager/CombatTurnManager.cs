using System.Collections;
using Game.Cards;
using Game.Combat.Turn;
using Game.Interface;
using Game.Slots;
using UnityEngine;

namespace Game.Managers
{
    public class CombatTurnManager : MonoBehaviour
    {
        public static CombatTurnManager Instance { get; private set; }

        private ISkillCard enemyCard;
        private ISkillCard playerCard;
        private CombatSlotPosition enemySlot;

        private ICombatTurnState currentState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Update()
        {
            currentState?.ExecuteState();
        }

        public void SetState(ICombatTurnState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        public void BeginEnemyTurn()
        {
            enemyCard = EnemyHandManager.Instance.GetCardForCombat();
            if (enemyCard == null)
            {
                Debug.LogWarning("[CombatTurnManager] 적 핸드 카드가 비어 있습니다.");
                return;
            }

            enemySlot = (Random.value < 0.5f) ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;
            enemyCard.SetCombatSlot(enemySlot);

            var combatSlot = SlotRegistry.Instance.GetCombatSlot(enemySlot);
            combatSlot.SetCard(enemyCard);

            var enemyCardUI = EnemyHandManager.Instance.GetCardUI(0);
            if (enemyCardUI != null)
            {
                combatSlot.SetCardUI(enemyCardUI);
                enemyCardUI.transform.SetParent(((MonoBehaviour)combatSlot).transform);
                enemyCardUI.transform.localPosition = Vector3.zero;
                enemyCardUI.transform.localScale = Vector3.one;
            }

            Debug.Log($"[CombatTurnManager] 적 카드 전투 슬롯 등록 완료 → {enemySlot}");

            var handSlot = SlotRegistry.Instance.GetHandSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            handSlot?.Clear();

            EnemyHandManager.Instance.AdvanceSlots();
        }

        public void RegisterPlayerCard(ISkillCard card)
        {
            playerCard = card;
        }

        public bool AreBothSlotsReady() => enemyCard != null && playerCard != null;

        public void ExecuteCombat()
        {
            if (!AreBothSlotsReady())
            {
                Debug.LogWarning("[CombatTurnManager] 카드가 모두 준비되지 않았습니다.");
                return;
            }

            StartCoroutine(ExecuteCombatSequence());
        }

        private IEnumerator ExecuteCombatSequence()
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
            yield return new WaitForSeconds(1.0f);

            slotSecond.ExecuteCardAutomatically();
            yield return new WaitForSeconds(1.0f);

            PlayerManager.Instance.GetPlayer()?.ProcessTurnEffects();
            EnemyManager.Instance.GetCurrentEnemy()?.ProcessTurnEffects();

            if (playerCard is PlayerSkillCardRuntime runtime)
            {
                runtime.ActivateCoolTime();
                runtime.TickCoolTime();
                PlayerManager.Instance.GetPlayerHandManager().RestoreCardToHand(runtime);
                Debug.Log($"[CombatTurnManager] 플레이어 카드 복귀 및 쿨타임 적용 완료: {runtime.GetCardName()}");
            }

            slotFirst.Clear();
            slotSecond.Clear();

            enemyCard = null;
            playerCard = null;

            yield return new WaitForSeconds(0.5f);

            var isEnemyDead = !EnemyManager.Instance.HasEnemy() || EnemyManager.Instance.GetCurrentEnemy()?.IsDead() == true;
            Debug.Log($"[CombatTurnManager] 적 생존 여부: {(isEnemyDead ? "사망" : "생존")}");

            if (isEnemyDead)
            {
                EnemyHandManager.Instance.ClearAllSlots();
                EnemyHandManager.Instance.ClearAllUI();
                StageManager.Instance.SpawnNextEnemy();
                yield return new WaitForSeconds(0.5f);
            }

            BeginEnemyTurn();
        }
    }
}
