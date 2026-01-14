using UnityEngine;

namespace Game.ItemSystem.Data.Reward
{
	[CreateAssetMenu(fileName = "EnemyRewardConfig", menuName = "ItemSystem/Reward/EnemyRewardConfig")]
	public class EnemyRewardConfig : ScriptableObject
	{
		[Header("드롭 개수 설정")]
		[Min(0)] public int activeCount = 1;
		[Min(0)] public int passiveCount = 1;

		[Header("사용할 보상 풀(선택)")]
		public RewardPool[] activePools;
		public RewardPool[] passivePools;
	}
}
