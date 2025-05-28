using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Data;

namespace Game.SkillCardSystem.Core
{
    [CreateAssetMenu(menuName = "Game/SkillCard/Enemy Skill Card")]
    public class EnemySkillCard : ScriptableObject
    {
        public SkillCardData CardData;

        [Header("카드 실행 시 적용할 효과 목록")]
        [SerializeField] private List<SkillCardEffectSO> effects = new();

        public List<SkillCardEffectSO> CreateEffects()
        {
            return effects ?? new List<SkillCardEffectSO>();
        }

        public SkillCardData GetCardData() => CardData; // ✅ 추가

        public string GetCardName() => CardData.Name;
        public int GetDamage() => CardData.Damage;
        public int GetCoolTime() => CardData.CoolTime;
    }
}
