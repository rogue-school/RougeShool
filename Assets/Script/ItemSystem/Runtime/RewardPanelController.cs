using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
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

		[Tooltip("인벤토리 가득 찼을 때 표시할 메시지 텍스트")]
		[SerializeField] private TMPro.TextMeshProUGUI inventoryFullMessage;

		[Tooltip("메시지 표시 지속 시간(초)")]
		[SerializeField] private float messageDisplayDuration = 2.0f;

		[SerializeField] private bool _isOpen;
		[SerializeField] private bool _isContentVisible = true; // 컨텐츠 표시 여부
		[SerializeField] private ActiveItemDefinition[] _candidates;
		[SerializeField] private PassiveItemDefinition[] _passiveCandidates;

		// UI 슬롯 관리
		private List<RewardSlotUIController> activeSlots = new List<RewardSlotUIController>();
		private List<RewardSlotUIController> passiveSlots = new List<RewardSlotUIController>();

		/// <summary>
		/// 보상 패널이 완전히 닫혔을 때 발생하는 이벤트
		/// </summary>
		public event System.Action OnRewardPanelClosed;

		private void OnEnable()
		{
			GameLogger.LogInfo($"[RewardPanel] OnEnable() 호출 - GameObject.activeSelf: {gameObject.activeSelf}, activeInHierarchy: {gameObject.activeInHierarchy}", GameLogger.LogCategory.UI);

			// 메시지 초기화 (활성화될 때 메시지 숨기기)
			if (inventoryFullMessage != null)
			{
				inventoryFullMessage.gameObject.SetActive(false);
			}

			// _itemService가 null이면 이벤트 구독 건너뛰기
			if (_itemService != null)
			{
				_itemService.OnActiveItemAdded += HandleInventoryChanged;
				_itemService.OnActiveItemRemoved += HandleInventoryChangedSlot;
				_itemService.OnActiveItemUsed += HandleInventoryUsed;
				GameLogger.LogInfo("[RewardPanel] 이벤트 구독 완료: OnActiveItemAdded, OnActiveItemRemoved, OnActiveItemUsed", GameLogger.LogCategory.UI);
			}
			else
			{
				GameLogger.LogWarning("[RewardPanel] _itemService가 null입니다. 이벤트 구독을 건너뜁니다.", GameLogger.LogCategory.UI);
			}
		}

		private void OnDisable()
		{
			GameLogger.LogInfo("[RewardPanel] OnDisable() 호출 - 이벤트 구독 해제", GameLogger.LogCategory.UI);

			// 실행 중인 모든 코루틴 중지
			StopAllCoroutines();

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

			// 메시지 숨기기
			HideInventoryFullMessage();

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

			// 메시지 숨기기
			HideInventoryFullMessage();

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

			// 메시지 숨기기
			HideInventoryFullMessage();

			// 액티브 + 패시브 동시 표시
			CreateItemSlots(_candidates);
			CreatePassiveSlots(_passiveCandidates);

			// Is Open 상태에 따라 GameObject 활성화 관리
			_isOpen = true;
			gameObject.SetActive(true);

			GameLogger.LogInfo($"[RewardPanel] 패널 열기 완료 - IsOpen: {_isOpen}, GameObject.activeSelf: {gameObject.activeSelf}", GameLogger.LogCategory.UI);
			GameLogger.LogInfo($"[Reward] 액티브 {(_candidates?.Length ?? 0)}개 + 패시브 {(_passiveCandidates?.Length ?? 0)}개 표시", GameLogger.LogCategory.UI);
		}

		public void Close()
		{
			// 패시브 아이템이 남아있으면 경고 없이 모두 자동 수령
			if (HasRemainingPassiveItems())
			{
				SelectAllRemainingPassiveItems();
			}

			// Is Open 상태에 따라 GameObject 비활성화 관리
			_isOpen = false;
			gameObject.SetActive(false);
			ClearAllSlots();

			// 메시지 숨기기
			HideInventoryFullMessage();

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
		/// 주의: panelContent만 SetActive(false)를 하고 GameObject 자체는 활성화 상태를 유지합니다.
		/// </summary>
		public void ToggleContent()
		{
			_isContentVisible = !_isContentVisible;

			// 메시지 숨기기 (접기/펼치기할 때 메시지 초기화)
			HideInventoryFullMessage();

			// SlotContainer 숨기기/보이기 (GameObject 자체는 그대로 두고)
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

			var item = _candidates[candidateIndex];

			// _itemService가 null이면 아이템 추가 실패
			if (_itemService == null)
			{
				GameLogger.LogError($"[Reward] _itemService가 null입니다. 아이템 '{item.DisplayName}' 추가 실패", GameLogger.LogCategory.UI);
				return;
			}

			// 인벤토리가 가득 찼는지 확인
			if (_itemService.IsActiveInventoryFull())
			{
				GameLogger.LogWarning($"[Reward] 인벤토리가 가득 참 - {item.DisplayName} 가져올 수 없음", GameLogger.LogCategory.UI);
				// TODO: UI에 "인벤토리가 가득 찼습니다!" 텍스트 표시
				ShowInventoryFullMessage();
				return;
			}

			bool ok = _itemService.AddActiveItem(item);
			GameLogger.LogInfo($"[Reward] 가져오기 요청: {item.DisplayName} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
			if (ok)
			{
				// 아이템을 성공적으로 가져간 경우 해당 슬롯 제거
				RemoveSlot(candidateIndex);
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

		/// <summary>
		/// 인벤토리가 가득 찼을 때 메시지를 표시합니다.
		/// </summary>
		private void ShowInventoryFullMessage()
		{
			GameLogger.LogWarning("[RewardPanel] 인벤토리가 가득 찼습니다!", GameLogger.LogCategory.UI);

			// 메시지 텍스트가 할당되어 있으면 표시
			if (inventoryFullMessage != null)
			{
				inventoryFullMessage.text = "인벤토리가 가득 찼습니다!\n아이템을 버려주세요.";
				inventoryFullMessage.gameObject.SetActive(true);

				// 자동으로 사라지게 하기 위해 코루틴 시작
				StopCoroutine(HideMessageCoroutine());
				StartCoroutine(HideMessageCoroutine());

				GameLogger.LogInfo("[RewardPanel] 인벤토리 가득 참 메시지 표시", GameLogger.LogCategory.UI);
			}
			else
			{
				GameLogger.LogWarning("[RewardPanel] inventoryFullMessage가 할당되지 않았습니다!", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 메시지를 일정 시간 후 숨기는 코루틴
		/// </summary>
		private System.Collections.IEnumerator HideMessageCoroutine()
		{
			yield return new WaitForSeconds(messageDisplayDuration);

			if (inventoryFullMessage != null)
			{
				inventoryFullMessage.gameObject.SetActive(false);
				GameLogger.LogInfo("[RewardPanel] 인벤토리 가득 참 메시지 숨김", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 메시지를 즉시 숨깁니다.
		/// </summary>
		private void HideInventoryFullMessage()
		{
			// 실행 중인 코루틴 중지
			StopAllCoroutines();

			// 메시지 숨기기
			if (inventoryFullMessage != null)
			{
				inventoryFullMessage.gameObject.SetActive(false);
				GameLogger.LogInfo("[RewardPanel] 인벤토리 가득 참 메시지 강제 숨김", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 남아있는 패시브 아이템이 있는지 확인합니다.
		/// </summary>
		/// <returns>패시브 아이템이 남아있으면 true</returns>
		private bool HasRemainingPassiveItems()
		{
			// 패시브 슬롯이 남아있는지 확인
			if (passiveSlots != null && passiveSlots.Count > 0)
			{
				// 실제로 활성화된 슬롯이 있는지 확인
				foreach (var slot in passiveSlots)
				{
					if (slot != null && slot.gameObject != null && slot.gameObject.activeInHierarchy)
					{
						return true;
					}
				}
			}

			// 패시브 후보가 남아있는지 확인
			if (_passiveCandidates != null && _passiveCandidates.Length > 0)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// 패시브 아이템 선택 경고 메시지를 표시합니다.
		/// </summary>
		private void ShowPassiveItemWarning()
		{
			int remainingCount = GetRemainingPassiveItemCount();
			
			if (inventoryFullMessage != null)
			{
				inventoryFullMessage.text = $"패시브 아이템 {remainingCount}개를 선택해주세요!\n아이템을 클릭하여 선택할 수 있습니다.";
				inventoryFullMessage.gameObject.SetActive(true);

				// 자동으로 사라지게 하기 위해 코루틴 시작
				StopCoroutine(HideMessageCoroutine());
				StartCoroutine(HideMessageCoroutine());

				GameLogger.LogInfo($"[RewardPanel] 패시브 아이템 선택 경고 메시지 표시 (남은 개수: {remainingCount})", GameLogger.LogCategory.UI);
			}
			else
			{
				GameLogger.LogWarning("[RewardPanel] inventoryFullMessage가 할당되지 않았습니다!", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 남아있는 패시브 아이템 개수를 반환합니다.
		/// </summary>
		/// <returns>남아있는 패시브 아이템 개수</returns>
		private int GetRemainingPassiveItemCount()
		{
			int count = 0;

			// 활성화된 패시브 슬롯 개수 확인
			if (passiveSlots != null)
			{
				foreach (var slot in passiveSlots)
				{
					if (slot != null && slot.gameObject != null && slot.gameObject.activeInHierarchy)
					{
						count++;
					}
				}
			}

			// 슬롯이 없으면 후보 배열 개수 확인
			if (count == 0 && _passiveCandidates != null)
			{
				count = _passiveCandidates.Length;
			}

			return count;
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

			foreach (var slot in passiveSlots)
			{
				if (slot != null)
				{
					slot.OnPassiveSlotSelected -= OnPassiveSlotSelected;
					Destroy(slot.gameObject);
				}
			}
			passiveSlots.Clear();
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
			// 인벤토리 변경 시 로그만 출력
			GameLogger.LogInfo($"[RewardPanel] 인벤토리 변경 감지 - 추가: {def?.DisplayName ?? "null"} @ 슬롯 {slot}", GameLogger.LogCategory.UI);
		}

		private void HandleInventoryChangedSlot(Game.ItemSystem.Data.ActiveItemDefinition item, int slot)
		{
			// 인벤토리 변경 시 로그만 출력
			GameLogger.LogInfo($"[RewardPanel] 인벤토리 변경 감지 - 제거: {item?.DisplayName ?? "Unknown"} (슬롯 {slot})", GameLogger.LogCategory.UI);
		}

		private void HandleInventoryUsed(ActiveItemDefinition def, int slot)
		{
			// 인벤토리 변경 시 로그만 출력
			GameLogger.LogInfo($"[RewardPanel] 인벤토리 변경 감지 - 사용: {def?.DisplayName ?? "null"} @ 슬롯 {slot}", GameLogger.LogCategory.UI);
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

		private void RemovePassiveSlot(int slotIndex)
		{
			if (passiveSlots == null || slotIndex < 0 || slotIndex >= passiveSlots.Count)
			{
				GameLogger.LogWarning($"[Reward] 잘못된 패시브 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.UI);
				return;
			}

			var slotToRemove = passiveSlots[slotIndex];
			if (slotToRemove != null)
			{
				slotToRemove.OnPassiveSlotSelected -= OnPassiveSlotSelected;
				Destroy(slotToRemove.gameObject);
				GameLogger.LogInfo($"[Reward] 패시브 슬롯 {slotIndex} 제거됨", GameLogger.LogCategory.UI);
			}

			passiveSlots.RemoveAt(slotIndex);

			if (_passiveCandidates != null && slotIndex < _passiveCandidates.Length)
			{
				var newCandidates = new PassiveItemDefinition[_passiveCandidates.Length - 1];
				for (int i = 0, j = 0; i < _passiveCandidates.Length; i++)
				{
					if (i != slotIndex)
					{
						newCandidates[j++] = _passiveCandidates[i];
					}
				}
				_passiveCandidates = newCandidates;
			}

			for (int i = 0; i < passiveSlots.Count; i++)
			{
				if (passiveSlots[i] != null)
				{
					passiveSlots[i].SetSlotIndex(i);
				}
			}
		}

		private void CreatePassiveSlots(PassiveItemDefinition[] candidates)
		{
			if (candidates == null || candidates.Length == 0) return;
			if (itemSlotPrefab == null || itemSlotsContainer == null)
			{
				GameLogger.LogError("[RewardPanel] 패시브 슬롯 생성 실패 - 프리팹 또는 컨테이너 누락", GameLogger.LogCategory.UI);
				return;
			}

			for (int i = 0; i < candidates.Length; i++)
			{
				var slotObject = Instantiate(itemSlotPrefab, itemSlotsContainer);
				var slotController = slotObject.GetComponent<RewardSlotUIController>();
				if (slotController != null)
				{
					slotController.SetupSlot(candidates[i], i);
					slotController.OnPassiveSlotSelected += OnPassiveSlotSelected;
					passiveSlots.Add(slotController);
				}
				else
				{
					GameLogger.LogError($"[RewardPanel] 패시브 슬롯 컨트롤러를 찾을 수 없습니다: {candidates[i].DisplayName}", GameLogger.LogCategory.UI);
					Destroy(slotObject);
				}
			}
		}

		private void OnPassiveSlotSelected(PassiveItemDefinition selectedItem, int slotIndex)
		{
			if (selectedItem == null)
			{
				GameLogger.LogWarning("[RewardPanel] 선택된 패시브 아이템이 null입니다", GameLogger.LogCategory.UI);
				return;
			}

			if (_itemService == null)
			{
				GameLogger.LogError($"[Reward] _itemService가 null입니다. 패시브 '{selectedItem.DisplayName}' 추가 실패", GameLogger.LogCategory.UI);
				return;
			}

			_itemService.AddPassiveItem(selectedItem);
			
			// 툴팁 매니저에 툴팁 업데이트 요청
			var tooltipManager = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Manager.ItemTooltipManager>();
			if (tooltipManager != null)
			{
				tooltipManager.RefreshPassiveItemTooltip(selectedItem);
			}
			
			RemovePassiveSlot(slotIndex);

			// 패시브 아이템을 선택했으므로 경고 관련 상태는 더 이상 사용하지 않음
		}

		/// <summary>
		/// 남아있는 모든 패시브 아이템을 자동으로 선택합니다.
		/// </summary>
		private void SelectAllRemainingPassiveItems()
		{
			if (_itemService == null)
			{
				GameLogger.LogError("[RewardPanel] _itemService가 null입니다. 패시브 아이템 자동 선택 실패", GameLogger.LogCategory.UI);
				return;
			}

			// 활성화된 패시브 슬롯들을 역순으로 처리 (인덱스 변경 방지)
			var slotsToProcess = new List<RewardSlotUIController>();
			foreach (var slot in passiveSlots)
			{
				if (slot != null && slot.gameObject != null && slot.gameObject.activeInHierarchy)
				{
					var passiveItem = slot.GetCurrentPassive();
					if (passiveItem != null)
					{
						slotsToProcess.Add(slot);
					}
				}
			}

			// 역순으로 처리하여 인덱스 문제 방지
			for (int i = slotsToProcess.Count - 1; i >= 0; i--)
			{
				var slot = slotsToProcess[i];
				var passiveItem = slot.GetCurrentPassive();
				if (passiveItem != null)
				{
					_itemService.AddPassiveItem(passiveItem);
					
					// 툴팁 매니저에 툴팁 업데이트 요청
					var tooltipManager = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Manager.ItemTooltipManager>();
					if (tooltipManager != null)
					{
						tooltipManager.RefreshPassiveItemTooltip(passiveItem);
					}

					// 슬롯 인덱스 찾기
					int slotIndex = passiveSlots.IndexOf(slot);
					if (slotIndex >= 0)
					{
						RemovePassiveSlot(slotIndex);
					}

					GameLogger.LogInfo($"[RewardPanel] 패시브 아이템 자동 선택: {passiveItem.DisplayName}", GameLogger.LogCategory.UI);
				}
			}

			// 후보 배열에서도 처리 (슬롯이 없는 경우)
			if (_passiveCandidates != null && _passiveCandidates.Length > 0)
			{
				foreach (var passiveItem in _passiveCandidates)
				{
					if (passiveItem != null)
					{
						_itemService.AddPassiveItem(passiveItem);
						
						// 툴팁 매니저에 툴팁 업데이트 요청
						var tooltipManager = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Manager.ItemTooltipManager>();
						if (tooltipManager != null)
						{
							tooltipManager.RefreshPassiveItemTooltip(passiveItem);
						}

						GameLogger.LogInfo($"[RewardPanel] 패시브 아이템 자동 선택 (후보 배열): {passiveItem.DisplayName}", GameLogger.LogCategory.UI);
					}
				}
				_passiveCandidates = null;
			}

			GameLogger.LogInfo($"[RewardPanel] 모든 패시브 아이템 자동 선택 완료 (총 {slotsToProcess.Count}개)", GameLogger.LogCategory.UI);
		}
	}
}