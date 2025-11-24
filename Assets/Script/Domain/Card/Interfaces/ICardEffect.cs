namespace Game.Domain.Card.Interfaces
{
    /// <summary>
    /// 도메인 레벨에서의 카드 효과 메타 정보를 정의하는 인터페이스입니다.
    /// 실제 효과 실행은 상위 레이어에서 조합합니다.
    /// </summary>
    public interface ICardEffect
    {
        /// <summary>
        /// 효과 고유 ID입니다.
        /// </summary>
        string Id { get; }

        /// <summary>
        /// 효과 이름입니다.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 효과 설명입니다.
        /// </summary>
        string Description { get; }
    }
}


