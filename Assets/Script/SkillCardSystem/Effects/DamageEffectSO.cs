using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "SkillEffects/DamageEffect")]
    public class DamageEffectSO : SkillCardEffectSO
    {
        [SerializeField] private int damageAmount;

        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new DamageEffectCommand(damageAmount + power);
        }

        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            context.Target?.TakeDamage(value);
            Debug.Log($"[DamageEffectSO] {context.Source?.GetCharacterName()}가 {context.Target?.GetCharacterName()}에게 {value} 피해를 입힘");
        }
    }
}
