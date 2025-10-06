using Game.SkillCardSystem.Data;
using UnityEngine;

namespace Game.SkillCardSystem.UI.Mappers
{
    /// <summary>
    /// SkillCardDefinition을 TooltipModel로 변환합니다.
    /// (현 단계: 이름/타입/설명만 매핑)
    /// </summary>
    public static class SkillCardTooltipMapper
    {
        public static TooltipModel From(SkillCardDefinition def)
        {
            var model = new TooltipModel();
            if (def == null) return model;

            model.Title = string.IsNullOrEmpty(def.displayNameKO) ? def.displayName : def.displayNameKO;
            model.CardType = GetTypeText(def);
            // 간단 강조: 숫자/퍼센트에 <color> 태그 적용 (과한 파싱은 지양)
            model.DescriptionRichText = SimpleRich.EmphasizeNumbers(def.description ?? string.Empty);
            model.Icon = def.artwork;

            // 효과 요약(간단) 채우기 - 기존 구성 로직과 동일한 조건 사용
            var config = def.configuration;
            if (config != null)
            {
                if (config.hasDamage)
                {
                    model.Effects.Add(new TooltipModel.EffectRow
                    {
                        Name = "데미지",
                        Description = $"기본 데미지: {config.damageConfig.baseDamage}",
                        Color = UnityEngine.Color.red
                    });
                }

                if (config.hasEffects && config.effects != null)
                {
                    foreach (var effectConfig in config.effects)
                    {
                        if (effectConfig.useCustomSettings && effectConfig.customSettings != null)
                        {
                            var cs = effectConfig.customSettings;
                            if (cs.bleedAmount > 0)
                            {
                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = "출혈",
                                    Description = $"출혈량: {cs.bleedAmount}, 지속: {cs.bleedDuration}턴",
                                    Color = UnityEngine.Color.red
                                });
                            }
                            if (cs.counterDuration > 0)
                            {
                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = "반격",
                                    Description = $"반격 지속: {cs.counterDuration}턴",
                                    Color = UnityEngine.Color.yellow
                                });
                            }
                            if (cs.guardDuration > 0)
                            {
                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = "가드",
                                    Description = $"가드 지속: {cs.guardDuration}턴",
                                    Color = UnityEngine.Color.blue
                                });
                            }
                            if (cs.stunDuration > 0)
                            {
                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = "스턴",
                                    Description = $"스턴 지속: {cs.stunDuration}턴",
                                    Color = UnityEngine.Color.red
                                });
                            }
                            if (cs.healAmount > 0)
                            {
                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = "치유",
                                    Description = $"치유량: {cs.healAmount}",
                                    Color = UnityEngine.Color.green
                                });
                            }
                            if (cs.resourceDelta != 0)
                            {
                                var text = cs.resourceDelta > 0 ? "획득" : "소모";
                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = "자원",
                                    Description = $"자원 {text}: {Mathf.Abs(cs.resourceDelta)}",
                                    Color = cs.resourceDelta > 0 ? UnityEngine.Color.green : UnityEngine.Color.red
                                });
                            }
                        }
                    }
                }
            }
            return model;
        }

        private static string GetTypeText(SkillCardDefinition def)
        {
            if (def?.configuration?.hasDamage == true) return "공격 카드";
            if (def?.configuration?.hasEffects == true) return "효과 카드";
            return "기본 카드";
        }
    }
    
    internal static class SimpleRich
    {
        private static readonly System.Text.RegularExpressions.Regex numberRegex = new("(\\d+%?)");
        public static string EmphasizeNumbers(string src)
        {
            if (string.IsNullOrEmpty(src)) return src;
            return numberRegex.Replace(src, "<b><color=#FFD54F>$1</color></b>");
        }
    }
}


