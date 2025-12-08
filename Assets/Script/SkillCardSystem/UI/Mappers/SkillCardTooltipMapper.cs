using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
using UnityEngine;
using Game.CoreSystem.Utility;

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
                // 자원 코스트: 효과 요약 및 효과 행에 표시
                if (config.hasResource && config.resourceConfig != null && config.resourceConfig.cost > 0)
                {
                    string resourceName = "자원";
                    var pm0 = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                    if (pm0 != null && !string.IsNullOrEmpty(pm0.ResourceName)) resourceName = pm0.ResourceName;

                    ruleLines.Add($"{resourceName} 소모: {config.resourceConfig.cost}");
                    model.Effects.Add(new TooltipModel.EffectRow
                    {
                        Name = resourceName,
                        Description = $"소모 {config.resourceConfig.cost}",
                        Color = UnityEngine.Color.red
                    });
                }

                if (config.hasDamage && config.damageConfig != null)
                {
                    var dmg = config.damageConfig.baseDamage;
                    var hits = Mathf.Max(1, config.damageConfig.hits);
                    bool useRandom = config.damageConfig.useRandomDamage;
                    var minDmg = Mathf.Max(0, config.damageConfig.minDamage);
                    var maxDmg = Mathf.Max(minDmg, config.damageConfig.maxDamage);

                    if (useRandom)
                    {
                        if (hits <= 1)
                        {
                            ruleLines.Add($"피해 {minDmg}~{maxDmg}을 줍니다.");
                        }
                        else
                        {
                            ruleLines.Add($"피해 {minDmg}~{maxDmg}을 {hits}번 줍니다.");
                        }
                    }
                    else
                    {
                        if (hits <= 1)
                        {
                            ruleLines.Add($"피해 {dmg}를 줍니다.");
                        }
                        else
                        {
                            ruleLines.Add($"피해 {dmg}를 {hits}번 줍니다.");
                        }
                    }

                    if (config.damageConfig.ignoreGuard)
                    {
                        ruleLines.Add("가드를 무시합니다.");
                    }
                    if (config.damageConfig.ignoreCounter)
                    {
                        ruleLines.Add("반격을 무시합니다.");
                    }

                    var damageDesc = useRandom
                        ? $"랜덤 {minDmg}~{maxDmg}{(hits > 1 ? $", {hits}회" : string.Empty)}"
                        : $"기본 {dmg}{(hits > 1 ? $", {hits}회" : string.Empty)}";

                    model.Effects.Add(new TooltipModel.EffectRow
                    {
                        Name = "데미지",
                        Description = damageDesc,
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
                                ruleLines.Add($"피해 {dmg}를 줍니다.");
                            }
                            else
                            {
                                ruleLines.Add($"피해 {dmg}를 {hits}번 줍니다.");
                            }
                        }

                        if (so is BleedEffectSO)
                        {
                            string effectName = GetEffectName(so, "출혈");
                            
                            if (cs != null && cs.bleedAmount > 0)
                            {
                                ruleLines.Add(KoreanTextHelper.FormatBleedEffectText(cs.bleedAmount, cs.bleedDuration, effectName));
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = cs != null ? $"{cs.bleedAmount}, {cs.bleedDuration}턴" : "적용",
                                Color = UnityEngine.Color.red
                            });
                        }
                        {
                            string effectName = GetEffectName(so, "치유");
                            if (cs != null && cs.healAmount > 0)
                            {
                                ruleLines.Add($"체력을 {cs.healAmount} 회복합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = cs != null ? $"{cs.healAmount}" : "회복",
                                Color = UnityEngine.Color.green
                            });
                        }

                        if (so is GuardEffectSO)
                        {
                            string effectName = GetEffectName(so, "가드");
                            ruleLines.Add($"{effectName} 1을 얻습니다.");
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = "+1",
                                Color = UnityEngine.Color.blue
                            });
                        }

                        if (so is StunEffectSO)
                        {
                            string effectName = GetEffectName(so, "스턴");
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
                                Name = effectName,
                                Description = cs != null && cs.stunDuration > 0 ? $"{cs.stunDuration}턴" : "적용",
                                Color = UnityEngine.Color.red
                            });
                        }

                        if (so is CounterEffectSO)
                        {
                            string effectName = GetEffectName(so, "반격");
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
                                Name = effectName,
                                Description = cs != null && cs.counterDuration > 0 ? $"{cs.counterDuration}턴" : "적용",
                                Color = UnityEngine.Color.yellow
                            });
                        }

                        if (so is CardUseStackEffectSO)
                        {
                            string effectName = GetEffectName(so, "스택");
                            if (cs != null && cs.stackIncreasePerUse > 0)
                            {
                                var inc = cs.stackIncreasePerUse;
                                var max = cs.maxStacks;
                                ruleLines.Add(max > 0 ? $"사용 시 스택 {inc} 증가합니다(최대 {max})." : $"사용 시 스택 {inc} 증가합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = cs != null && cs.stackIncreasePerUse > 0 ? $"+{cs.stackIncreasePerUse}" : "+스택",
                                Color = new Color(0.8f, 0.8f, 1f)
                            });
                        }

                        if (cs != null && cs.resourceDelta != 0)
                        {
                            var text = cs.resourceDelta > 0 ? "획득" : "소모";
                            string resourceName = "자원";
                            var pm = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                            if (pm != null && !string.IsNullOrEmpty(pm.ResourceName)) resourceName = pm.ResourceName;
                            ruleLines.Add($"{resourceName} {text}: {Mathf.Abs(cs.resourceDelta)}");
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = resourceName,
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

        /// <summary>
        /// 스택 정보를 포함하여 SkillCardDefinition을 TooltipModel로 변환합니다.
        /// </summary>
        /// <param name="def">카드 정의</param>
        /// <param name="currentStacks">현재 스택 수</param>
        /// <param name="playerCharacter">플레이어 캐릭터 (공격력 버프 확인용)</param>
        /// <param name="card">카드 인스턴스 (데미지 오버라이드 확인용, 선택적)</param>
        /// <returns>툴팁 모델</returns>
        public static TooltipModel FromWithStacks(SkillCardDefinition def, int currentStacks = 0, Game.CharacterSystem.Interface.ICharacter playerCharacter = null, Game.SkillCardSystem.Interface.ISkillCard card = null)
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

            // 효과 요약(간단) 채우기 - 스택 기반 실제 데미지 표시
            var config = def.configuration;
                if (config != null)
            {
                    // 자원 코스트 표시
                    if (config.hasResource && config.resourceConfig != null && config.resourceConfig.cost > 0)
                    {
                        string resourceName = "자원";
                        var pm1 = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                        if (pm1 != null && !string.IsNullOrEmpty(pm1.ResourceName)) resourceName = pm1.ResourceName;
                        ruleLines.Add($"{resourceName} 소모: {config.resourceConfig.cost}");
                        model.Effects.Add(new TooltipModel.EffectRow
                        {
                            Name = resourceName,
                            Description = $"소모 {config.resourceConfig.cost}",
                            Color = UnityEngine.Color.red
                        });
                    }
                if (config.hasDamage && config.damageConfig != null)
                {
                    var baseDmg = card != null ? card.GetBaseDamage() : config.damageConfig.baseDamage;
                    bool useRandom = config.damageConfig.useRandomDamage;
                    var minDmg = Mathf.Max(0, config.damageConfig.minDamage);
                    var maxDmg = Mathf.Max(minDmg, config.damageConfig.maxDamage);
                    var baseMin = minDmg;
                    var baseMax = maxDmg;
                    
                    int attackPotionBonus = 0;
                    if (playerCharacter != null)
                    {
                        attackPotionBonus = GetAttackPotionBonus(playerCharacter);
                    }
                    
                    int enhancementBonus = 0;
                    if (card != null && card.IsFromPlayer())
                    {
                        var itemService = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                        if (itemService != null)
                        {
                            string skillId = def.displayName;
                            enhancementBonus = itemService.GetSkillDamageBonus(skillId);
                        }
                    }
                    
                    var hits = Mathf.Max(1, config.damageConfig.hits);

                    int actualDmg = CalculateActualDamage(baseDmg, currentStacks, attackPotionBonus, enhancementBonus);
                    int actualMin = useRandom ? CalculateActualDamage(minDmg, currentStacks, attackPotionBonus, enhancementBonus) : actualDmg;
                    int actualMax = useRandom ? CalculateActualDamage(maxDmg, currentStacks, attackPotionBonus, enhancementBonus) : actualDmg;
                    bool hasBonus = currentStacks > 0 || attackPotionBonus > 0 || enhancementBonus > 0;

                    if (hits <= 1)
                    {
                        if (useRandom)
                        {
                            var bonusText = "";
                            if (hasBonus)
                            {
                                bonusText = $" (기본 {baseMin}~{baseMax}"
                                            + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                                            + (attackPotionBonus > 0 ? $", 공격력 물약 {attackPotionBonus}" : "")
                                            + (enhancementBonus > 0 ? $", 강화 {enhancementBonus}" : "")
                                            + ")";
                            }
                            ruleLines.Add($"피해 {actualMin}~{actualMax}을 줍니다.{bonusText}");
                        }
                        else
                        {
                            if (hasBonus)
                            {
                                string bonusText = $" (기본 피해 {baseDmg}"
                                    + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                                    + (attackPotionBonus > 0 ? $", 공격력 물약 {attackPotionBonus}" : "")
                                    + (enhancementBonus > 0 ? $", 강화 {enhancementBonus}" : "")
                                    + ")";
                                ruleLines.Add($"피해 {KoreanTextHelper.AddKoreanParticle(actualDmg, "을/를")} 줍니다.{bonusText}");
                            }
                            else
                            {
                                ruleLines.Add($"피해 {KoreanTextHelper.AddKoreanParticle(actualDmg, "을/를")} 줍니다.");
                            }
                        }
                    }
                    else
                    {
                        if (useRandom)
                        {
                            var totalMin = actualMin * hits;
                            var totalMax = actualMax * hits;
                            var bonusText = "";
                            if (hasBonus)
                            {
                                bonusText = $" (기본 {baseMin}~{baseMax}"
                                            + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                                            + (attackPotionBonus > 0 ? $", 공격력 물약 {attackPotionBonus}" : "")
                                            + (enhancementBonus > 0 ? $", 강화 {enhancementBonus}" : "")
                                            + ")";
                            }
                            ruleLines.Add($"피해 {totalMin}~{totalMax} (총 {hits}회){bonusText}");
                        }
                        else
                        {
                            if (hasBonus)
                            {
                                string bonusText = $" (기본 피해 {baseDmg}"
                                    + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                                    + (attackPotionBonus > 0 ? $", 공격력 물약 {attackPotionBonus}" : "")
                                    + (enhancementBonus > 0 ? $", 강화 {enhancementBonus}" : "")
                                    + ")";
                                ruleLines.Add($"피해 {KoreanTextHelper.AddKoreanParticle(actualDmg, "을/를")} {KoreanTextHelper.AddKoreanParticle(hits, "을/를")}번 줍니다.{bonusText}");
                            }
                            else
                            {
                                ruleLines.Add($"피해 {KoreanTextHelper.AddKoreanParticle(actualDmg, "을/를")} {KoreanTextHelper.AddKoreanParticle(hits, "을/를")}번 줍니다.");
                            }
                        }
                    }
                    
                    if (config.damageConfig.ignoreGuard)
                    {
                        ruleLines.Add("가드를 무시합니다.");
                    }
                    if (config.damageConfig.ignoreCounter)
                    {
                        ruleLines.Add("반격을 무시합니다.");
                    }

                    string effectDescription;
                    if (useRandom)
                    {
                        effectDescription = hits <= 1
                            ? $"랜덤 {actualMin}~{actualMax}"
                            : $"랜덤 {actualMin}~{actualMax} (총 {hits}회)";
                    }
                    else
                    {
                        effectDescription = $"기본 {actualDmg}";
                    }
                    
                    model.Effects.Add(new TooltipModel.EffectRow
                    {
                        Name = "데미지",
                        Description = effectDescription,
                        Color = UnityEngine.Color.red
                    });
                }

                // 기타 효과들은 기존과 동일하게 처리
                if (config.hasEffects && config.effects != null)
                {
                    foreach (var effectConfig in config.effects)
                    {
                        var so = effectConfig.effectSO;
                        var cs = effectConfig.useCustomSettings ? effectConfig.customSettings : null;

                        if (so is BleedEffectSO)
                        {
                            string effectName = GetEffectName(so, "출혈");
                            
                            if (cs != null && cs.bleedAmount > 0)
                            {
                                ruleLines.Add(KoreanTextHelper.FormatBleedEffectText(cs.bleedAmount, cs.bleedDuration, effectName));
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = cs != null ? $"{cs.bleedAmount}, {cs.bleedDuration}턴" : "적용",
                                Color = UnityEngine.Color.red
                            });
                        }
                        {
                            string effectName = GetEffectName(so, "치유");
                            if (cs != null && cs.healAmount > 0)
                            {
                                ruleLines.Add($"체력을 {cs.healAmount} 회복합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = cs != null ? $"{cs.healAmount}" : "회복",
                                Color = UnityEngine.Color.green
                            });
                        }

                        if (so is GuardEffectSO)
                        {
                            string effectName = GetEffectName(so, "가드");
                            ruleLines.Add($"{effectName} 1을 얻습니다.");
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = "+1",
                                Color = UnityEngine.Color.blue
                            });
                        }

                        if (so is StunEffectSO)
                        {
                            string effectName = GetEffectName(so, "스턴");
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
                                Name = effectName,
                                Description = cs != null && cs.stunDuration > 0 ? $"{cs.stunDuration}턴" : "적용",
                                Color = UnityEngine.Color.red
                            });
                        }

                        if (so is CounterEffectSO)
                        {
                            string effectName = GetEffectName(so, "반격");
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
                                Name = effectName,
                                Description = cs != null && cs.counterDuration > 0 ? $"{cs.counterDuration}턴" : "적용",
                                Color = UnityEngine.Color.yellow
                            });
                        }

                        if (so is CardUseStackEffectSO)
                        {
                            string effectName = GetEffectName(so, "스택");
                            if (cs != null && cs.stackIncreasePerUse > 0)
                            {
                                var inc = cs.stackIncreasePerUse;
                                var max = cs.maxStacks;
                                ruleLines.Add(max > 0 ? $"사용 시 스택 {inc} 증가합니다(최대 {max})." : $"사용 시 스택 {inc} 증가합니다.");
                            }
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = effectName,
                                Description = cs != null && cs.stackIncreasePerUse > 0 ? $"+{cs.stackIncreasePerUse}" : "+스택",
                                Color = new Color(0.8f, 0.8f, 1f)
                            });
                        }

                        if (cs != null && cs.resourceDelta != 0)
                        {
                            var text = cs.resourceDelta > 0 ? "획득" : "소모";
                            string resourceName = "자원";
                            var pm = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
                            if (pm != null && !string.IsNullOrEmpty(pm.ResourceName)) resourceName = pm.ResourceName;
                            ruleLines.Add($"{resourceName} {text}: {Mathf.Abs(cs.resourceDelta)}");
                            model.Effects.Add(new TooltipModel.EffectRow
                            {
                                Name = resourceName,
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

        /// <summary>
        /// 이펙트 SO에서 이름을 가져옵니다.
        /// </summary>
        /// <param name="effectSO">이펙트 SO</param>
        /// <param name="defaultName">기본 이름</param>
        /// <returns>이펙트 이름</returns>
        private static string GetEffectName(SkillCardEffectSO effectSO, string defaultName)
        {
            if (effectSO == null) return defaultName;
            
            string soName = effectSO.GetEffectName();
            if (!string.IsNullOrWhiteSpace(soName))
            {
                return soName;
            }
            else
            {
                // GetEffectName()이 비어있으면 SO의 name 사용
                return effectSO.name;
            }
        }

        /// <summary>
        /// 카드 타입 텍스트를 반환합니다.
        /// </summary>
        /// <param name="def">카드 정의</param>
        /// <returns>카드 타입 문자열</returns>
        private static string GetTypeText(SkillCardDefinition def)
        {
            if (def?.configuration?.hasDamage == true) return "공격 카드";
            if (def?.configuration?.hasEffects == true) return "효과 카드";
            return "기본 카드";
        }

        /// <summary>
        /// 공격력 물약 버프 보너스를 계산합니다.
        /// </summary>
        /// <param name="playerCharacter">플레이어 캐릭터</param>
        /// <returns>공격력 물약 보너스</returns>
        private static int GetAttackPotionBonus(Game.CharacterSystem.Interface.ICharacter playerCharacter)
        {
            if (playerCharacter == null) return 0;
            
            int totalBonus = 0;
            var buffs = playerCharacter.GetBuffs();
            
            foreach (var effect in buffs)
            {
                if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                {
                    totalBonus += attackBuff.GetAttackPowerBonus();
                }
            }
            
            return totalBonus;
        }

        /// <summary>
        /// 스택 기반 실제 데미지를 계산합니다.
        /// </summary>
        /// <param name="baseDamage">기본 데미지</param>
        /// <param name="currentStacks">현재 스택 수</param>
        /// <param name="attackPotionBonus">공격력 물약 보너스</param>
        /// <returns>실제 적용 데미지</returns>
        private static int CalculateActualDamage(int baseDamage, int currentStacks, int attackPotionBonus = 0, int enhancementBonus = 0)
        {
            // 선형 증가: 기본 데미지 + 스택 수 + 공격력 물약 + 강화 보너스
            return baseDamage + currentStacks + attackPotionBonus + enhancementBonus;
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


