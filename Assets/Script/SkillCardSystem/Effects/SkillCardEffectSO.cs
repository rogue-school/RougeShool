using UnityEngine;
using Game.SkillCardSystem.Interface;
using Game.CombatSystem.Interface;

namespace Game.SkillCardSystem.Effects
{
    /// <summary>
    /// ScriptableObject 기반 카드 이펙트의 추상 베이스 클래스입니다.
    /// 모든 SO 이펙트는 이 클래스를 상속해야 합니다.
    /// </summary>
    public abstract class SkillCardEffectSO : ScriptableObject, ICardEffect
    {
        [Header("이펙트 이름")]
        [SerializeField] private string effectName;

        [TextArea(2, 5)]
        [SerializeField] private string description;

        /// <summary>
        /// 이펙트 실행 메서드. 구체 구현체에서 오버라이드하여 구현해야 합니다.
        /// </summary>
        /// <param name="context">카드 실행 컨텍스트</param>
        /// <param name="value">효과 수치</param>
        /// <param name="controller">턴 상태 컨트롤러 (필요 시)</param>
        public abstract void ApplyEffect(ICardExecutionContext context, int value, ITurnStateController controller = null);

        /// <summary>
        /// 효과 이름을 반환합니다.
        /// </summary>
        public virtual string GetEffectName() => effectName;

        /// <summary>
        /// 효과 설명을 반환합니다.
        /// </summary>
        public virtual string GetDescription() => description;
    }
}
