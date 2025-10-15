using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using System.Linq;
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

		[Header("UI 구성 요소")]
		[SerializeField] private GameObject itemSlotPrefab;
		[SerializeField] private Transform itemSlotsContainer;
		[SerializeField] private Button closeButton;
		[SerializeField] private GameObject backgroundPanel;

	[SerializeField] private bool _isOpen;
	[SerializeField] private ActiveItemDefinition[] _candidates;
	[SerializeField] private PassiveItemDefinition[] _passiveCandidates;
	private bool _hasTakenOnceWhenThreeOfFour;

	// UI 슬롯 관리
	private List<RewardSlotUIController> activeSlots = new List<RewardSlotUIController>();

	/// <summary>
	/// 보상 패널이 완전히 닫혔을 때 발생하는 이벤트
	/// </summary>
	public event System.Action OnRewardPanelClosed;

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

	private void Start()
	{
		GameLogger.LogInfo($"[RewardPanel] Start() - GameObject.activeSelf: {gameObject.activeSelf}, activeInHierarchy: {gameObject.activeInHierarchy}", GameLogger.LogCategory.UI);
		InitializeUI();
	}

		private void OnDestroy()
		{
			ClearAllSlots();
		}

		/// <summary>
		/// UI를 초기화합니다.
		/// </summary>
		private void InitializeUI()
		{
			// 닫기 버튼 이벤트 연결
			if (closeButton != null)
			{
				closeButton.onClick.AddListener(Close);
			}

			GameLogger.LogInfo("[RewardPanel] 보상 패널 UI 초기화 완료", GameLogger.LogCategory.UI);
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
			
			// UI 슬롯 생성
			CreateItemSlots(candidates);
			
			// 패널 활성화
			gameObject.SetActive(true);
			
			GameLogger.LogInfo($"[Reward] 보상 {(_candidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
		}

		public void OpenPassive(PassiveItemDefinition[] candidates)
		{
			_passiveCandidates = candidates;
			_candidates = null;
			
			// Is Open 상태에 따라 GameObject 활성화 관리
			_isOpen = true;
			gameObject.SetActive(true);
			
			// 패시브 아이템은 현재 UI 미지원 (추후 확장 가능)
			GameLogger.LogInfo($"[RewardPanel] 패시브 패널 열기 완료 - IsOpen: {_isOpen}, GameObject.activeSelf: {gameObject.activeSelf}", GameLogger.LogCategory.UI);
			GameLogger.LogInfo($"[Reward] 패시브 보상 {(_passiveCandidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
		}

	/// <summary>
	/// RewardGenerator를 사용해 보상 후보를 생성하여 패널을 엽니다.
	/// 설정이 없어도 기본값으로 작동합니다.
	/// </summary>
	public void OpenGenerated(EnemyRewardConfig enemyCfg, PlayerRewardProfile playerProfile, int stageIndex, int runSeed)
	{
		if (_rewardGenerator == null)
		{
			GameLogger.LogWarning("[Reward] RewardGenerator가 주입되지 않았습니다. 기본 보상을 생성합니다.", GameLogger.LogCategory.UI);
			OpenDefaultReward();
			return;
		}
		
		var actives = _rewardGenerator.GenerateActive(enemyCfg, playerProfile, stageIndex, runSeed);
		Open(actives);
	}

	public void OpenGeneratedPassive(EnemyRewardConfig enemyCfg, PlayerRewardProfile playerProfile, int stageIndex, int runSeed)
	{
		if (_rewardGenerator == null)
		{
			GameLogger.LogWarning("[Reward] RewardGenerator가 주입되지 않았습니다. 기본 패시브 보상을 생성합니다.", GameLogger.LogCategory.UI);
			OpenDefaultPassiveReward();
			return;
		}
		
		var passives = _rewardGenerator.GeneratePassive(enemyCfg, playerProfile, stageIndex, runSeed);
		OpenPassive(passives);
	}

	public void OpenGeneratedCombined(EnemyRewardConfig enemyCfg, PlayerRewardProfile playerProfile, int stageIndex, int runSeed)
	{
		if (_rewardGenerator == null)
		{
			GameLogger.LogWarning("[Reward] RewardGenerator가 주입되지 않았습니다. 기본 보상을 생성합니다.", GameLogger.LogCategory.UI);
			OpenDefaultReward();
			return;
		}
		
		_candidates = _rewardGenerator.GenerateActive(enemyCfg, playerProfile, stageIndex, runSeed);
		_passiveCandidates = _rewardGenerator.GeneratePassive(enemyCfg, playerProfile, stageIndex, runSeed);
		_isOpen = true;
		_hasTakenOnceWhenThreeOfFour = false;
		
		// 액티브 아이템만 UI로 표시 (패시브는 추후 확장)
		CreateItemSlots(_candidates);
		
		// Is Open 상태에 따라 GameObject 활성화 관리
		_isOpen = true;
		gameObject.SetActive(true);
		
		GameLogger.LogInfo($"[RewardPanel] 패널 열기 완료 - IsOpen: {_isOpen}, GameObject.activeSelf: {gameObject.activeSelf}", GameLogger.LogCategory.UI);
		GameLogger.LogInfo($"[Reward] 액티브 {(_candidates?.Length ?? 0)}개 + 패시브 {(_passiveCandidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
	}

	public void Close()
	{
		// Is Open 상태에 따라 GameObject 비활성화 관리
		_isOpen = false;
		gameObject.SetActive(false);
		ClearAllSlots();
		
		// 보상 패널 완료 이벤트 발생
		OnRewardPanelClosed?.Invoke();
		GameLogger.LogInfo($"[RewardPanel] 패널 닫기 완료 - IsOpen: {_isOpen}, GameObject.activeSelf: {gameObject.activeSelf}", GameLogger.LogCategory.UI);
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
				UpdateSlotStates();
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

		/// <summary>
		/// 아이템 슬롯들을 생성합니다.
		/// </summary>
		/// <param name="candidates">보상 후보 아이템들</param>
		private void CreateItemSlots(ActiveItemDefinition[] candidates)
		{
			if (candidates == null || candidates.Length == 0)
			{
				GameLogger.LogWarning("[RewardPanel] 보상 후보가 없습니다", GameLogger.LogCategory.UI);
				return;
			}

			if (itemSlotPrefab == null)
			{
				GameLogger.LogError("[RewardPanel] 아이템 슬롯 프리팹이 설정되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}

			if (itemSlotsContainer == null)
			{
				GameLogger.LogError("[RewardPanel] 아이템 슬롯 컨테이너가 설정되지 않았습니다", GameLogger.LogCategory.UI);
				return;
			}

			// 기존 슬롯들 정리
			ClearAllSlots();

			// 새 슬롯들 생성
			for (int i = 0; i < candidates.Length; i++)
			{
				var slotObject = Instantiate(itemSlotPrefab, itemSlotsContainer);
				var slotController = slotObject.GetComponent<RewardSlotUIController>();

				if (slotController != null)
				{
					// 슬롯 설정
					slotController.SetupSlot(candidates[i], i);
					slotController.OnSlotSelected += OnSlotSelected;

					activeSlots.Add(slotController);
				}
				else
				{
					GameLogger.LogError($"[RewardPanel] 슬롯 컨트롤러를 찾을 수 없습니다: {candidates[i].DisplayName}", GameLogger.LogCategory.UI);
					Destroy(slotObject);
				}
			}
		}

		/// <summary>
		/// 모든 슬롯을 정리합니다.
		/// </summary>
		private void ClearAllSlots()
		{
			foreach (var slot in activeSlots)
			{
				if (slot != null)
				{
					slot.OnSlotSelected -= OnSlotSelected;
					Destroy(slot.gameObject);
				}
			}

			activeSlots.Clear();
		}

		/// <summary>
		/// 슬롯이 선택되었을 때 호출됩니다.
		/// </summary>
		/// <param name="selectedItem">선택된 아이템</param>
		/// <param name="slotIndex">슬롯 인덱스</param>
		private void OnSlotSelected(ActiveItemDefinition selectedItem, int slotIndex)
		{
			if (selectedItem == null)
			{
				GameLogger.LogWarning("[RewardPanel] 선택된 아이템이 null입니다", GameLogger.LogCategory.UI);
				return;
			}

			// 기존 OnClickTake 로직 재사용
			OnClickTake(slotIndex);
		}

		private void HandleInventoryChanged(ActiveItemDefinition def, int slot)
		{
			// 인벤토리 변경 시 슬롯 상태 업데이트
			UpdateSlotStates();
		}

		private void HandleInventoryChangedSlot(int slot)
		{
			// 버리기 후에는 3/4 제한 해제 가능
			_hasTakenOnceWhenThreeOfFour = false;
			UpdateSlotStates();
		}

		private void HandleInventoryUsed(ActiveItemDefinition def, int slot)
		{
			UpdateSlotStates();
		}

		/// <summary>
		/// 슬롯 상태를 업데이트합니다 (버튼 활성화/비활성화).
		/// </summary>
		private void UpdateSlotStates()
		{
			bool canTakeMore = CanTakeMore();
			
			foreach (var slot in activeSlots)
			{
				if (slot != null)
				{
					slot.SetInteractable(canTakeMore);
				}
			}
			
			GameLogger.LogInfo($"[RewardPanel] 슬롯 상태 업데이트: canTakeMore={canTakeMore}", GameLogger.LogCategory.UI);
		}

		/// <summary>
		/// 설정이 없을 때 사용할 기본 액티브 보상을 생성합니다.
		/// </summary>
		private void OpenDefaultReward()
		{
			// Resources 폴더에서 모든 액티브 아이템을 찾아서 랜덤 선택
			var allActiveItems = Resources.LoadAll<ActiveItemDefinition>("Data/Item");
			
			if (allActiveItems.Length == 0)
			{
				GameLogger.LogError("[RewardPanel] Resources에서 액티브 아이템을 찾을 수 없습니다.", GameLogger.LogCategory.UI);
				return;
			}

			// 기본적으로 3개 아이템 제공
			var selectedCount = Math.Min(3, allActiveItems.Length);
			var selected = new List<ActiveItemDefinition>();
			
			for (int i = 0; i < selectedCount; i++)
			{
				var randomIndex = UnityEngine.Random.Range(0, allActiveItems.Length);
				selected.Add(allActiveItems[randomIndex]);
			}

			GameLogger.LogInfo($"[RewardPanel] 기본 액티브 보상 생성: {selected.Count}개", GameLogger.LogCategory.UI);
			Open(selected.ToArray());
		}

		/// <summary>
		/// 설정이 없을 때 사용할 기본 패시브 보상을 생성합니다.
		/// </summary>
		private void OpenDefaultPassiveReward()
		{
			// Resources 폴더에서 모든 패시브 아이템을 찾아서 랜덤 선택
			var allPassiveItems = Resources.LoadAll<PassiveItemDefinition>("Data/Item");
			
			if (allPassiveItems.Length == 0)
			{
				GameLogger.LogInfo("[RewardPanel] Resources에서 패시브 아이템을 찾을 수 없습니다.", GameLogger.LogCategory.UI);
				return;
			}

			// 기본적으로 1개 아이템 제공
			var randomIndex = UnityEngine.Random.Range(0, allPassiveItems.Length);
			var selected = new PassiveItemDefinition[] { allPassiveItems[randomIndex] };

			GameLogger.LogInfo($"[RewardPanel] 기본 패시브 보상 생성: {selected.Length}개", GameLogger.LogCategory.UI);
			OpenPassive(selected);
		}
	}
}
