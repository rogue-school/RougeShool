using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Factory
{
    public class CardEffectCommandFactory : ICardEffectCommandFactory
    {
        public ICardEffectCommand Create(SkillCardEffectSO effect, int power)
        {
            return effect.CreateEffectCommand(power);
        }
    }
}
