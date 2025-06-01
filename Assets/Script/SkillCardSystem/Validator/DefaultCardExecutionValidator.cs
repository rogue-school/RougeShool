using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Validator
{
    public class DefaultCardExecutionValidator : ICardExecutionValidator
    {
        public bool CanExecute(ISkillCard card, ICardExecutionContext context)
        {
            if (card == null || context == null)
                return false;

            if (context.Target == null || context.Target.IsDead())
                return false;

            if (card.GetCoolTime() > 0)
                return false;

            return true;
        }
    }
}
