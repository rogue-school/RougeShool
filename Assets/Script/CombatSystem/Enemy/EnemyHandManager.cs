using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Core;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Enemy
{
    /// <summary>
    /// 적의 핸드 카드와 슬롯을 관리합니다.
    /// </summary>
    public class EnemyHandManager : MonoBehaviour
    {
        public static EnemyHandManager Instance { get; private set; }

        [SerializeField] private SkillCardUI cardUIPrefab;

        private Dictionary<SkillCardSlotPosition, IHandCardSlot> handSlots = new();
        private Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        private EnemyCharacter currentEnemy;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }
            Instance = this;
        }

        public void Initialize(EnemyCharacter enemy)
        {
            currentEnemy = enemy;

            if (cardUIPrefab == null)
                cardUIPrefab = Resources.Load<SkillCardUI>("UI/SkillCardUI");

            handSlots.Clear();
            foreach (var slot in SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY))
                handSlots[slot.GetSlotPosition()] = slot;

            cardUIs.Clear();

            if (handSlots.Count == 0)
                Debug.LogError("[EnemyHandManager] 적 핸드 슬롯이 초기화되지 않았습니다.");
        }

        public void GenerateInitialHand()
        {
            CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
            FillEmptySlots();
        }

        public void AdvanceSlots()
        {
            FillEmptySlots();
        }

        public void FillEmptySlots()
        {
            bool moved;
            do
            {
                moved = false;

                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_1) && !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2))
                {
                    ShiftCard(SkillCardSlotPosition.ENEMY_SLOT_2, SkillCardSlotPosition.ENEMY_SLOT_1);
                    moved = true;
                }

                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_2) && !IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    ShiftCard(SkillCardSlotPosition.ENEMY_SLOT_3, SkillCardSlotPosition.ENEMY_SLOT_2);
                    moved = true;
                }

                if (IsSlotEmpty(SkillCardSlotPosition.ENEMY_SLOT_3))
                {
                    CreateCardInSlot(SkillCardSlotPosition.ENEMY_SLOT_3);
                    moved = true;
                }

            } while (moved);
        }

        private bool IsSlotEmpty(SkillCardSlotPosition position)
        {
            return handSlots[position].GetCard() == null;
        }

        private void CreateCardInSlot(SkillCardSlotPosition position)
        {
            var slot = handSlots[position];
            var entry = currentEnemy.Data.GetRandomEntry();

            if (entry?.card == null)
                return;

            var runtimeCard = SkillCardFactory.CreateEnemyCard(entry.card, entry.damage);
            runtimeCard.SetHandSlot(position);

            var cardUI = Instantiate(cardUIPrefab, ((MonoBehaviour)slot).transform);
            cardUI.SetCard(runtimeCard);

            slot.SetCard(runtimeCard);
            cardUIs[position] = cardUI;
        }

        private void ShiftCard(SkillCardSlotPosition from, SkillCardSlotPosition to)
        {
            var fromSlot = handSlots[from];
            var toSlot = handSlots[to];

            var card = fromSlot.GetCard();
            fromSlot.Clear();
            toSlot.SetCard(card);
            card?.SetHandSlot(to);

            if (cardUIs.TryGetValue(from, out var cardUI))
            {
                cardUIs[to] = cardUI;
                cardUIs.Remove(from);
                cardUI.transform.SetParent(((MonoBehaviour)toSlot).transform);
                cardUI.transform.localPosition = Vector3.zero;
                cardUI.transform.localScale = Vector3.one;
            }
        }

        public ISkillCard GetCardForCombat()
        {
            return GetSlotCard(SkillCardSlotPosition.ENEMY_SLOT_1);
        }

        public ISkillCard GetSlotCard(SkillCardSlotPosition position)
        {
            return handSlots.TryGetValue(position, out var slot) ? slot.GetCard() : null;
        }

        public SkillCardUI GetCardUI(int index)
        {
            var slotPos = index switch
            {
                0 => SkillCardSlotPosition.ENEMY_SLOT_1,
                1 => SkillCardSlotPosition.ENEMY_SLOT_2,
                2 => SkillCardSlotPosition.ENEMY_SLOT_3,
                _ => SkillCardSlotPosition.ENEMY_SLOT_1
            };

            return cardUIs.TryGetValue(slotPos, out var ui) ? ui : null;
        }

        /// <summary>
        /// 슬롯과 카드 UI 모두 제거합니다.
        /// </summary>
        public void ClearAllSlots()
        {
            foreach (var slot in handSlots.Values)
                slot.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();

            Debug.Log("[EnemyHandManager] 모든 핸드 슬롯 클리어 완료");
        }

        /// <summary>
        /// UI를 완전히 제거하고 슬롯도 초기화합니다.
        /// </summary>
        public void ClearAllUI()
        {
            foreach (var slot in SlotRegistry.Instance.GetHandSlots(SlotOwner.ENEMY))
                slot.Clear();

            foreach (var ui in cardUIs.Values)
                if (ui != null) Destroy(ui.gameObject);

            cardUIs.Clear();

            Debug.Log("[EnemyHandManager] 모든 핸드 UI 정리 완료");
        }
    }
}
