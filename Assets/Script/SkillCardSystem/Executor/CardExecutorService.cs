using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 카드 실행을 실제로 수행하는 서비스 클래스입니다.
    /// 카드에 등록된 모든 이펙트를 반복 실행합니다.
    /// </summary>
    public class CardExecutorService : ICardExecutor
    {
        private readonly ICardExecutionContextProvider contextProvider;

        public CardExecutorService(ICardExecutionContextProvider contextProvider)
        {
            this.contextProvider = contextProvider;
        }

        public void Execute(ISkillCard card, ICardExecutionContext context)
        {
            if (card == null || context == null)
            {
                Debug.LogWarning("[CardExecutorService] 카드 또는 컨텍스트가 null입니다.");
                return;
            }

            foreach (var effect in card.CreateEffects())
            {
                int power = card.GetEffectPower(effect);
                effect.ApplyEffect(context, power);
                Debug.Log($"[CardExecutorService] {card.GetCardName()} → {effect.GetEffectName()}, power: {power}");
            }
        }

        public void Execute(ISkillCard card, ITurnCardRegistry registry)
        {
            var context = contextProvider.CreateContext(card);
            Execute(card, context);
        }
    }
}
