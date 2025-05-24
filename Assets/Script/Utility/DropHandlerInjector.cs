using System.Collections.Generic;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;
using Game.CombatSystem.DragDrop;

namespace Game.Utility
{
    public static class DropHandlerInjector
    {
        public static void InjectToAllCombatSlots(ISlotRegistry slotRegistry, ITurnCardRegistry cardRegistry)
        {
            if (slotRegistry == null || cardRegistry == null)
            {
                Debug.LogError("[DropHandlerInjector] 슬롯 레지스트리 또는 카드 레지스트리가 null입니다.");
                return;
            }

            foreach (var slot in slotRegistry.GetAllCombatSlots())
            {
                if (slot is MonoBehaviour slotComponent)
                {
                    var dropHandler = slotComponent.GetComponent<CardDropToSlotHandler>();
                    if (dropHandler != null)
                    {
                        dropHandler.Inject(cardRegistry);
                        Debug.Log($"[DropHandlerInjector] 드롭 핸들러 주입 완료: {slot.GetCombatPosition()}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DropHandlerInjector] {slot.GetCombatPosition()} 슬롯에 드롭 핸들러 없음");
                    }
                }
            }
        }
    }
}
