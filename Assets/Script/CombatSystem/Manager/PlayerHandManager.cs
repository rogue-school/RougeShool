using System.Collections.Generic;
using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Factory;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Core
{
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        private IPlayerCharacter owner;
        private IHandSlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;
        private SkillCardUI cardUIPrefab;

        private readonly Dictionary<SkillCardSlotPosition, ISkillCard> cards = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        [Inject]
        public void Construct(
            ISlotRegistry slotRegistry,
            ISkillCardFactory cardFactory,
            SkillCardUI cardUIPrefab)
        {
            this.slotRegistry = slotRegistry.GetHandSlotRegistry();
            this.cardFactory = cardFactory;
            this.cardUIPrefab = cardUIPrefab;

            cards.Clear();
            cardUIs.Clear();
        }

        public void SetPlayer(IPlayerCharacter player)
        {
            this.owner = player;
        }

        public void GenerateInitialHand()
        {
            var deck = owner?.Data?.SkillDeck;
            if (deck == null)
            {
                Debug.LogError("[PlayerHandManager] 플레이어 덱이 비어 있음");
                return;
            }

            foreach (var entry in deck.Cards)
            {
                var pos = entry.Slot;
                var card = cardFactory.CreatePlayerCard(entry.Card.CardData, entry.Card.CreateEffects());

                // 쿨타임은 0으로 초기화
                card.SetCurrentCoolTime(0);

                cards[pos] = card;

                var slot = slotRegistry.GetPlayerHandSlot(pos);
                if (slot != null)
                {
                    var ui = slot.AttachCard(card, cardUIPrefab);
                    if (ui != null) cardUIs[pos] = ui;
                }
            }
        }

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos) =>
            cards.TryGetValue(pos, out var c) ? c : null;

        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos) =>
            cardUIs.TryGetValue(pos, out var ui) ? ui : null;

        public void RestoreCardToHand(ISkillCard card)
        {
            foreach (var kvp in cards)
            {
                if (kvp.Value == null)
                {
                    cards[kvp.Key] = card;
                    var slot = slotRegistry.GetPlayerHandSlot(kvp.Key);
                    if (slot != null)
                    {
                        var ui = slot.AttachCard(card, cardUIPrefab);
                        if (ui != null) cardUIs[kvp.Key] = ui;
                    }
                    return;
                }
            }

            Debug.LogWarning("[PlayerHandManager] 빈 슬롯을 찾을 수 없어 카드 복귀 실패");
        }

        public void RestoreCardToHand(ISkillCard card, SkillCardSlotPosition slot)
        {
            cards[slot] = card;

            var handSlot = slotRegistry.GetPlayerHandSlot(slot);
            if (handSlot != null)
            {
                var ui = handSlot.AttachCard(card, cardUIPrefab);
                if (ui != null)
                    cardUIs[slot] = ui;
            }
            else
            {
                Debug.LogWarning($"[PlayerHandManager] 지정된 슬롯이 존재하지 않습니다: {slot}");
            }
        }

        public void LogPlayerHandSlotStates()
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var card = GetCardInSlot(pos);
                Debug.Log($"슬롯 {pos}: {(card != null ? card.CardData.Name : "비어 있음")}");
            }
        }

        public void EnableInput(bool enable)
        {
            foreach (var ui in cardUIs.Values)
                ui?.SetDraggable(enable);
        }

        public void ClearAll()
        {
            foreach (var pos in cards.Keys)
                slotRegistry.GetPlayerHandSlot(pos)?.DetachCard();

            cards.Clear();
            cardUIs.Clear();
        }

        /// <summary>
        /// 쿨타임 관리용 카드+UI 목록 반환
        /// </summary>
        public IEnumerable<(ISkillCard card, ISkillCardUI ui)> GetAllHandCards()
        {
            foreach (var kvp in cards)
            {
                var slot = kvp.Key;
                var card = kvp.Value;
                cardUIs.TryGetValue(slot, out var ui);
                yield return (card, ui);
            }
        }
    }
}
