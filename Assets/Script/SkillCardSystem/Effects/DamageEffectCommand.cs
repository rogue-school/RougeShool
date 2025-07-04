using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상에게 고정 피해를 주는 스킬 카드 효과 커맨드입니다.
    /// </summary>
    public class DamageEffectCommand : ICardEffectCommand
    {
        private readonly int damage;

        /// <summary>
        /// 피해량을 지정하여 커맨드를 생성합니다.
        /// </summary>
        /// <param name="damage">적용할 피해량</param>
        public DamageEffectCommand(int damage)
        {
            this.damage = damage;
        }

        /// <summary>
        /// 지정된 대상에게 피해를 적용합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트 (대상 포함)</param>
        /// <param name="turnManager">전투 턴 매니저 (옵션)</param>
        public void Execute(ICardExecutionContext context, ICombatTurnManager turnManager)
        {
            if (context?.Target == null)
            {
                Debug.LogWarning("[DamageEffectCommand] 대상이 null입니다. 피해를 적용할 수 없습니다.");
                return;
            }

            context.Target.TakeDamage(damage);

            Debug.Log($"[DamageEffectCommand] {context.Source?.GetCharacterName()} → {context.Target.GetCharacterName()} 피해: {damage}");
        }
    }
}
