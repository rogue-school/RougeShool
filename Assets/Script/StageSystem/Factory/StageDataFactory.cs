using UnityEngine;
using Game.StageSystem.Data;
using Game.CharacterSystem.Data;

namespace Game.StageSystem.Factory
{
    /// <summary>
    /// 스테이지 데이터 생성 팩토리
    /// 로그 스쿨 시스템: 준보스/보스 구성 스테이지 생성
    /// </summary>
    public static class StageDataFactory
    {

        #region 보상 데이터 생성

        /// <summary>
        /// 기본 보상 데이터 생성
        /// </summary>
        /// <param name="hasSubBossRewards">준보스 보상 포함 여부</param>
        /// <param name="hasBossRewards">보스 보상 포함 여부</param>
        /// <param name="hasCompletionRewards">완료 보상 포함 여부</param>
        /// <returns>생성된 StageRewardData</returns>
        public static StageRewardData CreateDefaultRewards(
            bool hasSubBossRewards = true,
            bool hasBossRewards = true,
            bool hasCompletionRewards = true)
        {
            var rewardData = ScriptableObject.CreateInstance<StageRewardData>();

            // 리플렉션을 사용하여 private 필드 설정
            var subBossRewardsField = typeof(StageRewardData).GetField("subBossRewards", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bossRewardsField = typeof(StageRewardData).GetField("bossRewards", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var completionRewardsField = typeof(StageRewardData).GetField("stageCompletionRewards", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (hasSubBossRewards)
            {
                var subBossRewards = new System.Collections.Generic.List<StageRewardData.RewardItem>
                {
                    new StageRewardData.RewardItem { itemName = "경험치", quantity = 100 },
                    new StageRewardData.RewardItem { itemName = "골드", quantity = 50 }
                };
                subBossRewardsField?.SetValue(rewardData, subBossRewards);
            }

            if (hasBossRewards)
            {
                var bossRewards = new System.Collections.Generic.List<StageRewardData.RewardItem>
                {
                    new StageRewardData.RewardItem { itemName = "경험치", quantity = 200 },
                    new StageRewardData.RewardItem { itemName = "골드", quantity = 100 }
                };
                bossRewardsField?.SetValue(rewardData, bossRewards);
            }

            if (hasCompletionRewards)
            {
                var completionRewards = new System.Collections.Generic.List<StageRewardData.RewardItem>
                {
                    new StageRewardData.RewardItem { itemName = "스킬 포인트", quantity = 1 },
                    new StageRewardData.RewardItem { itemName = "아이템 상자", quantity = 1 }
                };
                completionRewardsField?.SetValue(rewardData, completionRewards);
            }

            Debug.Log("[StageDataFactory] 기본 보상 데이터 생성 완료");
            return rewardData;
        }

        #endregion

    }
}
