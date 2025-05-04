using UnityEngine;
using Game.UI;

namespace Game.Battle.Initialization
{
    /// <summary>
    /// 전투 슬롯들을 자동으로 찾고 초기화하는 유틸리티입니다.
    /// </summary>
    public static class SlotInitializer
    {
        public static void AutoBindAllSlots()
        {
            var slots = Object.FindObjectsOfType<BaseCardSlotUI>();
            foreach (var slot in slots)
                slot.AutoBind();

            Debug.Log("[SlotInitializer] 모든 슬롯 자동 바인딩 완료");
        }
    }
}
