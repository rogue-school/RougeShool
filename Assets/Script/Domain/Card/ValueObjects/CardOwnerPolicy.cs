namespace Game.Domain.Card.ValueObjects
{
    /// <summary>
    /// 카드 소유자 정책을 나타내는 열거형입니다.
    /// </summary>
    public enum CardOwnerPolicy
    {
        /// <summary>
        /// 플레이어와 적이 모두 사용할 수 있는 카드입니다.
        /// </summary>
        Shared,

        /// <summary>
        /// 플레이어만 사용할 수 있는 카드입니다.
        /// </summary>
        PlayerOnly,

        /// <summary>
        /// 적만 사용할 수 있는 카드입니다.
        /// </summary>
        EnemyOnly
    }
}


