using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.SkillCardSystem.Deck;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using System;
using System.Linq;

namespace Game.CombatSystem.Manager
{
    public class EnemyHandManager : MonoBehaviour, IEnemyHandManager
    {
        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI cardUIPrefab;

        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();
        private readonly Dictionary<SkillCardSlotPosition, (ISkillCard, ISkillCardUI)> _cardsInSlots = new();

        private IEnemyCharacter currentEnemy;
        private EnemySkillDeck enemyDeck;

        private ISlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private ITurnCardRegistry cardRegistry;

        [Inject]
        public void Construct(ISlotRegistry slotRegistry, ISkillCardFactory cardFactory, ITurnCardRegistry cardRegistry)
        {
            this.slotRegistry = slotRegistry;
            this.cardFactory = cardFactory;
            this.cardRegistry = cardRegistry;
        }

        public void Initialize(IEnemyCharacter enemy)
        {
            currentEnemy = enemy;
            enemyDeck = enemy.Data?.EnemyDeck;

            handSlots.Clear();
            cardUIs.Clear();
            _cardsInSlots.Clear();

            foreach (var slot in slotRegistry.GetHandSlotRegistry().GetHandSlots(SlotOwner.ENEMY))
                handSlots[slot.GetSlotPosition()] = slot;

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");
        }

        public void GenerateInitialHand()
        {
            ClearHand();
            StartCoroutine(StepwiseFillSlotsFromBack());
        }

