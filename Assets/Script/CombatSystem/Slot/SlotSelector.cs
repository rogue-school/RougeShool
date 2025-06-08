using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Utility
{
    /// <summary>
    /// 적이 이미 카드를 올려둔 전투 슬롯을 기준으로, 남은 슬롯을 플레이어에게 할당합니다.
    /// </summary>
    public class SlotSelector : ISlotSelector
    {
        #region 필드

        private readonly ICombatSlotRegistry combatSlotRegistry;

        #endregion

        #region 생성자

        /// <summary>
        /// 슬롯 셀렉터를 생성합니다.
        /// </summary>
        /// <param name="combatSlotRegistry">전투 슬롯 레지스트리</param>
        public SlotSelector(ICombatSlotRegistry combatSlotRegistry)
        {
            this.combatSlotRegistry = combatSlotRegistry;
        }

        #endregion

        #region 슬롯 선택 로직

        /// <summary>
        /// 전투 슬롯 중 적이 점유한 슬롯을 찾고, 나머지를 플레이어 슬롯으로 지정합니다.
        /// </summary>
        /// <returns>플레이어 슬롯, 적 슬롯 튜플</returns>
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

        #endregion
    }
}
