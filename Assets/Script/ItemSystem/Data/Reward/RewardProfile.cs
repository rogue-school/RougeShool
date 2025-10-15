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
		public const RewardDuplicatePolicy DuplicatePolicy = RewardDuplicatePolicy.RerollOnDuplicate;
		public const RewardSeedPolicy SeedPolicy = RewardSeedPolicy.PerStage;
	}
}
