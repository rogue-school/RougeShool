using Game.SkillCardSystem.Interface;

namespace Game.CombatSystem.Interface
{
    public interface ICardExecutionContextProvider
    {
        ICardExecutionContext CreateContext(ISkillCard card);
    }
}
