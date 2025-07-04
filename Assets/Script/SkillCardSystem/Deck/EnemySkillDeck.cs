using UnityEngine;
using System;
using System.Collections.Generic;
using Game.SkillCardSystem.Core;

namespace Game.SkillCardSystem.Deck
{
    /// <summary>
    /// 적이 사용할 스킬 카드 덱을 정의하는 ScriptableObject입니다.
    /// 카드별 등장 확률을 설정할 수 있습니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game/Decks/Enemy Skill Deck")]
    public class EnemySkillDeck : ScriptableObject
    {
        /// <summary>
        /// 확률 기반 스킬 카드 엔트리 클래스입니다.
        /// </summary>
        [Serializable]
        public class CardEntry
        {
            /// <summary>
            /// 스킬 카드 객체
            /// </summary>
            public EnemySkillCard card;

            /// <summary>
            /// 이 카드가 선택될 확률 (0~1)
            /// </summary>
            [Range(0f, 1f)]
            public float probability;
        }

        [Header("적용될 카드 목록 (확률 기반)")]
        [Tooltip("각 카드와 해당 카드의 등장 확률을 설정합니다.")]
        [SerializeField] private List<CardEntry> cards = new();

        /// <summary>
        /// 확률을 기반으로 무작위 카드 엔트리를 선택합니다.
        /// </summary>
        /// <returns>선택된 카드 엔트리 (없을 경우 null)</returns>
        public CardEntry GetRandomEntry()
        {
            if (cards == null || cards.Count == 0)
                return null;

            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var entry in cards)
            {
                cumulative += entry.probability;
                if (roll <= cumulative)
                    return entry;
            }

            // 확률 총합이 1.0보다 작을 경우 마지막 카드 반환 (보정)
            return cards[^1];
        }

        /// <summary>
        /// 덱에 포함된 모든 카드 엔트리를 반환합니다.
        /// </summary>
        public List<CardEntry> GetAllCards() => cards;
    }
}
