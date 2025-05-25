using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Executor
{
    public class CardExecutorService : ICardExecutor
    {
        private readonly ICardExecutionContextProvider contextProvider;

        public CardExecutorService(ICardExecutionContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public void Execute(ISkillCard card, ICardExecutionContext context, ITurnStateController controller)
        {
            if (card == null || context == null)
            {
                Debug.LogWarning("[CardExecutorService] 카드 또는 컨텍스트가 null입니다.");
                return;
            }

            foreach (var effect in card.CreateEffects())
            {
                int power = card.GetEffectPower(effect);
                var command = effect.CreateEffectCommand(power);
                command.Execute(context, controller);

                Debug.Log($"[CardExecutorService] {card.GetCardName()} → {effect.GetType().Name}, power: {power}");
            }
        }
    }
}
