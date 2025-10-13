using UnityEngine;
using Game.ItemSystem.Interface;
using Game.ItemSystem.Data;
using Zenject;
using Game.CoreSystem.Utility;

namespace Game.ItemSystem.Runtime
{
	/// <summary>
	/// 액티브 인벤토리 4슬롯 UI와 상호작용을 중개합니다.
	/// 슬롯 클릭 시 컨텍스트(사용/버리기/취소) 동작을 노출하고,
	/// ItemService 이벤트를 수신해 슬롯 표시를 갱신합니다.
	/// </summary>
	public class InventoryPanelController : MonoBehaviour
	{
		[Inject] private IItemService _itemService;

		// 컨텍스트 메뉴가 실제 UI로 구현될 때 바인딩 예정
		// 여기서는 로깅으로 동작을 대체합니다.

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
		}

		/// <summary>
		/// 슬롯 UI를 서비스 상태로부터 갱신합니다.
		/// </summary>
		public void RefreshSlots()
		{
			var slots = _itemService.GetActiveSlots();
			for (int i = 0; i < slots.Length; i++)
			{
				var s = slots[i];
				string name = s.isEmpty || s.item == null ? "[빈 슬롯]" : s.item.DisplayName;
				GameLogger.LogInfo($"[Inventory] 슬롯 {i}: {name}", GameLogger.LogCategory.UI);
			}
		}

		/// <summary>
		/// 슬롯 클릭 시 컨텍스트 메뉴를 띄우는 대신 직접 동작을 노출합니다.
		/// 실제 UI 바인딩에서는 이 메서드를 버튼 이벤트에 연결하세요.
		/// </summary>
		public void OnClickUse(int slotIndex)
		{
			bool ok = _itemService.UseActiveItem(slotIndex);
			GameLogger.LogInfo($"[Inventory] 사용 요청: 슬롯 {slotIndex} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
		}

		public void OnClickDiscard(int slotIndex)
		{
			bool ok = _itemService.RemoveActiveItem(slotIndex);
			GameLogger.LogInfo($"[Inventory] 버리기 요청: 슬롯 {slotIndex} → {(ok ? "성공" : "실패")}", GameLogger.LogCategory.UI);
		}

		public void OnClickCancelContext()
		{
			GameLogger.LogInfo("[Inventory] 컨텍스트 닫기", GameLogger.LogCategory.UI);
		}

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
	}
}
