using UnityEngine;
using System.Collections.Generic;
using Zenject;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.Utility;
using Game.SkillCardSystem.Deck;

namespace Game.CombatSystem.Manager
{
    public class EnemyHandManager : MonoBehaviour, IEnemyHandManager
    {
        [Header("UI 프리팹")]
        [SerializeField] private SkillCardUI cardUIPrefab;

        private readonly Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        private IEnemyCharacter currentEnemy;
        private EnemySkillDeck enemyDeck;

        private ISlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private ITurnCardRegistry cardRegistry;

        [Inject]
        public void Construct(
            ISlotRegistry slotRegistry,
            ISkillCardFactory cardFactory,
            ITurnCardRegistry cardRegistry)
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

            foreach (var slot in slotRegistry.GetHandSlotRegistry().GetHandSlots(SlotOwner.ENEMY))
                handSlots[slot.GetSlotPosition()] = slot;

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");
        }
        public void FillEmptySlots()
        {
            FillAllEmptySlots();
        }

        public void GenerateInitialHand()
        {
            ClearHand();
            FillAllEmptySlots();
        }

        public void AdvanceSlots()
        {
            ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
            ShiftSlot(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
            FillAllEmptySlots();
        }

        private void FillAllEmptySlots()
        {
            foreach (var pos in new[]
            {
                SkillCardSlotPosition.ENEMY_SLOT_1,
                SkillCardSlotPosition.ENEMY_SLOT_2,
                SkillCardSlotPosition.ENEMY_SLOT_3
            })
            {
                if (IsSlotEmpty(pos))
                    CreateCardInSlot(pos);
            }
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

            return true;
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

            Debug.Log($"[EnemyHandManager] 카드 생성 완료: {runtimeCard.GetCardName()} → {pos}");
        }

        public ISkillCard GetCardForCombat()
        {
            var reserved = cardRegistry?.GetReservedEnemySlot();
            var slot = reserved.HasValue
                ? SlotPositionUtil.ToEnemyHandSlot(SlotPositionUtil.ToFieldSlot(reserved.Value))
                : SkillCardSlotPosition.ENEMY_SLOT_1;

            return GetSlotCard(slot);
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition pos) =>
            handSlots.TryGetValue(pos, out var slot) ? slot.GetCard() : null;

        public ISkillCardUI GetCardUI(int index)
        {
            var pos = (SkillCardSlotPosition)(index + (int)SkillCardSlotPosition.ENEMY_SLOT_1);
            return cardUIs.TryGetValue(pos, out var ui) ? ui : null;
        }

        public ISkillCard PickCardForSlot(SkillCardSlotPosition pos) => GetSlotCard(pos);

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

            Debug.Log($"[EnemyHandManager] PopCardFromSlot: {pos} - 카드: {card?.GetCardName() ?? "없음"}");
            return (card, ui);
        }

        public void RegisterCardToSlot(SkillCardSlotPosition pos, ISkillCard card, SkillCardUI ui)
        {
            if (!handSlots.TryGetValue(pos, out var slot))
            {
                Debug.LogError($"[EnemyHandManager] 슬롯 {pos}을 찾을 수 없습니다.");
                return;
            }

            slot.SetCard(card);
            cardUIs[pos] = ui;

            if (slot is Game.CombatSystem.UI.EnemyHandCardSlotUI uiSlot)
                uiSlot.SetCardUI(ui);

            Debug.Log($"[EnemyHandManager] 카드 등록 완료: {card.GetCardName()} → {pos}");
        }

        public void ClearHand()
        {
            foreach (var slot in handSlots.Values)
                slot?.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();
        }

        public void LogHandSlotStates()
        {
            foreach (var kvp in handSlots)
                Debug.Log($"[Slot {kvp.Key}] 카드 있음: {kvp.Value?.GetCard() != null}");
        }
    }
}
