using Game.CombatSystem.Slot;

namespace Game.CombatSystem.Interface
{
    /// <summary>
    /// 전투에 사용되는 슬롯 시스템의 레지스트리를 통합적으로 관리하는 인터페이스입니다.
    /// 핸드 슬롯, 전투 슬롯, 캐릭터 슬롯에 대한 접근을 제공합니다.
    /// </summary>
    public interface ISlotRegistry
    {
        /// <summary>
        /// 핸드 슬롯 레지스트리를 반환합니다.
        /// </summary>
        IHandSlotRegistry GetHandSlotRegistry();

        /// <summary>
        /// 전투 슬롯 레지스트리를 반환합니다.
        /// </summary>
        ICombatSlotRegistry GetCombatSlotRegistry();

        /// <summary>
        /// 캐릭터 슬롯 레지스트리를 반환합니다.
        /// </summary>
        ICharacterSlotRegistry GetCharacterSlotRegistry();

        /// <summary>
        /// 전투 슬롯 위치(논리 위치 기준)에 해당하는 슬롯을 반환합니다.
        /// </summary>
        /// <param name="position">전투 슬롯 위치</param>
        /// <returns>해당 슬롯</returns>
        ICombatCardSlot GetCombatSlot(CombatSlotPosition position);

        /// <summary>
        /// 전투 필드 위치(필드 기준)에 해당하는 슬롯을 반환합니다.
        /// </summary>
        /// <param name="fieldPosition">전투 필드 슬롯 위치</param>
        /// <returns>해당 슬롯</returns>
        ICombatCardSlot GetCombatSlot(CombatFieldSlotPosition fieldPosition);

        /// <summary>
        /// 슬롯 레지스트리가 모두 초기화되었음을 명시합니다.
        /// </summary>
        void MarkInitialized();

        /// <summary>
        /// 현재 슬롯 레지스트리가 초기화 상태인지 여부를 반환합니다.
        /// </summary>
        bool IsInitialized { get; }
    }
}
