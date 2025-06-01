using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 적이 이미 카드를 올려둔 전투 슬롯을 기준으로, 나머지 하나를 플레이어 슬롯으로 자동 선택합니다.
    /// </summary>
    public class SlotSelector : ISlotSelector
    {
        private readonly ICombatSlotRegistry combatSlotRegistry;

        public SlotSelector(ICombatSlotRegistry combatSlotRegistry)
        {
            this.combatSlotRegistry = combatSlotRegistry;
        }

        public (CombatSlotPosition playerSlot, CombatSlotPosition enemySlot) SelectSlots()
        {
            var allSlots = combatSlotRegistry.GetAllCombatSlots().ToList();

            var enemySlot = allSlots.FirstOrDefault(slot => slot.HasCard());
            var playerSlot = allSlots.FirstOrDefault(slot => !slot.HasCard());

            if (enemySlot == null || playerSlot == null)
            {
                Debug.LogWarning("[SlotSelector] 슬롯 자동 선택 실패: 슬롯이 부족하거나 상태가 불완전합니다.");
                return (CombatSlotPosition.NONE, CombatSlotPosition.NONE);
            }

            return (playerSlot.Position, enemySlot.Position);
        }
    }
}
