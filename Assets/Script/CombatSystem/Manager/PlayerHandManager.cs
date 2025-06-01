using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Slot;
using Game.SkillCardSystem.Deck;
using Game.SkillCardSystem.UI;
using Game.CharacterSystem.Interface;
using Game.SkillCardSystem.Factory;

namespace Game.SkillCardSystem.Core
{
    public class PlayerHandManager : MonoBehaviour, IPlayerHandManager
    {
        private IPlayerCharacter owner;
        private IHandSlotRegistry slotRegistry;
        private ISkillCardFactory cardFactory;

        private readonly Dictionary<SkillCardSlotPosition, ISkillCard> cards = new();
        private readonly Dictionary<SkillCardSlotPosition, SkillCardUI> cardUIs = new();

        public void Inject(
            IPlayerCharacter owner,
            IHandSlotRegistry slotRegistry,
            ISkillCardFactory cardFactory)
        {
            this.owner = owner;
            this.slotRegistry = slotRegistry;
            this.cardFactory = cardFactory;
        }

        public void Initialize()
        {
            cards.Clear();
            cardUIs.Clear();
        }

        public void GenerateInitialHand()
        {
            if (owner?.Data?.SkillDeck == null)
            {
                Debug.LogError("[PlayerHandManager] 플레이어 덱이 비어 있습니다.");
                return;
            }

            var deck = owner.Data.SkillDeck;

            foreach (var cardEntry in deck.Cards)
            {
                var pos = cardEntry.Slot;
                var cardSO = cardEntry.Card;

                if (cardSO == null)
                {
                    Debug.LogWarning($"[PlayerHandManager] 카드 데이터가 null입니다. 슬롯: {pos}");
                    continue;
                }

                var effects = cardSO.CreateEffects(); // 확실하게 정의되어 있어야 함
                var card = cardFactory.CreatePlayerCard(cardSO.CardData, cardSO.CreateEffects());


                cards[pos] = card;

                var ui = slotRegistry.GetPlayerHandSlot(pos)?.AttachCard(card);
                if (ui != null)
                {
                    cardUIs[pos] = ui;
                }
                else
                {
                    Debug.LogWarning($"[PlayerHandManager] 슬롯 {pos}에 카드 UI를 붙일 수 없습니다.");
                }
            }
        }

        public ISkillCard GetCardInSlot(SkillCardSlotPosition pos)
        {
            cards.TryGetValue(pos, out var card);
            return card;
        }

        public ISkillCardUI GetCardUIInSlot(SkillCardSlotPosition pos)
        {
            cardUIs.TryGetValue(pos, out var ui);
            return ui;
        }

        public void RestoreCardToHand(ISkillCard card)
        {
            foreach (var kvp in cards)
            {
                if (kvp.Value == null)
                {
                    cards[kvp.Key] = card;
                    var ui = slotRegistry.GetPlayerHandSlot(kvp.Key)?.AttachCard(card);
                    if (ui != null)
                        cardUIs[kvp.Key] = ui;

                    Debug.Log($"[PlayerHandManager] 카드가 {kvp.Key} 슬롯에 복귀됨");
                    return;
                }
            }

            Debug.LogWarning("[PlayerHandManager] 빈 슬롯이 없어 카드를 복귀할 수 없습니다.");
        }

        public void LogPlayerHandSlotStates()
        {
            foreach (SkillCardSlotPosition pos in System.Enum.GetValues(typeof(SkillCardSlotPosition)))
            {
                var card = GetCardInSlot(pos);
                var status = card != null ? card.CardData.Name : "(없음)";
                Debug.Log($"[PlayerHandManager] 슬롯 {pos}: {status}");
            }
        }

        public void EnableInput(bool enable)
        {
            foreach (var ui in cardUIs.Values)
            {
                if (ui != null)
                    ui.SetDraggable(enable);
            }

            Debug.Log($"[PlayerHandManager] 입력 {(enable ? "활성화" : "비활성화")}");
        }

        public void ClearAll()
        {
            foreach (var pos in cards.Keys)
            {
                var slot = slotRegistry.GetPlayerHandSlot(pos);
                slot?.DetachCard();
            }

            cards.Clear();
            cardUIs.Clear();

            Debug.Log("[PlayerHandManager] 모든 핸드 슬롯 초기화 완료");
        }
    }
}
