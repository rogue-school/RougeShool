using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Zenject;
using Game.CoreSystem.Utility;
using Game.ItemSystem.Data.Reward;
using Game.ItemSystem.Service.Reward;

namespace Game.ItemSystem.Runtime
{
	/// <summary>
	/// 스테이지 클리어 보상(액티브 아이템) 패널을 관리합니다.
	/// 접기/펼치기, 후보 표시, [가져오기] 클릭 시 서비스에 위임하며
	/// 인벤토리 수용 규칙(3/4=1개, 4/4=불가)을 가드합니다.
	/// </summary>
	public class RewardPanelController : MonoBehaviour
	{
		[Inject] private IItemService _itemService;
		[Inject(Optional = true)] private IRewardGenerator _rewardGenerator; // 주입 시 자동 생성 지원

		[SerializeField] private bool _isOpen;
		[SerializeField] private ActiveItemDefinition[] _candidates;
		[SerializeField] private PassiveItemDefinition[] _passiveCandidates;
		private bool _hasTakenOnceWhenThreeOfFour;

		private void OnEnable()
		{
			_itemService.OnActiveItemAdded += HandleInventoryChanged;
			_itemService.OnActiveItemRemoved += HandleInventoryChangedSlot;
			_itemService.OnActiveItemUsed += HandleInventoryUsed;
		}

		private void OnDisable()
		{
			_itemService.OnActiveItemAdded -= HandleInventoryChanged;
			_itemService.OnActiveItemRemoved -= HandleInventoryChangedSlot;
			_itemService.OnActiveItemUsed -= HandleInventoryUsed;
		}

		public void Toggle()
		{
			_isOpen = !_isOpen;
			GameLogger.LogInfo($"[Reward] 패널 {(_isOpen ? "열림" : "닫힘")}", GameLogger.LogCategory.UI);
		}

		public void Open(ActiveItemDefinition[] candidates)
		{
			_candidates = candidates;
			_passiveCandidates = null;
			_isOpen = true;
			_hasTakenOnceWhenThreeOfFour = false;
			RefreshButtons();
			GameLogger.LogInfo($"[Reward] 보상 {(_candidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
		}

		public void OpenPassive(PassiveItemDefinition[] candidates)
		{
			_passiveCandidates = candidates;
			_candidates = null;
			_isOpen = true;
			GameLogger.LogInfo($"[Reward] 패시브 보상 {(_passiveCandidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
		}

		/// <summary>
		/// RewardGenerator를 사용해 보상 후보를 생성하여 패널을 엽니다.
		/// </summary>
		public void OpenGenerated(EnemyRewardConfig enemyCfg, PlayerRewardProfile playerProfile, RewardProfile profile, int stageIndex, int runSeed)
		{
			if (_rewardGenerator == null)
			{
				GameLogger.LogWarning("[Reward] RewardGenerator가 주입되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}
			var actives = _rewardGenerator.GenerateActive(enemyCfg, playerProfile, profile, stageIndex, runSeed);
			Open(actives);
		}

		public void OpenGeneratedPassive(EnemyRewardConfig enemyCfg, PlayerRewardProfile playerProfile, RewardProfile profile, int stageIndex, int runSeed)
		{
			if (_rewardGenerator == null)
			{
				GameLogger.LogWarning("[Reward] RewardGenerator가 주입되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}
			var passives = _rewardGenerator.GeneratePassive(enemyCfg, playerProfile, profile, stageIndex, runSeed);
			OpenPassive(passives);
		}

		public void OpenGeneratedCombined(EnemyRewardConfig enemyCfg, PlayerRewardProfile playerProfile, RewardProfile profile, int stageIndex, int runSeed)
		{
			if (_rewardGenerator == null)
			{
				GameLogger.LogWarning("[Reward] RewardGenerator가 주입되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}
			_candidates = _rewardGenerator.GenerateActive(enemyCfg, playerProfile, profile, stageIndex, runSeed);
			_passiveCandidates = _rewardGenerator.GeneratePassive(enemyCfg, playerProfile, profile, stageIndex, runSeed);
			_isOpen = true;
			_hasTakenOnceWhenThreeOfFour = false;
			RefreshButtons();
			GameLogger.LogInfo($"[Reward] 액티브 {(_candidates?.Length ?? 0)}개 + 패시브 {(_passiveCandidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
		}

		public void Close()
		{
			_isOpen = false;
		}

		/// <summary>
		/// [가져오기] 버튼 핸들러. 버튼에서 후보 인덱스를 전달.
		/// </summary>
		public void OnClickTake(int candidateIndex)
		{
			if (_candidates == null || candidateIndex < 0 || candidateIndex >= _candidates.Length)
			{
				GameLogger.LogWarning("[Reward] 잘못된 후보 인덱스", GameLogger.LogCategory.UI);
				return;
			}

			if (!CanTakeMore())
			{
				GameLogger.LogWarning("[Reward] 인벤토리 가득/제한으로 가져올 수 없음", GameLogger.LogCategory.UI);
				return;
			}

			var item = _candidates[candidateIndex];
			bool ok = _itemService.AddActiveItem(item);
			GameLogger.LogInfo($"[Reward] 가져오기 요청: {item.DisplayName} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
			if (ok)
			{
				// 3/4에서 첫 1회 가져오면 제한 걸기
				if (GetInventoryCount() == 4)
				{
					_hasTakenOnceWhenThreeOfFour = true;
				}
				RefreshButtons();
			}
		}

		private bool CanTakeMore()
		{
			if (_itemService.IsActiveInventoryFull()) return false; // 4/4 불가
			int count = GetInventoryCount();
			if (count == 3 && _hasTakenOnceWhenThreeOfFour) return false; // 3/4에서 1회만
			return true;
		}

		private int GetInventoryCount()
		{
			var slots = _itemService.GetActiveSlots();
			int c = 0;
			for (int i = 0; i < slots.Length; i++) if (!slots[i].isEmpty && slots[i].item != null) c++;
			return c;
		}

		private void RefreshButtons()
		{
			int count = GetInventoryCount();
			bool isFull = count >= 4;
			bool limited = (count == 3 && _hasTakenOnceWhenThreeOfFour);
			GameLogger.LogInfo($"[Reward] 버튼 상태 갱신: count={count}, full={isFull}, limited={limited}", GameLogger.LogCategory.UI);
			// 실제 UI 버튼 상호작용/회색처리는 여기서 연결
		}

		private void HandleInventoryChanged(ActiveItemDefinition def, int slot)
		{
			RefreshButtons();
		}

		private void HandleInventoryChangedSlot(int slot)
		{
			// 버리기 후에는 3/4 제한 해제 가능
			_hasTakenOnceWhenThreeOfFour = false;
			RefreshButtons();
		}

		private void HandleInventoryUsed(ActiveItemDefinition def, int slot)
		{
			RefreshButtons();
		}
	}
}
