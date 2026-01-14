using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effect
{
    /// <summary>
    /// 분신 버프를 적용하는 스킬 카드 효과 ScriptableObject입니다.
    /// 대상에게 추가 체력 10을 부여하고, 추가 체력이 존재하는 동안 자신의 스킬 데미지가 2배로 증가합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CloneEffect", menuName = "SkillEffects/CloneEffect")]
    public class CloneEffectSO : SkillCardEffectSO
    {
        [Header("분신 설정")]
        [Tooltip("분신 추가 체력 (기본값: 10)")]
        [SerializeField] private int cloneHP = 10;

        /// <summary>
        /// 이펙트 실행 커맨드를 생성합니다.
        /// </summary>
        /// <param name="power">커맨드에 사용될 수치 (무시됨)</param>
        public override ICardEffectCommand CreateEffectCommand(int power)
        {
            return new CloneEffectCommand(cloneHP, GetIcon());
        }

        /// <summary>
        /// 효과를 즉시 적용합니다. (사용되지 않음 - CloneEffectCommand로 대체됨)
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">분신 추가 체력</param>
        /// <param name="turnManager">전투 턴 매니저 (사용되지 않음)</param>
        public override void ApplyEffect(ICardExecutionContext context, int value, ICombatTurnManager turnManager = null)
        {
            Debug.LogWarning("[CloneEffectSO] ApplyEffect는 레거시 메서드입니다. CloneEffectCommand를 사용하세요.");
        }
    }
}

