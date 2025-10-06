using System.Collections.Generic;
using UnityEngine;

namespace Game.SkillCardSystem.UI
{
    /// <summary>
    /// 메인 툴팁 표시를 위한 데이터 모델입니다.
    /// 프리팹 폭은 외부(프리팹)에서 결정하고, 본 모델은 내용과 서식을 포함합니다.
    /// </summary>
    public sealed class TooltipModel
    {
        public string Title { get; set; }
        public Sprite Icon { get; set; }
        public string CardType { get; set; }
        public string DescriptionRichText { get; set; }

        public readonly List<StatRow> StatRows = new();
        public readonly List<EffectRow> Effects = new();

        public sealed class StatRow
        {
            public string Label;
            public string Value;
            public Sprite Icon;
            public Color Color = Color.white;
        }

        public sealed class EffectRow
        {
            public string Name;
            public string Description;
            public Color Color = Color.white;
        }
    }

    /// <summary>
    /// 서브 툴팁(효과 상세)을 위한 데이터 모델입니다.
    /// </summary>
    public sealed class SubTooltipModel
    {
        public string Name { get; set; }
        public string DescriptionRichText { get; set; }
        public Sprite Icon { get; set; }
        public int RemainingTurns { get; set; }
        public readonly List<(string key, string value)> ExtraPairs = new();
    }
}


