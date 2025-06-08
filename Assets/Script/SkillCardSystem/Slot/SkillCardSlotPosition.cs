namespace Game.SkillCardSystem.Slot
{
    /// <summary>
    /// 스킬 카드가 위치할 수 있는 고정 슬롯 위치를 정의합니다.
    /// 각 슬롯은 플레이어 또는 적 진영의 특정 위치를 나타냅니다.
    /// </summary>
    public enum SkillCardSlotPosition
    {
        /// <summary>
        /// 플레이어의 첫 번째 슬롯 (왼쪽)
        /// </summary>
        PLAYER_SLOT_1,

        /// <summary>
        /// 플레이어의 두 번째 슬롯 (중앙)
        /// </summary>
        PLAYER_SLOT_2,

        /// <summary>
        /// 플레이어의 세 번째 슬롯 (오른쪽)
        /// </summary>
        PLAYER_SLOT_3,

        /// <summary>
        /// 적의 첫 번째 슬롯 (오른쪽)
        /// </summary>
        ENEMY_SLOT_1,

        /// <summary>
        /// 적의 두 번째 슬롯 (중앙)
        /// </summary>
        ENEMY_SLOT_2,

        /// <summary>
        /// 적의 세 번째 슬롯 (왼쪽)
        /// </summary>
        ENEMY_SLOT_3
    }
}
