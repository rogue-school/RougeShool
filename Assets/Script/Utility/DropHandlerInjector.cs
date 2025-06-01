using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.DragDrop;

namespace Game.Utility
{
    /// <summary>
    /// 전투 슬롯에 드롭 핸들러(CardDropToSlotHandler)를 주입하는 유틸리티
    /// SRP: 드롭 주입만 책임
    /// DIP: 구체 슬롯 대신 인터페이스에 의존
    /// </summary>
    public static class DropHandlerInjector
    {
        public static void InjectToAllCombatSlots(
            ICombatSlotRegistry slotRegistry,
            CardDropService dropService,
            ICombatFlowCoordinator flowCoordinator)
        {
            if (slotRegistry == null || dropService == null || flowCoordinator == null)
            {
                Debug.LogError("[DropHandlerInjector] 필수 인수 중 하나 이상이 null입니다.");
                return;
            }

            foreach (var slot in slotRegistry.GetAllCombatSlots())
            {
                if (slot is MonoBehaviour slotComponent)
                {
                    var dropHandler = slotComponent.GetComponent<CardDropToSlotHandler>();
                    if (dropHandler != null)
                    {
                        dropHandler.Inject(dropService, flowCoordinator);
                        Debug.Log($"[DropHandlerInjector] 드롭 핸들러 주입 완료: {slot.GetCombatPosition()}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DropHandlerInjector] {slot.GetCombatPosition()} 슬롯에 드롭 핸들러 없음");
                    }
                }
                else
                {
                    Debug.LogWarning($"[DropHandlerInjector] 슬롯이 MonoBehaviour가 아닙니다: {slot.GetCombatPosition()}");
                }
            }
        }
    }
}
