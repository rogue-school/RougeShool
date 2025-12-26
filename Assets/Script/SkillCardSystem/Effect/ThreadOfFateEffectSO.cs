using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 운명의 실 디버프를 적용하는 스킬 카드 효과 ScriptableObject입니다.
    /// 플레이어에게 운명의 실 디버프를 적용합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ThreadOfFateEffect", menuName = "SkillEffects/ThreadOfFateEffect")]
    public class ThreadOfFateEffectSO : SkillCardEffectSO
    {
        [Header("운명의 실 설정")]
        [Tooltip("디버프 지속 턴 수 (기본값: 1)")]
        [SerializeField] private int duration = 1;

        /// <summary>
        /// 이펙트 실행 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">커맨드에 사용될 수치 (무시됨)</param>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new ThreadOfFateEffectCommand(duration, GetIcon());
        }

        /// <summary>
        /// 효과를 즉시 적용합니다. (사용되지 않음 - ThreadOfFateEffectCommand로 대체됨)
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">디버프 지속 턴 수</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            Debug.LogWarning("[ThreadOfFateEffectSO] ApplyEffect는 레거시 메서드입니다. ThreadOfFateEffectCommand를 사용하세요.");
        }
    }
}

