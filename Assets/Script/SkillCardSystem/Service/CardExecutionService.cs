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
            // controller는 현재 사용하지 않으므로 null 전달
            executor.Execute(card, context, null);
        }
    }
}
