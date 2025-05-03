using UnityEngine;
using System.Collections.Generic;
using Game.Enemy;

namespace Game.Cards
{
    [CreateAssetMenu(menuName = "Card System/Enemy Skill Deck")]
    public class EnemySkillDeck : ScriptableObject
    {
        [System.Serializable]
        public class CardEntry
        {
            public EnemySkillCard card;

            [Range(0f, 1f)]
            public float probability;
        }

        public List<CardEntry> cards;

        public EnemySkillCard GetRandomCard()
        {
            float roll = Random.value;
            float cumulative = 0f;

            foreach (var entry in cards)
            {
                cumulative += entry.probability;
                if (roll <= cumulative)
                    return entry.card;
            }

            if (cards.Count > 0)
                return cards[cards.Count - 1].card;

            Debug.LogWarning("[EnemySkillDeck] 카드가 없습니다.");
            return null;
        }
    }
}
