using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 카드 순환 시스템 구현체입니다.
    /// Unused Storage ↔ Used Storage 간의 무한 순환을 관리합니다.
    /// </summary>
    public class CardCirculationSystem : MonoBehaviour, ICardCirculationSystem
    {
        #region 필드

        private readonly List<ISkillCard> unusedStorage = new();
        private readonly List<ISkillCard> usedStorage = new();
        private readonly List<ISkillCard> currentTurnCards = new();

        #endregion

        #region 프로퍼티

        public int CardsPerTurn { get; private set; } = 3;
        public int UnusedCardCount => unusedStorage.Count;
        public int UsedCardCount => usedStorage.Count;

        #endregion

        #region 초기화

        public void Initialize(List<ISkillCard> initialCards)
        {
            if (initialCards == null || initialCards.Count == 0)
            {
                Debug.LogError("[CardCirculationSystem] 초기 카드 리스트가 비어있습니다.");
                return;
            }

            // 초기 카드들을 Unused Storage에 추가
            unusedStorage.Clear();
            usedStorage.Clear();
            currentTurnCards.Clear();

            foreach (var card in initialCards)
            {
                unusedStorage.Add(card);
            }

            // 카드 순서를 랜덤하게 섞기
            ShuffleUnusedStorage();

            Debug.Log($"[CardCirculationSystem] 초기화 완료: {unusedStorage.Count}장의 카드");
        }

        public void Reset()
        {
            unusedStorage.Clear();
            usedStorage.Clear();
            currentTurnCards.Clear();
            Debug.Log("[CardCirculationSystem] 리셋 완료");
        }

        #endregion

        #region 카드 드로우

        public List<ISkillCard> DrawCardsForTurn()
        {
            currentTurnCards.Clear();

            // 목표 장수만큼 드로우하되, 부족하면 사용 보관함을 순환 후 이어서 드로우
            int targetCount = CardsPerTurn;
            while (currentTurnCards.Count < targetCount)
            {
                // 미사용 보관함이 비어 있으면 순환 시도
                if (unusedStorage.Count == 0)
                {
                    if (usedStorage.Count == 0)
                    {
                        // 더 이상 순환할 카드가 없음 → 현재 가능한 만큼만 드로우하고 종료
                        break;
                    }
                    CirculateCardsIfNeeded();
                }

                if (unusedStorage.Count > 0)
                {
                    var card = unusedStorage[0];
                    unusedStorage.RemoveAt(0);
                    currentTurnCards.Add(card);
                }
                else
                {
                    break;
                }
            }

            Debug.Log($"[CardCirculationSystem] 턴 드로우: {currentTurnCards.Count}장 (Unused: {unusedStorage.Count}, Used: {usedStorage.Count})");
            return new List<ISkillCard>(currentTurnCards);
        }

        #endregion

        #region 카드 이동

        public void MoveCardToUsedStorage(ISkillCard card)
        {
            if (card == null)
            {
                Debug.LogWarning("[CardCirculationSystem] null 카드를 Used Storage로 이동할 수 없습니다.");
                return;
            }

            // 현재 턴 카드에서 제거
            currentTurnCards.Remove(card);
            
            // Used Storage에 추가
            usedStorage.Add(card);

            Debug.Log($"[CardCirculationSystem] 카드 Used Storage 이동: {card.CardDefinition?.CardName ?? "Unknown"} (Used: {usedStorage.Count})");
        }

        public void MoveCardsToUsedStorage(List<ISkillCard> cards)
        {
            if (cards == null || cards.Count == 0)
            {
                Debug.LogWarning("[CardCirculationSystem] 이동할 카드가 없습니다.");
                return;
            }

            foreach (var card in cards)
            {
                MoveCardToUsedStorage(card);
            }
        }

        #endregion

        #region 카드 순환

        public void CirculateCardsIfNeeded()
        {
            if (unusedStorage.Count == 0 && usedStorage.Count > 0)
            {
                // Used Storage의 모든 카드를 Unused Storage로 이동
                foreach (var card in usedStorage)
                {
                    unusedStorage.Add(card);
                }
                usedStorage.Clear();

                // 카드 순서를 랜덤하게 섞기
                ShuffleUnusedStorage();

                Debug.Log($"[CardCirculationSystem] 카드 순환 완료: {unusedStorage.Count}장이 Unused Storage로 이동");
            }
        }

        #endregion

        #region 유틸리티

        /// <summary>
        /// Unused Storage의 카드들을 랜덤하게 섞습니다.
        /// </summary>
        private void ShuffleUnusedStorage()
        {
            for (int i = 0; i < unusedStorage.Count; i++)
            {
                var temp = unusedStorage[i];
                int randomIndex = Random.Range(i, unusedStorage.Count);
                unusedStorage[i] = unusedStorage[randomIndex];
                unusedStorage[randomIndex] = temp;
            }
        }

        /// <summary>
        /// 현재 턴의 카드들을 반환합니다.
        /// </summary>
        public List<ISkillCard> GetCurrentTurnCards()
        {
            return new List<ISkillCard>(currentTurnCards);
        }

        /// <summary>
        /// 턴당 드로우 카드 수를 설정합니다.
        /// </summary>
        public void SetCardsPerTurn(int count)
        {
            CardsPerTurn = Mathf.Max(1, count);
            Debug.Log($"[CardCirculationSystem] 턴당 드로우 카드 수 설정: {CardsPerTurn}");
        }

        /// <summary>
        /// 현재 Unused Storage의 모든 카드를 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>Unused Storage의 카드 리스트</returns>
        public List<ISkillCard> GetUnusedCards()
        {
            return new List<ISkillCard>(unusedStorage);
        }

        /// <summary>
        /// 현재 Used Storage의 모든 카드를 반환합니다. (저장 시스템용)
        /// </summary>
        /// <returns>Used Storage의 카드 리스트</returns>
        public List<ISkillCard> GetUsedCards()
        {
            return new List<ISkillCard>(usedStorage);
        }

        /// <summary>
        /// Unused Storage에 카드들을 복원합니다. (저장 시스템용)
        /// </summary>
        /// <param name="cards">복원할 카드들</param>
        public void RestoreUnusedCards(List<ISkillCard> cards)
        {
            if (cards == null) return;

            unusedStorage.Clear();
            unusedStorage.AddRange(cards);
            Debug.Log($"[CardCirculationSystem] Unused Storage 복원: {cards.Count}장");
        }

        /// <summary>
        /// Used Storage에 카드들을 복원합니다. (저장 시스템용)
        /// </summary>
        /// <param name="cards">복원할 카드들</param>
        public void RestoreUsedCards(List<ISkillCard> cards)
        {
            if (cards == null) return;

            usedStorage.Clear();
            usedStorage.AddRange(cards);
            Debug.Log($"[CardCirculationSystem] Used Storage 복원: {cards.Count}장");
        }

        #endregion
    }
}
