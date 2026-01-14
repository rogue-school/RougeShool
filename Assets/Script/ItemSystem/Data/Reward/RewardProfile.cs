using UnityEngine;

namespace Game.ItemSystem.Data.Reward
{
    /// <summary>
    /// 보상 정책을 정의하는 열거형들
    /// RewardProfile ScriptableObject 대신 기본값 사용
    /// </summary>
    public enum RewardDuplicatePolicy { AllowDuplicates, RerollOnDuplicate }
    public enum RewardSeedPolicy { PerRun, PerStage, PerEnemy }

    /// <summary>
    /// 기본 보상 정책 설정
    /// </summary>
    public static class DefaultRewardPolicy
    {
        // 중복 허용
        public const RewardDuplicatePolicy DuplicatePolicy = RewardDuplicatePolicy.AllowDuplicates;

        // 매 적마다 다른 랜덤 시드 사용 (진정한 랜덤)
        public const RewardSeedPolicy SeedPolicy = RewardSeedPolicy.PerEnemy;
    }
}