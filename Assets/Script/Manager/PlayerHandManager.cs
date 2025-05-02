using UnityEngine;
using System.Collections.Generic;
using Game.Cards;
using Game.UI;
using Game.Units;

namespace Game.Managers
{
    /// <summary>
    /// 플레이어의 핸드 슬롯을 관리하는 클래스입니다.
    /// 선택된 캐릭터가 가진 3개의 고정 스킬 카드를 카드 슬롯에 배치합니다.
    /// </summary>
    public class PlayerHandManager : MonoBehaviour
    {
        public PlayerUnit playerUnit;
        public CardUI cardUIPrefab;
        public Transform[] cardSlots = new Transform[3];

        private List<CardUI> currentCards = new();

        private void Start()
        {
            DrawInitialCards();
        }

        public void DrawInitialCards()
        {
            ClearHand();

            if (playerUnit?.characterData == null)
            {
                Debug.LogError("[PlayerHandManager] PlayerUnit 또는 캐릭터 카드 데이터가 연결되지 않았습니다.");
                return;
            }

            var deck = playerUnit.characterData.initialDeck;

            for (int i = 0; i < cardSlots.Length && i < deck.Count; i++)
            {
                var cardData = deck[i];
                var cardUI = Instantiate(cardUIPrefab, cardSlots[i]);
                cardUI.SetCard(cardData);
                currentCards.Add(cardUI);
            }
        }

        public void ClearHand()
        {
            foreach (var card in currentCards)
            {
                if (card != null) Destroy(card.gameObject);
            }

            currentCards.Clear();
        }
    }
}
