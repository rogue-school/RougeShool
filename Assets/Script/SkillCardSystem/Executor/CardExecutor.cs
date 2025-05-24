using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Executor;

namespace Game.CombatSystem.Executor
{
    public class CardExecutor : ICardExecutor
    {
        private readonly ICardExecutionContextProvider contextProvider;

        public CardExecutor(ICardExecutionContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public void Execute(ISkillCard card, ICardExecutionContext context)
        {
            if (card == null || context == null)
            {
                Debug.LogError("[CardExecutor] 카드 또는 컨텍스트가 null입니다.");
                return;
            }

            foreach (var effect in card.CreateEffects())
            {
                int power = card.GetEffectPower(effect);
                effect.ApplyEffect(context, power);
            }
        }

        public void Execute(ISkillCard card, ITurnCardRegistry registry)
        {
            var context = contextProvider.CreateContext(card);
            Execute(card, context);
        }
    }
}
