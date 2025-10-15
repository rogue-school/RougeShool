using Game.ItemSystem.Data;
using Game.ItemSystem.Data.Reward;

namespace Game.ItemSystem.Service.Reward
{
	/// <summary>
	/// 보상 생성 서비스 인터페이스
	/// RewardProfile 제거로 단순화됨
	/// </summary>
	public interface IRewardGenerator
	{
		/// <summary>
		/// 액티브 아이템 보상을 생성합니다
		/// </summary>
		ActiveItemDefinition[] GenerateActive(EnemyRewardConfig enemy, PlayerRewardProfile player, int stageIndex, int runSeed);
		
		/// <summary>
		/// 패시브 아이템 보상을 생성합니다
		/// </summary>
		PassiveItemDefinition[] GeneratePassive(EnemyRewardConfig enemy, PlayerRewardProfile player, int stageIndex, int runSeed);
	}
}
