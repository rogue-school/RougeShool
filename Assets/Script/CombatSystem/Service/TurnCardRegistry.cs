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

        public void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError($"[TurnCardRegistry] 플레이어 카드 등록 실패 - null (슬롯: {slot})");
                return;
            }

            _playerCards[slot] = card;
            Debug.Log($"[TurnCardRegistry] 플레이어 카드 등록됨 - 슬롯: {slot}, 카드: {card.CardData?.Name ?? "Unknown"}");
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("[TurnCardRegistry] 적 카드 등록 실패 - null");
                return;
            }

            _enemyCard = card;
            Debug.Log($"[TurnCardRegistry] 적 카드 등록됨 - 카드: {card.CardData?.Name ?? "Unknown"}");
        }

        /// <summary>
        /// 소유자(SlotOwner)를 기반으로 적/플레이어 카드를 구분 등록합니다.
        /// </summary>
        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui, SlotOwner owner)
        {
            if (owner == SlotOwner.PLAYER)
            {
                RegisterPlayerCard(position, card);
            }
            else if (owner == SlotOwner.ENEMY)
            {
                RegisterEnemyCard(card);
                ReserveNextEnemySlot(position);
            }

            Debug.Log($"[TurnCardRegistry] RegisterCard() - 소유자: {owner}, 슬롯: {position}, 카드: {card?.CardData?.Name ?? "null"}, UI 포함");
        }

        /// <summary>
        /// [사용 금지] 구버전 메서드. SlotOwner 없이 호출 불가
        /// </summary>
        [System.Obsolete("소유자 없이 RegisterCard를 호출하지 마십시오. 반드시 SlotOwner를 명시해야 합니다.")]
        public void RegisterCard(CombatSlotPosition position, ISkillCard card, SkillCardUI ui)
        {
            throw new System.InvalidOperationException("SlotOwner를 명시하지 않은 RegisterCard 호출은 금지되어 있습니다.");
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
            {
                Debug.Log($"[TurnCardRegistry] 플레이어 카드 제거됨 - 슬롯: {slot}");
            }
            else
            {
                Debug.LogWarning($"[TurnCardRegistry] 제거 실패 - 해당 슬롯에 등록된 카드 없음: {slot}");
            }
        }

        public void ClearEnemyCard()
        {
            if (_enemyCard != null)
            {
                Debug.Log($"[TurnCardRegistry] 적 카드 제거됨 - {_enemyCard.CardData?.Name ?? "Unknown"}");
                _enemyCard = null;
            }
        }

        public void ClearSlot(CombatSlotPosition slot) => ClearPlayerCard(slot);

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            _reservedEnemySlot = slot;
            Debug.Log($"[TurnCardRegistry] 다음 적 슬롯 예약됨 - {slot}");
        }

        public CombatSlotPosition? GetReservedEnemySlot() => _reservedEnemySlot;

        public void Reset()
        {
            _playerCards.Clear();
            _enemyCard = null;
            _reservedEnemySlot = null;
            Debug.Log("[TurnCardRegistry] 모든 등록 정보 초기화됨 (Reset)");
        }

        public void ClearAll() => Reset();

        public bool HasPlayerCard()
        {
            bool result = _playerCards.Count > 0;
            Debug.Log($"[TurnCardRegistry] HasPlayerCard(): {result}");
            return result;
        }
    }
}
