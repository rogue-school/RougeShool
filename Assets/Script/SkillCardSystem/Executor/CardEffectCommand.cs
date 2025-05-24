using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Executor
{
    public class CardEffectCommand : ICardEffectCommand
    {
        private readonly ICardEffect effect;
        private readonly ICardExecutionContext context;
        private readonly int power;
        private readonly ITurnStateController controller;

        public CardEffectCommand(ICardEffect effect, ICardExecutionContext context, int power, ITurnStateController controller = null)
        {
            this.effect = effect;
            this.context = context;
            this.power = power;
            this.controller = controller;
        }

        public void Execute()
        {
            effect.ApplyEffect(context, power, controller);
        }
    }
}
