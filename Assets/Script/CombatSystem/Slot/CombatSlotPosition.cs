namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯 위치를 나타냅니다.
    /// </summary>
    public enum CombatSlotPosition
    {
        #region 슬롯 위치

        /// <summary>
        /// 첫 번째 슬롯 (선공)
        /// </summary>
        FIRST,

        /// <summary>
        /// 두 번째 슬롯 (후공)
        /// </summary>
        SECOND,

        /// <summary>
        /// 슬롯 없음 (기본값 또는 비활성 상태)
        /// </summary>
        NONE

        #endregion
    }
}
