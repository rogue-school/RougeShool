using Game.Domain.Item.ValueObjects;

namespace Game.Domain.Item.Interfaces
{
    /// <summary>
    /// 도메인 레벨에서의 아이템 인터페이스입니다.
    /// </summary>
    public interface IItem
    {
        /// <summary>
        /// 아이템 정의입니다.
        /// </summary>
        ItemDefinition Definition { get; }
    }
}


