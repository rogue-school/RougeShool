using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.CombatSystem.Turn
{
    public class CardExecutionService : ICardExecutionService
    {
        private readonly ICardExecutor executor;

        public CardExecutionService(ICardExecutor executor)
        {
            this.executor = executor;
        }

        public void Execute(ISkillCard card, ICardExecutionContext context)
        {
            executor.Execute(card, context);
        }
    }
}
