using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Core;

namespace Game.CharacterSystem.Data
{
    [CreateAssetMenu(menuName = "Game/Character/Enemy Character Data")]
    public class EnemyCharacterData : ScriptableObject
    {
        [field: Header("기본 정보")]
        [field: SerializeField] public string DisplayName { get; private set; }
        [field: SerializeField] public int MaxHP { get; private set; }
        [field: SerializeField] public Sprite Portrait { get; private set; }

        [field: Header("프리팹 참조")]
        [field: SerializeField] public GameObject Prefab { get; private set; }

        [System.Serializable]
        public class SkillCardEntry
        {
            public EnemySkillCard Card;
        }

        [field: Header("고정 스킬 카드 덱")]
        [field: SerializeField] public List<SkillCardEntry> SkillDeck { get; private set; }

        [field: Header("패시브 이펙트")]
        [field: SerializeField] public List<ScriptableObject> PassiveEffects { get; private set; }

        public List<SkillCardEntry> GetAllCards() => SkillDeck;
        public List<ScriptableObject> GetPassiveEffects() => PassiveEffects;

        public SkillCardEntry GetRandomEntry()
        {
            if (SkillDeck == null || SkillDeck.Count == 0)
            {
                Debug.LogWarning($"[EnemyCharacterData] '{DisplayName}'의 스킬 덱이 비어 있습니다.");
                return null;
            }

            int index = Random.Range(0, SkillDeck.Count);
            return SkillDeck[index];
        }
    }
}
