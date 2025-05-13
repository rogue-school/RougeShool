using System.Collections;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Slot;
using Game.CombatSystem.State;

namespace Game.CombatSystem.Core
{
    public class CombatTurnManager : MonoBehaviour, ITurnStateController
    {
        public static CombatTurnManager Instance { get; private set; }

        private ISkillCard enemyCard;
        private ISkillCard playerCard;
        private CombatSlotPosition enemySlot;

        private ICombatTurnState currentState;
        private ICombatTurnState pendingNextState;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            RequestStateChange(new CombatPrepareState(this));
        }

        private void Update()
        {
            currentState?.ExecuteState();

            if (pendingNextState != null)
            {
                ChangeState(pendingNextState);
                pendingNextState = null;
            }
        }

        public void ChangeState(ICombatTurnState newState)
        {
            currentState?.ExitState();
            currentState = newState;
            currentState?.EnterState();
        }

        public void RequestStateChange(ICombatTurnState newState)
        {
            pendingNextState = newState;
        }

        public void TriggerTurnStart()
        {
            if (currentState is CombatPlayerInputState inputState)
            {
                inputState.TriggerTurnStart();
            }
        }

        public void RegisterPlayerCard(ISkillCard card) => playerCard = card;
        public void RegisterEnemyCard(ISkillCard card) => enemyCard = card;
        public ISkillCard GetPlayerCard() => playerCard;
        public ISkillCard GetEnemyCard() => enemyCard;

        public void ClearCards()
        {
            playerCard = null;
            enemyCard = null;
        }

        public bool AreBothSlotsReady() => enemyCard != null && playerCard != null;

        public void ExecuteCombat() => StartCoroutine(ExecuteCombatSequence());

        public void RegisterPlayerGuard()
        {
            PlayerManager.Instance.GetPlayer()?.SetGuarded(true);
            Debug.Log("[CombatTurnManager] 플레이어 방어 상태 등록됨");
        }

        public void ReserveEnemySlot(CombatSlotPosition slot)
        {
            var card = EnemyHandManager.Instance.GetCardForCombat();
            if (card != null)
            {
                card.SetCombatSlot(slot);
                var combatSlot = SlotRegistry.Instance.GetCombatSlot(slot);
                combatSlot.SetCard(card);

                var cardUI = EnemyHandManager.Instance.GetCardUI(0);
                if (cardUI != null)
                {
                    combatSlot.SetCardUI(cardUI);
                    cardUI.transform.SetParent(((MonoBehaviour)combatSlot).transform);
                    cardUI.transform.localPosition = Vector3.zero;
                    cardUI.transform.localScale = Vector3.one;
                }

                var handSlot = SlotRegistry.Instance.GetHandSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
                handSlot?.Clear();

                EnemyHandManager.Instance.AdvanceSlots();

                Debug.Log($"[CombatTurnManager] 적 카드 슬롯 예약 완료 → {slot}");
            }
            else
            {
                Debug.LogWarning("[CombatTurnManager] 적 카드가 없어 슬롯 예약 실패");
            }
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
            ClearCards();

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

            RequestStateChange(new CombatPrepareState(this));
        }
    }
}
