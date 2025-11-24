using System;

namespace Game.Domain.Card.ValueObjects
{
    /// <summary>
    /// 카드의 정적 정의 정보를 나타내는 값 객체입니다.
    /// </summary>
    public sealed class CardDefinition
    {
        /// <summary>
        /// 카드 고유 식별자입니다.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// 카드 표시 이름입니다.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// 카드 설명입니다.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// 카드 소유자 정책입니다.
        /// </summary>
        public CardOwnerPolicy OwnerPolicy { get; }

        /// <summary>
        /// 카드의 전투 관련 수치입니다.
        /// </summary>
        public CardStats Stats { get; }

        /// <summary>
        /// 카드 정의를 생성합니다.
        /// </summary>
        /// <param name="id">카드 ID (null/공백 불가)</param>
        /// <param name="name">카드 이름 (null/공백 불가)</param>
        /// <param name="description">카드 설명 (null이면 빈 문자열)</param>
        /// <param name="ownerPolicy">소유자 정책</param>
        /// <param name="stats">전투 수치</param>
        public CardDefinition(
            string id,
            string name,
            string description,
            CardOwnerPolicy ownerPolicy,
            CardStats stats)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                throw new ArgumentException("카드 ID는 null이거나 공백일 수 없습니다.", nameof(id));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException("카드 이름은 null이거나 공백일 수 없습니다.", nameof(name));
            }

            Stats = stats ?? throw new ArgumentNullException(nameof(stats));

            Id = id;
            Name = name;
            Description = description ?? string.Empty;
            OwnerPolicy = ownerPolicy;
        }
    }
}


