using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Deck;

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

        [field: Header("스킬 덱 (확률 기반)")]
        [field: SerializeField] public EnemySkillDeck EnemyDeck { get; private set; }

        [field: Header("패시브 이펙트")]
        [field: SerializeField] public List<ScriptableObject> PassiveEffects { get; private set; }
    }
}
