using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Utility;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Slot;
using Game.Utility;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Data;

namespace Game.CombatSystem.Manager
{
    public class EnemyHandManager : MonoBehaviour, IEnemyHandManager
    {
        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI cardUIPrefab;

        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        private IEnemyCharacter currentEnemy;
        private ISlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private ITurnCardRegistry cardRegistry;

        public void Initialize(IEnemyCharacter enemy, ISlotRegistry slotRegistry, ISkillCardFactory cardFactory)
        {
            currentEnemy = enemy;
            this.slotRegistry = slotRegistry;
            this.cardFactory = cardFactory;

            handSlots.Clear();
            cardUIs.Clear();

            // ✅ 수정: GetSlots → GetHandSlots
            foreach (var slot in slotRegistry.GetHandSlotRegistry().GetHandSlots(SlotOwner.ENEMY))
            {
                handSlots[slot.GetSlotPosition()] = slot;
            }

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");
        }

        public void InjectRegistry(ITurnCardRegistry registry)
        {
            cardRegistry = registry;
        }

        public void GenerateInitialHand()
        {
            ClearHand();
            RecursivelyFillFromBack();
        }

        public void FillEmptySlots() => RecursivelyFillFromBack();

        private void RecursivelyFillFromBack()
        {
            TryShift(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
            TryShift(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);

            if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
            {
                CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                RecursivelyFillFromBack();
            }
        }

        private bool TryShift(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            if (!handSlots.TryGetValue(from, out var fromSlot) || !handSlots.TryGetValue(to, out var toSlot))
                return false;

            if (fromSlot.GetCard() == null || toSlot.GetCard() != null)
                return false;

            var card = fromSlot.GetCard();
            var ui = cardUIs.TryGetValue(from, out var oldUI) ? oldUI : null;

            fromSlot.Clear();
            toSlot.SetCard(card);
            card.SetHandSlot(to);

            if (ui != null)
            {
                ui.transform.SetParent(((MonoBehaviour)toSlot).transform);
                ui.transform.localPosition = Vector3.zero;
                ui.transform.localScale = Vector3.one;
                cardUIs[to] = ui;
                cardUIs.Remove(from);
            }

            return true;
        }

        private bool IsSlotEmpty(SkillCardSlotPosition pos)
        {
            return handSlots.TryGetValue(pos, out var slot) && slot.GetCard() == null;
        }

        private void CreateCardInSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return;

            var entry = currentEnemy?.Data?.GetRandomEntry();
            if (entry?.Card == null) return;

            var cardData = entry.Card.GetCardData(); // SkillCardData
            var effects = entry.Card.CreateEffects();

            var runtimeCard = cardFactory.CreateEnemyCard(cardData, effects);
            runtimeCard.SetHandSlot(pos);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);

            slot.SetCard(runtimeCard);

            if (slot is IHandCardSlot handSlotWithUI && handSlotWithUI is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
            {
                uiSlot.SetCardUI(cardUI);
            }

            cardUIs[pos] = cardUI;
        }

        public ISkillCard GetCardForCombat()
        {
            var reservedSlot = cardRegistry?.GetReservedEnemySlot() ?? CombatSlotPosition.NONE;

            if (reservedSlot != CombatSlotPosition.NONE)
            {
                var fieldSlot = SlotPositionUtil.ToFieldSlot(reservedSlot);
                var handSlot = SlotPositionUtil.ToEnemyHandSlot(fieldSlot);
                return GetSlotCard(handSlot);
            }

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

        public SkillCardUI RemoveCardFromSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot)) return null;
            slot.Clear();

            if (cardUIs.TryGetValue(pos, out var ui))
            {
                Destroy(ui.gameObject);
                cardUIs.Remove(pos);
                return ui;
            }

            return null;
        }

        public void ClearHand()
        {
            foreach (var slot in handSlots.Values)
                slot?.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();
        }

        public void AdvanceSlots()
        {
            FillEmptySlots();
        }

        public (ISkillCard card, SkillCardUI ui) PopCardFromSlot(SkillCardSlotPosition pos)
        {
            if (!handSlots.TryGetValue(pos, out var slot))
                return (null, null);

            var card = slot.GetCard();
            var ui = cardUIs.TryGetValue(pos, out var foundUI) ? foundUI : null;

            slot.Clear();

            if (slot is IHandCardSlot handSlotWithUI && handSlotWithUI is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
            {
                uiSlot.SetCardUI(null);
            }

            if (ui != null)
            {
                ui.transform.SetParent(null);
                cardUIs.Remove(pos);
            }

            Debug.Log($"[EnemyHandManager] PopCardFromSlot: {pos} 슬롯에서 카드 '{card?.GetCardName()}' / UI 추출 완료");

            return (card, ui);
        }

        public void LogHandSlotStates()
        {
            foreach (var kvp in handSlots)
                Debug.Log($"[Slot {kvp.Key}] 카드 존재 여부: {kvp.Value?.GetCard() != null}");
        }
    }
}

