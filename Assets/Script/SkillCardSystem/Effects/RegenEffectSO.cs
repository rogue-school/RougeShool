using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.SkillCardSystem.Effect;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    /// <summary>
    /// 턴마다 체력을 회복시키는 재생 효과를 정의하는 ScriptableObject입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "RegenEffect", menuName = "SkillEffects/RegenEffect")]
    public class RegenEffectSO : SkillCardEffectSO
    {
        [Header("재생 설정")]
        [Tooltip("턴마다 회복할 체력량")]
        [SerializeField] private int healPerTurn;

        [Tooltip("지속 턴 수")]
        [SerializeField] private int duration;

        /// <summary>
        /// 해당 효과에 대한 실행 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">외부에서 추가된 효과 수치</param>
        /// <returns>재생 효과 커맨드</returns>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new RegenEffectCommand(healPerTurn + power, duration);
        }

        /// <summary>
        /// 재생 효과를 직접 실행하여 대상에게 적용합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">실제 회복량</param>
        /// <param name="turnManager">전투 턴 매니저 (필요 시)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            if (context.Target == null)
            {
                Debug.LogWarning("[RegenEffectSO] 대상이 null입니다. 효과를 적용할 수 없습니다.");
                return;
            }

            var regen = new RegenEffect(value, duration);
            context.Target.RegisterPerTurnEffect(regen);

            Debug.Log($"[RegenEffectSO] {context.Target.GetCharacterName()}에게 재생 {value} 적용 (지속 {duration}턴)");
        }
    }
}
