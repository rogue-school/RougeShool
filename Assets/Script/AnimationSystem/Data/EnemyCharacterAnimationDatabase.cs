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
        [SerializeField] private List<AnimationSystem.Data.EnemyCharacterAnimationEntry> characterAnimations = new();
        public List<AnimationSystem.Data.EnemyCharacterAnimationEntry> CharacterAnimations => characterAnimations;
    }
} 