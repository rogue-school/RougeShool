using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    public interface ICardExecutor
    {
        void Execute(ISkillCard card, ICardExecutionContext context, ICombatTurnManager turnManager);
    }
}
