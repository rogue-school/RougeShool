using System;
using Game.Domain.Card.Interfaces;
using Game.Domain.Card.ValueObjects;
using Game.Domain.Character.ValueObjects;

namespace Game.Domain.Card.Entities
{
    /// <summary>
    /// 스킬 카드의 도메인 엔티티입니다.
    /// </summary>
    public sealed class SkillCard : ISkillCard
    {
        /// <inheritdoc />
        public CardDefinition Definition { get; }

        /// <inheritdoc />
        public string Id => Definition.Id;

        /// <inheritdoc />
        public string Name => Definition.Name;

        /// <inheritdoc />
        public string Description => Definition.Description;

        /// <inheritdoc />
        public CardStats Stats => Definition.Stats;

        /// <summary>
        /// 스킬 카드를 생성합니다.
        /// </summary>
        /// <param name="definition">카드 정의</param>
        public SkillCard(CardDefinition definition)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
        }

        /// <inheritdoc />
        public bool CanPayCost(Resource resource)
        {
            int cost = Stats.ResourceCost;
            if (cost <= 0)
            {
                return true;
            }

            if (resource == null)
            {
                return false;
            }

            return resource.CurrentAmount >= cost;
        }
    }
}


