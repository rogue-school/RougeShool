namespace Game.Domain.Combat.ValueObjects
{
    /// <summary>
    /// 전투 슬롯 위치를 나타내는 값입니다.
    /// </summary>
    public enum SlotPosition
    {
        /// <summary>
        /// 슬롯이 없거나 비활성 상태입니다.
        /// </summary>
        None,

        /// <summary>
        /// 전투 슬롯 (카드 효과가 발동되는 슬롯)입니다.
        /// </summary>
        BattleSlot,

        /// <summary>
        /// 대기 슬롯 1번입니다.
        /// </summary>
        WaitSlot1,

        /// <summary>
        /// 대기 슬롯 2번입니다.
        /// </summary>
        WaitSlot2,

        /// <summary>
        /// 대기 슬롯 3번입니다.
        /// </summary>
        WaitSlot3,

        /// <summary>
        /// 대기 슬롯 4번입니다.
        /// </summary>
        WaitSlot4
    }
}