        public IEnumerator StepwiseFillSlotsFromBack(float delay = 0.5f)
        {
            while (true)
            {
                bool didSomething = false;

                // 1. SLOT_3 비어있으면 카드 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 2. SLOT_2 비어있고, 3에 카드가 있으면 → 이동
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2) &&
                    !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 3. SLOT_3 다시 카드 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 4. SLOT_1 비어있고, 2에 카드가 있으면 → 이동
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_1) &&
                    !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2))
                {
                    ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 5. SLOT_2 비어있고, 3에 카드가 있으면 → 이동
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2) &&
                    !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 6. SLOT_3 다시 카드 생성
                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    yield return new WaitForSeconds(delay);
                    didSomething = true;
                }

                // 더 이상 처리할 게 없다면 종료
                if (!didSomething)
                {
                    yield break;
                }

                yield return null;
            }
        }

        private bool hasRegisteredThisTurn = false;

        public (ISkillCard card, SkillCardUI ui, CombatSlotPosition pos) PopCardAndRegisterToCombatSlot(ICombatFlowCoordinator flowCoordinator)
        {
            var (card, ui) = PopCardFromSlot(SkillCardSlotPosition.ENEMY_SLOT_1);
            if (card == null || ui == null)
            {
                Debug.LogWarning("[EnemyHandManager] 전투 슬롯 등록 실패: 카드 또는 UI가 null");
                return (null, null, CombatSlotPosition.FIRST);
            }

            var isFirst = UnityEngine.Random.value < 0.5f;
            var pos = isFirst ? CombatSlotPosition.FIRST : CombatSlotPosition.SECOND;

            flowCoordinator.RegisterCardToCombatSlot(pos, card, ui);
            cardRegistry.RegisterCard(pos, card, ui, SlotOwner.ENEMY);

            Debug.Log($"[EnemyHandManager] 전투 슬롯 등록 완료 → {card.GetCardName()} to {pos}");
            return (card, ui, pos);
        }

        public void ResetTurnRegistrationFlag() => hasRegisteredThisTurn = false;


        public (ISkillCard, ISkillCardUI) PeekCardInSlot(SkillCardSlotPosition position)
        {
            if (_cardsInSlots.TryGetValue(position, out var value))
                return value;

            return (null, null);
        }

        public ISkillCard GetCardForCombat()
        {
            return GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;
        }

        public ISkillCardUI GetCardUI(int index)
        {
            var pos = (SkillCardSlotPosition)(index + (int)SkillCardSlotPosition.ENEMY_SLOT_1);
            return cardUIs.TryGetValue(pos, out var ui) ? ui : null;
        }

        public void ClearHand()
        {
            foreach (var slot in handSlots.Values)
                slot?.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();
            _cardsInSlots.Clear();
        }

        public void ClearAllCards()
        {
            ClearHand();
        }

        public void LogHandSlotStates()
        {
            foreach (var kvp in handSlots)
                Debug.Log($"[Slot {kvp.Key}] 카드 있음: {kvp.Value?.GetCard() != null}");
        }

        public SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return null;

            slot.Clear();

            if (cardUIs.TryGetValue(pos, out var ui))
            {
                Destroy(ui.gameObject);
                cardUIs.Remove(pos);
                _cardsInSlots.Remove(pos);
                return ui;
            }

            _cardsInSlots.Remove(pos);
            return null;
        }

        public (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return (null, null);

            var card = slot.GetCard();
            var ui = cardUIs.TryGetValue(pos, out var foundUI) ? foundUI : null;

            slot.Clear();
            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(null);

            if (ui != null)
            {
                ui.transform.SetParent(null);
                cardUIs.Remove(pos);
            }

            _cardsInSlots.Remove(pos);
            return (card, ui);
        }

        public (ISkillCard card, SkillCardUI ui) PopFirstAvailableCard()
        {
            foreach (SkillCardSlotPosition pos in Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var (card, ui) = PeekCardInSlot(pos);
                if (card != null)
                    return PopCardFromSlot(pos);
            }

            return (null, null);
        }

        public ISkillCard PickCardForSlot(SkillCardSlotPosition pos)
        {
            return GetSlotCard(pos);
        }

        public void RegisterCardToSlot(SkillCardSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return;

            slot.SetCard(card);
            cardUIs[pos] = ui;
            _cardsInSlots[pos] = (card, ui);

            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(ui);
        }

        public bool HasInitializedEnemy(IEnemyCharacter enemy)
        {
            return currentEnemy == enemy;
        }

        private bool IsSlotEmpty(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) && slot.GetCard() == null;
        }

        private void CreateCardInSlot(SkillCardSlotPosition pos)
        {
            if (enemyDeck == null)
            {
                Debug.LogError("[EnemyHandManager] EnemyDeck이 null입니다.");
                return;
            }

            if (!handSlots.TryGetValue(pos, out var slot))
            {
                Debug.LogError($"[EnemyHandManager] 슬롯 {pos}를 찾을 수 없습니다.");
                return;
            }

            var entry = enemyDeck.GetRandomEntry();
            if (entry?.card == null)
            {
                Debug.LogWarning("[EnemyHandManager] 생성할 카드 엔트리가 유효하지 않음");
                return;
            }

            var runtimeCard = cardFactory.CreateEnemyCard(entry.card.GetCardData(), entry.card.CreateEffects());
            runtimeCard.SetHandSlot(pos);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);
            cardUI.transform.localPosition = Vector3.zero;
            cardUI.transform.localScale = Vector3.one;

            slot.SetCard(runtimeCard);
            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(cardUI);

            cardUIs[pos] = cardUI;
            _cardsInSlots[pos] = (runtimeCard, cardUI);

            Debug.Log($"[EnemyHandManager] 카드 생성 완료: {runtimeCard.GetCardName()} → {pos}");
        }

        private bool ShiftSlot(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            if (!handSlots.TryGetValue(from, out var fromSlot) || !handSlots.TryGetValue(to, out var toSlot))
                return false;

            var card = fromSlot.GetCard();
            if (card == null || toSlot.GetCard() != null)
                return false;

            var ui = cardUIs.TryGetValue(from, out var oldUI) ? oldUI : null;

            fromSlot.Clear();
            toSlot.SetCard(card);
            card.SetHandSlot(to);

            if (ui != null)
            {
                ui.transform.SetParent(((MonoBehaviour)toSlot).transform, false);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
                cardUIs[to] = ui;
                cardUIs.Remove(from);
            }

            _cardsInSlots[to] = (card, ui);
            _cardsInSlots.Remove(from);

            Debug.Log($"[EnemyHandManager] 카드 이동: {from} → {to}");
            return true;
        }
    }
}
