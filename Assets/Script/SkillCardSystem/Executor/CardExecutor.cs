using System.Collections.Generic;
using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Factory;
using Game.SkillCardSystem.Validator;

namespace Game.SkillCardSystem.Executor
{
    public class CardExecutor : ICardExecutor
    {
        private readonly ICardEffectCommandFactory commandFactory;
        private readonly ICardExecutionValidator validator;

        public CardExecutor(
            ICardEffectCommandFactory commandFactory,
            ICardExecutionValidator validator)
        {
            this.commandFactory = commandFactory;
            this.validator = validator;
        }

        public void Execute(ISkillCard card, ICardExecutionContext context, ITurnStateController controller)
        {
            if (!validator.CanExecute(card, context))
            {
                Debug.LogWarning($"[CardExecutor] 실행 조건 불충족: {card?.GetCardName() ?? "알 수 없음"}");
                return;
            }

            var effects = card.CreateEffects();
            if (effects == null || effects.Count == 0)
            {
                Debug.LogWarning($"[CardExecutor] '{card.GetCardName()}'에 등록된 이펙트가 없습니다.");
                return;
            }

            foreach (var effect in effects)
            {
                if (effect == null) continue;

                int power = card.GetEffectPower(effect);
                var command = commandFactory.Create(effect, power);
                command.Execute(context, controller);

                Debug.Log($"[CardExecutor] {card.GetCardName()} → {effect.GetType().Name}, power: {power}");
            }
        }
    }
}