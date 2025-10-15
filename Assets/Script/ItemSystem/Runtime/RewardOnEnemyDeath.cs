using UnityEngine;
using Zenject;
using Game.ItemSystem.Data.Reward;
using Game.ItemSystem.Service.Reward;
using Game.ItemSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CharacterSystem.Data;
using Game.CharacterSystem.Manager;
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
	[SerializeField] private RewardPanelController rewardPanelPrefab;
	[SerializeField] private Transform rewardPanelParent;

		[Header("플레이어 타입별 프로필")]
		[Tooltip("검 캐릭터용 보상 프로필")]
		[SerializeField] private PlayerRewardProfile swordPlayerRewardProfile;
		[Tooltip("활 캐릭터용 보상 프로필")]
		[SerializeField] private PlayerRewardProfile bowPlayerRewardProfile;
		[Tooltip("지팡이 캐릭터용 보상 프로필")]
		[SerializeField] private PlayerRewardProfile staffPlayerRewardProfile;

		[Header("표시 모드")]
		[Tooltip("보상 패널에 표시할 항목 선택 (액티브/패시브/둘 다)")]
		[SerializeField] private RewardDisplayMode displayMode = RewardDisplayMode.Combined;

		[Header("컨텍스트")]
		[SerializeField] private int stageIndex;
		[SerializeField] private int runSeed;

	[Inject(Optional = true)] private IRewardGenerator _generator;
	[Inject(Optional = true)] private PlayerManager _playerManager;
	[Inject(Optional = true)] private IItemService _itemService;

	/// <summary>
	/// 보상 처리가 완료되었을 때 발생하는 이벤트
	/// </summary>
	public event System.Action OnRewardProcessCompleted;

		/// <summary>
		/// 외부에서 적 처치 시 호출하세요.
		/// </summary>
		public void OnEnemyKilled()
		{
			GameLogger.LogInfo("[RewardOnEnemyDeath] 적 처치 보상 처리 시작", GameLogger.LogCategory.UI);

			// 프리팹이 없으면 경고
			if (rewardPanelPrefab == null)
			{
				GameLogger.LogWarning("[RewardOnEnemyDeath] RewardPanel 프리팹이 할당되지 않았습니다.", GameLogger.LogCategory.UI);
				OnRewardProcessCompleted?.Invoke();
				return;
			}

			// 부모 Transform이 없으면 Canvas를 찾아서 사용
			if (rewardPanelParent == null)
			{
				var canvas = Object.FindFirstObjectByType<Canvas>();
				if (canvas != null)
				{
					rewardPanelParent = canvas.transform;
					GameLogger.LogInfo("[RewardOnEnemyDeath] Canvas를 부모로 설정했습니다.", GameLogger.LogCategory.UI);
				}
				else
				{
					GameLogger.LogError("[RewardOnEnemyDeath] Canvas를 찾을 수 없습니다.", GameLogger.LogCategory.UI);
					OnRewardProcessCompleted?.Invoke();
					return;
				}
			}

			// 보상 패널 프리팹을 동적으로 생성
			var rewardPanelInstance = Instantiate(rewardPanelPrefab, rewardPanelParent);
			GameLogger.LogInfo($"[RewardOnEnemyDeath] 보상 패널 인스턴스 생성 완료 - activeSelf: {rewardPanelInstance.gameObject.activeSelf}", GameLogger.LogCategory.UI);

			// Zenject DI 수동 주입 (프리팹 생성 시 DI가 자동으로 작동하지 않음)
			if (_generator != null)
			{
				// IRewardGenerator 주입
				var rewardGeneratorField = typeof(RewardPanelController).GetField("_rewardGenerator", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				rewardGeneratorField?.SetValue(rewardPanelInstance, _generator);
				GameLogger.LogInfo("[RewardOnEnemyDeath] IRewardGenerator 수동 주입 완료", GameLogger.LogCategory.UI);
			}

			if (_itemService != null)
			{
				// IItemService 주입
				var itemServiceField = typeof(RewardPanelController).GetField("_itemService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				itemServiceField?.SetValue(rewardPanelInstance, _itemService);
				GameLogger.LogInfo("[RewardOnEnemyDeath] IItemService 수동 주입 완료", GameLogger.LogCategory.UI);
			}

			// 보상 패널 닫힘 이벤트 연결
			rewardPanelInstance.OnRewardPanelClosed += () => OnRewardPanelClosed(rewardPanelInstance);

			// IRewardGenerator가 없으면 기본 보상 패널 열기
			if (_generator == null)
			{
				GameLogger.LogWarning("[RewardOnEnemyDeath] IRewardGenerator가 주입되지 않았습니다. 기본 보상 패널을 엽니다.", GameLogger.LogCategory.UI);
				rewardPanelInstance.Toggle();
				return;
			}

			// 설정이 없어도 기본값으로 작동하도록 개선
			var profileToUse = ResolvePlayerRewardProfile();
			
			// 보상 생성 및 패널 열기
			switch (displayMode)
			{
				case RewardDisplayMode.ActiveOnly:
					rewardPanelInstance.OpenGenerated(enemyRewardConfig, profileToUse, stageIndex, runSeed);
					break;
				case RewardDisplayMode.PassiveOnly:
					rewardPanelInstance.OpenGeneratedPassive(enemyRewardConfig, profileToUse, stageIndex, runSeed);
					break;
				case RewardDisplayMode.Combined:
				default:
					rewardPanelInstance.OpenGeneratedCombined(enemyRewardConfig, profileToUse, stageIndex, runSeed);
					break;
			}
		}

		/// <summary>
		/// 보상 패널이 닫혔을 때 호출되는 콜백
		/// </summary>
		private void OnRewardPanelClosed(RewardPanelController panelInstance)
		{
			// 콜백 해제
			if (panelInstance != null)
			{
				panelInstance.OnRewardPanelClosed -= () => OnRewardPanelClosed(panelInstance);
			}

			// 보상 처리 완료 이벤트 발생
			OnRewardProcessCompleted?.Invoke();
			GameLogger.LogInfo("[RewardOnEnemyDeath] 보상 처리 완료 - 다음 진행 가능", GameLogger.LogCategory.UI);
		}

		// 선택: 런타임 중 컨텍스트 갱신 API
		public void SetContext(int newStageIndex, int newRunSeed)
		{
			stageIndex = newStageIndex;
			runSeed = newRunSeed;
		}

		/// <summary>
		/// 현재 플레이어 캐릭터 타입에 맞는 PlayerRewardProfile을 자동 선택합니다.
		/// 해당 타입의 프로필이 없으면 null을 반환합니다.
		/// </summary>
		private PlayerRewardProfile ResolvePlayerRewardProfile()
		{
			// PlayerManager를 통해 플레이어 캐릭터 정보 가져오기
			if (_playerManager == null)
			{
				GameLogger.LogInfo("[RewardOnEnemyDeath] PlayerManager가 주입되지 않았습니다. 보상 필터링이 비활성화됩니다.", GameLogger.LogCategory.UI);
				return null;
			}

			var playerCharacter = _playerManager.GetCharacter();
			if (playerCharacter == null)
			{
				GameLogger.LogInfo("[RewardOnEnemyDeath] 플레이어 캐릭터가 생성되지 않았습니다. 보상 필터링이 비활성화됩니다.", GameLogger.LogCategory.UI);
				return null;
			}

			var data = playerCharacter.CharacterData as PlayerCharacterData;
			if (data == null)
			{
				GameLogger.LogInfo("[RewardOnEnemyDeath] 플레이어 캐릭터 데이터를 가져올 수 없습니다. 보상 필터링이 비활성화됩니다.", GameLogger.LogCategory.UI);
				return null;
			}

			PlayerRewardProfile selectedProfile = null;
			
			switch (data.CharacterType)
			{
				case PlayerCharacterType.Sword:
					selectedProfile = swordPlayerRewardProfile;
					break;
				case PlayerCharacterType.Bow:
					selectedProfile = bowPlayerRewardProfile;
					break;
				case PlayerCharacterType.Staff:
					selectedProfile = staffPlayerRewardProfile;
					break;
				default:
					GameLogger.LogInfo($"[RewardOnEnemyDeath] 알 수 없는 캐릭터 타입: {data.CharacterType}. 보상 필터링이 비활성화됩니다.", GameLogger.LogCategory.UI);
					return null;
			}

			if (selectedProfile == null)
			{
				GameLogger.LogInfo($"[RewardOnEnemyDeath] {data.CharacterType} 캐릭터용 보상 프로필이 설정되지 않았습니다. 보상 필터링이 비활성화됩니다.", GameLogger.LogCategory.UI);
			}
			else
			{
				GameLogger.LogInfo($"[RewardOnEnemyDeath] {data.CharacterType} 캐릭터용 보상 프로필 사용: {selectedProfile.name}", GameLogger.LogCategory.UI);
			}

			return selectedProfile;
		}

		private void OnValidate()
		{
			// 에디터에서 필수 항목 점검 (경고만, 에러 아님)
			if (rewardPanelPrefab == null)
			{
				GameLogger.LogWarning("[RewardOnEnemyDeath] RewardPanel 프리팹이 연결되지 않았습니다", GameLogger.LogCategory.UI);
			}
			
			// 선택적 설정들에 대한 정보성 로그
			if (enemyRewardConfig == null)
			{
				GameLogger.LogInfo("[RewardOnEnemyDeath] EnemyRewardConfig가 연결되지 않았습니다 - 기본 보상 사용", GameLogger.LogCategory.UI);
			}
			if (swordPlayerRewardProfile == null && bowPlayerRewardProfile == null && staffPlayerRewardProfile == null)
			{
				GameLogger.LogInfo("[RewardOnEnemyDeath] 모든 플레이어 타입별 보상 프로필이 연결되지 않았습니다 - 필터링 없이 진행", GameLogger.LogCategory.UI);
			}
		}
	}
}
