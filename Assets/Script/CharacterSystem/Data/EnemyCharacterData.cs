using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Core;

namespace Game.CharacterSystem.Data
{
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject
    {
        [Header("기본 정보")]
        public string displayName;
        public int maxHP;
        public Sprite portrait;

        [Header("프리팹 참조")]
        public GameObject prefab;

        [System.Serializable]
        public class SkillCardEntry
        {
            public EnemySkillCard card;
        }

        [Header("고정 스킬 카드 덱")]
        [SerializeField] private List<SkillCardEntry> skillDeck = new();

        [Header("패시브 이펙트 (Regen, Buff 등)")]
        [SerializeField] private List<ScriptableObject> passiveEffects = new();

        /// <summary>
        /// 고정 덱에서 무작위 카드를 반환합니다.
        /// </summary>
        public SkillCardEntry GetRandomEntry()
        {
            if (skillDeck == null || skillDeck.Count == 0)
            {
                Debug.LogWarning($"[EnemyCharacterData] '{displayName}'의 스킬 덱이 비어 있습니다.");
                return null;
            }

            int index = Random.Range(0, skillDeck.Count);
            return skillDeck[index];
        }

        /// <summary>
        /// 전체 스킬 카드 덱 반환
        /// </summary>
        public List<SkillCardEntry> GetAllCards() => skillDeck;

        /// <summary>
        /// 패시브 이펙트 목록 반환
        /// </summary>
        public List<ScriptableObject> GetPassiveEffects() => passiveEffects;
    }
}
