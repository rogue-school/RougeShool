using UnityEngine;
using UnityEngine.UI;
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
	public class InventoryPanelController : MonoBehaviour
	{
		[Inject] private IItemService _itemService;

	[Header("인벤토리 슬롯")]
	[SerializeField] private Transform[] slotTransforms = new Transform[4];

	[Header("아이템 프리팹 설정")]
	[SerializeField] private ActiveItemUI itemPrefab;

	[Header("기본 설정")]
	[SerializeField] private Sprite emptySlotSprite;
	[SerializeField] private Color emptySlotColor = Color.gray;

	// 동적으로 생성된 아이템들
	private ActiveItemUI[] itemUIs = new ActiveItemUI[4];

		private void Start()
		{
			InitializeInventory();
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

			// 시작 시에는 아이템 프리팹을 생성하지 않음 (빈 슬롯 상태)
			GameLogger.LogInfo("[Inventory] 인벤토리 초기화 완료 (빈 슬롯 상태)", GameLogger.LogCategory.UI);
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
					// 아이템 사용
					OnClickUse(slotIndex);
				}
				else
				{
					GameLogger.LogInfo($"[Inventory] 빈 슬롯 클릭: {slotIndex}", GameLogger.LogCategory.UI);
				}
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
				
				string itemName = isEmpty ? "[빈 슬롯]" : slot.item.DisplayName;
				GameLogger.LogInfo($"[Inventory] 슬롯 {i}: {itemName}", GameLogger.LogCategory.UI);
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
				
				itemUIs[slotIndex] = itemInstance;
				GameLogger.LogInfo($"[Inventory] 아이템 프리팹 생성: {item.DisplayName} @ 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
			}
			else
			{
				// 기존 아이템 프리팹 업데이트
				itemUIs[slotIndex].SetItem(item);
				
				// GameObject 이름도 업데이트
				itemUIs[slotIndex].gameObject.name = item.DisplayName;
				
				GameLogger.LogInfo($"[Inventory] 아이템 프리팹 업데이트: {item.DisplayName} @ 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
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
				itemUIs[slotIndex].OnItemClicked -= OnItemClicked;
				Destroy(itemUIs[slotIndex].gameObject);
				itemUIs[slotIndex] = null;
				GameLogger.LogInfo($"[Inventory] 아이템 프리팹 제거: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 모든 아이템 프리팹을 정리합니다.
		/// </summary>
		private void ClearAllItemPrefabs()
		{
			for (int i = 0; i < itemUIs.Length; i++)
			{
				if (itemUIs[i] != null)
				{
					itemUIs[i].OnItemClicked -= OnItemClicked;
					Destroy(itemUIs[i].gameObject);
					itemUIs[i] = null;
				}
			}
		}

		/// <summary>
		/// 슬롯 클릭 시 아이템을 사용합니다.
		/// </summary>
		/// <param name="slotIndex">슬롯 인덱스</param>
		public void OnClickUse(int slotIndex)
		{
			bool ok = _itemService.UseActiveItem(slotIndex);
			GameLogger.LogInfo($"[Inventory] 사용 요청: 슬롯 {slotIndex} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
		}

		/// <summary>
		/// 슬롯 클릭 시 아이템을 버립니다.
		/// </summary>
		/// <param name="slotIndex">슬롯 인덱스</param>
		public void OnClickDiscard(int slotIndex)
		{
			bool ok = _itemService.RemoveActiveItem(slotIndex);
			GameLogger.LogInfo($"[Inventory] 버리기 요청: 슬롯 {slotIndex} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
		}

		#region ItemService 이벤트 처리

		private void HandleItemAdded(ActiveItemDefinition def, int slotIndex)
		{
			GameLogger.LogInfo($"[Inventory] 추가 이벤트: {def.DisplayName} @ {slotIndex}", GameLogger.LogCategory.UI);
			RefreshSlots();
		}

		private void HandleItemRemoved(int slotIndex)
		{
			GameLogger.LogInfo($"[Inventory] 제거 이벤트: 슬롯 {slotIndex}", GameLogger.LogCategory.UI);
			RefreshSlots();
		}

		private void HandleItemUsed(ActiveItemDefinition def, int slotIndex)
		{
			GameLogger.LogInfo($"[Inventory] 사용 이벤트: {def.DisplayName} @ {slotIndex}", GameLogger.LogCategory.UI);
			RefreshSlots();
		}

		#endregion
	}
}

