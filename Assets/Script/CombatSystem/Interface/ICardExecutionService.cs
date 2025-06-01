using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardExecutionService
    {
        void Execute(ISkillCard card, ICardExecutionContext context);
    }
}