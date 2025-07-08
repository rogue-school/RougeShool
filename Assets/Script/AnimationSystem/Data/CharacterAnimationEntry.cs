using UnityEngine;
using Game.CharacterSystem.Data;

namespace AnimationSystem.Data
{
    [System.Serializable]
    public class PlayerCharacterAnimationEntry
    {
        [Header("플레이어 캐릭터 데이터")]
        [Tooltip("애니메이션을 지정할 플레이어 캐릭터 데이터 (PlayerCharacterData만 선택)")]
        [SerializeField] private PlayerCharacterData playerCharacter;
        public PlayerCharacterData PlayerCharacter => playerCharacter;

        [Header("생성 애니메이션")]
        [Tooltip("캐릭터가 등장할 때 사용할 애니메이션")] 
        [SerializeField] private CharacterAnimationSettings spawnAnimation = CharacterAnimationSettings.Default;
        public CharacterAnimationSettings SpawnAnimation => spawnAnimation;

        [Header("사망 애니메이션")]
        [Tooltip("캐릭터가 사망할 때 사용할 애니메이션")] 
        [SerializeField] private CharacterAnimationSettings deathAnimation = CharacterAnimationSettings.Default;
        public CharacterAnimationSettings DeathAnimation => deathAnimation;

        public CharacterAnimationSettings GetSettingsByType(string type)
        {
            switch(type)
            {
                case "spawn": return SpawnAnimation;
                case "death": return DeathAnimation;
                default: return CharacterAnimationSettings.Default;
            }
        }
    }

    [System.Serializable]
    public class EnemyCharacterAnimationEntry
    {
        [Header("적 캐릭터 데이터")]
        [Tooltip("애니메이션을 지정할 적 캐릭터 데이터 (EnemyCharacterData만 선택)")]
        [SerializeField] private EnemyCharacterData enemyCharacter;
        public EnemyCharacterData EnemyCharacter => enemyCharacter;

        [Header("생성 애니메이션")]
        [Tooltip("캐릭터가 등장할 때 사용할 애니메이션")] 
        [SerializeField] private CharacterAnimationSettings spawnAnimation = CharacterAnimationSettings.Default;
        public CharacterAnimationSettings SpawnAnimation => spawnAnimation;

        [Header("사망 애니메이션")]
        [Tooltip("캐릭터가 사망할 때 사용할 애니메이션")] 
        [SerializeField] private CharacterAnimationSettings deathAnimation = CharacterAnimationSettings.Default;
        public CharacterAnimationSettings DeathAnimation => deathAnimation;

        public CharacterAnimationSettings GetSettingsByType(string type)
        {
            switch(type)
            {
                case "spawn": return SpawnAnimation;
                case "death": return DeathAnimation;
                default: return CharacterAnimationSettings.Default;
            }
        }
    }
} 