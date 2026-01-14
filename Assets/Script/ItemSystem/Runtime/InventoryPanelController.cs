using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Zenject;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Runtime
{
	/// <summary>
	/// 액티브 인벤토리 4슬롯 UI와 상호작용을 중개합니다.
	/// 기존 슬롯 UI를 관리하고, 아이템 프리팹을 동적으로 생성/제거합니다.
	/// </summary>
	public class InventoryPanelController : MonoBehaviour, IPointerClickHandler
	{
		[Inject] private IItemService _itemService;
	[Inject(Optional = true)] private Game.ItemSystem.Manager.ItemTooltipManager itemTooltipManager;
	[Inject(Optional = true)] private Game.CombatSystem.Manager.TurnManager turnManager;

	[Header("인벤토리 슬롯")]
	[SerializeField] private Transform[] slotTransforms = new Transform[4];

	[Header("아이템 프리팹 설정")]
	[SerializeField] private ActiveItemUI itemPrefab;

	[Header("기본 설정")]
	[SerializeField] private Sprite emptySlotSprite;
	[SerializeField] private Color emptySlotColor = Color.gray;

	[Header("패널 UI 설정")]
	[Tooltip("인벤토리 패널 GameObject (접기/펼치기 대상)")]
	[SerializeField] private GameObject panelContent;
	
	[Tooltip("접기/펼치기 버튼")]
	[SerializeField] private UnityEngine.UI.Button toggleButton;

	// 동적으로 생성된 아이템들
	private ActiveItemUI[] itemUIs = new ActiveItemUI[4];

	// 패널 상태
	private bool isPanelOpen = true;

		private void Start()
		{
			InitializeInventory();
			SubscribeToTurnChanges();
		}

		private void Update()
		{
			// ESC 닫기 기능 제거 (요청)

			// 마우스 클릭으로 팝업 외부 클릭 감지
			if (Input.GetMouseButtonDown(0))
			{
				HandleGlobalClick();
			}
		}

		private void OnEnable()
		{
			_itemService.OnActiveItemAdded += HandleItemAdded;
			_itemService.OnActiveItemRemoved += HandleItemRemoved;
			_itemService.OnActiveItemUsed += HandleItemUsed;
			RefreshSlots();
		}

		private void OnDisable()
		{
			_itemService.OnActiveItemAdded -= HandleItemAdded;
			_itemService.OnActiveItemRemoved -= HandleItemRemoved;
			_itemService.OnActiveItemUsed -= HandleItemUsed;

			UnsubscribeFromTurnChanges();

			// 아이템 프리팹들 정리
			ClearAllItemPrefabs();
		}

		/// <summary>
		/// 인벤토리를 초기화합니다.
		/// </summary>
		private void InitializeInventory()
		{
			if (itemPrefab == null)
			{
				GameLogger.LogError("[Inventory] 아이템 프리팹이 할당되지 않았습니다!", GameLogger.LogCategory.UI);
				return;
			}

			// 슬롯 Transform 검증
			for (int i = 0; i < 4; i++)
			{
				if (slotTransforms[i] == null)
				{
					GameLogger.LogError($"[Inventory] 슬롯 Transform {i}이 할당되지 않았습니다!", GameLogger.LogCategory.UI);
				}
			}

			// 접기/펼치기 버튼 이벤트 연결
			if (toggleButton != null)
			{
				toggleButton.onClick.AddListener(TogglePanel);
			}
			else
			{
				GameLogger.LogWarning("[Inventory] toggleButton이 할당되지 않았습니다!", GameLogger.LogCategory.UI);
			}

			// 시작 시에는 아이템 프리팹을 생성하지 않음 (빈 슬롯 상태)
		}

		/// <summary>
		/// 아이템이 클릭되었을 때 호출됩니다.
		/// </summary>
		/// <param name="slotIndex">클릭된 슬롯 인덱스</param>
		private void OnItemClicked(int slotIndex)
		{
			var slots = _itemService.GetActiveSlots();
			if (slotIndex >= 0 && slotIndex < slots.Length)
			{
				var slot = slots[slotIndex];
				if (!slot.isEmpty && slot.item != null)
				{
					// 다른 아이템의 액션 팝업들만 닫기 (현재 클릭 슬롯은 유지)
					HideAllActionPopupsExcept(slotIndex);
					
					// 클릭된 아이템의 액션 팝업은 ActiveItemUI에서 자동으로 표시됨
				}
			}
			else
			{
				GameLogger.LogError($"[Inventory] 잘못된 슬롯 인덱스: {slotIndex}", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 슬롯 UI를 서비스 상태로부터 갱신합니다.
		/// </summary>
		public void RefreshSlots()
		{
			var slots = _itemService.GetActiveSlots();
			
			for (int i = 0; i < slots.Length && i < 4; i++)
			{
				var slot = slots[i];
				bool isEmpty = slot.isEmpty || slot.item == null;
				
				if (isEmpty)
				{
					// 슬롯이 비어있으면 아이템 프리팹 제거
					RemoveItemPrefab(i);
				}
				else
				{
					// 슬롯에 아이템이 있으면 아이템 프리팹 생성/업데이트
					CreateOrUpdateItemPrefab(i, slot.item);
				}
				
			}
		}

		/// <summary>
		/// 아이템 프리팹을 생성하거나 업데이트합니다.
		/// </summary>
		/// <param name="slotIndex">슬롯 인덱스</param>
		/// <param name="item">아이템 데이터</param>
		private void CreateOrUpdateItemPrefab(int slotIndex, ActiveItemDefinition item)
		{
			if (slotTransforms[slotIndex] == null)
			{
				GameLogger.LogError($"[Inventory] 슬롯 Transform {slotIndex}이 null입니다!", GameLogger.LogCategory.UI);
				return;
			}

			if (itemUIs[slotIndex] == null)
			{
				// 아이템 프리팹 생성
				var itemInstance = Instantiate(itemPrefab, slotTransforms[slotIndex]);
				
				// GameObject 이름을 아이템의 displayName으로 설정
				itemInstance.gameObject.name = item.DisplayName;
				
				itemInstance.SetSlotIndex(slotIndex);
				itemInstance.SetItem(item);
				
				// 클릭 이벤트 연결
				itemInstance.OnItemClicked += OnItemClicked;
				itemInstance.OnUseButtonClicked += OnClickUse;
				itemInstance.OnDiscardButtonClicked += OnClickDiscard;
				
				itemUIs[slotIndex] = itemInstance;
			}
			else
			{
				// 기존 아이템 프리팹 업데이트
				itemUIs[slotIndex].SetItem(item);
				
				// GameObject 이름도 업데이트
				itemUIs[slotIndex].gameObject.name = item.DisplayName;
			}
		}

		/// <summary>
		/// 아이템 프리팹을 제거합니다.
		/// </summary>
		/// <param name="slotIndex">슬롯 인덱스</param>
		private void RemoveItemPrefab(int slotIndex)
		{
			if (itemUIs[slotIndex] != null)
			{
				// 이벤트 해제
				itemUIs[slotIndex].OnItemClicked -= OnItemClicked;
				itemUIs[slotIndex].OnUseButtonClicked -= OnClickUse;
				itemUIs[slotIndex].OnDiscardButtonClicked -= OnClickDiscard;
				
				Destroy(itemUIs[slotIndex].gameObject);
				itemUIs[slotIndex] = null;
			}
		}

		/// <summary>
		/// 모든 아이템 프리팹을 정리합니다.
		/// </summary>
		public void ClearAllItemPrefabs()
		{
			for (int i = 0; i < itemUIs.Length; i++)
			{
				if (itemUIs[i] != null)
				{
					// 이벤트 해제
					itemUIs[i].OnItemClicked -= OnItemClicked;
					itemUIs[i].OnUseButtonClicked -= OnClickUse;
					itemUIs[i].OnDiscardButtonClicked -= OnClickDiscard;
					
					Destroy(itemUIs[i].gameObject);
					itemUIs[i] = null;
				}
			}
		}

		/// <summary>
		/// 모든 아이템의 액션 팝업들을 닫습니다.
		/// </summary>
		private void HideAllActionPopups()
		{
			for (int i = 0; i < itemUIs.Length; i++)
			{
				if (itemUIs[i] != null)
				{
					itemUIs[i].CloseActionPopupExternal();
				}
			}
		}

		/// <summary>
		/// 지정한 슬롯을 제외하고 모든 아이템의 액션 팝업들을 닫습니다.
		/// </summary>
		/// <param name="exceptIndex">닫지 않을 슬롯 인덱스</param>
		private void HideAllActionPopupsExcept(int exceptIndex)
		{
			for (int i = 0; i < itemUIs.Length; i++)
			{
				if (i == exceptIndex) continue;
				if (itemUIs[i] != null)
				{
					itemUIs[i].CloseActionPopupExternal();
				}
			}
		}

	// 한 프레임 동안 글로벌 닫기 억제 플래그
	private bool _suppressGlobalCloseThisFrame;
	
	/// <summary>
	/// 한 프레임 동안 글로벌 닫기 동작을 억제합니다.
	/// 팝업 전환 시 툴팁이 닫히지 않도록 보호하는 데 사용됩니다.
	/// </summary>
	public void SuppressGlobalCloseOneFrame()
	{
		_suppressGlobalCloseThisFrame = true;
		StartCoroutine(ClearSuppressFlagEndOfFrame());
	}

		private System.Collections.IEnumerator ClearSuppressFlagEndOfFrame()
		{
			yield return null;
			_suppressGlobalCloseThisFrame = false;
		}

		/// <summary>
		/// 모든 팝업과 아이템 툴팁을 즉시 닫습니다.
		/// 빈 공간 클릭 등 완전한 종료 시에만 사용하세요.
		/// </summary>
		public void CloseAllPopupsAndTooltip()
		{
			HideAllActionPopups();
			if (itemTooltipManager != null)
			{
				itemTooltipManager.UnpinTooltip();
				itemTooltipManager.ForceHideTooltip();
			}
		}

		/// <summary>
		/// 다른 슬롯으로 전환할 때 팝업만 닫고 툴팁은 유지합니다.
		/// 새로운 아이템의 툴팁/팝업이 다음 프레임에 열릴 준비를 합니다.
		/// </summary>
		public void CloseAllPopupsOnly()
		{
			HideAllActionPopups();
			// 툴팁은 닫지 않음 - 각 ActiveItemUI의 CloseActionPopup에서 조건부로 처리됨
		}

		/// <summary>
		/// 슬롯 클릭 시 아이템을 사용합니다.
		/// </summary>
		/// <param name="slotIndex">슬롯 인덱스</param>
		public void OnClickUse(int slotIndex)
		{
			bool ok = _itemService.UseActiveItem(slotIndex);
		}

		/// <summary>
		/// 슬롯 클릭 시 아이템을 버립니다.
		/// </summary>
		/// <param name="slotIndex">슬롯 인덱스</param>
		public void OnClickDiscard(int slotIndex)
		{
			bool ok = _itemService.RemoveActiveItem(slotIndex);
		}

		#region ItemService 이벤트 처리

		private void HandleItemAdded(ActiveItemDefinition def, int slotIndex)
		{
			RefreshSlots();
		}

		private void HandleItemRemoved(Game.ItemSystem.Data.ActiveItemDefinition item, int slotIndex)
		{
			RefreshSlots();
		}

		private void HandleItemUsed(ActiveItemDefinition def, int slotIndex)
		{
			if (def != null)
			{

			}
			else
			{

			}
			RefreshSlots();
		}

		/// <summary>
		/// 전역 클릭을 처리하여 팝업 외부 클릭 시 팝업을 닫습니다.
		/// </summary>
		private void HandleGlobalClick()
		{
			if (_suppressGlobalCloseThisFrame)
			{
				return;
			}
			// 현재 열린 팝업이 있는지 확인
			bool hasOpenPopup = false;
			for (int i = 0; i < itemUIs.Length; i++)
			{
				if (itemUIs[i] != null && itemUIs[i].HasOpenPopup())
				{
					hasOpenPopup = true;
					break;
				}
			}

			if (!hasOpenPopup)
			{
				return; // 열린 팝업이 없으면 아무것도 하지 않음
			}

			// 클릭된 오브젝트 확인
			var eventData = new UnityEngine.EventSystems.PointerEventData(UnityEngine.EventSystems.EventSystem.current);
			eventData.position = Input.mousePosition;

			var results = new System.Collections.Generic.List<UnityEngine.EventSystems.RaycastResult>();
			UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

			// 클릭된 오브젝트가 팝업이나 아이템 UI가 아닌 경우 팝업 닫기
			bool clickedOnPopupOrItem = false;
			foreach (var result in results)
			{
				var clickedObject = result.gameObject;
				
				// ActionPopupUI나 ActiveItemUI인지 확인
				if (clickedObject.GetComponent<ActionPopupUI>() != null ||
					clickedObject.GetComponent<ActiveItemUI>() != null ||
					clickedObject.GetComponentInParent<ActionPopupUI>() != null ||
					clickedObject.GetComponentInParent<ActiveItemUI>() != null)
				{
					clickedOnPopupOrItem = true;
					break;
				}
			}

			// 팝업이나 아이템 UI가 아닌 곳을 클릭한 경우 모든 팝업 닫기
			if (!clickedOnPopupOrItem)
			{
				HideAllActionPopups();
			}
		}

		/// <summary>
		/// 턴 변경 이벤트를 구독합니다.
		/// </summary>
		private void SubscribeToTurnChanges()
		{
			if (turnManager != null)
			{
				turnManager.OnTurnChanged += HandleTurnChanged;
			}
		}

		/// <summary>
		/// 턴 변경 이벤트 구독을 해제합니다.
		/// </summary>
		private void UnsubscribeFromTurnChanges()
		{
			if (turnManager != null)
			{
				turnManager.OnTurnChanged -= HandleTurnChanged;
			}
		}

		/// <summary>
		/// 턴이 변경되면 모든 팝업을 닫습니다.
		/// </summary>
		private void HandleTurnChanged(Game.CombatSystem.Manager.TurnManager.TurnType newTurn)
		{
			GameLogger.LogWarning($"[InventoryPanel] ⚠️ 턴 변경 감지 ({newTurn}) - 모든 팝업 강제 닫기", GameLogger.LogCategory.UI);
			HideAllActionPopups();
		}

		#endregion

		#region 외부 클릭 감지

		/// <summary>
		/// 포인터 클릭 이벤트를 처리합니다.
		/// </summary>
		/// <param name="eventData">포인터 이벤트 데이터</param>
		public void OnPointerClick(PointerEventData eventData)
		{
			// 클릭된 오브젝트가 인벤토리 패널 자체인지 확인
			if (eventData.pointerCurrentRaycast.gameObject == gameObject)
			{
				// 인벤토리 패널 자체를 클릭한 경우에만 액션 팝업들 닫기
				HideAllActionPopups();
			}
		}

		#endregion

		#region 패널 접기/펼치기

		/// <summary>
		/// 패널을 엽니다 (펼치기).
		/// </summary>
		public void OpenPanel()
		{
			if (panelContent == null)
			{
				GameLogger.LogError("[Inventory] panelContent가 설정되지 않았습니다!", GameLogger.LogCategory.UI);
				return;
			}

			isPanelOpen = true;
			panelContent.SetActive(true);
		}

		/// <summary>
		/// 패널을 닫습니다 (접기).
		/// </summary>
		public void ClosePanel()
		{
			if (panelContent == null)
			{
				GameLogger.LogError("[Inventory] panelContent가 설정되지 않았습니다!", GameLogger.LogCategory.UI);
				return;
			}

			isPanelOpen = false;
			panelContent.SetActive(false);

			// 패널을 닫을 때 모든 액션 팝업도 닫기
			HideAllActionPopups();
		}

		/// <summary>
		/// 패널을 토글합니다 (접기 ↔ 펼치기).
		/// </summary>
		public void TogglePanel()
		{
			if (isPanelOpen)
			{
				ClosePanel();
			}
			else
			{
				OpenPanel();
			}
		}

		#endregion
	}
}

