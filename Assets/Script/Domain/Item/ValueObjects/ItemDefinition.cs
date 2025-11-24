using System;

namespace Game.Domain.Item.ValueObjects
{
    /// <summary>
    /// 공통 아이템 정의 정보를 나타내는 값 객체입니다.
    /// </summary>
    public sealed class ItemDefinition
    {
        /// <summary>
        /// 아이템 ID입니다.
        /// </summary>
        public ItemId Id { get; }

        /// <summary>
        /// 아이템 이름입니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 아이템 설명입니다.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 아이템 타입입니다.
        /// </summary>
        public ItemType ItemType { get; }

        /// <summary>
        /// 아이템 정의를 생성합니다.
        /// </summary>
        /// <param name="id">아이템 ID</param>
        /// <param name="name">아이템 이름</param>
        /// <param name="description">아이템 설명</param>
        /// <param name="itemType">아이템 타입</param>
        public ItemDefinition(ItemId id, string name, string description, ItemType itemType)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("아이템 이름은 null이거나 공백일 수 없습니다.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            ItemType = itemType;
        }
    }
}


