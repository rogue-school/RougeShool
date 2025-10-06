using Game.SkillCardSystem.UI;

namespace Game.SkillCardSystem.UI.Mappers
{
    /// <summary>
    /// SkillCardTooltip.EffectData → SubTooltipModel 변환 유틸.
    /// </summary>
    public static class EffectTooltipMapper
    {
        public static SubTooltipModel From(SkillCardTooltip.EffectData data)
        {
            var m = new SubTooltipModel();
            if (data == null) return m;
            m.Name = data.name;
            m.DescriptionRichText = data.description;
            return m;
        }
    }
}


