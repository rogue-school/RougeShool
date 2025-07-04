using UnityEngine;
using Game.CharacterSystem.Slot;
using Game.CombatSystem.Slot;

namespace Game.CharacterSystem.Interface
{
    /// <summary>
    /// 캐릭터 슬롯 인터페이스.
    /// 슬롯에 캐릭터를 설정하거나 제거하고, 슬롯 위치 및 소유자 정보를 제공합니다.
    /// </summary>
    public interface ICharacterSlot
    {
        #region 슬롯 제어

        /// <summary>
        /// 슬롯에 캐릭터를 설정합니다.
        /// </summary>
        /// <param name="character">슬롯에 배치할 캐릭터</param>
        void SetCharacter(ICharacter character);

        /// <summary>
        /// 슬롯에서 캐릭터를 제거합니다.
        /// </summary>
        void Clear();

        #endregion

        #region 슬롯 상태 조회

        /// <summary>
        /// 현재 슬롯에 설정된 캐릭터를 반환합니다.
        /// </summary>
        /// <returns>캐릭터 객체. 없으면 null</returns>
        ICharacter GetCharacter();

        /// <summary>
        /// 슬롯의 Transform을 반환합니다.
        /// </summary>
        /// <returns>Unity Transform 객체</returns>
        Transform GetTransform();

        #endregion

        #region 슬롯 메타 정보

        /// <summary>
        /// 슬롯의 위치 정보를 반환합니다.
        /// </summary>
        /// <returns>CharacterSlotPosition 열거형</returns>
        CharacterSlotPosition GetSlotPosition();

        /// <summary>
        /// 슬롯의 소유자 정보를 반환합니다.
        /// </summary>
        /// <returns>SlotOwner 열거형</returns>
        SlotOwner GetOwner();

        #endregion
    }
}
