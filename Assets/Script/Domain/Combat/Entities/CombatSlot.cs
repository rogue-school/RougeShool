using System;
using Game.Domain.Combat.ValueObjects;

namespace Game.Domain.Combat.Entities
{
    /// <summary>
    /// 전투 슬롯 한 칸을 표현하는 엔티티입니다.
    /// </summary>
    public sealed class CombatSlot
    {
        /// <summary>
        /// 슬롯 위치입니다.
        /// </summary>
        public SlotPosition Position { get; }

        /// <summary>
        /// 슬롯에 카드가 존재하는지 여부입니다.
        /// </summary>
        public bool HasCard => CardId != null;

        /// <summary>
        /// 슬롯에 배치된 카드의 ID입니다. (없으면 null)
        /// </summary>
        public string CardId { get; }

        /// <summary>
        /// 카드 소유자 턴 타입입니다. (플레이어/적)
        /// </summary>
        public TurnType? Owner { get; }

        /// <summary>
        /// 전투 슬롯을 생성합니다.
        /// </summary>
        /// <param name="position">슬롯 위치</param>
        /// <param name="cardId">카드 ID (없으면 null)</param>
        /// <param name="owner">카드 소유자 (없으면 null)</param>
        public CombatSlot(SlotPosition position, string cardId = null, TurnType? owner = null)
        {
            Position = position;
            CardId = cardId;
            Owner = owner;
        }

        /// <summary>
        /// 슬롯에 카드를 배치한 새 인스턴스를 반환합니다.
        /// </summary>
        /// <param name="cardId">배치할 카드 ID</param>
        /// <param name="owner">카드 소유자</param>
        public CombatSlot WithCard(string cardId, TurnType owner)
        {
            if (string.IsNullOrWhiteSpace(cardId))
            {
                throw new ArgumentException("카드 ID는 null이거나 공백일 수 없습니다.", nameof(cardId));
            }

            return new CombatSlot(Position, cardId, owner);
        }

        /// <summary>
        /// 슬롯을 비운 새 인스턴스를 반환합니다.
        /// </summary>
        public CombatSlot Clear()
        {
            return new CombatSlot(Position);
        }
    }
}


