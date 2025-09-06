using UnityEngine;
using System.Collections.Generic;
using Game.CharacterSystem.Data;


namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 플레이어 캐릭터 애니메이션 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerCharacterAnimationDatabase", menuName = "Animation System/Player Character Animation Database")]
    public class PlayerCharacterAnimationDatabase : ScriptableObject
    {
        [Header("플레이어 캐릭터 애니메이션 매핑")]
        [SerializeField] private List<AnimationSystem.Data.PlayerCharacterAnimationEntry> characterAnimations = new();
        public List<AnimationSystem.Data.PlayerCharacterAnimationEntry> CharacterAnimations => characterAnimations;
    }
} 