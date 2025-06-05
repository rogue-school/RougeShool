using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.SkillCardSystem.Executor;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    [CreateAssetMenu(fileName = "GuardEffect", menuName = "SkillEffects/GuardEffect")]
    public class GuardEffectSO : SkillCardEffectSO
    {
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new GuardEffectCommand();
        }

        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            context.Target?.GainGuard(value);
            Debug.Log($"[GuardEffectSO] {context.Target?.GetCharacterName()}에게 가드 {value} 적용");
        }
    }
}
