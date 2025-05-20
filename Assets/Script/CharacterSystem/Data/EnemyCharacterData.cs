using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Core;

namespace Game.CharacterSystem.Data
{
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject
    {
        public string displayName;
        public int maxHP;
        public Sprite portrait;

        [Header("프리팹 참조")]
        public GameObject prefab;

        [System.Serializable]
        public class SkillCardEntry
        {
            public EnemySkillCard card;
            // public int damage; // 제거됨
        }

        [Header("고정 스킬 카드 덱")]
        [SerializeField] private List<SkillCardEntry> skillDeck = new();

        [Header("패시브 이펙트 (Regen, Buff 등)")]
        [SerializeField] private List<ScriptableObject> passiveEffects = new();

        /// <summary>
        /// 스킬 카드 덱에서 무작위 카드 1장을 반환합니다.
        /// </summary>
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

        /// <summary>
        /// 주어진 카드의 데미지를 반환합니다. 카드 자체의 CardData에서 추출됩니다.
        /// </summary>
        public int GetDamageOfCard(EnemySkillCard card)
        {
            return card?.CardData.Damage ?? 5; // 기본값 5
        }

        public List<SkillCardEntry> GetAllCards() => skillDeck;

        public List<ScriptableObject> GetPassiveEffects() => passiveEffects;
    }
}
