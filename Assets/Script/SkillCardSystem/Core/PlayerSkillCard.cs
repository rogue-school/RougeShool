using UnityEngine;
using Game.SkillCardSystem.Effects;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Data
{
    [CreateAssetMenu(menuName = "Game/SkillCard/Player Skill Card")]
    public class PlayerSkillCard : ScriptableObject
    {
        [Header("카드 데이터")]
        public SkillCardData CardData;
        public int GetCoolTime() => CardData.CoolTime;

        [Header("카드 효과 목록")]
        [SerializeField] private List<SkillCardEffectSO> effects;

        public List<SkillCardEffectSO> CreateEffects()
        {
            return effects ?? new List<SkillCardEffectSO>();
        }
    }
}
