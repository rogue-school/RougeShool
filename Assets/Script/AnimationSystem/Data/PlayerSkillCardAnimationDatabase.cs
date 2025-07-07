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
        [SerializeField] private List<PlayerSkillCardAnimationEntry> skillCardAnimations = new();
        public List<PlayerSkillCardAnimationEntry> SkillCardAnimations => skillCardAnimations;
    }

    /// <summary>
    /// 플레이어용 스킬카드 애니메이션 항목
    /// </summary>
    [System.Serializable]
    public class PlayerSkillCardAnimationEntry
    {
        [Header("스킬카드 데이터 참조")]
        [SerializeField] private PlayerSkillCard skillCardData;
        public PlayerSkillCard SkillCardData => skillCardData;

        [Header("생성 애니메이션 (Cast Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings castAnimation = SkillCardAnimationSettings.Default;
        [Header("카드 사용 애니메이션 (Use Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings useAnimation = SkillCardAnimationSettings.Default;
        [Header("카드 호버 애니메이션 (Hover Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings hoverAnimation = SkillCardAnimationSettings.Default;
        [Header("카드 소멸 애니메이션 (Vanish Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings vanishAnimation = SkillCardAnimationSettings.Default;
        public AnimationSystem.Data.SkillCardAnimationSettings CastAnimation => castAnimation;
        public AnimationSystem.Data.SkillCardAnimationSettings UseAnimation => useAnimation;
        public AnimationSystem.Data.SkillCardAnimationSettings HoverAnimation => hoverAnimation;
        public AnimationSystem.Data.SkillCardAnimationSettings VanishAnimation => vanishAnimation;
    }
} 