using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effects;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 대상에게 고정 피해를 주는 스킬 카드 효과 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "DamageEffect", menuName = "SkillEffects/DamageEffect")]
    public class DamageEffectSO : SkillCardEffectSO
    {
        [Header("기본 피해량")]
        [SerializeField] private int damageAmount;

        /// <summary>
        /// 커맨드 방식 실행을 위한 이펙트 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">추가 파워 수치</param>
        /// <returns>피해 커맨드</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new DamageEffectCommand(damageAmount + power);
        }

        /// <summary>
        /// 대상에게 피해를 직접 적용하는 로직입니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">적용할 피해량</param>
        /// <param name="turnManager">턴 매니저 (옵션)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            if (context?.Target == null)
            {
                Debug.LogWarning("[DamageEffectSO] 대상이 null입니다. 피해 적용 실패.");
                return;
            }

            context.Target.TakeDamage(value);

            Debug.Log($"[DamageEffectSO] {context.Source?.GetCharacterName()} → {context.Target.GetCharacterName()} 피해: {value}");
        }
    }
}
