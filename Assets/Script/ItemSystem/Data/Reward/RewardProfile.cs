using UnityEngine;

namespace Game.ItemSystem.Data.Reward
{
	public enum RewardDuplicatePolicy { AllowDuplicates, RerollOnDuplicate }
	public enum RewardSeedPolicy { PerRun, PerStage, PerEnemy }

	[CreateAssetMenu(fileName = "RewardProfile", menuName = "ItemSystem/Reward/RewardProfile")]
	public class RewardProfile : ScriptableObject
	{
		[Header("중복/시드 정책")]
		public RewardDuplicatePolicy duplicatePolicy = RewardDuplicatePolicy.RerollOnDuplicate;
		public RewardSeedPolicy seedPolicy = RewardSeedPolicy.PerStage;
	}
}
