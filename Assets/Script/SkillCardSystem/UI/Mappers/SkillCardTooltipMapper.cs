using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
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
            var ruleLines = new System.Collections.Generic.List<string>();
            var previewLines = new System.Collections.Generic.List<string>();
            if (!string.IsNullOrEmpty(def.description))
            {
                ruleLines.Add(def.description);
            }
            model.Icon = def.artwork;

            // 효과 요약(간단) 채우기 - 기존 구성 로직과 동일한 조건 사용
            var config = def.configuration;
            if (config != null)
            {
                if (config.hasDamage && config.damageConfig != null)
                {
                    var dmg = config.damageConfig.baseDamage;
                    var hits = Mathf.Max(1, config.damageConfig.hits);
                    if (hits <= 1)
                    {
                        ruleLines.Add($"대상에게 피해 {dmg}를 줍니다.");
                        previewLines.Add($"예상 피해(기본): {dmg}");
                    }
                    else
                    {
                        ruleLines.Add($"피해 {dmg}를 {hits}번 줍니다.");
                        previewLines.Add($"총 예상 피해(기본): {dmg * hits} (각 {dmg} × {hits})");
                    }
                    if (config.damageConfig.ignoreGuard)
                    {
                        ruleLines.Add("가드를 무시합니다.");
                    }
                    if (config.damageConfig.ignoreCounter)
                    {
                        ruleLines.Add("반격을 무시합니다.");
                    }

                    model.Effects.Add(new TooltipModel.EffectRow
                    {
                        Name = "데미지",
                        Description = $"기본 {dmg}{(hits > 1 ? $", {hits}회" : string.Empty)}",
                        Color = UnityEngine.Color.red
                    });
                }

                if (config.hasEffects && config.effects != null)
                {
                    foreach (var effectConfig in config.effects)
                    {
                        var so = effectConfig.effectSO;
                        var cs = effectConfig.useCustomSettings ? effectConfig.customSettings : null;

                        if (so is DamageEffectSO && cs != null && cs.damageAmount > 0)
                        {
                            var dmg = cs.damageAmount;
                            var hits = Mathf.Max(1, cs.damageHits);
                            if (hits <= 1)
                            {
                                ruleLines.Add($"대상에게 피해 {dmg}를 줍니다.");
                                previewLines.Add($"예상 피해(기본): {dmg}");
                            }
                            else
                            {
                                ruleLines.Add($"피해 {dmg}를 {hits}번 줍니다.");
                                previewLines.Add($"총 예상 피해(기본): {dmg * hits} (각 {dmg} × {hits})");
                            }
                        }

                        if (so is BleedEffectSO)
                        {
                            if (cs != null && cs.bleedAmount > 0)
                            {
                                ruleLines.Add($"출혈 {cs.bleedAmount} (지속 {cs.bleedDuration}턴). 턴 종료마다 피해 {cs.bleedAmount}.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "출혈",
                                Description = cs != null ? $"{cs.bleedAmount}, {cs.bleedDuration}턴" : "적용",
                                Color = UnityEngine.Color.red
                            });
                        }

                        if (so is HealEffectSO)
                        {
                            if (cs != null && cs.healAmount > 0)
                            {
                                ruleLines.Add($"체력을 {cs.healAmount} 회복합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "치유",
                                Description = cs != null ? $"{cs.healAmount}" : "회복",
                                Color = UnityEngine.Color.green
                            });
                        }

                        if (so is GuardEffectSO)
                        {
                            ruleLines.Add("가드 1을 얻습니다.");
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "가드",
                                Description = "+1",
                                Color = UnityEngine.Color.blue
                            });
                        }

                        if (so is StunEffectSO)
                        {
                            if (cs != null && cs.stunDuration > 0)
                            {
                                ruleLines.Add($"대상을 {cs.stunDuration}턴 동안 기절시킵니다.");
                            }
                            else
                            {
                                ruleLines.Add("대상을 기절시킵니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "스턴",
                                Description = cs != null && cs.stunDuration > 0 ? $"{cs.stunDuration}턴" : "적용",
                                Color = UnityEngine.Color.red
                            });
                        }

                        if (so is CounterEffectSO)
                        {
                            if (cs != null && cs.counterDuration > 0)
                            {
                                ruleLines.Add($"{cs.counterDuration}턴 동안 공격받으면 즉시 반격합니다.");
                            }
                            else
                            {
                                ruleLines.Add("공격받으면 즉시 반격합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "반격",
                                Description = cs != null && cs.counterDuration > 0 ? $"{cs.counterDuration}턴" : "적용",
                                Color = UnityEngine.Color.yellow
                            });
                        }

                        if (so is CardUseStackEffectSO)
                        {
                            if (cs != null && cs.stackIncreasePerUse > 0)
                            {
                                var inc = cs.stackIncreasePerUse;
                                var max = cs.maxStacks;
                                ruleLines.Add(max > 0 ? $"사용 시 스택 {inc} 증가합니다(최대 {max})." : $"사용 시 스택 {inc} 증가합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "스택",
                                Description = cs != null && cs.stackIncreasePerUse > 0 ? $"+{cs.stackIncreasePerUse}" : "+스택",
                                Color = new Color(0.8f, 0.8f, 1f)
                            });
                        }

                        if (cs != null && cs.resourceDelta != 0)
                        {
                            var text = cs.resourceDelta > 0 ? "획득" : "소모";
                            ruleLines.Add($"자원 {text}: {Mathf.Abs(cs.resourceDelta)}");
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = "자원",
                                Description = $"{text} {Mathf.Abs(cs.resourceDelta)}",
                                Color = cs.resourceDelta > 0 ? UnityEngine.Color.green : UnityEngine.Color.red
                            });
                        }
                    }
                }
            }
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < ruleLines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(ruleLines[i])) sb.AppendLine(ruleLines[i]);
            }
            for (int i = 0; i < previewLines.Count; i++)
            {
                if (!string.IsNullOrWhiteSpace(previewLines[i])) sb.AppendLine(previewLines[i]);
            }
            model.DescriptionRichText = SimpleRich.EmphasizeNumbers(sb.ToString().TrimEnd());
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


