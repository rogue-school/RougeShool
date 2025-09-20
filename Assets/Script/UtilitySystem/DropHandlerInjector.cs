using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CombatSystem.Service;
using Game.CombatSystem.DragDrop;
using Game.CombatSystem.Data;
using Game.CombatSystem.Slot;

namespace Game.UtilitySystem
{
    /// <summary>
    /// 싱글게임용 드롭 핸들러 주입 유틸리티
    /// </summary>
    public static class DropHandlerInjector
    {
        /// <summary>
        /// 모든 전투 슬롯에 드롭 핸들러를 주입합니다.
        /// </summary>
        /// <param name="slotManager">슬롯 관리자</param>
        /// <param name="dropService">드롭 서비스</param>
        public static void InjectToAllCombatSlots(
            CombatSlotManager slotManager,
            CardDropService dropService)
        {
            if (slotManager == null || dropService == null)
            {
                Debug.LogError("[DropHandlerInjector] 필수 인수 중 하나 이상이 null입니다.");
                return;
            }

            // 새로운 시스템에서는 슬롯별로 직접 주입
            var combatSlots = new[] { 
                CombatSlotPosition.BATTLE_SLOT,
                CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_4
            };

            foreach (var position in combatSlots)
            {
                var slot = slotManager.GetSlot(position);
                if (slot != null)
                {
                    Debug.Log($"[DropHandlerInjector] 슬롯 주입 완료: {position}");
                }
                else
                {
                    Debug.LogWarning($"[DropHandlerInjector] 슬롯을 찾을 수 없습니다: {position}");
                }
            }
        }
    }
}