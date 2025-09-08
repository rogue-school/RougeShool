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
        #region 단계별 스테이지 생성

        /// <summary>
        /// 준보스/보스 구성 스테이지 생성
        /// </summary>
        /// <param name="subBoss">준보스 데이터</param>
        /// <param name="boss">보스 데이터</param>
        /// <param name="stageName">스테이지 이름</param>
        /// <param name="stageNumber">스테이지 번호</param>
        /// <returns>생성된 StagePhaseData</returns>
        public static StagePhaseData CreateBossRushStage(
            EnemyCharacterData subBoss,
            EnemyCharacterData boss,
            string stageName = "보스 러시",
            int stageNumber = 1)
        {
            if (subBoss == null || boss == null)
            {
                Debug.LogError("[StageDataFactory] 준보스 또는 보스 데이터가 null입니다.");
                return null;
            }

            var stageData = ScriptableObject.CreateInstance<StagePhaseData>();
            
            // 리플렉션을 사용하여 private 필드 설정
            var subBossField = typeof(StagePhaseData).GetField("subBoss", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var bossField = typeof(StagePhaseData).GetField("boss", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stageNameField = typeof(StagePhaseData).GetField("stageName", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stageNumberField = typeof(StagePhaseData).GetField("stageNumber", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var stageDescriptionField = typeof(StagePhaseData).GetField("stageDescription", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            subBossField?.SetValue(stageData, subBoss);
            bossField?.SetValue(stageData, boss);
            stageNameField?.SetValue(stageData, stageName);
            stageNumberField?.SetValue(stageData, stageNumber);
            stageDescriptionField?.SetValue(stageData, $"준보스: {subBoss.CharacterName} → 보스: {boss.CharacterName}");

            Debug.Log($"[StageDataFactory] 스테이지 생성 완료: {stageName}");
            return stageData;
        }

        #endregion

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

        #region 유틸리티

        /// <summary>
        /// 스테이지 데이터 유효성 검증
        /// </summary>
        /// <param name="stageData">검증할 스테이지 데이터</param>
        /// <returns>유효성 여부</returns>
        public static bool ValidateStageData(StagePhaseData stageData)
        {
            if (stageData == null)
            {
                Debug.LogError("[StageDataFactory] 스테이지 데이터가 null입니다.");
                return false;
            }

            if (!stageData.IsValid())
            {
                Debug.LogError("[StageDataFactory] 스테이지 데이터가 유효하지 않습니다.");
                return false;
            }

            if (!stageData.HasValidSubBoss())
            {
                Debug.LogError("[StageDataFactory] 준보스 데이터가 유효하지 않습니다.");
                return false;
            }

            if (!stageData.HasValidBoss())
            {
                Debug.LogError("[StageDataFactory] 보스 데이터가 유효하지 않습니다.");
                return false;
            }

            Debug.Log($"[StageDataFactory] 스테이지 데이터 유효성 검증 통과: {stageData.StageName}");
            return true;
        }

        #endregion
    }
}
