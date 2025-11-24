using System;

namespace Game.Domain.Item.ValueObjects
{
    /// <summary>
    /// 액티브 아이템 슬롯 정보를 나타내는 값 객체입니다.
    /// </summary>
    public sealed class ActiveItemSlot
    {
        /// <summary>
        /// 슬롯 인덱스입니다. (0 이상)
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// 슬롯이 비어있는지 여부입니다.
        /// </summary>
        public bool IsEmpty => ItemDefinition == null;

        /// <summary>
        /// 슬롯에 장착된 아이템 정의입니다. (없으면 null)
        /// </summary>
        public ItemDefinition ItemDefinition { get; }

        /// <summary>
        /// 액티브 아이템 슬롯을 생성합니다.
        /// </summary>
        /// <param name="index">슬롯 인덱스 (0 이상)</param>
        /// <param name="itemDefinition">아이템 정의 (없으면 null)</param>
        public ActiveItemSlot(int index, ItemDefinition itemDefinition)
        {
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(index),
                    index,
                    "슬롯 인덱스는 0 이상이어야 합니다.");
            }

            Index = index;
            ItemDefinition = itemDefinition;
        }

        /// <summary>
        /// 슬롯에 새로운 아이템을 장착한 새 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="itemDefinition">장착할 아이템 정의</param>
        public ActiveItemSlot WithItem(ItemDefinition itemDefinition)
        {
            return new ActiveItemSlot(Index, itemDefinition);
        }

        /// <summary>
        /// 슬롯을 비운 새 인스턴스를 반환합니다.
        /// </summary>
        public ActiveItemSlot Clear()
        {
            return new ActiveItemSlot(Index, null);
        }
    }
}


