using UnityEngine;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Effect
{
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "SkillEffects/DamageEffect")]
    public class DamageEffectSO : SkillCardEffectSO
    {
        [SerializeField] private int damageAmount;

        public override void ApplyEffect(ICardExecutionContext context, int power, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[DamageEffectSO] 대상이 null입니다. 효과를 적용할 수 없습니다.");
                return;
            }

            int totalDamage = damageAmount + power;
            context.Target.TakeDamage(totalDamage);

            Debug.Log($"[DamageEffectSO] {context.Source?.GetCharacterName()} 가 {context.Target.GetCharacterName()} 에게 {totalDamage} 피해를 입힘");
        }
    }
}
