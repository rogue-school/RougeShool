using UnityEngine;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Deck
{
    [CreateAssetMenu(menuName = "Game/Decks/Player Skill Deck")]
    public class PlayerSkillDeck : ScriptableObject
    {
        [Header("플레이어 카드 덱 (순서 고정 및 슬롯 지정)")]
        [SerializeField] private List<PlayerSkillCardEntry> cards = new();

        public List<PlayerSkillCardEntry> Cards => new(cards);
        public int Count => cards?.Count ?? 0;

        public PlayerSkillCardEntry GetEntryAt(int index)
        {
            if (index < 0 || index >= Count)
                return null;

            return cards[index];
        }
    }
}
