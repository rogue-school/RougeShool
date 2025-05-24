using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardExecutor
    {
        void Execute(ISkillCard card, ICardExecutionContext context);
        void Execute(ISkillCard card, ITurnCardRegistry registry);
    }
}
