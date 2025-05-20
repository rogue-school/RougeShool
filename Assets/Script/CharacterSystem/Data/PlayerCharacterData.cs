using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Deck;

namespace Game.CharacterSystem.Data
{
    [CreateAssetMenu(menuName = "Game/Character/PlayerCharacterData")]
    public class PlayerCharacterData : ScriptableObject
    {
        public string displayName;
        public Sprite portrait;
        public int maxHP;

        public PlayerSkillDeck skillDeck;

    }
}
