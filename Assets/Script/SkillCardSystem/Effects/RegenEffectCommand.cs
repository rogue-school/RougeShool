using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    public class RegenEffectCommand : ICardEffectCommand
    {
        private readonly int _healAmount;
        private readonly int _duration;

        public RegenEffectCommand(int healAmount, int duration)
        {
            _healAmount = healAmount;
            _duration = duration;
        }

        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[RegenEffectCommand] 대상이 null입니다.");
                return;
            }

            var effect = new RegenEffect(_healAmount, _duration);
            context.Target.RegisterPerTurnEffect(effect);
            Debug.Log($"[RegenEffectCommand] {context.Target.GetCharacterName()} 재생 {_healAmount} 적용 (지속 {_duration}턴)");
        }
    }
}
