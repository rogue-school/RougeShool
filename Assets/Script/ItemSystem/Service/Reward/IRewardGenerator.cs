using Game.ItemSystem.Data;
using Game.ItemSystem.Data.Reward;

namespace Game.ItemSystem.Service.Reward
{
	public interface IRewardGenerator
	{
		ActiveItemDefinition[] GenerateActive(EnemyRewardConfig enemy, PlayerRewardProfile player, RewardProfile profile, int stageIndex, int runSeed);
		PassiveItemDefinition[] GeneratePassive(EnemyRewardConfig enemy, PlayerRewardProfile player, RewardProfile profile, int stageIndex, int runSeed);
	}
}
