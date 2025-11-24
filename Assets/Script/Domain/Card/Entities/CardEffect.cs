using System;
using Game.Domain.Card.Interfaces;

namespace Game.Domain.Card.Entities
{
    /// <summary>
    /// 카드 효과의 메타 정보를 표현하는 도메인 엔티티입니다.
    /// </summary>
    public sealed class CardEffect : ICardEffect
    {
        /// <inheritdoc />
        public string Id { get; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Description { get; }

        /// <summary>
        /// 카드 효과를 생성합니다.
        /// </summary>
        /// <param name="id">효과 ID</param>
        /// <param name="name">효과 이름</param>
        /// <param name="description">효과 설명</param>
        public CardEffect(string id, string name, string description)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("효과 ID는 null이거나 공백일 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("효과 이름은 null이거나 공백일 수 없습니다.", nameof(name));
            }

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
        }
    }
}


