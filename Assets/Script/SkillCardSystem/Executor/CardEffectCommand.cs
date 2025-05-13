using Game.SkillCardSystem.Interface;
using Game.CharacterSystem.Core;

namespace Game.SkillCardSystem.Executor
{
    /// <summary>
    /// 하나의 이펙트를 실행하는 명령 객체입니다.
    /// </summary>
    public class CardEffectCommand : ICardEffectCommand
    {
        private readonly ICardEffect effect;
        private readonly CharacterBase caster;
        private readonly CharacterBase target;
        private readonly int power;

        public CardEffectCommand(ICardEffect effect, CharacterBase caster, CharacterBase target, int power)
        {
            this.effect = effect;
            this.caster = caster;
            this.target = target;
            this.power = power;
        }

        public void Execute()
        {
            effect.ExecuteEffect(caster, target, power);
        }
    }
}
