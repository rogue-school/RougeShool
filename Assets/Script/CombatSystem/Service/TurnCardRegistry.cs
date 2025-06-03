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
        private readonly Dictionary<CombatSlotPosition, ISkillCard> _playerCards = new();
        private ISkillCard _enemyCard;
        private CombatSlotPosition? _reservedEnemySlot;

        public event System.Action OnCardStateChanged;

        public void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError($"[TurnCardRegistry] 플레이어 카드 등록 실패 - null (슬롯: {slot})");
                return;
            }

            _playerCards[slot] = card;
            OnCardStateChanged?.Invoke();
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("[TurnCardRegistry] 적 카드 등록 실패 - null");
                return;
            }

            _enemyCard = card;
            OnCardStateChanged?.Invoke();
        }

        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            if (owner == SlotOwner.PLAYER)
                RegisterPlayerCard(position, card);
            else if (owner == SlotOwner.ENEMY)
            {
                RegisterEnemyCard(card);
                ReserveNextEnemySlot(position);
            }
        }

        public ISkillCard GetPlayerCard(CombatSlotPosition slot)
        {
            _playerCards.TryGetValue(slot, out var card);
            return card;
        }

        public ISkillCard GetEnemyCard() => _enemyCard;

        public void ClearPlayerCard(CombatSlotPosition slot)
        {
            if (_playerCards.Remove(slot))
                OnCardStateChanged?.Invoke();
        }

        public void ClearEnemyCard()
        {
            if (_enemyCard != null)
            {
                _enemyCard = null;
                OnCardStateChanged?.Invoke();
            }
        }

        public bool HasPlayerCard() => _playerCards.Count > 0;

        public bool HasEnemyCard() => _enemyCard != null;

        public void Reset()
        {
            _playerCards.Clear();
            _enemyCard = null;
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }

        public void ReserveNextEnemySlot(CombatSlotPosition slot) => _reservedEnemySlot = slot;

        public CombatSlotPosition? GetReservedEnemySlot() => _reservedEnemySlot;

        public void ClearAll() => Reset();

        public void ClearSlot(CombatSlotPosition slot) => ClearPlayerCard(slot);
    }
}
