using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Effect
{
    public class BleedEffectCommand : ICardEffectCommand
    {
        private readonly int _amount;
        private readonly int _duration;

        public BleedEffectCommand(int amount, int duration)
        {
            _amount = amount;
            _duration = duration;
        }

        public void Execute(ICardExecutionContext context, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[BleedEffectCommand] 대상이 null입니다.");
                return;
            }

            var effect = new BleedEffect(_amount, _duration);
            context.Target.RegisterPerTurnEffect(effect);
            Debug.Log($"[BleedEffectCommand] {context.Target.GetCharacterName()}에게 출혈 {_amount} 적용 (지속 {_duration}턴)");
        }
    }
}
