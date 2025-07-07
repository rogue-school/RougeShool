using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Data;


namespace AnimationSystem.Data
{
    /// <summary>
    /// 적 캐릭터 애니메이션 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "EnemyCharacterAnimationDatabase", menuName = "Animation System/Enemy Character Animation Database")]
    public class EnemyCharacterAnimationDatabase : ScriptableObject
    {
        [Header("적 캐릭터 애니메이션 매핑")]
        [SerializeField] private List<EnemyCharacterAnimationEntry> characterAnimations = new();
        public List<EnemyCharacterAnimationEntry> CharacterAnimations => characterAnimations;
    }

    /// <summary>
    /// 적 캐릭터 애니메이션 항목
    /// </summary>
    [System.Serializable]
    public class EnemyCharacterAnimationEntry
    {
        [Header("캐릭터 데이터 참조")]
        [SerializeField] private EnemyCharacterData enemyCharacterData;
        public EnemyCharacterData EnemyCharacterData => enemyCharacterData;

        [Header("생성 애니메이션 (Spawn Animation)")]
        [SerializeField] private AnimationSystem.Data.CharacterAnimationSettings spawnAnimation = CharacterAnimationSettings.Default;
        [Header("사망 애니메이션 (Death Animation)")]
        [SerializeField] private AnimationSystem.Data.CharacterAnimationSettings deathAnimation = CharacterAnimationSettings.Default;

        public AnimationSystem.Data.CharacterAnimationSettings SpawnAnimation => spawnAnimation;
        public AnimationSystem.Data.CharacterAnimationSettings DeathAnimation => deathAnimation;
    }
} 