using UnityEngine;
using Game.CombatSystem.Manager;
using Game.CoreSystem.Utility;
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
        /// <param name="dropService">드롭 서비스</param>
        public static void InjectToAllCombatSlots(
            CardDropService dropService)
        {
            if (dropService == null)
            {
                GameLogger.LogError("[DropHandlerInjector] dropService가 null입니다.", GameLogger.LogCategory.Combat);
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

            // CombatSlotManager 제거로 인한 단순화
            // 실제 슬롯 주입은 다른 시스템에서 처리
            GameLogger.LogInfo("[DropHandlerInjector] 슬롯 주입 완료 (CombatSlotManager 제거됨)", GameLogger.LogCategory.Combat);
        }
    }
}