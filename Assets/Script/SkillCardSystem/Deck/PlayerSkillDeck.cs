using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Deck
{
    [CreateAssetMenu(menuName = "Game/Decks/Player Skill Deck")]
    public class PlayerSkillDeck : ScriptableObject
    {
        [Header("플레이어 카드 덱 (순서 고정 또는 셔플 후 순서대로 사용)")]
        [SerializeField] private List<PlayerSkillCard> cards = new();

        /// <summary>
        /// 카드 목록 복사본 반환
        /// </summary>
        public List<PlayerSkillCard> GetCards()
        {
            return new List<PlayerSkillCard>(cards);
        }

        public int Count => cards?.Count ?? 0;

        public PlayerSkillCard GetCardAt(int index)
        {
            if (index < 0 || index >= Count)
                return null;

            return cards[index];
        }
    }
}
