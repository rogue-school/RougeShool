using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;


namespace AnimationSystem.Data
{
    /// <summary>
    /// 플레이어용 스킬카드 애니메이션 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "PlayerSkillCardAnimationDatabase", menuName = "Animation System/Player SkillCard Animation Database")]
    public class PlayerSkillCardAnimationDatabase : ScriptableObject
    {
        [Header("플레이어 스킬카드 애니메이션 매핑")]
        [SerializeField] private List<AnimationSystem.Data.PlayerSkillCardAnimationEntry> skillCardAnimations = new();
        public List<AnimationSystem.Data.PlayerSkillCardAnimationEntry> SkillCardAnimations => skillCardAnimations;
    }
} 