using UnityEngine;
using System.Collections.Generic;
using Game.Enemy;
using Game.Interface; // ICardEffect용 (필요한 경우)

namespace Game.Data
{
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject
    {
        public string displayName;
        public int maxHP;
        public Sprite portrait;

        [System.Serializable]
        public class SkillCardEntry
        {
            public EnemySkillCard card;
            public int damage;
        }

        [Header("고정 스킬 카드 덱")]
        [SerializeField] private List<SkillCardEntry> skillDeck = new();

        [Header("패시브 이펙트 (Regen, Buff 등)")]
        [SerializeField] private List<ScriptableObject> passiveEffects = new();
        public SkillCardEntry GetRandomEntry()
        {
            if (skillDeck == null || skillDeck.Count == 0)
            {
                Debug.LogWarning("[EnemyCharacterData] 스킬 덱이 비어 있습니다.");
                return null;
            }

            int index = Random.Range(0, skillDeck.Count);
            return skillDeck[index];
        }

        public int GetDamageOfCard(EnemySkillCard card)
        {
            foreach (var entry in skillDeck)
            {
                if (entry.card == card)
                    return entry.damage;
            }

            Debug.LogWarning($"[EnemyCharacterData] 카드 '{card?.name}'에 대한 데미지가 정의되지 않았습니다.");
            return 5;
        }

        public List<SkillCardEntry> GetAllCards() => skillDeck;

        public List<ScriptableObject> GetPassiveEffects() => passiveEffects; // 필요 시 public getter도 제공
    }
}
