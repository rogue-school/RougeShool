using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 시공간 역행 효과를 적용하는 스킬 카드 효과 ScriptableObject입니다.
    /// 대상의 체력을 3턴 전 상태로 복원합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "SpaceTimeReversalEffect", menuName = "SkillEffects/SpaceTimeReversalEffect")]
    public class SpaceTimeReversalEffectSO : SkillCardEffectSO
    {
        [Header("시공간 역행 설정")]
        [Tooltip("몇 턴 전 체력으로 복원할지 (기본값: 3)")]
        [SerializeField] private int turnsAgo = 3;

        /// <summary>
        /// 이펙트 실행 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">커맨드에 사용될 수치 (무시됨)</param>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new SpaceTimeReversalEffectCommand(turnsAgo);
        }

        /// <summary>
        /// 효과를 즉시 적용합니다. (사용되지 않음 - SpaceTimeReversalEffectCommand로 대체됨)
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">몇 턴 전인지</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            Debug.LogWarning("[SpaceTimeReversalEffectSO] ApplyEffect는 레거시 메서드입니다. SpaceTimeReversalEffectCommand를 사용하세요.");
        }
    }
}

