using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CombatSystem.Data;
using Game.SkillCardSystem.Slot;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 플레이어 및 적의 핸드 슬롯을 등록하고 관리하는 레지스트리 클래스입니다.
    /// </summary>
    public class HandSlotRegistry : MonoBehaviour
    {
        #region 필드

        private List<IHandCardSlot> handSlots = new();

        #endregion

        #region 슬롯 등록

        /// <summary>
        /// 핸드 슬롯들을 등록합니다.
        /// </summary>
        /// <param name="slots">등록할 핸드 슬롯 목록</param>
        public void RegisterHandSlots(IEnumerable<IHandCardSlot> slots)
        {
            handSlots = slots.ToList();
        }

        #endregion

        #region 슬롯 조회

        /// <summary>
        /// 슬롯 위치를 기준으로 핸드 슬롯을 반환합니다.
        /// </summary>
        /// <param name="position">조회할 슬롯 위치</param>
        /// <returns>해당 위치의 핸드 슬롯 또는 null</returns>
        public IHandCardSlot GetHandSlot(SkillCardSlotPosition position)
        {
            return handSlots.FirstOrDefault(slot => slot.GetSlotPosition() == position);
        }

        /// <summary>
        /// 모든 핸드 슬롯을 반환합니다.
        /// </summary>
        public IEnumerable<IHandCardSlot> GetAllHandSlots()
        {
            return handSlots;
        }

        /// <summary>
        /// 슬롯 소유자 기준으로 핸드 슬롯을 필터링하여 반환합니다.
        /// </summary>
        /// <param name="owner">슬롯 소유자 (PLAYER 또는 ENEMY)</param>
        public IEnumerable<IHandCardSlot> GetHandSlots(SlotOwner owner)
        {
            return handSlots.Where(slot => slot.GetOwner() == owner);
        }

        /// <summary>
        /// 플레이어 핸드 슬롯을 반환합니다.
        /// </summary>
        public IEnumerable<IHandCardSlot> GetPlayerHandSlot()
        {
            return GetHandSlots(SlotOwner.PLAYER);
        }

        #endregion
    }
}
