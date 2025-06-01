using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effects;
using System.Collections.Generic;

namespace Game.SkillCardSystem.Interface
{
    public interface ISkillCardFactory
    {
        ISkillCard CreateEnemyCard(SkillCardData data, List<SkillCardEffectSO> effects);
        ISkillCard CreatePlayerCard(SkillCardData data, List<SkillCardEffectSO> effects);
    }
}
