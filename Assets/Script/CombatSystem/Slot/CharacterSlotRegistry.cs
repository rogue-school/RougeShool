using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;

namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 캐릭터 슬롯을 등록하고 조회하는 레지스트리입니다.
    /// </summary>
    public class CharacterSlotRegistry : MonoBehaviour, ICharacterSlotRegistry
    {
        #region 필드

        private List<ICharacterSlot> characterSlots = new();

        #endregion

        #region 슬롯 등록 및 초기화

        /// <summary>
        /// 외부에서 전달된 캐릭터 슬롯 리스트를 등록합니다.
        /// </summary>
        /// <param name="slots">등록할 캐릭터 슬롯 목록</param>
        public void RegisterCharacterSlots(IEnumerable<ICharacterSlot> slots)
        {
            characterSlots = slots.ToList();
        }

        #endregion

        #region 슬롯 조회

        /// <summary>
        /// 모든 캐릭터 슬롯을 반환합니다.
        /// </summary>
        public IEnumerable<ICharacterSlot> GetAllCharacterSlots()
        {
            return characterSlots;
        }

        /// <summary>
        /// 소유자(SlotOwner)에 해당하는 캐릭터 슬롯을 반환합니다.
        /// </summary>
        public ICharacterSlot GetCharacterSlot(SlotOwner owner)
        {
            return characterSlots.FirstOrDefault(slot => slot.GetOwner() == owner);
        }

        #endregion

        #region 클리어

        /// <summary>
        /// 모든 슬롯 정보를 초기화합니다.
        /// </summary>
        public void Clear()
        {
            characterSlots.Clear();
        }

        #endregion
    }
}
