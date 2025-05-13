using System.Collections.Generic;
using Game.SkillCardSystem.Core;
using UnityEngine;

namespace Game.CharacterSystem.Data
{
    [CreateAssetMenu(menuName = "Game/Character/PlayerCharacterData")]
    public class PlayerCharacterData : ScriptableObject
    {
        public string displayName;
        public Sprite portrait;
        public int maxHP;

        [System.Serializable]
        public class SkillCardEntry
        {
            public PlayerSkillCard card;
            public int damage;
        }

        public List<SkillCardEntry> skillDeck;
    }
}
