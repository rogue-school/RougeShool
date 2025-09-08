using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 출혈 효과를 정의하는 스킬 카드 이펙트 ScriptableObject입니다.
    /// 대상에게 매 턴 피해를 주는 <see cref="BleedEffect"/>를 적용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "BleedEffect", menuName = "SkillEffects/BleedEffect")]
    public class BleedEffectSO : SkillCardEffectSO
    {
        [Header("출혈 수치 설정")]
        [Tooltip("기본 출혈 피해량")]
        [SerializeField] private int bleedAmount;

        [Tooltip("출혈 지속 턴 수")]
        [SerializeField] private int duration;

        /// <summary>
        /// 출혈 이펙트 커맨드를 생성합니다. 파워 수치를 기반으로 피해량이 증가합니다.
        /// </summary>
        /// <param name="power">외부에서 전달된 파워 수치</param>
        /// <returns>출혈 커맨드 객체</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new BleedEffectCommand(bleedAmount + power, duration);
        }

        /// <summary>
        /// 출혈 효과를 즉시 적용합니다. 커맨드가 아닌 직접 실행 방식입니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">적용할 피해량</param>
        /// <param name="controller">전투 턴 관리자 (필요 시)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager controller = null)
        {
            if (context?.Target == null)
            {
                Debug.LogWarning("[BleedEffectSO] 대상이 null이므로 출혈 적용 불가");
                return;
            }

            var bleed = new BleedEffect(value, duration);
            context.Target.RegisterPerTurnEffect(bleed);

            Debug.Log($"[BleedEffectSO] {context.Target.GetCharacterName()}에게 출혈 {value} 적용 (지속 {duration}턴)");
        }
    }
}
