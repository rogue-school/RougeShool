using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;
using Game.SkillCardSystem.Effect;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 가드 효과를 적용하는 스킬 카드 효과 ScriptableObject입니다.
    /// 대상에게 가드 수치를 부여하거나 GuardEffectCommand를 생성합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "GuardEffect", menuName = "SkillEffects/GuardEffect")]
    public class GuardEffectSO : SkillCardEffectSO
    {
        // 주의: 이펙트와 사운드는 SkillCardDefinition의 EffectCustomSettings에서 관리됩니다.
        // 이 SO는 버프/디버프 아이콘만 제공합니다.
        /// <summary>
        /// 이펙트 실행 커맨드를 생성합니다. 
        /// 단순한 가드 상태 부여 커맨드를 반환합니다.
        /// </summary>
        /// <param name="power">커맨드에 사용될 수치 (무시됨)</param>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new GuardEffectCommand(1, null);
        }

        /// <summary>
        /// 효과를 즉시 적용합니다. (사용되지 않음 - GuardEffectCommand로 대체됨)
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">가드 수치</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            Debug.LogWarning("[GuardEffectSO] ApplyEffect는 레거시 메서드입니다. GuardEffectCommand를 사용하세요.");
        }
    }
}
