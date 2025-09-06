using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;


namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 적용(적) 스킬카드 애니메이션 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySkillCardAnimationDatabase", menuName = "Animation System/Enemy SkillCard Animation Database")]
    public class EnemySkillCardAnimationDatabase : ScriptableObject
    {
        [Header("적 스킬카드 애니메이션 매핑")]
        [SerializeField] private List<AnimationSystem.Data.EnemySkillCardAnimationEntry> skillCardAnimations = new();
        public List<AnimationSystem.Data.EnemySkillCardAnimationEntry> SkillCardAnimations => skillCardAnimations;
    }
} 