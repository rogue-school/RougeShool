using UnityEngine;
using System.Collections.Generic;
using System;
using Game.SkillCardSystem.Core;

namespace Game.SkillCardSystem.Deck
{
    [CreateAssetMenu(menuName = "Game/Decks/Enemy Skill Deck")]
    public class EnemySkillDeck : ScriptableObject
    {
        [Serializable]
        public class CardEntry
        {
            public EnemySkillCard card;

            [Range(0f, 1f)]
            public float probability;
        }

        [SerializeField] private List<CardEntry> cards;

        public CardEntry GetRandomEntry()
        {
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var entry in cards)
            {
                cumulative += entry.probability;
                if (roll <= cumulative)
                    return entry;
            }

            // 확률 합이 부족한 경우 마지막 카드 보정
            return cards.Count > 0 ? cards[^1] : null;
        }

        public List<CardEntry> GetAllCards() => cards;
    }
}
