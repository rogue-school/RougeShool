using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 카드 순환 시스템 구현체입니다.
    /// 플레이어 덱에서 랜덤하게 카드를 드로우하는 시스템입니다.
    /// </summary>
    public class CardCirculationSystem : MonoBehaviour, ICardCirculationSystem
    {
        #region 필드

        private readonly List<ISkillCard> playerDeck = new();
        private readonly List<ISkillCard> currentTurnCards = new();

        #endregion

        #region 프로퍼티

        public int CardsPerTurn { get; private set; } = 3;
        public int DeckCardCount => playerDeck.Count;

        #endregion

        #region 초기화

        public void Initialize(List<ISkillCard> initialCards)
        {
            if (initialCards == null || initialCards.Count == 0)
            {
                Debug.LogError("[CardCirculationSystem] 초기 카드 리스트가 비어있습니다.");
                return;
            }

            // 초기 카드들을 플레이어 덱에 추가
            playerDeck.Clear();
            currentTurnCards.Clear();

            foreach (var card in initialCards)
            {
                playerDeck.Add(card);
            }

            Debug.Log($"[CardCirculationSystem] 초기화 완료: {playerDeck.Count}장의 카드");
        }

        public void Clear()
        {
            playerDeck.Clear();
            currentTurnCards.Clear();
            Debug.Log("[CardCirculationSystem] 모든 카드가 제거되었습니다.");
        }

        #endregion

        #region 카드 드로우

        public List<ISkillCard> DrawCardsForTurn()
        {
            currentTurnCards.Clear();

            if (playerDeck.Count == 0)
            {
                Debug.LogWarning("[CardCirculationSystem] 덱이 비어있어 카드를 드로우할 수 없습니다.");
                return currentTurnCards;
            }

            // 목표 장수만큼 랜덤하게 드로우 (중복 허용)
            for (int i = 0; i < CardsPerTurn; i++)
            {
                if (playerDeck.Count > 0)
                {
                    int randomIndex = Random.Range(0, playerDeck.Count);
                    var card = playerDeck[randomIndex];
                    currentTurnCards.Add(card);
                }
            }

            Debug.Log($"[CardCirculationSystem] 턴 드로우: {currentTurnCards.Count}장 (덱: {playerDeck.Count}장)");
            return currentTurnCards;
        }

        #endregion

        #region 카드 관리 (레거시 호환성)

        public void MoveCardToUsedStorage(ISkillCard card)
        {
            // 카드 보관함 시스템이 제거되었으므로 아무것도 하지 않음
            Debug.Log($"[CardCirculationSystem] 카드 사용 완료: {card.CardDefinition?.CardName ?? "Unknown"} (보관함 시스템 제거됨)");
        }

        public void MoveCardsToUsedStorage(List<ISkillCard> cards)
        {
            // 카드 보관함 시스템이 제거되었으므로 아무것도 하지 않음
            Debug.Log($"[CardCirculationSystem] 카드들 사용 완료: {cards.Count}장 (보관함 시스템 제거됨)");
        }

        public void ShuffleUnusedStorage()
        {
            // 카드 보관함 시스템이 제거되었으므로 아무것도 하지 않음
            Debug.Log("[CardCirculationSystem] 보관함 시스템이 제거되어 셔플 기능이 비활성화되었습니다.");
        }

        public void CirculateCardsIfNeeded()
        {
            // 카드 보관함 시스템이 제거되었으므로 아무것도 하지 않음
            Debug.Log("[CardCirculationSystem] 보관함 시스템이 제거되어 순환 기능이 비활성화되었습니다.");
        }

        #endregion

        #region 핸드 관리

        public void MoveCardToHand(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardCirculationSystem] null 카드를 핸드로 이동할 수 없습니다.");
                return;
            }

            Debug.Log($"[CardCirculationSystem] 카드 핸드 이동: {card.CardDefinition?.CardName ?? "Unknown"}");
        }

        public void MoveCardToDiscard(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardCirculationSystem] null 카드를 버린 카드 더미로 이동할 수 없습니다.");
                return;
            }

            Debug.Log($"[CardCirculationSystem] 카드 버린 카드 더미 이동: {card.CardDefinition?.CardName ?? "Unknown"}");
        }

        public void MoveCardToExhaust(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardCirculationSystem] null 카드를 소멸 더미로 이동할 수 없습니다.");
                return;
            }

            Debug.Log($"[CardCirculationSystem] 카드 소멸 더미 이동: {card.CardDefinition?.CardName ?? "Unknown"}");
        }

        #endregion

        #region 저장 시스템 호환성 (레거시)

        public List<ISkillCard> GetUnusedCards()
        {
            // 보관함 시스템이 제거되었으므로 빈 리스트 반환
            return new List<ISkillCard>();
        }

        public List<ISkillCard> GetUsedCards()
        {
            // 보관함 시스템이 제거되었으므로 빈 리스트 반환
            return new List<ISkillCard>();
        }

        public void RestoreUnusedCards(List<ISkillCard> cards)
        {
            // 보관함 시스템이 제거되었으므로 아무것도 하지 않음
            Debug.Log("[CardCirculationSystem] 보관함 시스템이 제거되어 복원 기능이 비활성화되었습니다.");
        }

        public void RestoreUsedCards(List<ISkillCard> cards)
        {
            // 보관함 시스템이 제거되었으므로 아무것도 하지 않음
            Debug.Log("[CardCirculationSystem] 보관함 시스템이 제거되어 복원 기능이 비활성화되었습니다.");
        }

        #endregion
    }
}