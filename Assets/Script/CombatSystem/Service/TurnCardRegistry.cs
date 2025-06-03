using System;
using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using Game.SkillCardSystem.UI;

namespace Game.CombatSystem.Service
{
    /// <summary>
    /// 슬롯 위치 기준으로 전투 카드를 관리하는 레지스트리
    /// 플레이어와 적의 카드 모두 통합하여 관리하며, 카드 내부 소유자 정보로 판단함
    /// </summary>
    public class TurnCardRegistry : ITurnCardRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ISkillCard> _cards = new();
        private CombatSlotPosition? _reservedEnemySlot;

        public event Action OnCardStateChanged;

        /// <summary>
        /// 카드 등록 (플레이어/적 구분 없이)
        /// </summary>
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

        /// <summary>
        /// 특정 슬롯에 등록된 카드 반환 (없으면 null)
        /// </summary>
        public ISkillCard GetCardInSlot(CombatSlotPosition slot)
        {
            _cards.TryGetValue(slot, out var card);
            return card;
        }

        /// <summary>
        /// 특정 슬롯에 등록된 카드 제거
        /// </summary>
        public void ClearSlot(CombatSlotPosition slot)
        {
            if (_cards.Remove(slot))
                OnCardStateChanged?.Invoke();
        }

        /// <summary>
        /// 전체 카드 초기화
        /// </summary>
        public void Reset()
        {
            _cards.Clear();
            _reservedEnemySlot = null;
            OnCardStateChanged?.Invoke();
        }

        /// <summary>
        /// 플레이어 카드가 하나라도 등록되어 있는지 확인
        /// </summary>
        public bool HasPlayerCard()
        {
            foreach (var card in _cards.Values)
                if (card.IsFromPlayer()) return true;
            return false;
        }

        /// <summary>
        /// 적 카드가 하나라도 등록되어 있는지 확인
        /// </summary>
        public bool HasEnemyCard()
        {
            foreach (var card in _cards.Values)
                if (!card.IsFromPlayer()) return true;
            return false;
        }

        /// <summary>
        /// 적 카드가 예약된 슬롯 반환 (적 드롭 전 UI 연출 등에 사용 가능)
        /// </summary>
        public CombatSlotPosition? GetReservedEnemySlot() => _reservedEnemySlot;

        /// <summary>
        /// 적 슬롯 예약
        /// </summary>
        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _reservedEnemySlot = slot;
        }

        /// <summary>
        /// 전체 초기화
        /// </summary>
        public void ClearAll() => Reset();
    }
}
