using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Data;
using Game.StageSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 카드 순환 및 보상 시스템 통합 구현체입니다.
    /// 플레이어 덱에서 랜덤하게 카드를 드로우하고 보상을 관리합니다.
    /// </summary>
    public class CardCirculationSystem : ICardCirculationSystem
    {
        #region 필드

        private readonly List<ISkillCard> playerDeck = new();
        private readonly List<ISkillCard> currentTurnCards = new();
        private readonly IPlayerDeckManager playerDeckManager;

        #endregion

        #region 생성자

        public CardCirculationSystem(IPlayerDeckManager playerDeckManager)
        {
            this.playerDeckManager = playerDeckManager;
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

            GameLogger.LogInfo($"카드 순환 시스템 초기화 완료: {playerDeck.Count}장", GameLogger.LogCategory.SkillCard);
        }

        public void Clear()
        {
            playerDeck.Clear();
            currentTurnCards.Clear();
            GameLogger.LogInfo("카드 순환 시스템 초기화됨", GameLogger.LogCategory.SkillCard);
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

            GameLogger.LogInfo($"턴 드로우 완료: {currentTurnCards.Count}장 (덱: {playerDeck.Count}장)", GameLogger.LogCategory.SkillCard);
            return currentTurnCards;
        }

        #endregion


        #region 보상 관리

        /// <summary>
        /// 적 캐릭터 처치 보상 카드를 지급합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        public void GiveEnemyDefeatCardRewards(StageRewardData rewardData)
        {
            if (rewardData == null)
            {
                GameLogger.LogWarning("보상 데이터가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 적 처치 보상 카드 지급
            foreach (var cardReward in rewardData.EnemyDefeatCards)
            {
                if (cardReward.cardDefinition != null)
                {
                    bool success = playerDeckManager.AddCardToDeck(cardReward.cardDefinition, cardReward.quantity);
                    if (success)
                    {
                        GameLogger.LogInfo($"적 처치 카드 보상 지급: {cardReward.cardDefinition.displayName} x{cardReward.quantity}", GameLogger.LogCategory.SkillCard);
                    }
                    else
                    {
                        GameLogger.LogWarning($"적 처치 카드 보상 지급 실패: {cardReward.cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
                    }
                }
                else
                {
                    GameLogger.LogWarning("카드 정의가 null인 보상이 있습니다.", GameLogger.LogCategory.SkillCard);
                }
            }
        }

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
            if (success)
            {
                GameLogger.LogInfo($"카드 보상 지급: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
            }
            else
            {
                GameLogger.LogWarning($"카드 보상 지급 실패: {cardDefinition.displayName} x{quantity}", GameLogger.LogCategory.SkillCard);
            }

            return success;
        }

        #endregion
    }
}