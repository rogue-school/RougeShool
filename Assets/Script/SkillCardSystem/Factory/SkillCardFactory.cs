using UnityEngine;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;
using Game.SkillCardSystem.Effects;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 스킬 카드 런타임 객체를 생성하는 팩토리입니다.
    /// SRP: 런타임 카드 인스턴스를 생성하는 책임만 가집니다.
    /// DIP: SkillCardData, Effect 정보를 기반으로 생성합니다.
    /// </summary>
    public class SkillCardFactory : ISkillCardFactory
    {
        public ISkillCard CreateEnemyCard(SkillCardData data, List<SkillCardEffectSO> effects)
        {
            if (data == null)
            {
                Debug.LogError("[SkillCardFactory] Enemy SkillCardData가 null입니다.");
                return null;
            }

            if (effects == null)
            {
                Debug.LogWarning("[SkillCardFactory] EnemyCard 효과 리스트가 null입니다. 빈 리스트로 대체합니다.");
                effects = new List<SkillCardEffectSO>();
            }

            return new EnemySkillCardRuntime(data, CloneEffects(effects));
        }

        public ISkillCard CreatePlayerCard(SkillCardData data, List<SkillCardEffectSO> effects)
        {
            if (data == null)
            {
                Debug.LogError("[SkillCardFactory] Player SkillCardData가 null입니다.");
                return null;
            }

            if (effects == null)
            {
                Debug.LogWarning("[SkillCardFactory] PlayerCard 효과 리스트가 null입니다. 빈 리스트로 대체합니다.");
                effects = new List<SkillCardEffectSO>();
            }

            return new PlayerSkillCardRuntime(data, CloneEffects(effects));
        }

        /// <summary>
        /// 효과 리스트를 복제합니다. (현재는 얕은 복사)
        /// 필요 시 ScriptableObject 복제 또는 DeepCopy 구조로 확장 가능합니다.
        /// </summary>
        private List<SkillCardEffectSO> CloneEffects(List<SkillCardEffectSO> original)
        {
            return new List<SkillCardEffectSO>(original);
        }
    }
}
