using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Interface
{
    public interface ICardEffectCommandFactory
    {
        ICardEffectCommand Create(SkillCardEffectSO effect, int power);
    }
}
