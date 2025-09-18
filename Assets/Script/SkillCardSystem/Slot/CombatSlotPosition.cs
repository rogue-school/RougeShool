namespace Game.CombatSystem.Slot
{
    /// <summary>
    /// 전투 슬롯 위치를 나타냅니다.
    /// 새로운 시스템: 전투슬롯 1개 + 대기슬롯 4개 (총 5개 슬롯)
    /// </summary>
    public enum CombatSlotPosition
    {
        #region 새로운 5슬롯 시스템

        /// <summary>
        /// 전투 슬롯 (카드 효과가 발동하는 슬롯)
        /// </summary>
        BATTLE_SLOT,

        /// <summary>
        /// 대기 슬롯 1번
        /// </summary>
        WAIT_SLOT_1,

        /// <summary>
        /// 대기 슬롯 2번
        /// </summary>
        WAIT_SLOT_2,

        /// <summary>
        /// 대기 슬롯 3번
        /// </summary>
        WAIT_SLOT_3,

        /// <summary>
        /// 대기 슬롯 4번
        /// </summary>
        WAIT_SLOT_4,

        #endregion

        #region 레거시 호환성 (하위 호환성 유지)

        /// <summary>
        /// 첫 번째 슬롯 (레거시 - BATTLE_SLOT과 동일)
        /// </summary>
        [System.Obsolete("새로운 시스템에서는 BATTLE_SLOT을 사용하세요")]
        SLOT_1 = BATTLE_SLOT,

        /// <summary>
        /// 두 번째 슬롯 (레거시 - WAIT_SLOT_1과 동일)
        /// </summary>
        [System.Obsolete("새로운 시스템에서는 WAIT_SLOT_1을 사용하세요")]
        SLOT_2 = WAIT_SLOT_1,

        /// <summary>
        /// 세 번째 슬롯 (레거시 - WAIT_SLOT_2과 동일)
        /// </summary>
        [System.Obsolete("새로운 시스템에서는 WAIT_SLOT_2을 사용하세요")]
        SLOT_3 = WAIT_SLOT_2,

        /// <summary>
        /// 네 번째 슬롯 (레거시 - WAIT_SLOT_3과 동일)
        /// </summary>
        [System.Obsolete("새로운 시스템에서는 WAIT_SLOT_3을 사용하세요")]
        SLOT_4 = WAIT_SLOT_3,

        #endregion

        #region 공통

        /// <summary>
        /// 슬롯 없음 (기본값 또는 비활성 상태)
        /// </summary>
        NONE

        #endregion
    }

    /// <summary>
    /// CombatSlotPosition 확장 메서드
    /// </summary>
    public static class CombatSlotPositionExtensions
    {
        /// <summary>
        /// 전투 슬롯인지 확인합니다.
        /// </summary>
        /// <param name="position">확인할 슬롯 위치</param>
        /// <returns>전투 슬롯 여부</returns>
        public static bool IsBattleSlot(this CombatSlotPosition position)
        {
            return position == CombatSlotPosition.BATTLE_SLOT;
        }

        /// <summary>
        /// 대기 슬롯인지 확인합니다.
        /// </summary>
        /// <param name="position">확인할 슬롯 위치</param>
        /// <returns>대기 슬롯 여부</returns>
        public static bool IsWaitSlot(this CombatSlotPosition position)
        {
            return position >= CombatSlotPosition.WAIT_SLOT_1 && position <= CombatSlotPosition.WAIT_SLOT_4;
        }

        /// <summary>
        /// 대기 슬롯 번호를 반환합니다. (1~4)
        /// </summary>
        /// <param name="position">확인할 슬롯 위치</param>
        /// <returns>대기 슬롯 번호 (1~4), 대기 슬롯이 아니면 0</returns>
        public static int GetWaitSlotNumber(this CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.WAIT_SLOT_1 => 1,
                CombatSlotPosition.WAIT_SLOT_2 => 2,
                CombatSlotPosition.WAIT_SLOT_3 => 3,
                CombatSlotPosition.WAIT_SLOT_4 => 4,
                _ => 0
            };
        }

        /// <summary>
        /// 대기 슬롯 번호로 CombatSlotPosition을 생성합니다.
        /// </summary>
        /// <param name="slotNumber">대기 슬롯 번호 (1~4)</param>
        /// <returns>해당하는 CombatSlotPosition</returns>
        public static CombatSlotPosition FromWaitSlotNumber(int slotNumber)
        {
            return slotNumber switch
            {
                1 => CombatSlotPosition.WAIT_SLOT_1,
                2 => CombatSlotPosition.WAIT_SLOT_2,
                3 => CombatSlotPosition.WAIT_SLOT_3,
                4 => CombatSlotPosition.WAIT_SLOT_4,
                _ => CombatSlotPosition.NONE
            };
        }

        /// <summary>
        /// 다음 대기 슬롯을 반환합니다. (앞으로 이동)
        /// </summary>
        /// <param name="position">현재 슬롯 위치</param>
        /// <returns>다음 슬롯 위치</returns>
        public static CombatSlotPosition GetNextSlot(this CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.WAIT_SLOT_4 => CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_3 => CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_2 => CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_1 => CombatSlotPosition.BATTLE_SLOT,
                CombatSlotPosition.BATTLE_SLOT => CombatSlotPosition.NONE,
                _ => CombatSlotPosition.NONE
            };
        }

        /// <summary>
        /// 이전 대기 슬롯을 반환합니다. (뒤로 이동)
        /// </summary>
        /// <param name="position">현재 슬롯 위치</param>
        /// <returns>이전 슬롯 위치</returns>
        public static CombatSlotPosition GetPreviousSlot(this CombatSlotPosition position)
        {
            return position switch
            {
                CombatSlotPosition.BATTLE_SLOT => CombatSlotPosition.WAIT_SLOT_1,
                CombatSlotPosition.WAIT_SLOT_1 => CombatSlotPosition.WAIT_SLOT_2,
                CombatSlotPosition.WAIT_SLOT_2 => CombatSlotPosition.WAIT_SLOT_3,
                CombatSlotPosition.WAIT_SLOT_3 => CombatSlotPosition.WAIT_SLOT_4,
                CombatSlotPosition.WAIT_SLOT_4 => CombatSlotPosition.NONE,
                _ => CombatSlotPosition.NONE
            };
        }
    }
}
