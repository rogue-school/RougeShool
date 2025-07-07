using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Data;


namespace AnimationSystem.Data
{
    /// <summary>
    /// 플레이어 캐릭터 애니메이션 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerCharacterAnimationDatabase", menuName = "Animation System/Player Character Animation Database")]
    public class PlayerCharacterAnimationDatabase : ScriptableObject
    {
        [Header("플레이어 캐릭터 애니메이션 매핑")]
        [SerializeField] private List<PlayerCharacterAnimationEntry> characterAnimations = new();
        public List<PlayerCharacterAnimationEntry> CharacterAnimations => characterAnimations;
    }

    /// <summary>
    /// 플레이어 캐릭터 애니메이션 항목
    /// </summary>
    [System.Serializable]
    public class PlayerCharacterAnimationEntry
    {
        [Header("캐릭터 데이터 참조")]
        [SerializeField] private PlayerCharacterData playerCharacterData;
        public PlayerCharacterData PlayerCharacterData => playerCharacterData;

        [Header("생성 애니메이션 (Spawn Animation)")]
        [SerializeField] private AnimationSystem.Data.CharacterAnimationSettings spawnAnimation = CharacterAnimationSettings.Default;
        [Header("사망 애니메이션 (Death Animation)")]
        [SerializeField] private AnimationSystem.Data.CharacterAnimationSettings deathAnimation = CharacterAnimationSettings.Default;

        public AnimationSystem.Data.CharacterAnimationSettings SpawnAnimation => spawnAnimation;
        public AnimationSystem.Data.CharacterAnimationSettings DeathAnimation => deathAnimation;
    }
} 