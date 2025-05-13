using UnityEngine;
using System;
using System.Collections.Generic;
using Game.SkillCardSystem.Core;

namespace Game.SkillCardSystem.Deck
{
    /// <summary>
    /// 적의 스킬 카드 덱 데이터입니다.
    /// 랜덤 확률에 따라 적 카드가 선택됩니다.
    /// </summary>
    [CreateAssetMenu(menuName = "Game Assets/Decks/Enemy Skill Deck")]
    public class EnemySkillDeck : ScriptableObject
    {
        /// <summary>
        /// 카드와 등장 확률을 함께 저장하는 구조체입니다.
        /// </summary>
        [Serializable]
        public class CardEntry
        {
            public EnemySkillCard card;

            [Range(0f, 1f)]
            public float probability;
        }

        /// <summary>
        /// 적 카드 목록과 각 카드의 등장 확률입니다.
        /// </summary>
        [SerializeField] public List<CardEntry> cards;

        /// <summary>
        /// 확률 기반으로 카드 하나를 무작위로 선택합니다.
        /// 누적 확률 방식으로 처리됩니다.
        /// </summary>
        public EnemySkillCard GetRandomCard()
        {
            float roll = UnityEngine.Random.value;
            float cumulative = 0f;

            foreach (var entry in cards)
            {
                cumulative += entry.probability;
                if (roll <= cumulative)
                    return entry.card;
            }

            // 누적 확률이 부족한 경우 마지막 카드 반환 (보정용)
            if (cards.Count > 0)
                return cards[cards.Count - 1].card;

            Debug.LogWarning("[EnemySkillDeck] 카드가 없습니다.");
            return null;
        }
    }
}
