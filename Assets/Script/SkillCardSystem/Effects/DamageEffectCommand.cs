using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    public class DamageEffectCommand : ICardEffectCommand
    {
        private readonly int _damage;

        public DamageEffectCommand(int damage)
        {
            _damage = damage;
        }

        public void Execute(ICardExecutionContext context, ITurnStateController controller)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[DamageEffectCommand] 대상이 null입니다.");
                return;
            }

            context.Target.TakeDamage(_damage);
            Debug.Log($"[DamageEffectCommand] {context.Source?.GetCharacterName()} → {context.Target?.GetCharacterName()} 피해: {_damage}");
        }
    }
}
