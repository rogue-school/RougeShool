using UnityEngine;
using Zenject;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.StageSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.SkillCardSystem.Manager
{
    /// <summary>
    /// 카드 보상을 관리하는 매니저 클래스입니다.
    /// 스테이지 완료 시 카드 보상을 지급하고 플레이어 덱에 추가합니다.
    /// </summary>
    public class CardRewardManager : MonoBehaviour
    {
        #region 필드

        private IPlayerDeckManager playerDeckManager;

        #endregion

        #region 의존성 주입

        [Inject]
        public void Construct(IPlayerDeckManager playerDeckManager)
        {
            this.playerDeckManager = playerDeckManager;
        }

        #endregion

        #region 보상 지급

        /// <summary>
        /// 준보스 보상 카드를 지급합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        public void GiveSubBossCardRewards(StageRewardData rewardData)
        {
            if (rewardData == null)
            {
                GameLogger.LogWarning("보상 데이터가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            foreach (var cardReward in rewardData.SubBossCards)
            {
                if (cardReward.cardDefinition != null)
                {
                    bool success = playerDeckManager.AddCardToDeck(cardReward.cardDefinition, cardReward.quantity);
                    if (success)
                    {
                        GameLogger.LogInfo($"준보스 카드 보상 지급: {cardReward.cardDefinition.displayName} x{cardReward.quantity}", GameLogger.LogCategory.SkillCard);
                    }
                    else
                    {
                        GameLogger.LogWarning($"준보스 카드 보상 지급 실패: {cardReward.cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
                    }
                }
                else
                {
                    GameLogger.LogWarning("카드 정의가 null인 보상이 있습니다.", GameLogger.LogCategory.SkillCard);
                }
            }
        }

        /// <summary>
        /// 보스 보상 카드를 지급합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        public void GiveBossCardRewards(StageRewardData rewardData)
        {
            if (rewardData == null)
            {
                GameLogger.LogWarning("보상 데이터가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            foreach (var cardReward in rewardData.BossCards)
            {
                if (cardReward.cardDefinition != null)
                {
                    bool success = playerDeckManager.AddCardToDeck(cardReward.cardDefinition, cardReward.quantity);
                    if (success)
                    {
                        GameLogger.LogInfo($"보스 카드 보상 지급: {cardReward.cardDefinition.displayName} x{cardReward.quantity}", GameLogger.LogCategory.SkillCard);
                    }
                    else
                    {
                        GameLogger.LogWarning($"보스 카드 보상 지급 실패: {cardReward.cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
                    }
                }
                else
                {
                    GameLogger.LogWarning("카드 정의가 null인 보상이 있습니다.", GameLogger.LogCategory.SkillCard);
                }
            }
        }

        /// <summary>
        /// 스테이지 완료 보상 카드를 지급합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        public void GiveStageCompletionCardRewards(StageRewardData rewardData)
        {
            if (rewardData == null)
            {
                GameLogger.LogWarning("보상 데이터가 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            foreach (var cardReward in rewardData.StageCompletionCards)
            {
                if (cardReward.cardDefinition != null)
                {
                    bool success = playerDeckManager.AddCardToDeck(cardReward.cardDefinition, cardReward.quantity);
                    if (success)
                    {
                        GameLogger.LogInfo($"스테이지 완료 카드 보상 지급: {cardReward.cardDefinition.displayName} x{cardReward.quantity}", GameLogger.LogCategory.SkillCard);
                    }
                    else
                    {
                        GameLogger.LogWarning($"스테이지 완료 카드 보상 지급 실패: {cardReward.cardDefinition.displayName}", GameLogger.LogCategory.SkillCard);
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

        #region 보상 확인

        /// <summary>
        /// 준보스 카드 보상이 있는지 확인합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        /// <returns>카드 보상 존재 여부</returns>
        public bool HasSubBossCardRewards(StageRewardData rewardData)
        {
            return rewardData != null && rewardData.SubBossCards.Count > 0;
        }

        /// <summary>
        /// 보스 카드 보상이 있는지 확인합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        /// <returns>카드 보상 존재 여부</returns>
        public bool HasBossCardRewards(StageRewardData rewardData)
        {
            return rewardData != null && rewardData.BossCards.Count > 0;
        }

        /// <summary>
        /// 스테이지 완료 카드 보상이 있는지 확인합니다.
        /// </summary>
        /// <param name="rewardData">보상 데이터</param>
        /// <returns>카드 보상 존재 여부</returns>
        public bool HasStageCompletionCardRewards(StageRewardData rewardData)
        {
            return rewardData != null && rewardData.StageCompletionCards.Count > 0;
        }

        #endregion
    }
}
