using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공의 폭풍 효과를 적용하는 커맨드.
    /// 플레이어에게 목표 데미지를 입혀야 하는 기믹 디버프를 적용합니다.
    /// </summary>
    public class StormOfSpaceTimeEffectCommand : ICardEffectCommand
    {
        private readonly int targetDamage;
        private readonly int duration;
        private readonly Sprite icon;

        public StormOfSpaceTimeEffectCommand(int targetDamage = 30, int duration = 3, Sprite icon = null)
        {
            this.targetDamage = targetDamage;
            this.duration = duration;
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target is not ICharacter target)
            {
                GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] 대상이 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // 플레이어에게만 적용
            if (!target.IsPlayerControlled())
            {
                GameLogger.LogWarning("[StormOfSpaceTimeEffectCommand] 플레이어가 아닌 대상에게 시공의 폭풍을 적용하려고 시도했습니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
            int finalTargetDamage = targetDamage;
            int finalDuration = duration;
            Sprite finalIcon = icon;

            if (context.Card?.CardDefinition != null)
            {
                var cfg = context.Card.CardDefinition.configuration;
                if (cfg != null && cfg.hasEffects)
                {
                    foreach (var eff in cfg.effects)
                    {
                        if (eff?.effectSO is StormOfSpaceTimeEffectSO && eff.useCustomSettings)
                        {
                            finalTargetDamage = eff.customSettings.stormOfSpaceTimeTargetDamage;
                            finalDuration = eff.customSettings.stormOfSpaceTimeDuration;
                            GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] 커스텀 설정 사용: targetDamage={finalTargetDamage}, duration={finalDuration}", GameLogger.LogCategory.SkillCard);
                            break;
                        }
                    }
                }
            }

            var debuff = new StormOfSpaceTimeDebuff(finalTargetDamage, finalDuration, finalIcon);
            target.RegisterPerTurnEffect(debuff);

            GameLogger.LogInfo($"[StormOfSpaceTimeEffectCommand] {target.GetCharacterName()}에게 시공의 폭풍 디버프 적용 (목표: {finalTargetDamage} 데미지, {finalDuration}턴)", GameLogger.LogCategory.SkillCard);
        }
    }
}

