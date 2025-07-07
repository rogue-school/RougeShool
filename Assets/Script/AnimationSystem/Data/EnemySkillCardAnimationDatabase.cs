using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Core;


namespace AnimationSystem.Data
{
    /// <summary>
    /// 적용(적) 스킬카드 애니메이션 데이터베이스
    /// </summary>
    [CreateAssetMenu(fileName = "EnemySkillCardAnimationDatabase", menuName = "Animation System/Enemy SkillCard Animation Database")]
    public class EnemySkillCardAnimationDatabase : ScriptableObject
    {
        [Header("적 스킬카드 애니메이션 매핑")]
        [SerializeField] private List<EnemySkillCardAnimationEntry> skillCardAnimations = new();
        public List<EnemySkillCardAnimationEntry> SkillCardAnimations => skillCardAnimations;
    }

    /// <summary>
    /// 적용(적) 스킬카드 애니메이션 항목
    /// </summary>
    [System.Serializable]
    public class EnemySkillCardAnimationEntry
    {
        [Header("스킬카드 데이터 참조")]
        [SerializeField] private EnemySkillCard skillCardData;
        public EnemySkillCard SkillCardData => skillCardData;

        [Header("생성 애니메이션 (Cast Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings castAnimation = SkillCardAnimationSettings.Default;
        [Header("슬롯 이동 애니메이션 (Slot Move Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings slotMoveAnimation = SkillCardAnimationSettings.Default;
        [Header("전투 슬롯 등록/이동 애니메이션 (Battle Slot Place/Move Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings battleSlotPlaceAnimation = SkillCardAnimationSettings.Default;
        [Header("카드 사용 애니메이션 (Use Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings useAnimation = SkillCardAnimationSettings.Default;
        [Header("카드 소멸 애니메이션 (Vanish Animation)")]
        [SerializeField] private AnimationSystem.Data.SkillCardAnimationSettings vanishAnimation = SkillCardAnimationSettings.Default;
        public AnimationSystem.Data.SkillCardAnimationSettings VanishAnimation => vanishAnimation;

        public AnimationSystem.Data.SkillCardAnimationSettings CastAnimation => castAnimation;
        public AnimationSystem.Data.SkillCardAnimationSettings SlotMoveAnimation => slotMoveAnimation;
        public AnimationSystem.Data.SkillCardAnimationSettings BattleSlotPlaceAnimation => battleSlotPlaceAnimation;
        public AnimationSystem.Data.SkillCardAnimationSettings UseAnimation => useAnimation;
    }
} 