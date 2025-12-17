using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.StageSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 카드 순환, 보상, 턴 관리 통합 시스템입니다.
    /// 플레이어 덱에서 랜덤하게 카드를 드로우하고 보상 및 턴 관리를 담당합니다.
    /// </summary>
    public class CardCirculationSystem : ICardCirculationSystem
    {
        #region 필드

        private readonly List<ISkillCard> playerDeck = new();
        private readonly List<ISkillCard> currentTurnCards = new();
        private readonly IPlayerDeckManager playerDeckManager;
        private readonly ISkillCardFactory skillCardFactory;
        
        // 턴 관리 필드
        private bool isTurnStarted = false;
        private bool hasPlayedThisTurn = false;

        #endregion

        #region 생성자

        public CardCirculationSystem(IPlayerDeckManager playerDeckManager, ISkillCardFactory skillCardFactory)
        {
            this.playerDeckManager = playerDeckManager;
            this.skillCardFactory = skillCardFactory;
        }

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
                GameLogger.LogError("초기 카드 리스트가 비어있습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 초기 카드들을 플레이어 덱에 추가
            playerDeck.Clear();
            currentTurnCards.Clear();

            foreach (var card in initialCards)
            {
                playerDeck.Add(card);
            }

            // 덱 구성 상세 로그
            var cardGroups = playerDeck.GroupBy(c => c.CardDefinition).Select(g => new { 
                CardName = g.Key.displayName, 
                Count = g.Count() 
            });
        }

        public void Clear()
        {
            playerDeck.Clear();
            currentTurnCards.Clear();
        }

        #endregion

        #region 카드 드로우

        public List<ISkillCard> DrawCardsForTurn()
        {
            currentTurnCards.Clear();

            if (playerDeck.Count == 0)
            {
                GameLogger.LogWarning("덱이 비어있어 카드를 드로우할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                return currentTurnCards;
            }

            // 사용 가능한 카드 수만큼만 드로우 (중복 방지)
            int cardsToDraw = Mathf.Min(CardsPerTurn, playerDeck.Count);
            
            // 덱을 복사하여 셔플
            var availableCards = new List<ISkillCard>(playerDeck);
            
            // Fisher-Yates 셔플 알고리즘
            for (int i = availableCards.Count - 1; i > 0; i--)
            {
                int randomIndex = Random.Range(0, i + 1);
                (availableCards[i], availableCards[randomIndex]) = (availableCards[randomIndex], availableCards[i]);
            }
            
            // 앞에서부터 목표 장수만큼 드로우
            for (int i = 0; i < cardsToDraw; i++)
            {
                currentTurnCards.Add(availableCards[i]);
            }

            // 드로우된 카드 상세 로그
            var drawnCardGroups = currentTurnCards.GroupBy(c => c.CardDefinition).Select(g => new { 
                CardName = g.Key.displayName, 
                Count = g.Count() 
            });
            
            return currentTurnCards;
        }

        #endregion


        #region 보상 관리

        /// <summary>
        /// 특정 카드를 보상으로 지급합니다.
        /// </summary>
        /// <param name="cardDefinition">지급할 카드 정의</param>
        /// <param name="quantity">지급할 수량</param>
        /// <returns>지급 성공 여부</returns>
        public bool GiveCardReward(SkillCardDefinition cardDefinition, int quantity = 1)
        {
            if (cardDefinition == null)
            {
                GameLogger.LogError("카드 정의가 null입니다.", GameLogger.LogCategory.SkillCard);
                return false;
            }

            if (quantity <= 0)
            {
                GameLogger.LogError($"잘못된 수량입니다: {quantity}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            bool success = playerDeckManager.AddCardToDeck(cardDefinition, quantity);
            if (!success)
            {
                GameLogger.LogWarning($"카드 보상 지급 실패: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
                return false;
            }

            // 런타임 덱에도 동일한 수량만큼 카드 인스턴스를 추가하여 다음 턴부터 사용할 수 있도록 합니다.
            if (skillCardFactory == null)
            {
                GameLogger.LogWarning("[CardCirculationSystem] SkillCardFactory가 주입되지 않았습니다. 런타임 덱에는 카드가 추가되지 않습니다.", GameLogger.LogCategory.SkillCard);
                return true;
            }

            for (int i = 0; i < quantity; i++)
            {
                var cardInstance = skillCardFactory.CreatePlayerCard(cardDefinition);
                if (cardInstance != null)
                {
                    playerDeck.Add(cardInstance);
                }
                else
                {
                    GameLogger.LogWarning($"[CardCirculationSystem] 스킬카드 인스턴스 생성 실패: {cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
                }
            }

            return true;
        }

        #endregion
        
        #region 턴 관리 (TurnBasedCardManager 통합)
        
        /// <summary>
        /// 턴을 시작합니다.
        /// </summary>
        public void StartTurn()
        {
            if (isTurnStarted)
            {
                GameLogger.LogWarning("턴이 이미 시작되었습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 카드 드로우
            currentTurnCards.Clear();
            currentTurnCards.AddRange(DrawCardsForTurn());
            hasPlayedThisTurn = false;
            isTurnStarted = true;
        }

        /// <summary>
        /// 턴을 종료합니다.
        /// </summary>
        public void EndTurn()
        {
            if (!isTurnStarted)
            {
                GameLogger.LogWarning("턴이 시작되지 않았습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 턴 종료 처리
            if (currentTurnCards.Count > 0)
            {
                currentTurnCards.Clear();
            }

            isTurnStarted = false;
            hasPlayedThisTurn = false;
        }

        /// <summary>
        /// 카드를 사용합니다.
        /// </summary>
        /// <param name="card">사용할 카드</param>
        public void UseCard(ISkillCard card)
        {
            if (card == null)
            {
                GameLogger.LogWarning("null 카드를 사용할 수 없습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (!currentTurnCards.Contains(card))
            {
                GameLogger.LogWarning("현재 턴의 카드가 아닙니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            if (hasPlayedThisTurn)
            {
                GameLogger.LogWarning("이 턴에는 이미 카드를 사용했습니다. 턴당 1장 제한.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 카드 사용 처리
            currentTurnCards.Remove(card);
            hasPlayedThisTurn = true;
        }

        /// <summary>
        /// 현재 턴의 카드들을 반환합니다.
        /// </summary>
        /// <returns>현재 턴 카드 리스트</returns>
        public List<ISkillCard> GetCurrentTurnCards()
        {
            return new List<ISkillCard>(currentTurnCards);
        }

        /// <summary>
        /// 턴이 시작되었는지 여부를 반환합니다.
        /// </summary>
        public bool IsTurnStarted => isTurnStarted;

        /// <summary>
        /// 사용 가능한 카드 수를 반환합니다.
        /// </summary>
        public int AvailableCardsCount => currentTurnCards.Count;

        #endregion
    }
}