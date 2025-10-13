using UnityEngine;
using Zenject;
using Game.ItemSystem.Data.Reward;
using Game.ItemSystem.Service.Reward;

namespace Game.ItemSystem.Runtime
{
	/// <summary>
	/// 적 사망 시 보상 패널을 여는 브리지 컴포넌트입니다.
	/// 전투 시스템에서 적 처치 시 이 컴포넌트의 OnEnemyKilled를 호출하거나
	/// 이벤트로 연결해 사용합니다.
	/// </summary>
	public class RewardOnEnemyDeath : MonoBehaviour
	{
		[Header("보상 구성 참조")]
		[SerializeField] private EnemyRewardConfig enemyRewardConfig;
		[SerializeField] private PlayerRewardProfile playerRewardProfile;
		[SerializeField] private RewardProfile rewardProfile;
		[SerializeField] private RewardPanelController rewardPanel;

		[Header("컨텍스트")]
		[SerializeField] private int stageIndex;
		[SerializeField] private int runSeed;

		[Inject(Optional = true)] private IRewardGenerator _generator;

		/// <summary>
		/// 외부에서 적 처치 시 호출하세요.
		/// </summary>
		public void OnEnemyKilled()
		{
			if (_generator == null || rewardPanel == null)
			{
				// 주입/바인딩이 안 되어 있으면 아무 것도 하지 않음
				return;
			}
			rewardPanel.OpenGenerated(enemyRewardConfig, playerRewardProfile, rewardProfile, stageIndex, runSeed);
		}

		// 선택: 런타임 중 컨텍스트 갱신 API
		public void SetContext(int newStageIndex, int newRunSeed)
		{
			stageIndex = newStageIndex;
			runSeed = newRunSeed;
		}
	}
}
