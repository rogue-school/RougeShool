using UnityEngine;
using System.Collections.Generic;
using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Runtime;

namespace Game.SkillCardSystem.Factory
{
    /// <summary>
    /// 스킬 카드 런타임 인스턴스를 생성하는 팩토리 클래스입니다.
    /// <para>SRP: 카드 인스턴스 생성만 담당</para>
    /// <para>DIP: SkillCardData와 Effect 데이터에 의존</para>
    /// </summary>
    public class SkillCardFactory : ISkillCardFactory
    {
        /// <summary>
        /// 적 캐릭터용 스킬 카드 런타임 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="data">카드 데이터</param>
        /// <param name="effects">카드 효과 리스트</param>
        /// <returns>생성된 적 카드 런타임 인스턴스</returns>
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

        /// <summary>
        /// 플레이어용 스킬 카드 런타임 인스턴스를 생성합니다.
        /// </summary>
        /// <param name="data">카드 데이터</param>
        /// <param name="effects">카드 효과 리스트</param>
        /// <returns>생성된 플레이어 카드 런타임 인스턴스</returns>
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
        /// 효과 리스트를 복제합니다. 현재는 얕은 복사이며, 필요 시 깊은 복사로 확장 가능합니다.
        /// </summary>
        /// <param name="original">원본 효과 리스트</param>
        /// <returns>복제된 효과 리스트</returns>
        private List<SkillCardEffectSO> CloneEffects(List<SkillCardEffectSO> original)
        {
            return new List<SkillCardEffectSO>(original);
        }
    }
}
