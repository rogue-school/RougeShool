using Game.SkillCardSystem.Data;
using Game.SkillCardSystem.Effect;
using Game.SkillCardSystem.Utility;
using UnityEngine;
using Game.CoreSystem.Utility;
using Game.CharacterSystem.Interface;
using System.Linq;

namespace Game.SkillCardSystem.UI.Mappers
{
    /// <summary>
    /// SkillCardDefinition을 TooltipModel로 변환합니다.
    /// (현 단계: 이름/타입/설명만 매핑)
    /// </summary>
    public static class SkillCardTooltipMapper
    {
        public static TooltipModel From(SkillCardDefinition def, Game.CharacterSystem.Manager.PlayerManager playerManager = null, Game.ItemSystem.Service.ItemService itemService = null)
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
                if (config.HasResourceCost())
                {
                    string resourceName = GetResourceName(playerManager);

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

                        // 물약 효과 (집중 등)
                        if (so is AttackPowerBuffSkillEffectSO attackPowerBuff)
                        {
                            string effectName = GetEffectName(so, "공격력 증가");

                            int bonus = 0;
                            int duration = 0;
                            if (cs != null)
                            {
                                bonus = Mathf.Max(0, cs.damageAmount);
                                duration = Mathf.Max(0, cs.guardDuration);
                            }

                            if (bonus <= 0)
                            {
                                bonus = attackPowerBuff.DefaultAttackPowerBonus;
                            }

                            if (duration <= 0)
                            {
                                duration = attackPowerBuff.DefaultDuration;
                            }

                            if (bonus > 0)
                            {
                                if (duration > 0)
                                {
                                    ruleLines.Add($"{duration}턴 동안 피해가 {bonus} 증가합니다.");
                                }
                                else
                                {
                                    ruleLines.Add($"피해가 {bonus} 증가합니다.");
                                }

                                model.Effects.Add(new TooltipModel.EffectRow
                                {
                                    Name = effectName,
                                    Description = duration > 0 ? $"+{bonus}, {duration}턴" : $"+{bonus}",
                                    Color = new Color(1f, 0.85f, 0.4f)
                                });
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
                            string resourceName = GetResourceName(playerManager);
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
        /// <param name="playerCharacter">플레이어 캐릭터 (물약 확인용)</param>
        /// <param name="card">카드 인스턴스 (데미지 오버라이드 확인용, 선택적)</param>
        /// <param name="playerManager">플레이어 매니저 (리소스 이름, 강화 보너스 확인용, 선택적)</param>
        /// <param name="itemService">아이템 서비스 (강화 보너스 확인용, 선택적)</param>
        /// <returns>툴팁 모델</returns>
        public static TooltipModel FromWithStacks(SkillCardDefinition def, int currentStacks = 0, Game.CharacterSystem.Interface.ICharacter playerCharacter = null, Game.SkillCardSystem.Interface.ISkillCard card = null, Game.CharacterSystem.Manager.PlayerManager playerManager = null, Game.ItemSystem.Service.ItemService itemService = null)
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

            var config = def.configuration;
            if (config == null)
            {
                return BuildFinalModel(model, ruleLines, previewLines);
            }

            ProcessResourceCost(config, ruleLines, model, playerManager);
            ProcessDamageWithStacks(config, ruleLines, model, currentStacks, playerCharacter, card, def, playerManager, itemService);
            ProcessEffects(config, ruleLines, model, playerManager, itemService);

            return BuildFinalModel(model, ruleLines, previewLines);
        }

        /// <summary>
        /// 리소스 코스트를 처리합니다
        /// </summary>
        private static void ProcessResourceCost(CardConfiguration config, System.Collections.Generic.List<string> ruleLines, TooltipModel model, Game.CharacterSystem.Manager.PlayerManager playerManager = null)
        {
            if (!config.HasResourceCost()) return;

            string resourceName = GetResourceName(playerManager);
            ruleLines.Add($"{resourceName} 소모: {config.resourceConfig.cost}");
            model.Effects.Add(new TooltipModel.EffectRow
            {
                Name = resourceName,
                Description = $"소모 {config.resourceConfig.cost}",
                Color = UnityEngine.Color.red
            });
        }

        /// <summary>
        /// 스택 기반 데미지를 처리합니다
        /// </summary>
        private static void ProcessDamageWithStacks(
            CardConfiguration config,
            System.Collections.Generic.List<string> ruleLines,
            TooltipModel model,
            int currentStacks,
            Game.CharacterSystem.Interface.ICharacter playerCharacter,
            Game.SkillCardSystem.Interface.ISkillCard card,
            SkillCardDefinition def,
            Game.CharacterSystem.Manager.PlayerManager playerManager = null,
            Game.ItemSystem.Service.ItemService itemService = null)
        {
            if (!config.hasDamage || config.damageConfig == null) return;

            bool isPlayerCard = card != null && card.IsFromPlayer();
            var baseDmg = card != null ? card.GetBaseDamage() : config.damageConfig.baseDamage;
            bool useRandom = config.damageConfig.useRandomDamage && isPlayerCard;
            var minDmg = Mathf.Max(0, config.damageConfig.minDamage);
            var maxDmg = Mathf.Max(minDmg, config.damageConfig.maxDamage);
            var baseMin = minDmg;
            var baseMax = maxDmg;

            int potionBonus = 0;
            int focusBonus = 0;
            if (playerCharacter != null)
            {
                GetAttackPowerBuffs(playerCharacter, out potionBonus, out focusBonus);
                if (potionBonus > 0 || focusBonus > 0)
                {
                    GameLogger.LogInfo($"[SkillCardTooltipMapper] 버프 확인: 물약 +{potionBonus}, 집중 +{focusBonus}, 캐릭터: {playerCharacter.GetCharacterName()}", GameLogger.LogCategory.UI);
                }
            }
            else if (isPlayerCard)
            {
                GameLogger.LogWarning($"[SkillCardTooltipMapper] 플레이어 카드인데 playerCharacter가 null입니다. 공격력 버프를 확인할 수 없습니다.", GameLogger.LogCategory.UI);
            }
            int enhancementBonus = isPlayerCard ? GetEnhancementBonus(def, itemService) : 0;
            var hits = Mathf.Max(1, config.damageConfig.hits);

            int totalBuffBonus = potionBonus + focusBonus;
            int actualDmg = CalculateActualDamage(baseDmg, currentStacks, totalBuffBonus, enhancementBonus);
            int actualMin = useRandom ? CalculateActualDamage(minDmg, currentStacks, totalBuffBonus, enhancementBonus) : actualDmg;
            int actualMax = useRandom ? CalculateActualDamage(maxDmg, currentStacks, totalBuffBonus, enhancementBonus) : actualDmg;
            bool hasBonus = currentStacks > 0 || potionBonus > 0 || focusBonus > 0 || enhancementBonus > 0;

            AddDamageRuleLines(ruleLines, useRandom, hits, actualDmg, actualMin, actualMax, baseDmg, baseMin, baseMax, currentStacks, potionBonus, focusBonus, enhancementBonus, hasBonus);
            AddDamageSpecialFlags(ruleLines, config.damageConfig);
            AddDamageEffectRow(model, useRandom, hits, actualDmg, actualMin, actualMax);
        }

        /// <summary>
        /// 데미지 규칙 텍스트를 추가합니다
        /// </summary>
        private static void AddDamageRuleLines(
            System.Collections.Generic.List<string> ruleLines,
            bool useRandom,
            int hits,
            int actualDmg,
            int actualMin,
            int actualMax,
            int baseDmg,
            int baseMin,
            int baseMax,
            int currentStacks,
            int potionBonus,
            int focusBonus,
            int enhancementBonus,
            bool hasBonus)
        {
            if (hits <= 1)
            {
                if (useRandom)
                {
                    string bonusText = hasBonus ? BuildBonusText(baseMin, baseMax, currentStacks, potionBonus, focusBonus, enhancementBonus) : "";
                    ruleLines.Add($"피해 {actualMin}~{actualMax}을 줍니다.{bonusText}");
                }
                else
                {
                    if (hasBonus)
                    {
                        string bonusText = BuildDamageBonusText(baseDmg, currentStacks, potionBonus, focusBonus, enhancementBonus);
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
                    string bonusText = hasBonus ? BuildBonusText(baseMin, baseMax, currentStacks, potionBonus, focusBonus, enhancementBonus) : "";
                    ruleLines.Add($"피해 {totalMin}~{totalMax} (총 {hits}회){bonusText}");
                }
                else
                {
                    if (hasBonus)
                    {
                        string bonusText = BuildDamageBonusText(baseDmg, currentStacks, potionBonus, focusBonus, enhancementBonus);
                        ruleLines.Add($"피해 {KoreanTextHelper.AddKoreanParticle(actualDmg, "을/를")} {KoreanTextHelper.AddKoreanParticle(hits, "을/를")}번 줍니다.{bonusText}");
                    }
                    else
                    {
                        ruleLines.Add($"피해 {KoreanTextHelper.AddKoreanParticle(actualDmg, "을/를")} {KoreanTextHelper.AddKoreanParticle(hits, "을/를")}번 줍니다.");
                    }
                }
            }
        }

        /// <summary>
        /// 데미지 특수 플래그를 추가합니다
        /// </summary>
        private static void AddDamageSpecialFlags(System.Collections.Generic.List<string> ruleLines, DamageConfiguration damageConfig)
        {
            if (damageConfig.ignoreGuard)
            {
                ruleLines.Add("가드를 무시합니다.");
            }
            if (damageConfig.ignoreCounter)
            {
                ruleLines.Add("반격을 무시합니다.");
            }
        }

        /// <summary>
        /// 데미지 효과 행을 추가합니다
        /// </summary>
        private static void AddDamageEffectRow(TooltipModel model, bool useRandom, int hits, int actualDmg, int actualMin, int actualMax)
        {
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

        /// <summary>
        /// 보너스 텍스트를 생성합니다 (랜덤 데미지용)
        /// </summary>
        private static string BuildBonusText(int baseMin, int baseMax, int currentStacks, int potionBonus, int focusBonus, int enhancementBonus)
        {
            return $" (기본 {baseMin}~{baseMax}"
                + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                + (potionBonus > 0 ? $", 공격력 물약 {potionBonus}" : "")
                + (focusBonus > 0 ? $", 집중 {focusBonus}" : "")
                + (enhancementBonus > 0 ? $", 강화 {enhancementBonus}" : "")
                + ")";
        }

        /// <summary>
        /// 데미지 보너스 텍스트를 생성합니다 (고정 데미지용)
        /// </summary>
        private static string BuildDamageBonusText(int baseDmg, int currentStacks, int potionBonus, int focusBonus, int enhancementBonus)
        {
            return $" (기본 피해 {baseDmg}"
                + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                + (potionBonus > 0 ? $", 공격력 물약 {potionBonus}" : "")
                + (focusBonus > 0 ? $", 집중 {focusBonus}" : "")
                + (enhancementBonus > 0 ? $", 강화 {enhancementBonus}" : "")
                + ")";
        }

        /// <summary>
        /// 강화 보너스를 가져옵니다
        /// </summary>
        private static int GetEnhancementBonus(SkillCardDefinition def, Game.ItemSystem.Service.ItemService itemService = null)
        {
            if (itemService == null)
            {
                // 폴백: FindFirstObjectByType 사용
                itemService = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
            }
            if (itemService == null) return 0;

            string skillId = def.displayName;
            return itemService.GetSkillDamageBonus(skillId);
        }

        /// <summary>
        /// 리소스 이름을 가져옵니다
        /// </summary>
        private static string GetResourceName(Game.CharacterSystem.Manager.PlayerManager playerManager = null)
        {
            string resourceName = "자원";
            if (playerManager == null)
            {
                // 폴백: FindFirstObjectByType 사용
                playerManager = UnityEngine.Object.FindFirstObjectByType<Game.CharacterSystem.Manager.PlayerManager>();
            }
            if (playerManager != null && !string.IsNullOrEmpty(playerManager.ResourceName))
            {
                resourceName = playerManager.ResourceName;
            }
            return resourceName;
        }

        /// <summary>
        /// 효과들을 처리합니다
        /// </summary>
        private static void ProcessEffects(CardConfiguration config, System.Collections.Generic.List<string> ruleLines, TooltipModel model, Game.CharacterSystem.Manager.PlayerManager playerManager = null, Game.ItemSystem.Service.ItemService itemService = null)
        {
            if (!config.hasEffects || config.effects == null) return;

            foreach (var effectConfig in config.effects)
            {
                ProcessEffect(effectConfig, ruleLines, model, playerManager, itemService);
            }
        }

        /// <summary>
        /// 개별 효과를 처리합니다
        /// </summary>
        private static void ProcessEffect(EffectConfiguration effectConfig, System.Collections.Generic.List<string> ruleLines, TooltipModel model, Game.CharacterSystem.Manager.PlayerManager playerManager = null, Game.ItemSystem.Service.ItemService itemService = null)
        {
            var so = effectConfig.effectSO;
            var cs = effectConfig.useCustomSettings ? effectConfig.customSettings : null;

            if (so is AttackPowerBuffSkillEffectSO attackPowerBuff)
            {
                ProcessAttackPowerBuff(attackPowerBuff, cs, ruleLines, model);
                return;
            }

            if (so is BleedEffectSO)
            {
                ProcessBleedEffect(so, cs, ruleLines, model);
                return;
            }

            if (so is GuardEffectSO)
            {
                ProcessGuardEffect(so, ruleLines, model);
                return;
            }

            if (so is StunEffectSO)
            {
                ProcessStunEffect(so, cs, ruleLines, model);
                return;
            }

            if (so is CounterEffectSO)
            {
                ProcessCounterEffect(so, cs, ruleLines, model);
                return;
            }

            if (so is CardUseStackEffectSO)
            {
                ProcessCardUseStackEffect(so, cs, ruleLines, model);
                return;
            }

            if (so is ReplayPreviousTurnCardEffectSO replayEffect)
            {
                ProcessReplayPreviousTurnEffect(replayEffect, cs, ruleLines, model);
                return;
            }

            // 치유 효과는 항상 처리 (조건 없음)
            ProcessHealEffect(so, cs, ruleLines, model);

            // 리소스 델타 처리
            if (cs != null && cs.resourceDelta != 0)
            {
                ProcessResourceDelta(cs, ruleLines, model, playerManager);
            }
        }

        /// <summary>
        /// 물약 효과를 처리합니다
        /// </summary>
        private static void ProcessAttackPowerBuff(AttackPowerBuffSkillEffectSO attackPowerBuff, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
        {
            string effectName = GetEffectName(attackPowerBuff, "공격력 증가");

            int bonus = 0;
            int duration = 0;
            if (cs != null)
            {
                bonus = Mathf.Max(0, cs.damageAmount);
                duration = Mathf.Max(0, cs.guardDuration);
            }

            if (bonus <= 0)
            {
                bonus = attackPowerBuff.DefaultAttackPowerBonus;
            }

            if (duration <= 0)
            {
                duration = attackPowerBuff.DefaultDuration;
            }

            if (bonus > 0)
            {
                if (duration > 0)
                {
                    ruleLines.Add($"{duration}턴 동안 피해가 {bonus} 증가합니다.");
                }
                else
                {
                    ruleLines.Add($"피해가 {bonus} 증가합니다.");
                }

                model.Effects.Add(new TooltipModel.EffectRow
                {
                    Name = effectName,
                    Description = duration > 0 ? $"+{bonus}, {duration}턴" : $"+{bonus}",
                    Color = new Color(1f, 0.85f, 0.4f)
                });
            }
        }

        /// <summary>
        /// 출혈 효과를 처리합니다
        /// </summary>
        private static void ProcessBleedEffect(SkillCardEffectSO so, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
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

        /// <summary>
        /// 치유 효과를 처리합니다
        /// </summary>
        private static void ProcessHealEffect(SkillCardEffectSO so, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
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

        /// <summary>
        /// 가드 효과를 처리합니다
        /// </summary>
        private static void ProcessGuardEffect(SkillCardEffectSO so, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
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

        /// <summary>
        /// 스턴 효과를 처리합니다
        /// </summary>
        private static void ProcessStunEffect(SkillCardEffectSO so, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
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

        /// <summary>
        /// 반격 효과를 처리합니다
        /// </summary>
        private static void ProcessCounterEffect(SkillCardEffectSO so, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
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

        /// <summary>
        /// 카드 사용 스택 효과를 처리합니다
        /// </summary>
        private static void ProcessCardUseStackEffect(SkillCardEffectSO so, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
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

        /// <summary>
        /// 연계 효과를 처리합니다
        /// </summary>
        private static void ProcessReplayPreviousTurnEffect(ReplayPreviousTurnCardEffectSO replayEffect, EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model)
        {
            string effectName = GetEffectName(replayEffect, "연계");
            
            int repeatCount = 2; // 기본값
            if (cs != null && cs.damageAmount > 0)
            {
                repeatCount = cs.damageAmount; // damageAmount를 재실행 횟수로 사용
            }
            else
            {
                // SO에서 직접 가져오기 (리플렉션 사용)
                var field = typeof(ReplayPreviousTurnCardEffectSO).GetField("_repeatCount", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (field != null)
                {
                    repeatCount = (int)field.GetValue(replayEffect);
                }
            }
            
            if (repeatCount > 0)
            {
                ruleLines.Add($"이전 턴에 사용한 비-연계 스킬을 {repeatCount}번 재실행합니다.");
                
                model.Effects.Add(new TooltipModel.EffectRow
                {
                    Name = effectName,
                    Description = $"{repeatCount}회",
                    Color = new Color(0.9f, 0.7f, 1f) // 보라색 계열
                });
            }
        }

        /// <summary>
        /// 리소스 델타를 처리합니다
        /// </summary>
        private static void ProcessResourceDelta(EffectCustomSettings cs, System.Collections.Generic.List<string> ruleLines, TooltipModel model, Game.CharacterSystem.Manager.PlayerManager playerManager = null)
        {
            var text = cs.resourceDelta > 0 ? "획득" : "소모";
            string resourceName = GetResourceName(playerManager);
            ruleLines.Add($"{resourceName} {text}: {Mathf.Abs(cs.resourceDelta)}");
            model.Effects.Add(new TooltipModel.EffectRow
            {
                Name = resourceName,
                Description = $"{text} {Mathf.Abs(cs.resourceDelta)}",
                Color = cs.resourceDelta > 0 ? UnityEngine.Color.green : UnityEngine.Color.red
            });
        }

        /// <summary>
        /// 최종 모델을 빌드합니다
        /// </summary>
        private static TooltipModel BuildFinalModel(TooltipModel model, System.Collections.Generic.List<string> ruleLines, System.Collections.Generic.List<string> previewLines)
        {
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
        /// 한글 이름이 없으면 기본 이름을 사용합니다.
        /// </summary>
        /// <param name="effectSO">이펙트 SO</param>
        /// <param name="defaultName">기본 이름 (한글)</param>
        /// <returns>이펙트 이름</returns>
        private static string GetEffectName(SkillCardEffectSO effectSO, string defaultName)
        {
            if (effectSO == null) return defaultName;
            
            // 1. GetEffectName() 먼저 확인
            string soName = effectSO.GetEffectName();
            if (!string.IsNullOrWhiteSpace(soName))
            {
                // 한글이 포함되어 있는지 확인
                if (ContainsKorean(soName))
                {
                    return soName; // 한글이 있으면 사용
                }
                // 영어만 있으면 기본 이름 사용
            }
            
            // 2. GetEffectName()이 비어있거나 영어만 있으면 SO의 name 필드 확인
            string soObjectName = effectSO.name;
            if (!string.IsNullOrWhiteSpace(soObjectName))
            {
                // 한글이 포함되어 있는지 확인
                if (ContainsKorean(soObjectName))
                {
                    return soObjectName; // 한글이 있으면 사용
                }
                // 영어만 있으면 기본 이름 사용
            }
            
            // 3. 모두 영어이거나 비어있으면 기본 한글 이름 사용
            return defaultName;
        }
        
        /// <summary>
        /// 문자열에 한글이 포함되어 있는지 확인합니다.
        /// </summary>
        /// <param name="text">확인할 문자열</param>
        /// <returns>한글 포함 여부</returns>
        private static bool ContainsKorean(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return false;
            
            // 알려진 영어 효과 이름 패턴 체크 (명시적 체크로 더 안전)
            string lowerText = text.ToLower();
            if (lowerText.Contains("guard") || lowerText.Contains("bleed") || 
                lowerText.Contains("stun") || lowerText.Contains("counter") ||
                lowerText.Contains("heal") || lowerText.Contains("damage") ||
                lowerText.Contains("effect") || lowerText.Contains("buff") ||
                lowerText.Contains("debuff") || lowerText.Contains("resource") ||
                lowerText.Contains("stack") || lowerText.Contains("replay"))
            {
                return false; // 영어 효과 이름으로 간주
            }
            
            // 한글 유니코드 범위: 0xAC00-0xD7A3 (가-힣)
            foreach (char c in text)
            {
                if (c >= 0xAC00 && c <= 0xD7A3)
                {
                    return true;
                }
            }
            return false;
        }

        private static void ProcessDamageWithStacksInternal(
            CardConfiguration config,
            int currentStacks,
            ICharacter playerCharacter,
            bool isPlayerCard,
            SkillCardDefinition def,
            bool useRandom,
            int minDmg,
            int baseDmg,
            System.Collections.Generic.List<string> ruleLines,
            TooltipModel model,
            Game.ItemSystem.Service.ItemService itemService = null)
        {
            if (!config.hasDamage || config.damageConfig == null) return;
            
            var maxDmg = Mathf.Max(minDmg, config.damageConfig.maxDamage);
                    var baseMin = minDmg;
                    var baseMax = maxDmg;
                    
                    int potionBonus = 0;
                    int focusBonus = 0;
                    if (playerCharacter != null)
                    {
                        GetAttackPowerBuffs(playerCharacter, out potionBonus, out focusBonus);
                    }
                    
                    int enhancementBonus = 0;
                    if (isPlayerCard)
                    {
                        // itemService는 매개변수로 전달받음
                        if (itemService == null)
                        {
                            // 폴백: FindFirstObjectByType 사용
                            itemService = UnityEngine.Object.FindFirstObjectByType<Game.ItemSystem.Service.ItemService>();
                        }
                        if (itemService != null)
                        {
                            string skillId = def.displayName;
                            enhancementBonus = itemService.GetSkillDamageBonus(skillId);
                        }
                    }
                    
                    var hits = Mathf.Max(1, config.damageConfig.hits);
                    int totalBuffBonus = potionBonus + focusBonus;

                    int actualDmg = CalculateActualDamage(baseDmg, currentStacks, totalBuffBonus, enhancementBonus);
                    int actualMin = useRandom ? CalculateActualDamage(minDmg, currentStacks, totalBuffBonus, enhancementBonus) : actualDmg;
                    int actualMax = useRandom ? CalculateActualDamage(maxDmg, currentStacks, totalBuffBonus, enhancementBonus) : actualDmg;
                    bool hasBonus = currentStacks > 0 || potionBonus > 0 || focusBonus > 0 || enhancementBonus > 0;

                    if (hits <= 1)
                    {
                        if (useRandom)
                        {
                            var bonusText = "";
                            if (hasBonus)
                            {
                                bonusText = $" (기본 {baseMin}~{baseMax}"
                                            + (currentStacks > 0 ? $", 스택 {currentStacks}" : "")
                                            + (potionBonus > 0 ? $", 물약 {potionBonus}" : "")
                                            + (focusBonus > 0 ? $", 집중 {focusBonus}" : "")
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
                                    + (potionBonus > 0 ? $", 물약 {potionBonus}" : "")
                                    + (focusBonus > 0 ? $", 집중 {focusBonus}" : "")
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
                                            + (potionBonus > 0 ? $", 물약 {potionBonus}" : "")
                                            + (focusBonus > 0 ? $", 집중 {focusBonus}" : "")
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
                                    + (potionBonus > 0 ? $", 물약 {potionBonus}" : "")
                                    + (focusBonus > 0 ? $", 집중 {focusBonus}" : "")
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
        /// 물약 버프와 집중 버프를 분리하여 계산합니다.
        /// </summary>
        /// <param name="playerCharacter">플레이어 캐릭터</param>
        /// <param name="potionBonus">물약 버프 보너스 (출력)</param>
        /// <param name="focusBonus">집중 버프 보너스 (출력)</param>
        private static void GetAttackPowerBuffs(Game.CharacterSystem.Interface.ICharacter playerCharacter, out int potionBonus, out int focusBonus)
        {
            potionBonus = 0;
            focusBonus = 0;
            
            if (playerCharacter == null)
            {
                GameLogger.LogWarning("[SkillCardTooltipMapper] GetAttackPowerBuffs: playerCharacter가 null입니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            var buffs = playerCharacter.GetBuffs();
            
            if (buffs == null)
            {
                GameLogger.LogWarning($"[SkillCardTooltipMapper] GetAttackPowerBuffs: {playerCharacter.GetCharacterName()}의 GetBuffs()가 null을 반환했습니다.", GameLogger.LogCategory.UI);
                return;
            }
            
            GameLogger.LogInfo($"[SkillCardTooltipMapper] GetAttackPowerBuffs: {playerCharacter.GetCharacterName()}의 버프 수: {buffs.Count}", GameLogger.LogCategory.UI);
            
            foreach (var effect in buffs)
            {
                if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                {
                    int bonus = attackBuff.GetAttackPowerBonus();
                    
                    // SourceItemName이 있으면 물약 버프, SourceEffectName이 있으면 집중 버프
                    bool hasItemName = !string.IsNullOrEmpty(attackBuff.SourceItemName);
                    bool hasEffectName = !string.IsNullOrEmpty(attackBuff.SourceEffectName);
                    
                    if (hasItemName)
                    {
                        potionBonus += bonus;
                        GameLogger.LogInfo($"[SkillCardTooltipMapper] 물약 버프 발견: +{bonus} (총: {potionBonus})", GameLogger.LogCategory.UI);
                    }
                    else if (hasEffectName)
                    {
                        focusBonus += bonus;
                        GameLogger.LogInfo($"[SkillCardTooltipMapper] 집중 버프 발견: +{bonus} (총: {focusBonus})", GameLogger.LogCategory.UI);
                    }
                    else
                    {
                        // 둘 다 없으면 기본적으로 물약으로 간주
                        potionBonus += bonus;
                        GameLogger.LogInfo($"[SkillCardTooltipMapper] 공격력 버프 발견 (출처 불명): +{bonus} (물약으로 분류, 총: {potionBonus})", GameLogger.LogCategory.UI);
                    }
                }
            }
            
            if (potionBonus == 0 && focusBonus == 0)
            {
                GameLogger.LogInfo($"[SkillCardTooltipMapper] GetAttackPowerBuffs: 공격력 버프가 없습니다. 버프 목록: {string.Join(", ", buffs.Select(b => b?.GetType().Name ?? "null"))}", GameLogger.LogCategory.UI);
            }
        }
        
        /// <summary>
        /// 물약(포션/스킬 포함) 보너스를 계산합니다. (레거시 호환용)
        /// </summary>
        /// <param name="playerCharacter">플레이어 캐릭터</param>
        /// <returns>물약 보너스</returns>
        private static int GetAttackPotionBonus(Game.CharacterSystem.Interface.ICharacter playerCharacter)
        {
            if (playerCharacter == null)
            {
                GameLogger.LogWarning("[SkillCardTooltipMapper] GetAttackPotionBonus: playerCharacter가 null입니다.", GameLogger.LogCategory.UI);
                return 0;
            }
            
            int totalBonus = 0;
            var buffs = playerCharacter.GetBuffs();
            
            if (buffs == null)
            {
                GameLogger.LogWarning($"[SkillCardTooltipMapper] GetAttackPotionBonus: {playerCharacter.GetCharacterName()}의 GetBuffs()가 null을 반환했습니다.", GameLogger.LogCategory.UI);
                return 0;
            }
            
            GameLogger.LogInfo($"[SkillCardTooltipMapper] GetAttackPotionBonus: {playerCharacter.GetCharacterName()}의 버프 수: {buffs.Count}", GameLogger.LogCategory.UI);
            
            foreach (var effect in buffs)
            {
                if (effect is Game.ItemSystem.Effect.AttackPowerBuffEffect attackBuff)
                {
                    int bonus = attackBuff.GetAttackPowerBonus();
                    totalBonus += bonus;
                    GameLogger.LogInfo($"[SkillCardTooltipMapper] 물약 발견: +{bonus} (총: {totalBonus})", GameLogger.LogCategory.UI);
                }
            }
            
            if (totalBonus == 0)
            {
                GameLogger.LogInfo($"[SkillCardTooltipMapper] GetAttackPotionBonus: 물약가 없습니다. 버프 목록: {string.Join(", ", buffs.Select(b => b?.GetType().Name ?? "null"))}", GameLogger.LogCategory.UI);
            }
            
            return totalBonus;
        }

        /// <summary>
        /// 스택 기반 실제 데미지를 계산합니다.
        /// </summary>
        /// <param name="baseDamage">기본 데미지</param>
        /// <param name="currentStacks">현재 스택 수</param>
        /// <param name="totalBuffBonus">총 버프 보너스 (물약 + 집중)</param>
        /// <param name="enhancementBonus">강화 보너스</param>
        /// <returns>실제 적용 데미지</returns>
        private static int CalculateActualDamage(int baseDamage, int currentStacks, int totalBuffBonus = 0, int enhancementBonus = 0)
        {
            // 선형 증가: 기본 데미지 + 스택 수 + 버프 보너스(물약+집중) + 강화 보너스
            return baseDamage + currentStacks + totalBuffBonus + enhancementBonus;
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


