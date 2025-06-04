using System;
using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Service
{
    public class TurnCardRegistry : ITurnCardRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ISkillCard> _cards = new();
        private CombatSlotPosition? _reservedEnemySlot;

        public event Action OnCardStateChanged;

        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            if (card == null)
            {
                Debug.LogError($"[TurnCardRegistry] 카드 등록 실패 - null (슬롯: {position})");
                return;
            }

            _cards[position] = card;

            if (owner == SlotOwner.ENEMY)
                _reservedEnemySlot = position;

            OnCardStateChanged?.Invoke();
        }

        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            _cards.TryGetValue(slot, out var card);
            return card;
        }

        public void ClearSlot(CombatSlotPosition slot)
        {
            if (_cards.Remove(slot))
                OnCardStateChanged?.Invoke();
        }

        public void Reset()
        {
            _cards.Clear();
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }

        public bool HasPlayerCard()
        {
            foreach (var card in _cards.Values)
                if (card.IsFromPlayer()) return true;
            return false;
        }

        public bool HasEnemyCard()
        {
            foreach (var card in _cards.Values)
                if (!card.IsFromPlayer()) return true;
            return false;
        }

        public CombatSlotPosition? GetReservedEnemySlot() => _reservedEnemySlot;

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _reservedEnemySlot = slot;
        }

        public void ClearAll() => Reset();

        /// <summary>
        /// 적 카드만 제거 (플레이어 카드 보존)
        /// </summary>
        public void ClearEnemyCardsOnly()
        {
            var toRemove = new List<CombatSlotPosition>();

            foreach (var kvp in _cards)
            {
                if (!kvp.Value.IsFromPlayer())
                    toRemove.Add(kvp.Key);
            }

            foreach (var key in toRemove)
                _cards.Remove(key);

            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }
    }
}