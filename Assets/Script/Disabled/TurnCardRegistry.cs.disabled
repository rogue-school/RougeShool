using System;
using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 한 턴 동안 플레이어와 적의 전투 슬롯에 등록된 카드를 관리하는 레지스트리입니다.
    /// </summary>
    public class TurnCardRegistry : ITurnCardRegistry
    {
        #region 필드

        private readonly Dictionary<CombatSlotPosition, ISkillCard> _cards = new();
        private CombatSlotPosition? _reservedEnemySlot;

        #endregion

        #region 이벤트

        /// <summary>
        /// 카드 상태가 변경될 때 발생하는 이벤트
        /// </summary>
        public event Action OnCardStateChanged;

        #endregion

        #region 카드 등록 및 조회

        /// <inheritdoc />
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

        /// <inheritdoc />
        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            _cards.TryGetValue(slot, out var card);
            return card;
        }

        #endregion

        #region 클리어 및 리셋

        /// <inheritdoc />
        public void ClearSlot(CombatSlotPosition slot)
        {
            if (_cards.Remove(slot))
                OnCardStateChanged?.Invoke();
        }

        /// <inheritdoc />
        public void ClearAll() => Reset();

        /// <inheritdoc />
        public void Reset()
        {
            _cards.Clear();
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }

        /// <summary>
        /// 적 카드만 제거하고 플레이어 카드 보존
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

        #endregion

        #region 상태 확인

        /// <inheritdoc />
        public bool HasPlayerCard()
        {
            foreach (var card in _cards.Values)
                if (card.IsFromPlayer()) return true;

            return false;
        }

        /// <inheritdoc />
        public bool HasEnemyCard()
        {
            foreach (var card in _cards.Values)
                if (!card.IsFromPlayer()) return true;

            return false;
        }

        #endregion

        #region 적 슬롯 예약 관리

        /// <inheritdoc />
        public CombatSlotPosition? GetReservedEnemySlot() => _reservedEnemySlot;

        /// <inheritdoc />
        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _reservedEnemySlot = slot;
        }

        /// <summary>
        /// 특정 슬롯에서 카드를 제거합니다.
        /// </summary>
        /// <param name="position">제거할 슬롯 위치</param>
        public void RemoveCardFromSlot(CombatSlotPosition position)
        {
            if (_cards.ContainsKey(position))
            {
                _cards.Remove(position);
                OnCardStateChanged?.Invoke();
                Debug.Log($"[TurnCardRegistry] 슬롯에서 카드 제거: {position}");
            }
        }

        /// <summary>
        /// 카드를 슬롯에 등록합니다.
        /// </summary>
        /// <param name="position">등록할 슬롯 위치</param>
        /// <param name="card">등록할 카드</param>
        /// <param name="ui">카드 UI</param>
        /// <param name="owner">카드 소유자</param>
        public void RegisterCardToSlot(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            _cards[position] = card;
            OnCardStateChanged?.Invoke();
            Debug.Log($"[TurnCardRegistry] 슬롯에 카드 등록: {position}, 카드: {card?.CardDefinition?.CardName ?? "Unknown"}");
        }

        #endregion
    }
}
