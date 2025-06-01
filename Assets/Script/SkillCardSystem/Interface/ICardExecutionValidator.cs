using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    public interface ICardExecutionValidator
    {
        bool CanExecute(ISkillCard card, ICardExecutionContext context);
    }
}
