using UnityEngine;
using System.Collections.Generic;
using Game.Player;

namespace Game.Cards
{
    [CreateAssetMenu(menuName = "Card System/Player Character")]
    public class PlayerCharacterData : ScriptableObject
    {
        public string characterName;
        public Sprite portrait;
        public int maxHP;

        [Header("전투 시작 시 보유할 스킬 카드")]
        public List<PlayerSkillCard> initialDeck;

        [Header("시작 시 적용할 능력치 (옵션)")]
        public int baseAttackBonus;
    }
}
