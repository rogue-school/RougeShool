using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;

namespace Game.AnimationSystem.Data
{
    /// <summary>
    /// 통합된 스킬카드 애니메이션 데이터베이스
    /// 플레이어와 적 스킬카드를 하나의 데이터베이스에서 관리합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "UnifiedSkillCardAnimationDatabase", menuName = "Animation System/Unified SkillCard Animation Database")]
    public class UnifiedSkillCardAnimationDatabase : ScriptableObject
    {
        [Header("통합 스킬카드 애니메이션 매핑")]
        [SerializeField] private List<UnifiedSkillCardAnimationEntry> skillCardAnimations = new();
        public List<UnifiedSkillCardAnimationEntry> SkillCardAnimations => skillCardAnimations;
        
        /// <summary>
        /// 카드 ID로 애니메이션 엔트리를 찾습니다.
        /// </summary>
        public UnifiedSkillCardAnimationEntry FindEntryByCardId(string cardId)
        {
            return skillCardAnimations.Find(entry => 
                entry.SkillCardDefinition != null && 
                entry.SkillCardDefinition.cardId == cardId);
        }
        
        /// <summary>
        /// 카드 이름으로 애니메이션 엔트리를 찾습니다.
        /// </summary>
        public UnifiedSkillCardAnimationEntry FindEntryByCardName(string cardName)
        {
            return skillCardAnimations.Find(entry => 
                entry.SkillCardDefinition != null && 
                entry.SkillCardDefinition.displayName == cardName);
        }
    }
}
