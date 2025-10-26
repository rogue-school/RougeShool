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
	
	[Tooltip("접기/펼치기 버튼")]
	[SerializeField] private Button toggleButton;
	
	[Tooltip("패널 컨텐츠 (접기/펼치기 대상 - SlotContainer)")]
	[SerializeField] private GameObject panelContent;
	
	[Tooltip("닫기 버튼 (접기 시 숨김)")]
	[SerializeField] private GameObject closeButtonObject;

	[SerializeField] private bool _isOpen;
	[SerializeField] private bool _isContentVisible = true; // 컨텐츠 표시 여부
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
			// _itemService가 null이면 이벤트 구독 건너뛰기
			if (_itemService != null)
			{
				_itemService.OnActiveItemAdded += HandleInventoryChanged;
				_itemService.OnActiveItemRemoved += HandleInventoryChangedSlot;
				_itemService.OnActiveItemUsed += HandleInventoryUsed;
			}
			else
			{
				GameLogger.LogWarning("[RewardPanel] _itemService가 null입니다. 이벤트 구독을 건너뜁니다.", GameLogger.LogCategory.UI);
			}
		}

		private void OnDisable()
		{
			// _itemService가 null이면 이벤트 해제 건너뛰기
			if (_itemService != null)
			{
				_itemService.OnActiveItemAdded -= HandleInventoryChanged;
				_itemService.OnActiveItemRemoved -= HandleInventoryChangedSlot;
				_itemService.OnActiveItemUsed -= HandleInventoryUsed;
			}
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
		
		// 접기/펼치기 버튼 이벤트 연결 및 루트 밖으로 이동
		if (toggleButton != null)
		{
			toggleButton.onClick.AddListener(ToggleContent);
			
			// 패널이 닫혀도 토글 버튼은 보이도록 부모를 캔버스로 변경
			var canvas = GetComponentInParent<Canvas>();
			if (canvas != null)
			{
				// ToggleButton을 캔버스의 직접 자식으로 이동
				toggleButton.transform.SetParent(canvas.transform, true);
				GameLogger.LogInfo("[RewardPanel] 토글 버튼을 캔버스 루트로 이동 완료", GameLogger.LogCategory.UI);
			}
			
			GameLogger.LogInfo("[RewardPanel] 접기/펼치기 버튼 이벤트 연결 완료", GameLogger.LogCategory.UI);
		}
		else
		{
			GameLogger.LogWarning("[RewardPanel] toggleButton이 할당되지 않았습니다!", GameLogger.LogCategory.UI);
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
		
		// 토글 버튼도 함께 활성화
		if (toggleButton != null)
		{
			toggleButton.gameObject.SetActive(true);
			GameLogger.LogInfo("[RewardPanel] 토글 버튼 활성화", GameLogger.LogCategory.UI);
		}
		
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
		
		// 토글 버튼도 함께 비활성화
		if (toggleButton != null)
		{
			toggleButton.gameObject.SetActive(false);
			GameLogger.LogInfo("[RewardPanel] 토글 버튼 비활성화", GameLogger.LogCategory.UI);
		}
		
		// 보상 패널 완료 이벤트 발생
		OnRewardPanelClosed?.Invoke();
		GameLogger.LogInfo($"[RewardPanel] 패널 닫기 완료 - IsOpen: {_isOpen}, GameObject.activeSelf: {gameObject.activeSelf}", GameLogger.LogCategory.UI);
	}
	
	/// <summary>
	/// 패널 컨텐츠를 접기/펼치기합니다.
	/// 패널은 유지하되 컨텐츠만 숨겨서 인벤토리 사용 가능하게 합니다.
	/// </summary>
	public void ToggleContent()
	{
		_isContentVisible = !_isContentVisible;
		
		// SlotContainer 숨기기/보이기
		if (panelContent != null)
		{
			panelContent.SetActive(_isContentVisible);
		}
		else
		{
			GameLogger.LogWarning("[RewardPanel] panelContent가 할당되지 않았습니다!", GameLogger.LogCategory.UI);
		}
		
		// 닫기 버튼도 함께 숨기기/보이기
		if (closeButtonObject != null)
		{
			closeButtonObject.SetActive(_isContentVisible);
		}
		
		// Title도 함께 숨기기/보이기 (선택)
		if (backgroundPanel != null && backgroundPanel.transform.Find("Title") != null)
		{
			var titleObject = backgroundPanel.transform.Find("Title").gameObject;
			if (titleObject != null)
			{
				titleObject.SetActive(_isContentVisible);
			}
		}
		
		GameLogger.LogInfo($"[RewardPanel] 컨텐츠 {(_isContentVisible ? "펼침" : "접힘")}", GameLogger.LogCategory.UI);
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
			
			// _itemService가 null이면 아이템 추가 실패
			if (_itemService == null)
			{
				GameLogger.LogError($"[Reward] _itemService가 null입니다. 아이템 '{item.DisplayName}' 추가 실패", GameLogger.LogCategory.UI);
				return;
			}

			bool ok = _itemService.AddActiveItem(item);
			GameLogger.LogInfo($"[Reward] 가져오기 요청: {item.DisplayName} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
			if (ok)
			{
				// 아이템을 성공적으로 가져간 경우 해당 슬롯 제거
				RemoveSlot(candidateIndex);
				
				// 3/4에서 첫 1회 가져오면 제한 걸기
				if (GetInventoryCount() == 4)
				{
					_hasTakenOnceWhenThreeOfFour = true;
				}
				UpdateSlotStates();
			}
		}

		/// <summary>
		/// 특정 인덱스의 슬롯을 제거합니다.
		/// </summary>
		/// <param name="slotIndex">제거할 슬롯 인덱스</param>
		private void RemoveSlot(int slotIndex)
		{
			if (activeSlots == null || slotIndex < 0 || slotIndex >= activeSlots.Count)
			{
				GameLogger.LogWarning($"[Reward] 잘못된 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.UI);
				return;
			}

			var slotToRemove = activeSlots[slotIndex];
			if (slotToRemove != null)
			{
				// 슬롯 UI 제거
				Destroy(slotToRemove.gameObject);
				GameLogger.LogInfo($"[Reward] 슬롯 {slotIndex} 제거됨", GameLogger.LogCategory.UI);
			}

			// 배열에서 해당 슬롯 제거
			activeSlots.RemoveAt(slotIndex);
			
			// 후보 배열에서도 제거
			if (_candidates != null && slotIndex < _candidates.Length)
			{
				var newCandidates = new ActiveItemDefinition[_candidates.Length - 1];
				for (int i = 0, j = 0; i < _candidates.Length; i++)
				{
					if (i != slotIndex)
					{
						newCandidates[j++] = _candidates[i];
					}
				}
				_candidates = newCandidates;
			}

			// 남은 슬롯들의 인덱스 재설정
			for (int i = 0; i < activeSlots.Count; i++)
			{
				if (activeSlots[i] != null)
				{
					activeSlots[i].SetSlotIndex(i);
				}
			}

			GameLogger.LogInfo($"[Reward] 슬롯 {slotIndex} 완전히 제거됨 (남은 슬롯 인덱스 재설정 완료)", GameLogger.LogCategory.UI);
		}

		private bool CanTakeMore()
		{
			// _itemService가 null이면 기본적으로 true 반환
			if (_itemService == null)
			{
				GameLogger.LogWarning("[RewardPanel] _itemService가 null입니다. 아이템 선택을 허용합니다.", GameLogger.LogCategory.UI);
				return true;
			}

			if (_itemService.IsActiveInventoryFull()) return false; // 4/4 불가
			int count = GetInventoryCount();
			if (count == 3 && _hasTakenOnceWhenThreeOfFour) return false; // 3/4에서 1회만
			return true;
		}

		private int GetInventoryCount()
		{
			// _itemService가 null이면 0 반환
			if (_itemService == null)
			{
				return 0;
			}

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
			GameLogger.LogInfo($"[RewardPanel] 인벤토리 변경 감지 - 추가: {def?.DisplayName ?? "null"} @ 슬롯 {slot}", GameLogger.LogCategory.UI);
			UpdateSlotStates();
		}

		private void HandleInventoryChangedSlot(int slot)
		{
			// 버리기 후에는 3/4 제한 해제 가능
			GameLogger.LogInfo($"[RewardPanel] 인벤토리 변경 감지 - 제거: 슬롯 {slot}", GameLogger.LogCategory.UI);
			_hasTakenOnceWhenThreeOfFour = false;
			UpdateSlotStates();
		}

		private void HandleInventoryUsed(ActiveItemDefinition def, int slot)
		{
			GameLogger.LogInfo($"[RewardPanel] 인벤토리 변경 감지 - 사용: {def?.DisplayName ?? "null"} @ 슬롯 {slot}", GameLogger.LogCategory.UI);
			UpdateSlotStates();
		}

		/// <summary>
		/// 슬롯 상태를 업데이트합니다 (버튼 활성화/비활성화).
		/// </summary>
	private void UpdateSlotStates()
	{
		// GameObject가 비활성화 상태일 때만 건너뜀
		// 패널이 닫혀도(_isOpen = false) 슬롯 상태는 업데이트됨 (토글 버튼으로 다시 열 수 있으므로)
		if (!gameObject.activeSelf)
		{
			GameLogger.LogInfo("[RewardPanel] GameObject가 비활성화 상태 - 슬롯 상태 업데이트 건너뜀", GameLogger.LogCategory.UI);
			return;
		}
		
		bool canTakeMore = CanTakeMore();
		int inventoryCount = GetInventoryCount();
		
		GameLogger.LogInfo($"[RewardPanel] 슬롯 상태 업데이트: canTakeMore={canTakeMore}, inventoryCount={inventoryCount}, activeSlots.Count={activeSlots.Count}, _isOpen={_isOpen}", GameLogger.LogCategory.UI);
		
		foreach (var slot in activeSlots)
		{
			if (slot != null)
			{
				slot.SetInteractable(canTakeMore);
			}
		}
	}

		/// <summary>
		/// 설정이 없을 때 사용할 기본 액티브 보상을 생성합니다.
		/// </summary>
		private void OpenDefaultReward()
		{
			// DefaultRewardService를 사용하여 중복 코드 제거
			var rewards = Game.ItemSystem.Service.DefaultRewardService.GenerateDefaultActiveReward();
			Open(rewards);
		}

		/// <summary>
		/// 설정이 없을 때 사용할 기본 패시브 보상을 생성합니다.
		/// </summary>
		private void OpenDefaultPassiveReward()
		{
			// DefaultRewardService를 사용하여 중복 코드 제거
			var rewards = Game.ItemSystem.Service.DefaultRewardService.GenerateDefaultPassiveReward();
			OpenPassive(rewards);
		}
	}
}
