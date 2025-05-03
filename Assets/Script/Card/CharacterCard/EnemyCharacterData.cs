using UnityEngine;
using System.Collections.Generic;
using Game.Enemy;

namespace Game.Cards
{
    [CreateAssetMenu(menuName = "Card System/Enemy Character")]
    public class EnemyCharacterData : ScriptableObject
    {
        public string characterName;
        public Sprite portrait;
        public int maxHP;

        [Header("전투 시 사용할 스킬 카드")]
        public List<EnemySkillCard> initialDeck;

        [Header("패시브 효과 카드 (전투 전 자동 적용)")]
        public List<EnemySkillCard> passiveSkills;
    }
}
