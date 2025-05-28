using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Interface
{
    public interface ICardEffectCommandFactory
    {
        ICardEffectCommand Create(SkillCardEffectSO effect, int power);
    }
}
