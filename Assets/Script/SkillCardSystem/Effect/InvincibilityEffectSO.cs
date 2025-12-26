using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 무적 버프를 적용하는 스킬 카드 효과 ScriptableObject입니다.
    /// 대상에게 완전 무적 상태를 부여합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "InvincibilityEffect", menuName = "SkillEffects/InvincibilityEffect")]
    public class InvincibilityEffectSO : SkillCardEffectSO
    {
        /// <summary>
        /// 이펙트 실행 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">커맨드에 사용될 수치 (무시됨)</param>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new InvincibilityEffectCommand(2, null);
        }

        /// <summary>
        /// 효과를 즉시 적용합니다. (사용되지 않음 - InvincibilityEffectCommand로 대체됨)
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">무적 지속 턴 수</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            Debug.LogWarning("[InvincibilityEffectSO] ApplyEffect는 레거시 메서드입니다. InvincibilityEffectCommand를 사용하세요.");
        }
    }
}

