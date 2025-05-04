using Game.Interface;
using Game.Characters;
using System.Collections.Generic;

namespace Game.Cards
{
    /// <summary>
    /// 카드의 이펙트를 커맨드 패턴으로 실행합니다.
    /// </summary>
    public static class CardExecutor
    {
        /// <summary>
        /// 카드를 실행 가능한 커맨드 목록으로 변환합니다.
        /// </summary>
        public static List<ICardEffectCommand> CreateCommands(ISkillCard card, CharacterBase caster, CharacterBase target)
        {
            var commands = new List<ICardEffectCommand>();

            foreach (var effect in card.CreateEffects())
            {
                int power = card.GetEffectPower(effect);
                var cmd = new CardEffectCommand(effect, caster, target, power);
                commands.Add(cmd);
            }

            return commands;
        }

        /// <summary>
        /// 모든 커맨드를 실행합니다.
        /// </summary>
        public static void ExecuteCard(ISkillCard card, CharacterBase caster, CharacterBase target)
        {
            foreach (var command in CreateCommands(card, caster, target))
            {
                command.Execute();
            }
        }
    }
}
