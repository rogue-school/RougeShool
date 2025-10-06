using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.UI.Mappers
{
    /// <summary>
    /// IPerTurnEffect → SubTooltipModel 변환.
    /// </summary>
    public static class PerTurnEffectTooltipMapper
    {
        public static SubTooltipModel From(IPerTurnEffect effect)
        {
            var m = new SubTooltipModel();
            if (effect == null) return m;

            var typeName = effect.GetType().Name;
            m.Name = Normalize(typeName);
            m.DescriptionRichText = string.Empty;
            m.RemainingTurns = effect.RemainingTurns;
            m.ExtraPairs.Add(("남은 턴", effect.RemainingTurns.ToString()));

            // 리플렉션으로 일반적인 런타임 수치 추출(있을 때만)
            TryAddNumeric(effect, m, new[] { "Amount", "Value", "Power", "Intensity" }, "수치");
            TryAddNumeric(effect, m, new[] { "DamagePerTurn", "Damage", "BleedAmount" }, "데미지");
            TryAddNumeric(effect, m, new[] { "HealPerTurn", "HealAmount" }, "치유량");
            TryAddNumeric(effect, m, new[] { "Shield", "Guard", "Block" }, "가드");
            TryAddNumeric(effect, m, new[] { "Stack", "Stacks" }, "스택");
            return m;
        }

        private static string Normalize(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return "효과";
            return raw.Replace("Effect", string.Empty).Replace("Buff", string.Empty);
        }

        private static void TryAddNumeric(IPerTurnEffect effect, SubTooltipModel m, string[] propNames, string label)
        {
            var t = effect.GetType();
            foreach (var name in propNames)
            {
                var pi = t.GetProperty(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (pi != null && pi.CanRead)
                {
                    var val = pi.GetValue(effect);
                    if (val is int iv)
                    {
                        m.ExtraPairs.Add((label, iv.ToString()));
                        return;
                    }
                    if (val is float fv)
                    {
                        m.ExtraPairs.Add((label, fv.ToString("0.##")));
                        return;
                    }
                }
                var fi = t.GetField(name, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                if (fi != null)
                {
                    var val = fi.GetValue(effect);
                    if (val is int iv2)
                    {
                        m.ExtraPairs.Add((label, iv2.ToString()));
                        return;
                    }
                    if (val is float fv2)
                    {
                        m.ExtraPairs.Add((label, fv2.ToString("0.##")));
                        return;
                    }
                }
            }
        }
    }
}


