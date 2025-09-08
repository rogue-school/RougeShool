namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 필드 내에서 슬롯의 위치를 나타냅니다.
    /// </summary>
    public enum CombatFieldSlotPosition
    {
        #region 슬롯 위치

        /// <summary>
        /// 왼쪽 필드 슬롯
        /// </summary>
        FIELD_LEFT,

        /// <summary>
        /// 오른쪽 필드 슬롯
        /// </summary>
        FIELD_RIGHT,

        /// <summary>
        /// 슬롯 없음 (기본값 또는 비활성 상태)
        /// </summary>
        NONE

        #endregion
    }
}
