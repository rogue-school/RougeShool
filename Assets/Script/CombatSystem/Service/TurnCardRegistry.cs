using System.Collections.Generic;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Slot;
using UnityEngine;

namespace Game.CombatSystem.Service
{
    public class TurnCardRegistry : ITurnCardRegistry
    {
        private readonly Dictionary<CombatSlotPosition, ISkillCard> playerCards = new();
        private ISkillCard enemyCard;
        private CombatSlotPosition? reservedEnemySlot;

        public void RegisterPlayerCard(CombatSlotPosition slot, ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("[TurnCardRegistry] 플레이어 카드 등록 실패: null");
                return;
            }

            playerCards[slot] = card;
            Debug.Log($"[TurnCardRegistry] 플레이어 카드 등록됨 - 슬롯: {slot}, 카드: {card.CardData?.Name ?? "Unknown"}");
        }

        public void RegisterEnemyCard(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogError("[TurnCardRegistry] 적 카드 등록 실패: null");
                return;
            }

            enemyCard = card;
            Debug.Log($"[TurnCardRegistry] 적 카드 등록됨 - {card.CardData?.Name ?? "Unknown"}");
        }

        public ISkillCard GetPlayerCard(CombatSlotPosition slot)
        {
            return playerCards.TryGetValue(slot, out var card) ? card : null;
        }

        public ISkillCard GetEnemyCard() => enemyCard;

        public void ClearPlayerCard(CombatSlotPosition slot)
        {
            if (playerCards.Remove(slot))
            {
                Debug.Log($"[TurnCardRegistry] 플레이어 카드 제거됨 - 슬롯: {slot}");
            }
        }

        public void ClearEnemyCard()
        {
            enemyCard = null;
            Debug.Log("[TurnCardRegistry] 적 카드 제거됨");
        }

        public void ClearSlot(CombatSlotPosition slot)
        {
            if (playerCards.ContainsKey(slot))
            {
                ClearPlayerCard(slot);
            }
            else
            {
                Debug.LogWarning($"[TurnCardRegistry] 알 수 없는 플레이어 슬롯: {slot}");
            }
        }

        public CombatSlotPosition? GetReservedEnemySlot() => reservedEnemySlot;

        public void ReserveNextEnemySlot(CombatSlotPosition slot)
        {
            reservedEnemySlot = slot;
            Debug.Log($"[TurnCardRegistry] 다음 적 슬롯 예약됨 - {slot}");
        }

        public void Reset()
        {
            playerCards.Clear();
            enemyCard = null;
            reservedEnemySlot = null;
            Debug.Log("[TurnCardRegistry] 모든 카드 등록 정보 초기화됨");
        }
    }
}
