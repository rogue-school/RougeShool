using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.CharacterSystem.Interface;
using Game.CoreSystem.Utility;
using UnityEngine;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 반격 버프를 적용하는 커맨드. 소스 캐릭터에게 CounterBuff를 1턴 적용합니다.
    /// </summary>
    public class CounterEffectCommand : ICardEffectCommand
    {
        private readonly int duration;
        private readonly Sprite icon;

        public CounterEffectCommand(int duration = 1, Sprite icon = null)
        {
            this.duration = duration;
            this.icon = icon;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Source is not ICharacter character)
            {
                GameLogger.LogWarning("[CounterEffectCommand] 소스가 캐릭터가 아니거나 null입니다.", GameLogger.LogCategory.SkillCard);
                return;
            }

            // SkillCardDefinition의 커스텀 설정이 있으면 우선 사용
            int finalDuration = duration;
            if (context.Card?.CardDefinition != null)
            {
                var cfg = context.Card.CardDefinition.configuration;
                if (cfg != null && cfg.hasEffects)
                {
                    // 해당 카드의 EffectConfiguration 중 CounterEffectSO 항목의 커스텀 설정을 확인
                    foreach (var eff in cfg.effects)
                    {
                        if (eff?.effectSO is CounterEffectSO && eff.useCustomSettings)
                        {
                            if (eff.customSettings != null && eff.customSettings.counterDuration > 0)
                            {
                                finalDuration = eff.customSettings.counterDuration;
                            }
                            break;
                        }
                    }
                }
            }

            var buff = new CounterBuff(finalDuration, icon);
            character.RegisterPerTurnEffect(buff);
            GameLogger.LogInfo($"[CounterEffectCommand] {character.GetCharacterName()}에게 반격 버프 적용 ({finalDuration}턴)", GameLogger.LogCategory.SkillCard);
        }
    }
}


