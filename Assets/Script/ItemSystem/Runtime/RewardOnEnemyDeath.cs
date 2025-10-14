using UnityEngine;
using Zenject;
using Game.ItemSystem.Data.Reward;
using Game.ItemSystem.Service.Reward;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Runtime
{
	/// <summary>
	/// 적 사망 시 보상 패널을 여는 브리지 컴포넌트입니다.
	/// 전투 시스템에서 적 처치 시 이 컴포넌트의 OnEnemyKilled를 호출하거나
	/// 이벤트로 연결해 사용합니다.
	/// </summary>
	public class RewardOnEnemyDeath : MonoBehaviour
	{
		public enum RewardDisplayMode
		{
			ActiveOnly,
			PassiveOnly,
			Combined
		}

		[Header("보상 구성 참조")]
		[SerializeField] private EnemyRewardConfig enemyRewardConfig;
		[SerializeField] private PlayerRewardProfile playerRewardProfile; // 기본값(폴백)
		[Tooltip("플레이어 타입별 자동 선택 프로필 (선택)")]
		[SerializeField] private PlayerRewardProfile swordPlayerRewardProfile;
		[SerializeField] private PlayerRewardProfile bowPlayerRewardProfile;
		[SerializeField] private PlayerRewardProfile staffPlayerRewardProfile;
		[SerializeField] private RewardProfile rewardProfile;
		[SerializeField] private RewardPanelController rewardPanel;

		[Header("표시 모드")]
		[Tooltip("보상 패널에 표시할 항목 선택 (액티브/패시브/둘 다)")]
		[SerializeField] private RewardDisplayMode displayMode = RewardDisplayMode.Combined;

		[Header("컨텍스트")]
		[SerializeField] private int stageIndex;
		[SerializeField] private int runSeed;

		[Inject(Optional = true)] private IRewardGenerator _generator;
		[Inject(Optional = true)] private ICharacter _playerCharacter;

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
			if (enemyRewardConfig == null)
			{
				GameLogger.LogWarning("[Reward] EnemyRewardConfig가 설정되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}

			var profileToUse = ResolvePlayerRewardProfile();
			switch (displayMode)
			{
				case RewardDisplayMode.ActiveOnly:
					rewardPanel.OpenGenerated(enemyRewardConfig, profileToUse, rewardProfile, stageIndex, runSeed);
					break;
				case RewardDisplayMode.PassiveOnly:
					rewardPanel.OpenGeneratedPassive(enemyRewardConfig, profileToUse, rewardProfile, stageIndex, runSeed);
					break;
				case RewardDisplayMode.Combined:
				default:
					rewardPanel.OpenGeneratedCombined(enemyRewardConfig, profileToUse, rewardProfile, stageIndex, runSeed);
					break;
			}
		}

		// 선택: 런타임 중 컨텍스트 갱신 API
		public void SetContext(int newStageIndex, int newRunSeed)
		{
			stageIndex = newStageIndex;
			runSeed = newRunSeed;
		}

		/// <summary>
		/// 현재 플레이어 캐릭터 타입에 맞는 PlayerRewardProfile을 자동 선택합니다.
		/// 적절한 프로필이 없으면 인스펙터에 연결된 기본 프로필을 사용합니다.
		/// </summary>
		private PlayerRewardProfile ResolvePlayerRewardProfile()
		{
			// 플레이어 캐릭터 정보가 없으면 폴백
			if (_playerCharacter == null)
			{
				if (playerRewardProfile == null)
				{
					GameLogger.LogWarning("[Reward] 플레이어 정보와 기본 프로필이 모두 없습니다. 보상 필터링이 비활성화됩니다.", GameLogger.LogCategory.UI);
				}
				return playerRewardProfile;
			}

			var data = _playerCharacter.CharacterData as PlayerCharacterData;
			if (data == null)
			{
				// 예상 타입이 아니면 폴백
				return playerRewardProfile;
			}

			switch (data.CharacterType)
			{
				case PlayerCharacterType.Sword:
					return swordPlayerRewardProfile != null ? swordPlayerRewardProfile : playerRewardProfile;
				case PlayerCharacterType.Bow:
					return bowPlayerRewardProfile != null ? bowPlayerRewardProfile : playerRewardProfile;
				case PlayerCharacterType.Staff:
					return staffPlayerRewardProfile != null ? staffPlayerRewardProfile : playerRewardProfile;
				default:
					return playerRewardProfile;
			}
		}

		private void OnValidate()
		{
			// 에디터에서 기본 필수 항목 점검
			if (rewardPanel == null)
			{
				GameLogger.LogWarning("[Reward] RewardPanelController가 연결되지 않았습니다", GameLogger.LogCategory.UI);
			}
			if (rewardProfile == null)
			{
				GameLogger.LogWarning("[Reward] RewardProfile이 연결되지 않았습니다", GameLogger.LogCategory.UI);
			}
		}
	}
}
