using System;
using Game.Domain.Item.Interfaces;
using Game.Domain.Item.ValueObjects;

namespace Game.Domain.Item.Entities
{
    /// <summary>
    /// 아이템 도메인 엔티티입니다.
    /// </summary>
    public sealed class Item : IItem
    {
        /// <inheritdoc />
        public ItemDefinition Definition { get; }

        /// <summary>
        /// 아이템을 생성합니다.
        /// </summary>
        /// <param name="definition">아이템 정의</param>
        public Item(ItemDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }
    }
}


