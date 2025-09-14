namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯 위치를 나타냅니다.
    /// 새로운 시스템: 4개 슬롯 (1, 2, 3, 4번)
    /// </summary>
    public enum CombatSlotPosition
    {
        #region 슬롯 위치

        /// <summary>
        /// 첫 번째 슬롯
        /// </summary>
        SLOT_1,

        /// <summary>
        /// 두 번째 슬롯
        /// </summary>
        SLOT_2,

        /// <summary>
        /// 세 번째 슬롯
        /// </summary>
        SLOT_3,

        /// <summary>
        /// 네 번째 슬롯
        /// </summary>
        SLOT_4,

        /// <summary>
        /// 슬롯 없음 (기본값 또는 비활성 상태)
        /// </summary>
        NONE,

        #region 호환성 (기존 코드 지원)

        /// <summary>
        /// 첫 번째 슬롯 (기존 호환성)
        /// </summary>
        FIRST = SLOT_1,

        /// <summary>
        /// 두 번째 슬롯 (기존 호환성)
        /// </summary>
        SECOND = SLOT_2

        #endregion

        #endregion
    }
}
